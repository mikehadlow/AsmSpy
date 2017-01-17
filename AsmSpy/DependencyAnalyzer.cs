using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using AsmSpy.Native;

namespace AsmSpy
{
    public class DependencyAnalyzer : IDependencyAnalyzer
    {
        #region Properties

        public virtual IEnumerable<FileInfo> Files { get; }

        #endregion

        #region Analyze Support

        public DependencyAnalyzer(IEnumerable<FileInfo> files)
        {
            Files = files;
        }

        private static AssemblyReferenceInfo GetAssemblyReferenceInfo(IDictionary<string, AssemblyReferenceInfo> assemblies, AssemblyName assemblyName)
        {
            AssemblyReferenceInfo assemblyReferenceInfo;
            if (assemblies.TryGetValue(assemblyName.FullName, out assemblyReferenceInfo))
            {
                return assemblyReferenceInfo;
            }

            assemblyReferenceInfo = new AssemblyReferenceInfo(assemblyName);
            assemblies.Add(assemblyName.FullName, assemblyReferenceInfo);
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
                assemblyReferenceInfo.ReflectionOnlyAssembly = assembly;
                assemblyReferenceInfo.AssemblySource = AssemblySource.Local;
                foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                {
                    var referencedAssemblyReferenceInfo = GetAssemblyReferenceInfo(result.Assemblies, referencedAssembly);
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

