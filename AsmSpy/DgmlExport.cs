using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AsmSpy
{
    public class DgmlExport : IDependencyVisualizer
    {
        DependencyAnalyzerResult _Result;
        string _ExportFilename;
        ILogger _Logger;

        public DgmlExport(DependencyAnalyzerResult result, string exportFilename, ILogger logger)
        {
            _Result = result;
            _ExportFilename = exportFilename;
            _Logger = logger;
        }

        public void Visualize()
        {
            var nodes = new StringBuilder();

            foreach (var assemblyReference in _Result.Assemblies.Values)
            {
                nodes.AppendFormat("<Node Id=\"{0}\" Label=\"{1}\" Category=\"Assembly\" />\n",
                    assemblyReference.AssemblyName.FullName, assemblyReference.AssemblyName.Name);
            }

            var links = new StringBuilder();
            foreach (var assemblyReference in _Result.Assemblies.Values)
            {
                foreach (var referenceTo in assemblyReference.References)
                {
                    links.AppendFormat("<Link Source=\"{0}\" Target=\"{1}\" Category=\"Reference\" />\n",
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
                File.WriteAllText(_ExportFilename, dgml.ToString());
                _Logger.LogMessage(string.Format("Exported to file {0}", _ExportFilename));
            }
            catch (System.UnauthorizedAccessException uae)
            {
                _Logger.LogError(string.Format("Could not write file {0} due to error {1}", _ExportFilename, uae.Message));
            }
            catch (System.IO.DirectoryNotFoundException dnfe)
            {
                _Logger.LogError(string.Format("Could not write file {0} due to error {1}", _ExportFilename, dnfe.Message));            
            }
        }
    }
}
