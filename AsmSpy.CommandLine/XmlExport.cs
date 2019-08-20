using System;
using System.IO;
using System.Linq;
using System.Xml;
using AsmSpy.Core;

namespace AsmSpy.CommandLine
{
    public sealed class XmlExport : IDependencyVisualizer
    {
        private readonly IDependencyAnalyzerResult _result;
        private readonly string _fileName;
        private readonly ILogger _logger;

        public string ReferencedStartsWith { get; set; }
        public bool SkipSystem { get; set; }

        public XmlExport(IDependencyAnalyzerResult result, string fileName, ILogger logger)
        {
            _result = result ?? throw new ArgumentNullException(nameof(result));
            _fileName = fileName?.Trim();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Visualize()
        {
            if (string.IsNullOrWhiteSpace(_fileName))
            {
                _logger.LogError("No valid filename specified.");
                return;
            }

            _logger.LogMessage($"Exporting to {_fileName}...");

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                CloseOutput = false
            };

            using (var stream = File.Open(_fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();
                WriteAssemblies(writer);
                writer.WriteEndDocument();
            }
        }

        private void WriteAssemblies(XmlWriter writer)
        {
            writer.WriteStartElement("Assemblies");
            var assemblyGroups = _result.Assemblies.Values.GroupBy(x => x.RedirectedAssemblyName);
            foreach (var assemblyGroup in assemblyGroups.OrderBy(i => i.Key.Name))
            {
                if (SkipSystem && AssemblyInformationProvider.IsSystemAssembly(assemblyGroup.Key))
                {
                    continue;
                }

                var assemblyInfos = assemblyGroup.OrderBy(x => x.AssemblyName.Name).ToList();
                if (assemblyInfos.Count <= 1)
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
                    .Where(x => x.Key.ToUpperInvariant().StartsWith(ReferencedStartsWith.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(ReferencedStartsWith) && !referenced.Any())
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
                                if (!string.IsNullOrEmpty(ReferencedStartsWith))
                                {
                                    if (!referer.AssemblyName.Name.ToUpperInvariant().StartsWith(ReferencedStartsWith.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        continue;
                                    }
                                }

                                using (writer.WriteElementScope("Referer"))
                                {
                                    writer.WriteAttributeString("Name", referer.AssemblyName.Name);
                                    writer.WriteAttributeString("FullName", referer.AssemblyName.FullName);
                                }
                            }
                        }
                    }
                }
            }
            writer.WriteEndElement();
        }
    }
}
