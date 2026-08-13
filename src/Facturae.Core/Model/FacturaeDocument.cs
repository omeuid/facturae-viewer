// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Xml;

namespace FacturaeViewer.Core.Model;

/// <summary>
/// Documento FacturaE cargado: modelo deserializado, XML original
/// (necesario para validar la firma sobre los bytes exactos) y la
/// versión de esquema detectada.
/// </summary>
public sealed class FacturaeDocument
{
    public FacturaeDocument(Facturae facturae, XmlDocument xml, string schemaVersion, string rootNamespace)
    {
        Facturae = facturae;
        Xml = xml;
        SchemaVersion = schemaVersion;
        RootNamespace = rootNamespace;
    }

    /// <summary>Modelo deserializado del documento.</summary>
    public Facturae Facturae { get; }

    /// <summary>XML original, con el whitespace preservado, tal cual se leyó del fichero.</summary>
    public XmlDocument Xml { get; }

    /// <summary>Versión del esquema: "3.2", "3.2.1" o "3.2.2".</summary>
    public string SchemaVersion { get; }

    /// <summary>Espacio de nombres raíz detectado en el fichero.</summary>
    public string RootNamespace { get; }
}

/// <summary>
/// Error producido al cargar o interpretar un fichero FacturaE.
/// </summary>
public sealed class FacturaeParseException : Exception
{
    public FacturaeParseException(string message) : base(message) { }
    public FacturaeParseException(string message, Exception inner) : base(message, inner) { }
}