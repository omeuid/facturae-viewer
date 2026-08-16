// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Xml;
using System.Xml.Serialization;
using FacturaeViewer.Core.Model;

namespace FacturaeViewer.Core.IO;

/// <summary>
/// Carga un fichero FacturaE (XML, .xsig o .xpsig) en un
/// <see cref="FacturaeDocument"/>. Detecta la versión del esquema a partir
/// del campo <c>FileHeader/SchemaVersion</c> y del espacio de nombres raíz.
/// </summary>
public static class FacturaeLoader
{
    /// <summary>Versión de esquema -> espacio de nombres oficial.</summary>
    public static readonly IReadOnlyDictionary<string, string> NamespaceByVersion =
        new Dictionary<string, string>
        {
            ["3.1"] = FacturaeNamespaces.NamespaceV3_1,
            ["3.2"] = FacturaeNamespaces.NamespaceV3_2,
            ["3.2.1"] = FacturaeNamespaces.NamespaceV3_2_1,
            ["3.2.2"] = FacturaeNamespaces.NamespaceV3_2_2,
        };

    /// <summary>Espacio de nombres oficial -> versión de esquema.</summary>
    public static readonly IReadOnlyDictionary<string, string> VersionByNamespace =
        NamespaceByVersion.ToDictionary(kv => kv.Value, kv => kv.Key);

    private static readonly XmlSerializer Serializer = new(typeof(Facturae));

    public static FacturaeDocument Load(string path)
    {
        using var stream = File.OpenRead(path);
        return Load(stream);
    }

    public static FacturaeDocument Load(Stream stream)
    {
        var xml = new XmlDocument { PreserveWhitespace = true };
        xml.Load(stream);
        return Parse(xml);
    }

    public static FacturaeDocument Parse(string xml)
    {
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.LoadXml(xml);
        return Parse(doc);
    }

    public static FacturaeDocument Parse(XmlDocument xml)
    {
        var root = xml.DocumentElement
            ?? throw new FacturaeParseException("El documento no tiene elemento raíz.");

        string rootNamespace = root.NamespaceURI;
        string? schemaVersion = ReadSchemaVersion(root);

        string version = ResolveVersion(rootNamespace, schemaVersion);
        string modelNamespace = NamespaceByVersion[version];

        Facturae model;
        try
        {
            var target = rootNamespace == FacturaeNamespaces.NamespaceV3_2
                ? xml
                : NormalizeRootNamespace(xml, FacturaeNamespaces.NamespaceV3_2);

            using var reader = new XmlNodeReader(target);
            model = (Facturae)(Serializer.Deserialize(reader)
                ?? throw new FacturaeParseException("No se pudo deserializar el contenido del documento."));
        }
        catch (InvalidOperationException ex)
        {
            throw new FacturaeParseException("El contenido del fichero no se corresponde con el formato FacturaE.", ex);
        }

        return new FacturaeDocument(model, xml, version, modelNamespace);
    }

    private static string ResolveVersion(string rootNamespace, string? schemaVersion)
    {
        if (VersionByNamespace.TryGetValue(rootNamespace, out var versionFromNs))
            return versionFromNs;

        if (schemaVersion is not null && NamespaceByVersion.ContainsKey(schemaVersion))
            return schemaVersion;

        // La versión se conoce pero no está soportada por esta aplicación.
        var detected = schemaVersion is null ? rootNamespace : schemaVersion;
        if (detected.StartsWith("3.", StringComparison.Ordinal))
            throw new FacturaeParseException(
                $"El documento usa el formato FacturaE {detected}, que no está soportado por esta aplicación. " +
                $"Versiones soportadas: {string.Join(", ", NamespaceByVersion.Keys)}.");

        throw new FacturaeParseException(
            "No se pudo determinar la versión del formato FacturaE (espacio de nombres y SchemaVersion desconocidos).");
    }

    private static string? ReadSchemaVersion(XmlElement root)
    {
        // FileHeader/SchemaVersion (local name: independiente de namespace).
        var fileHeader = FirstChild(root, "FileHeader");
        var schemaVersion = fileHeader is null ? null : FirstChild(fileHeader, "SchemaVersion");
        return schemaVersion?.InnerText?.Trim();
    }

    private static XmlElement? FirstChild(XmlElement parent, string localName)
    {
        foreach (XmlNode child in parent.ChildNodes)
        {
            if (child is XmlElement e && e.LocalName == localName)
                return e;
        }
        return null;
    }

    /// <summary>
    /// Re-escribe el espacio de nombres del elemento raíz al del modelo
    /// (los elementos internos no llevan namespace: <c>elementFormDefault=unqualified</c>).
    /// </summary>
    private static XmlDocument NormalizeRootNamespace(XmlDocument xml, string targetNamespace)
    {
        var clone = (XmlDocument)xml.CloneNode(true);
        var oldRoot = clone.DocumentElement!;
        var newRoot = clone.CreateElement(oldRoot.Prefix, oldRoot.LocalName, targetNamespace);

        foreach (XmlAttribute attr in oldRoot.Attributes)
        {
            if (attr.Name == "xmlns" || attr.Prefix == "xmlns")
                continue;
            newRoot.SetAttribute(attr.Name, attr.NamespaceURI, attr.Value);
        }

        while (oldRoot.HasChildNodes)
            newRoot.AppendChild(oldRoot.FirstChild!);

        clone.ReplaceChild(newRoot, oldRoot);
        return clone;
    }
}