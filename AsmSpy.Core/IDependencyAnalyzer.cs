using System.Collections.Generic;
using System.IO;

namespace AsmSpy.Core
{
    public interface IDependencyAnalyzer
    {
        IDependencyAnalyzerResult Analyze(ILogger logger);
        IEnumerable<FileInfo> Files { get; }
    }
}
