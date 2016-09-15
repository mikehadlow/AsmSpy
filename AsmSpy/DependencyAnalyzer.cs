using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AsmSpy.Native;

namespace AsmSpy
{
    public class DependencyAnalyzer : IDependencyAnalyzer
    {
        #region Properties

        public DirectoryInfo DirectoryInfo { get; set; }

        #endregion

        #region Analyze Support

        private IEnumerable<FileInfo> GetLibrariesAndExecutables()
        {
            return DirectoryInfo.GetFiles("*.dll").Concat(DirectoryInfo.GetFiles("*.exe"));
        }

        private AssemblyReferenceInfo GetAssemblyReferenceInfo(Dictionary<string, AssemblyReferenceInfo> assemblies, AssemblyName assemblyName)
        {
            AssemblyReferenceInfo assemblyReferenceInfo;
            if (!assemblies.TryGetValue(assemblyName.FullName, out assemblyReferenceInfo))
            {
                assemblyReferenceInfo = new AssemblyReferenceInfo(assemblyName);
                assemblies.Add(assemblyName.FullName, assemblyReferenceInfo);
            }
            return assemblyReferenceInfo;
        }


        public DependencyAnalyzerResult Analyze(ILogger logger)
        {
            var result = new DependencyAnalyzerResult();

            result.AnalyzedFiles = GetLibrariesAndExecutables().ToArray();

            if (result.AnalyzedFiles.Length <= 0)
            {
                return result;
            }

            result.Assemblies = new Dictionary<string, AssemblyReferenceInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var fileInfo in result.AnalyzedFiles.OrderBy(asm => asm.Name))
            {
                logger.LogMessage(string.Format("Checking file {0}", fileInfo.Name));
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
                    logger.LogWarning(string.Format("Failed to load assembly '{0}': {1}", fileInfo.FullName, ex.Message));
                    continue;
                }
                var assemblyReferenceInfo = GetAssemblyReferenceInfo(result.Assemblies, assembly.GetName());

                foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                {
                    var referencedAssemblyReferenceInfo = GetAssemblyReferenceInfo(result.Assemblies, referencedAssembly); ;
                    assemblyReferenceInfo.AddReference(referencedAssemblyReferenceInfo);
                    referencedAssemblyReferenceInfo.AddReferencedBy(assemblyReferenceInfo);
                }
            }
            return result;
        }

        #endregion
    }
}

