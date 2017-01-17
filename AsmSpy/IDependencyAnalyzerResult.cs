using System.Collections.Generic;
using System.IO;

namespace AsmSpy
{
    public interface IDependencyAnalyzerResult
    {
        ICollection<FileInfo> AnalyzedFiles { get; }
        IDictionary<string, AssemblyReferenceInfo> Assemblies { get; }
    }
}