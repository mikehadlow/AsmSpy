using AsmSpy.Core;
using System.IO;

namespace AsmSpy.CommandLine
{
    public class DotExport : IDependencyVisualizer
    {
        private readonly IDependencyAnalyzerResult _result;
        private readonly string _exportFileName;
        private readonly ILogger _logger;

        public bool SkipSystem { get; set; } = false;

        public DotExport(IDependencyAnalyzerResult result, string exportFileName, ILogger logger)
        {
            _result = result;
            _exportFileName = exportFileName;
            _logger = logger;
        }

        public void Visualize()
        {
            using(var writer = new StreamWriter(_exportFileName))
            {
                writer.WriteLine("digraph {");

                foreach (var assemblyReference in _result.Assemblies.Values)
                {
                    if (SkipSystem && assemblyReference.IsSystem)
                        continue;

                    var redBackground = ", color=red";

                    writer.WriteLine($"    {assemblyReference.AssemblyName.FullName.GetHashCode()} " + 
                        $"[label=\"{assemblyReference.AssemblyName.Name}\\n{assemblyReference.AssemblyName.Version.ToString()}\"" + 
                        $"{(assemblyReference.AssemblySource == AssemblySource.NotFound ? redBackground : string.Empty)}]");
                }

                writer.WriteLine();

                foreach (var assemblyReference in _result.Assemblies.Values)
                {
                    if (SkipSystem && assemblyReference.IsSystem)
                        continue;

                    foreach (var referenceTo in assemblyReference.References)
                    {
                        if (SkipSystem && referenceTo.IsSystem)
                            continue;

                        writer.WriteLine($"    {assemblyReference.AssemblyName.FullName.GetHashCode()} -> {referenceTo.AssemblyName.FullName.GetHashCode()};");
                    }
                }

                writer.WriteLine("}");
            }
            _logger.LogMessage($"Exported to file {_exportFileName}");
        }
    }
}
