using System;
using System.Xml;

namespace AsmSpy.CommandLine.Utilities
{
    public static class XmlWriterExtensions
    {
        public static void WriteElement(this XmlWriter writer, string name, string value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Element name cannot be empty.", nameof(name));
            }

            writer.WriteStartElement(name);
            writer.WriteValue(value ?? string.Empty);
            writer.WriteEndElement();
        }

        public static IDisposable WriteElementScope(this XmlWriter writer, string name)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Element name for scope cannot be empty.", nameof(name));
            }

            writer.WriteStartElement(name);
            return new XmlWriterElementScope(writer);
        }
    }
}
