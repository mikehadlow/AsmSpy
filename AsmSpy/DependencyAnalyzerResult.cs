using System.Collections.Generic;
using System.IO;

namespace AsmSpy
{
    public class DependencyAnalyzerResult
    {
        public ICollection<FileInfo> AnalyzedFiles { get; set; }
        public Dictionary<string, AssemblyReferenceInfo> Assemblies { get; set; }
    }
}
