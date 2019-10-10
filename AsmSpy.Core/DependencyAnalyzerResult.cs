using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AsmSpy.Core
{
    public class DependencyAnalyzerResult
    {
        public ICollection<FileInfo> AnalyzedFiles { get; }
        public IDictionary<string, AssemblyReferenceInfo> Assemblies { get; }
        public IEnumerable<AssemblyReferenceInfo> MissingAssemblies => Assemblies.Where(x => x.Value.AssemblySource == AssemblySource.NotFound).Select(x => x.Value);
        public IEnumerable<AssemblyReferenceInfo> Roots => roots;

        private IList<AssemblyReferenceInfo> roots;

        public DependencyAnalyzerResult(ICollection<FileInfo> analyzedFiles)
            : this(analyzedFiles, new Dictionary<string, AssemblyReferenceInfo>(StringComparer.OrdinalIgnoreCase))
        { }

        public DependencyAnalyzerResult(ICollection<FileInfo> analyzedFiles, IDictionary<string, AssemblyReferenceInfo> assemblies) 
        {
            AnalyzedFiles = analyzedFiles;
            Assemblies = assemblies;
            roots = new List<AssemblyReferenceInfo>();
        }

        public void AddRoot(AssemblyReferenceInfo root) => roots.Add(root);
    }
}
