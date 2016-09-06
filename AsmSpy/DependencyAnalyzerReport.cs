using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AsmSpy
{
    public class DependencyAnalyzerResult
    {
        public FileInfo[] AnalyzedFiles { get; set; }
        public Dictionary<string, AssemblyReferenceInfo> Assemblies { get; set; }
    }
}
