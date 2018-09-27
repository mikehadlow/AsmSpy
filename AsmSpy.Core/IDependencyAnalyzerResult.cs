using System.Collections.Generic;
using System.IO;

namespace AsmSpy.Core
{
    public interface IDependencyAnalyzerResult
    {
        ICollection<FileInfo> AnalyzedFiles { get; }
        IDictionary<string, AssemblyReferenceInfo> Assemblies { get; }
        IEnumerable<AssemblyReferenceInfo> MissingAssemblies { get; }
    }
}