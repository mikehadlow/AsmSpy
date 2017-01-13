using System.IO;

namespace AsmSpy
{
    public interface IDependencyAnalyzer
    {
        DependencyAnalyzerResult Analyze(ILogger logger);
        DirectoryInfo DirectoryInfo { get; }
    }
}
