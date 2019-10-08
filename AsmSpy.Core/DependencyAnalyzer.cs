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
        private static AssemblyReferenceInfo GetAssemblyReferenceInfo(
            IDictionary<string, AssemblyReferenceInfo> assemblies, 
            AssemblyName assemblyName,
            AppDomain appDomainWithBindingRedirects,
            VisualizerOptions options)
        {
            if (options.SkipSystem && AssemblyInformationProvider.IsSystemAssembly(assemblyName))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(options.ReferencedStartsWith) && !assemblyName.FullName.StartsWith(options.ReferencedStartsWith, StringComparison.OrdinalIgnoreCase))
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

            assemblyReferenceInfo = new AssemblyReferenceInfo(assemblyName, new AssemblyName(assemblyFullName));
            assemblies.Add(assemblyFullName, assemblyReferenceInfo);
            return assemblyReferenceInfo;
        }


        public static DependencyAnalyzerResult Analyze(
            IEnumerable<FileInfo> files, 
            AppDomain appDomainWithBindingRedirects,
            ILogger logger,
            VisualizerOptions options)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            var result = new DependencyAnalyzerResult(files.ToArray());

            if (result.AnalyzedFiles.Count <= 0)
            {
                return result;
            }

            foreach (var fileInfo in result.AnalyzedFiles.Where(x => x.IsAssembly()).OrderBy(asm => asm.Name))
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.ReflectionOnlyLoadFrom(fileInfo.FullName);
                    logger.LogMessage($"File {fileInfo.FullName} => {assembly.GetName().Name} {assembly.GetName().Version.ToString()}");
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Failed to load assembly '{fileInfo.FullName}': {ex.Message}");
                    continue;
                }

                var assemblyReferenceInfo = GetAssemblyReferenceInfo(result.Assemblies, assembly.GetName(), appDomainWithBindingRedirects, options);
                if (assemblyReferenceInfo == null)
                {
                    continue;
                }

                assemblyReferenceInfo.ReflectionOnlyAssembly = assembly;
                assemblyReferenceInfo.AssemblySource = AssemblySource.Local;

                foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                {
                    var referencedAssemblyReferenceInfo = GetAssemblyReferenceInfo(result.Assemblies, referencedAssembly, appDomainWithBindingRedirects, options);
                    if (referencedAssemblyReferenceInfo == null)
                    {
                        continue;
                    }

                    assemblyReferenceInfo.AddReference(referencedAssemblyReferenceInfo);
                    referencedAssemblyReferenceInfo.AddReferencedBy(assemblyReferenceInfo);
                }
            }

            foreach (var assembly in result.Assemblies.Values.Where(x => x.ReflectionOnlyAssembly == null).OrderBy(x => x.AssemblyName.Name))
            {
                try
                {
                    assembly.ReflectionOnlyAssembly = Assembly.ReflectionOnlyLoad(assembly.AssemblyName.FullName);
                    assembly.AssemblySource = assembly.ReflectionOnlyAssembly.GlobalAssemblyCache 
                        ? AssemblySource.GlobalAssemblyCache 
                        : AssemblySource.Unknown;

                    logger.LogMessage($"Found reference {assembly.AssemblyName.Name} {assembly.AssemblyName.Version.ToString()}");
                }
                catch
                {
                    logger.LogWarning($"Could not load reference {assembly.AssemblyName.Name} {assembly.AssemblyName.Version.ToString()}");
                    assembly.AssemblySource = AssemblySource.NotFound;
                }
            }
            return result;
        }
    }
}

