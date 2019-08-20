using System;
using System.Xml;

namespace AsmSpy.CommandLine
{
    public sealed class XmlWriterElementScope : IDisposable
    {
        private readonly XmlWriter _writer;

        public XmlWriterElementScope(XmlWriter writer)
        {
            _writer = writer;
        }

        public void Dispose()
        {
            _writer.WriteEndElement();
        }
    }
}
