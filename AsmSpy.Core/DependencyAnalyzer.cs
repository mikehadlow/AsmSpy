using AsmSpy.Core.Native;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AsmSpy.Core
{
    public static class DependencyAnalyzer
    {
        public static DependencyAnalyzerResult Analyze(
            IEnumerable<FileInfo> files, 
            AppDomain appDomainWithBindingRedirects,
            ILogger logger,
            VisualizerOptions options,
            string rootFileName = "")
        {
            if (files == null) throw new ArgumentNullException(nameof(files));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (rootFileName == null) throw new ArgumentNullException(nameof(rootFileName));

            var result = new DependencyAnalyzerResult(files.ToArray());

            if (result.AnalyzedFiles.Count <= 0)
            {
                return result;
            }

            ResolveFileReferences(result, logger, appDomainWithBindingRedirects, options);
            MapAssemblyReferences(result, appDomainWithBindingRedirects, options);
            ResolveNonFileReferences(result, logger);
            FindRootAssemblies(result, logger, rootFileName);
            MarkReferencedAssemblies(result);

            return result;
        }

        private static void ResolveFileReferences(
            DependencyAnalyzerResult result, 
            ILogger logger, 
            AppDomain appDomainWithBindingRedirects, 
            VisualizerOptions options)
        {
            foreach (var fileInfo in result.AnalyzedFiles.Where(x => x.IsAssembly()).OrderBy(asm => asm.Name))
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.ReflectionOnlyLoadFrom(fileInfo.FullName);
                    logger.LogMessage($"File {fileInfo.Name} => {assembly.GetName().Name} {assembly.GetName().Version.ToString()}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Failed to load assembly '{fileInfo.FullName}': {ex.Message}");
                    continue;
                }

                var assemblyReferenceInfo = GetAssemblyReferenceInfo(
                    result.Assemblies,
                    assembly.GetName(),
                    appDomainWithBindingRedirects,
                    options,
                    fileInfo.Name);

                if (assemblyReferenceInfo != null)
                {
                    assemblyReferenceInfo.ReflectionOnlyAssembly = assembly;
                    assemblyReferenceInfo.AssemblySource = AssemblySource.Local;
                }
            }
        }

        private static void MapAssemblyReferences(
            DependencyAnalyzerResult result, 
            AppDomain appDomainWithBindingRedirects,
            VisualizerOptions options)
        {
            var copyOfAssemblies = new AssemblyReferenceInfo[result.Assemblies.Count];
            result.Assemblies.Values.CopyTo(copyOfAssemblies, 0);
            foreach(var assembly in copyOfAssemblies)
            {
                foreach (var referencedAssembly in assembly.ReflectionOnlyAssembly.GetReferencedAssemblies())
                {
                    var referencedAssemblyReferenceInfo = GetAssemblyReferenceInfo(
                        result.Assemblies,
                        referencedAssembly,
                        appDomainWithBindingRedirects,
                        options);

                    if (referencedAssemblyReferenceInfo != null)
                    {
                        assembly.AddReference(referencedAssemblyReferenceInfo);
                        referencedAssemblyReferenceInfo.AddReferencedBy(assembly);
                    }
                }
            }
        }

        private static void ResolveNonFileReferences(
            DependencyAnalyzerResult result,
            ILogger logger) 
        {
            foreach (var assembly in result.Assemblies.Values.Where(x => x.ReflectionOnlyAssembly == null).OrderBy(x => x.AssemblyName.Name))
            {
                try
                {
                    assembly.ReflectionOnlyAssembly = Assembly.ReflectionOnlyLoad(assembly.RedirectedAssemblyName?.FullName ?? assembly.AssemblyName.FullName);
                    assembly.AssemblySource = assembly.ReflectionOnlyAssembly.GlobalAssemblyCache
                        ? AssemblySource.GlobalAssemblyCache
                        : AssemblySource.Unknown;

                    logger.LogMessage($"Found reference {assembly.AssemblyName.Name} {assembly.AssemblyName.Version.ToString()}");
                }
                catch(FileNotFoundException)
                {
                    var alternativeVersion = result.Assemblies.Values
                        .Where(x => x.AssemblyName.Name == assembly.AssemblyName.Name)
                        .Where(x => x.ReflectionOnlyAssembly != null)   
                        .FirstOrDefault();

                    if(alternativeVersion != null)
                    {
                        logger.LogWarning($"Found different version reference {assembly.AssemblyName.Name}, " + 
                            $"requested: {assembly.AssemblyName.Version.ToString()}" +
                            $"-> found: {alternativeVersion.AssemblyName.Version.ToString()}");

                        assembly.SetAlternativeFoundVersion(alternativeVersion);
                    }
                    else
                    {
                        logger.LogWarning($"Could not load reference {assembly.AssemblyName.Name} {assembly.AssemblyName.Version.ToString()}");
                        assembly.AssemblySource = AssemblySource.NotFound;
                    }
                }
                catch (FileLoadException)
                {
                    logger.LogError($"Failed to load {assembly.AssemblyName.Name} {assembly.AssemblyName.Version.ToString()}");
                    assembly.AssemblySource = AssemblySource.NotFound;
                }
            }
        }

        private static void FindRootAssemblies(
            DependencyAnalyzerResult result, 
            ILogger logger,
            string rootFileName) 
        {
            if(rootFileName == string.Empty)
            {
                foreach(var root in result.Assemblies.Values.Where(x => x.ReferencedBy.Count == 0))
                {
                    result.AddRoot(root);
                }
            }
            else
            {
                var root = result.Assemblies.Values.SingleOrDefault(x => x.FileName == rootFileName);
                if(root == null)
                {
                    logger.LogError($"Could not find root file: '{rootFileName}'");
                    foreach(var asm in result.Assemblies.Values)
                    {
                        logger.LogMessage($"Assembly filename: '{asm.FileName}'");
                    }
                }
                else
                {
                    result.AddRoot(root);
                }
            }
            foreach(var root in result.Roots)
            {
                logger.LogMessage($"Root: {root.AssemblyName.Name}");
            }
        }

        private static void MarkReferencedAssemblies(DependencyAnalyzerResult result)
        {
            foreach(var assembly in result.Roots)
            {
                WalkAndMark(assembly);
            }
            
            void WalkAndMark(IAssemblyReferenceInfo assembly)
            {
                if (assembly.ReferencedByRoot)
                {
                    return;
                }

                assembly.ReferencedByRoot = true;
                foreach(var dependency in assembly.References)
                {
                    WalkAndMark(dependency);
                }
            }
        }

        private static AssemblyReferenceInfo GetAssemblyReferenceInfo(
            IDictionary<string, AssemblyReferenceInfo> assemblies, 
            AssemblyName assemblyName,
            AppDomain appDomainWithBindingRedirects,
            VisualizerOptions options,
            string fileName = "")
        {
            if (options.SkipSystem && AssemblyInformationProvider.IsSystemAssembly(assemblyName))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(options.ReferencedStartsWith) && 
                !assemblyName.FullName.StartsWith(options.ReferencedStartsWith, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (options.Exclude.Any(e => assemblyName.FullName.StartsWith(e, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            var assemblyFullName = appDomainWithBindingRedirects != null 
                ? appDomainWithBindingRedirects.ApplyPolicy(assemblyName.FullName) 
                : assemblyName.FullName;

            if (assemblies.TryGetValue(assemblyFullName, out AssemblyReferenceInfo assemblyReferenceInfo))
            {
                return assemblyReferenceInfo;
            }

            assemblyReferenceInfo = new AssemblyReferenceInfo(assemblyName, new AssemblyName(assemblyFullName), fileName);
            assemblies.Add(assemblyFullName, assemblyReferenceInfo);
            return assemblyReferenceInfo;
        }
    }
}

