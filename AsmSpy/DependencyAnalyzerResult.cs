using System;
using System.Collections.Generic;
using System.IO;

namespace AsmSpy
{
    public class DependencyAnalyzerResult
    {
        public DependencyAnalyzerResult(ICollection<FileInfo> analyzedFiles)
        {
            AnalyzedFiles = analyzedFiles;
            Assemblies = new Dictionary<string, AssemblyReferenceInfo>(StringComparer.OrdinalIgnoreCase);
        }

        public DependencyAnalyzerResult(ICollection<FileInfo> analyzedFiles, Dictionary<string, AssemblyReferenceInfo> assemblies) : this(analyzedFiles)
        {
            AnalyzedFiles = analyzedFiles;
            Assemblies = assemblies;
        }

        public ICollection<FileInfo> AnalyzedFiles { get; private set; }
        public Dictionary<string, AssemblyReferenceInfo> Assemblies { get; private set; }
    }
}
