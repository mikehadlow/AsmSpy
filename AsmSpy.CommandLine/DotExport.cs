using AsmSpy.Core;
using Microsoft.Extensions.CommandLineUtils;
using System.IO;

namespace AsmSpy.CommandLine
{
    public class DotExport : IDependencyVisualizer
    {
        private string exportFileName;

        public void Visualize(IDependencyAnalyzerResult result, ILogger logger, VisualizerOptions visualizerOptions)
        {
            using(var writer = new StreamWriter(exportFileName))
            {
                writer.WriteLine("digraph {");

                foreach (var assemblyReference in result.Assemblies.Values)
                {
                    if (visualizerOptions.SkipSystem && assemblyReference.IsSystem)
                        continue;

                    var redBackground = ", color=red";

                    writer.WriteLine($"    {assemblyReference.AssemblyName.FullName.GetHashCode()} " + 
                        $"[label=\"{assemblyReference.AssemblyName.Name}\\n{assemblyReference.AssemblyName.Version.ToString()}\"" + 
                        $"{(assemblyReference.AssemblySource == AssemblySource.NotFound ? redBackground : string.Empty)}]");
                }

                writer.WriteLine();

                foreach (var assemblyReference in result.Assemblies.Values)
                {
                    if (visualizerOptions.SkipSystem && assemblyReference.IsSystem)
                        continue;

                    foreach (var referenceTo in assemblyReference.References)
                    {
                        if (visualizerOptions.SkipSystem && referenceTo.IsSystem)
                            continue;

                        writer.WriteLine($"    {assemblyReference.AssemblyName.FullName.GetHashCode()} -> {referenceTo.AssemblyName.FullName.GetHashCode()};");
                    }
                }

                writer.WriteLine("}");
            }
            logger.LogMessage($"Exported to file {exportFileName}");
        }

        private CommandOption dotExport;

        public void CreateOption(CommandLineApplication commandLineApplication)
        {
            dotExport = commandLineApplication.Option("-dt|--dot <filename>", "Export to a DOT file", CommandOptionType.SingleValue);
        }

        public bool IsConfigured()
        {
            if (dotExport.HasValue())
            {
                exportFileName = dotExport.Value();
            }
            return false;
        }
    }
}
