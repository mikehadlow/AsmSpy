using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using AsmSpy.Core;
using static System.FormattableString;

namespace AsmSpy.CommandLine
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

        private readonly IDependencyAnalyzerResult _result;
        private readonly string _exportFileName;
        private readonly ILogger _logger;

        public DgmlExport(IDependencyAnalyzerResult result, string exportFileName, ILogger logger)
        {
            _result = result;
            _exportFileName = exportFileName;
            _logger = logger;
        }

        #region Properties

        public bool SkipSystem { get; set; }

        #endregion

        public void Visualize()
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
                    foreach (var assemblyReference in _result.Assemblies.Values)
                    {
                        if (SkipSystem && assemblyReference.IsSystem)
                            continue;

                        dgml.WriteLine(Invariant($@"<Node Id=""{assemblyReference.AssemblyName.FullName}"" Label=""{assemblyReference.AssemblyName.Name}"" Category=""Assembly"">"));
                        dgml.WriteLine(Invariant($@"<Category Ref=""{assemblyReference.AssemblySource}"" />"));
                        dgml.WriteLine(@"</Node>");
                    }

                    dgml.WriteLine(@"</Nodes>");

                    dgml.WriteLine(@"<Links>");

                    foreach (var assemblyReference in _result.Assemblies.Values)
                    {
                        if (SkipSystem && assemblyReference.IsSystem)
                            continue;

                        foreach (var referenceTo in assemblyReference.References)
                        {
                            if (SkipSystem && referenceTo.IsSystem)
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

                _logger.LogMessage(Invariant($"Exported to file {_exportFileName}"));
            }
            catch (UnauthorizedAccessException uae)
            {
                _logger.LogError(Invariant($"Could not write file {_exportFileName} due to error {uae.Message}"));
            }
            catch (DirectoryNotFoundException dnfe)
            {
                _logger.LogError(Invariant($"Could not write file {_exportFileName} due to error {dnfe.Message}"));
            }
            finally
            {
                fileStream?.Dispose();
            }
        }
    }
}
