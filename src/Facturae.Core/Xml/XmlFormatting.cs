// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Xml;

namespace FacturaeViewer.Core.Xml;

/// <summary>
/// Utilidades para presentar documentos XML: formateado legible (indentado)
/// e índice de líneas para navegación. Sin dependencias de UI.
/// </summary>
public static class XmlFormatting
{
    /// <summary>
    /// Serializa un documento XML con indentación legible. Se escribe
    /// directamente desde el documento cargado (que preserva el whitespace del
    /// fichero) saltando los nodos de texto de whitespace para que el writer
    /// aplique su propia indentación, evitando así duplicar el documento en
    /// memoria (OuterXml + re-parseo).
    /// </summary>
    public static string Format(XmlDocument xml)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = true,
            NewLineChars = Environment.NewLine,
        };

        using var sw = new StringWriter();
        using (var writer = XmlWriter.Create(sw, settings))
            WriteElement(xml.DocumentElement, writer);
        return sw.ToString();
    }

    /// <summary>
    /// Calcula la línea (base 0) en el XML formateado donde empieza cada
    /// elemento, indexada por su nombre local. Solo se conserva la primera
    /// aparición de cada elemento (suficiente para navegar al nodo).
    /// </summary>
    public static IReadOnlyDictionary<string, int> IndexElementLines(string xml)
    {
        var lines = new Dictionary<string, int>();
        if (string.IsNullOrWhiteSpace(xml))
            return lines;

        var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit };
        using var reader = XmlReader.Create(new StringReader(xml), settings);
        if (reader is not IXmlLineInfo lineInfo || !lineInfo.HasLineInfo())
            return lines;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
                lines.TryAdd(reader.LocalName, lineInfo.LineNumber - 1);
        }

        return lines;
    }

    /// <summary>Escribe un elemento y sus hijos, omitiendo el whitespace original.</summary>
    private static void WriteElement(XmlElement? element, XmlWriter writer)
    {
        if (element is null)
            return;

        writer.WriteStartElement(element.Prefix, element.LocalName, element.NamespaceURI);

        foreach (XmlAttribute attribute in element.Attributes)
        {
            if (attribute.Prefix == "xmlns" || attribute.Name == "xmlns")
                continue;
            writer.WriteAttributeString(attribute.Prefix, attribute.LocalName, attribute.NamespaceURI, attribute.Value);
        }

        foreach (XmlNode child in element.ChildNodes)
        {
            switch (child.NodeType)
            {
                case XmlNodeType.Element:
                    WriteElement((XmlElement)child, writer);
                    break;
                case XmlNodeType.Text:
                    writer.WriteString(child.Value);
                    break;
                case XmlNodeType.CDATA:
                    writer.WriteCData(child.Value ?? string.Empty);
                    break;
                case XmlNodeType.Comment:
                    writer.WriteComment(child.Value ?? string.Empty);
                    break;
                case XmlNodeType.ProcessingInstruction:
                    writer.WriteProcessingInstruction(child.Name, child.Value ?? string.Empty);
                    break;
            }
        }

        writer.WriteEndElement();
    }
}