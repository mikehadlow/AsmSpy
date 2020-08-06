using AsmSpy.Core;

using Microsoft.Extensions.CommandLineUtils;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using static System.FormattableString;

namespace AsmSpy.CommandLine.Visualizers
{
    public class DgmlExport : IDependencyVisualizer
    {
        private static readonly IReadOnlyDictionary<AssemblySource, Color> AssemblySourceColors = new Dictionary<AssemblySource, Color>()
        {
            { AssemblySource.NotFound, Color.Red },
            { AssemblySource.Local, Color.Green },
            { AssemblySource.GlobalAssemblyCache, Color.Yellow },
            { AssemblySource.Unknown, Color.Gray },
        };

        private string _exportFileName;

        private CommandOption dgmlExport;
        private CommandOption showVersion;

        public void CreateOption(CommandLineApplication commandLineApplication)
        {
            dgmlExport = commandLineApplication.Option("-dg|--dgml <filename>", "Export to a dgml file", CommandOptionType.SingleValue);
            showVersion = commandLineApplication.Option("-dgsv|--dgshowversion", "Show the assembly version on the label", CommandOptionType.NoValue);
        }

        public bool IsConfigured()
        {
            if (dgmlExport.HasValue())
            {
                _exportFileName = dgmlExport.Value();
                return true;
            }
            return false;
        }

        public void Visualize(DependencyAnalyzerResult result, ILogger logger, VisualizerOptions visualizerOptions)
        {
            Stream fileStream = null;
            try
            {
                fileStream = File.OpenWrite(_exportFileName);
                using (var dgml = new StreamWriter(fileStream))
                {
                    fileStream = null; // now the StreamWriter owns the stream (fix warning CA2202)

                    dgml.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
                    dgml.WriteLine(@"<DirectedGraph Title=""AsmSpy:References"" xmlns=""http://schemas.microsoft.com/vs/2009/dgml"">");

                    dgml.WriteLine(@"<Nodes>");
                    foreach (var assemblyReference in result.Assemblies.Values)
                    {
                        if (visualizerOptions.SkipSystem && assemblyReference.IsSystem)
                            continue;

                        var label = !showVersion.HasValue() ? assemblyReference.AssemblyName.Name : $"{assemblyReference.AssemblyName.Name}&#13;{ assemblyReference.AssemblyName.Version}";
                        dgml.WriteLine(Invariant($@"<Node Id=""{assemblyReference.AssemblyName.FullName}"" Label=""{label}"" Category=""Assembly"">"));
                        dgml.WriteLine(Invariant($@"<Category Ref=""{assemblyReference.AssemblySource}"" />"));
                        dgml.WriteLine(@"</Node>");
                    }

                    dgml.WriteLine(@"</Nodes>");

                    dgml.WriteLine(@"<Links>");

                    foreach (var assemblyReference in result.Assemblies.Values)
                    {
                        if (visualizerOptions.SkipSystem && assemblyReference.IsSystem)
                            continue;

                        foreach (var referenceTo in assemblyReference.References)
                        {
                            if (visualizerOptions.SkipSystem && referenceTo.IsSystem)
                                continue;

                            dgml.WriteLine(Invariant($@"<Link Source=""{assemblyReference.AssemblyName.FullName}"" Target=""{referenceTo.AssemblyName.FullName}"" Category=""Reference"" />"));
                        }
                    }

                    dgml.WriteLine(@"</Links>");

                    dgml.WriteLine(@"<Categories>");
                    dgml.WriteLine(@"<Category Id=""Assembly""/>");
                    dgml.WriteLine(@"<Category Id=""Reference""/>");

                    foreach (var kvp in AssemblySourceColors)
                    {
                        dgml.WriteLine(Invariant($@"<Category Id=""{kvp.Key}"" Label=""{kvp.Key}"" Background=""{ColorTranslator.ToHtml(kvp.Value)}"" IsTag=""True"" />"));
                    }

                    dgml.WriteLine(@"</Categories>");

                    dgml.WriteLine(@"<Styles>");

                    foreach (var kvp in AssemblySourceColors)
                    {
                        dgml.WriteLine(Invariant($@"<Style TargetType=""Node"" GroupLabel=""AssemblySource: {kvp.Key}"" ValueLabel=""Has category"">"));
                        dgml.WriteLine(Invariant($@"<Condition Expression=""HasCategory('{kvp.Key}')"" />"));
                        dgml.WriteLine(Invariant($@"<Setter Property=""Background"" Value=""{ColorTranslator.ToHtml(kvp.Value)}"" />"));
                        dgml.WriteLine(@"</Style>");
                    }

                    dgml.WriteLine(@"</Styles>");

                    dgml.WriteLine(@"</DirectedGraph>");
                }

                logger.LogMessage(Invariant($"Exported to file {_exportFileName}"));
            }
            catch (UnauthorizedAccessException uae)
            {
                logger.LogError(Invariant($"Could not write file {_exportFileName} due to error {uae.Message}"));
            }
            catch (DirectoryNotFoundException dnfe)
            {
                logger.LogError(Invariant($"Could not write file {_exportFileName} due to error {dnfe.Message}"));
            }
            finally
            {
                fileStream?.Dispose();
            }
        }
    }
}
