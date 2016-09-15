using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AsmSpy
{
    public interface IDependencyAnalyzer
    {
        DependencyAnalyzerResult Analyze(ILogger logger);
        DirectoryInfo DirectoryInfo { get; }
    }
}
