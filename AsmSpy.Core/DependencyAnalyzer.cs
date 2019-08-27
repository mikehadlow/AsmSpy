using AsmSpy.Core.Native;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AsmSpy.Core
{
    public class DependencyAnalyzer : IDependencyAnalyzer
    {
        #region Properties

        public virtual IEnumerable<FileInfo> Files { get; }

        protected virtual AppDomain AppDomainWithBindingRedirects { get; }

        public bool SkipSystem { get; set; }

        public string ReferencedStartsWith { get; set; }

        #endregion

        #region Analyze Support

        public DependencyAnalyzer(IEnumerable<FileInfo> files, AppDomain appDomainWithBindingRedirects = null)
        {
            Files = files;
            AppDomainWithBindingRedirects = appDomainWithBindingRedirects;
        }

        private AssemblyReferenceInfo GetAssemblyReferenceInfo(IDictionary<string, AssemblyReferenceInfo> assemblies, AssemblyName assemblyName)
        {
            if (SkipSystem && AssemblyInformationProvider.IsSystemAssembly(assemblyName))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(ReferencedStartsWith) && !assemblyName.FullName.StartsWith(ReferencedStartsWith, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var assemblyFullName = AppDomainWithBindingRedirects != null ? AppDomainWithBindingRedirects.ApplyPolicy(assemblyName.FullName) : assemblyName.FullName;

            AssemblyReferenceInfo assemblyReferenceInfo;
            if (assemblies.TryGetValue(assemblyFullName, out assemblyReferenceInfo))
            {
                return assemblyReferenceInfo;
            }

            assemblyReferenceInfo = new AssemblyReferenceInfo(assemblyName, new AssemblyName(assemblyFullName));
            assemblies.Add(assemblyFullName, assemblyReferenceInfo);
            return assemblyReferenceInfo;
        }


        public virtual IDependencyAnalyzerResult Analyze(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            var result = new DependencyAnalyzerResult(Files.ToArray());


            if (result.AnalyzedFiles.Count <= 0)
            {
                return result;
            }

            foreach (var fileInfo in result.AnalyzedFiles.OrderBy(asm => asm.Name))
            {
                logger.LogMessage(string.Format(CultureInfo.InvariantCulture, "Checking file {0}", fileInfo.Name));
                Assembly assembly;
                try
                {
                    if (!fileInfo.IsAssembly())
                    {
                        continue;
                    }
                    assembly = Assembly.ReflectionOnlyLoadFrom(fileInfo.FullName);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(string.Format(CultureInfo.InvariantCulture, "Failed to load assembly '{0}': {1}", fileInfo.FullName, ex.Message));
                    continue;
                }

                var assemblyReferenceInfo = GetAssemblyReferenceInfo(result.Assemblies, assembly.GetName());
                if (assemblyReferenceInfo == null)
                {
                    continue;
                }

                assemblyReferenceInfo.ReflectionOnlyAssembly = assembly;
                assemblyReferenceInfo.AssemblySource = AssemblySource.Local;
                foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                {
                    var referencedAssemblyReferenceInfo = GetAssemblyReferenceInfo(result.Assemblies, referencedAssembly);
                    if (referencedAssemblyReferenceInfo == null)
                    {
                        continue;
                    }

                    assemblyReferenceInfo.AddReference(referencedAssemblyReferenceInfo);
                    referencedAssemblyReferenceInfo.AddReferencedBy(assemblyReferenceInfo);
                }
            }

            foreach (var assembly in result.Assemblies.Values)
            {
                if (assembly.ReflectionOnlyAssembly != null)
                {
                    continue;
                }
                logger.LogMessage(string.Format(CultureInfo.InvariantCulture, "Checking reference {0}", assembly.AssemblyName.Name));
                try
                {
                    assembly.ReflectionOnlyAssembly = Assembly.ReflectionOnlyLoad(assembly.AssemblyName.FullName);
                    assembly.AssemblySource = assembly.ReflectionOnlyAssembly.GlobalAssemblyCache ? AssemblySource.GlobalAssemblyCache : AssemblySource.Unknown;
                }
                catch
                {
                    // TODO: Show message?
                }
            }
            return result;
        }

        #endregion
    }
}

