using System;
using System.IO;
using System.Linq;
using System.Xml;
using AsmSpy.Core;
using Microsoft.Extensions.CommandLineUtils;

namespace AsmSpy.CommandLine
{
    public sealed class XmlExport : IDependencyVisualizer
    {
        private string outputFile;

        public void Visualize(IDependencyAnalyzerResult result, ILogger logger, VisualizerOptions visualizerOptions)
        {
            if (string.IsNullOrWhiteSpace(outputFile))
            {
                logger.LogError("No valid filename specified.");
                return;
            }

            logger.LogMessage($"Exporting to {outputFile}...");

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                CloseOutput = false
            };

            using (var stream = File.Open(outputFile, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();
                WriteAssemblies(writer, result, visualizerOptions);
                writer.WriteEndDocument();
            }
        }

        private void WriteAssemblies(XmlWriter writer, IDependencyAnalyzerResult result, VisualizerOptions visualizerOptions)
        {
            writer.WriteStartElement("Assemblies");
            var assemblyGroups = result.Assemblies.Values.GroupBy(x => x.RedirectedAssemblyName);
            foreach (var assemblyGroup in assemblyGroups.OrderBy(i => i.Key.Name))
            {
                if (visualizerOptions.SkipSystem && AssemblyInformationProvider.IsSystemAssembly(assemblyGroup.Key))
                {
                    continue;
                }

                var assemblyInfos = assemblyGroup.OrderBy(x => x.AssemblyName.Name).ToList();
                if (visualizerOptions.OnlyConflicts && assemblyInfos.Count <= 1)
                {
                    if (assemblyInfos.Count == 1 && assemblyInfos[0].AssemblySource == AssemblySource.Local)
                    {
                        continue;
                    }
                    if (assemblyInfos.Count <= 0)
                    {
                        continue;
                    }
                }

                // Got any references? Respect the user's choices here.
                var referenced = assemblyInfos.SelectMany(x => x.ReferencedBy)
                    .GroupBy(x => x.AssemblyName.Name)
                    .Where(x => x.Key.ToUpperInvariant().StartsWith(visualizerOptions.ReferencedStartsWith.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(visualizerOptions.ReferencedStartsWith) && !referenced.Any())
                {
                    continue;
                }

                using (writer.WriteElementScope("Assembly"))
                {
                    writer.WriteAttributeString("Name", assemblyGroup.Key.Name);
                    writer.WriteAttributeString("Version", assemblyGroup.Key.Version.ToString());
                    writer.WriteAttributeString("FullName", assemblyGroup.Key.FullName);

                    foreach (var assemblyInfo in assemblyInfos)
                    {
                        using (writer.WriteElementScope("Reference"))
                        {
                            writer.WriteAttributeString("Source", assemblyInfo.AssemblySource.ToString());
                            if (assemblyInfo.AssemblySource != AssemblySource.NotFound)
                            {
                                writer.WriteAttributeString("Location", assemblyInfo.ReflectionOnlyAssembly.Location);
                            }

                            foreach (var referer in assemblyInfo.ReferencedBy.OrderBy(x => x.AssemblyName.ToString()))
                            {
                                // Skip this referer? Respect the user's choices here.
                                if (!string.IsNullOrEmpty(visualizerOptions.ReferencedStartsWith))
                                {
                                    if (!referer.AssemblyName.Name.ToUpperInvariant().StartsWith(visualizerOptions.ReferencedStartsWith.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        continue;
                                    }
                                }

                                using (writer.WriteElementScope("Referer"))
                                {
                                    writer.WriteAttributeString("Name", referer.AssemblyName.Name);
                                    writer.WriteAttributeString("Version", referer.AssemblyName.Version.ToString());
                                    writer.WriteAttributeString("FullName", referer.AssemblyName.FullName);
                                }
                            }
                        }
                    }
                }
            }
            writer.WriteEndElement();
        }

        private CommandOption xml;

        public void CreateOption(CommandLineApplication commandLineApplication)
        {
            xml = commandLineApplication.Option("-x|--xml <filename>", "Export to a xml file", CommandOptionType.SingleValue);
        }

        public bool IsConfigured()
        {
            if (xml.HasValue())
            {
                outputFile = xml.Value();
            }
            return false;
        }
    }
}
