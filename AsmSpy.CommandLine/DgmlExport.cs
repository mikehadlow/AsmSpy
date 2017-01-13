using System.Globalization;
using System.IO;
using System.Text;

namespace AsmSpy.CommandLine
{
    public class DgmlExport : IDependencyVisualizer
    {
        private readonly DependencyAnalyzerResult _result;
        private readonly string _exportFileName;
        private readonly ILogger _logger;

        public DgmlExport(DependencyAnalyzerResult result, string exportFileName, ILogger logger)
        {
            _result = result;
            _exportFileName = exportFileName;
            _logger = logger;
        }

        public void Visualize()
        {
            var nodes = new StringBuilder();

            foreach (var assemblyReference in _result.Assemblies.Values)
            {
                nodes.AppendFormat(CultureInfo.InvariantCulture, "<Node Id=\"{0}\" Label=\"{1}\" Category=\"Assembly\" />\n",
                    assemblyReference.AssemblyName.FullName, assemblyReference.AssemblyName.Name);
            }

            var links = new StringBuilder();
            foreach (var assemblyReference in _result.Assemblies.Values)
            {
                foreach (var referenceTo in assemblyReference.References)
                {
                    links.AppendFormat(CultureInfo.InvariantCulture, "<Link Source=\"{0}\" Target=\"{1}\" Category=\"Reference\" />\n",
                        assemblyReference.AssemblyName.FullName, referenceTo.AssemblyName.FullName);
                }
            }

            var dgml = new StringBuilder();
            dgml.Append("<?xml version=\"1.0\" encoding=\"utf - 8\"?>\n");
            dgml.Append("<DirectedGraph Title=\"AsmSpy:References\" xmlns=\"http://schemas.microsoft.com/vs/2009/dgml\">\n");

            dgml.Append("<Nodes>\n");
            dgml.Append(nodes);
            
            dgml.Append("</Nodes>\n");

            dgml.Append("<Links>\n");
            dgml.Append(links);
            dgml.Append("</Links>\n");

            dgml.Append("<Categories>\n");
            dgml.Append("<Category Id=\"Assembly\"/>\n");
            dgml.Append("<Category Id=\"Reference\"/>\n");
            dgml.Append("</Categories>\n");

            dgml.Append("</DirectedGraph>\n");
            try
            {
                File.WriteAllText(_exportFileName, dgml.ToString());
                _logger.LogMessage(string.Format(CultureInfo.InvariantCulture, "Exported to file {0}", _exportFileName));
            }
            catch (System.UnauthorizedAccessException uae)
            {
                _logger.LogError(string.Format(CultureInfo.InvariantCulture, "Could not write file {0} due to error {1}", _exportFileName, uae.Message));
            }
            catch (DirectoryNotFoundException dnfe)
            {
                _logger.LogError(string.Format(CultureInfo.InvariantCulture, "Could not write file {0} due to error {1}", _exportFileName, dnfe.Message));            
            }
        }
    }
}
