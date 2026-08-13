// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Xml.Serialization;

namespace FacturaeViewer.Core.Model;

/// <summary>
/// Elemento raíz de un fichero FacturaE. El XML declara la versión del
/// esquema (3.2, 3.2.1 o 3.2.2) que determina el espacio de nombres del
/// elemento raíz. La firma XAdES es opcional según el esquema, pero
/// necesaria para la validez legal (ver <see cref="FacturaeDocument"/>).
/// </summary>
[XmlRoot("Facturae", Namespace = FacturaeNamespaces.NamespaceV3_2)]
public class Facturae
{
    /// <summary>Cabecera del fichero.</summary>
    [XmlElement("FileHeader", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public FileHeader? FileHeader { get; set; }

    /// <summary>Partes intervinientes (emisor y receptor).</summary>
    [XmlElement("Parties", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Parties? Parties { get; set; }

    /// <summary>Facturas contenidas (puede ser un lote).</summary>
    [XmlArray("Invoices", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("Invoice", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Invoice[]? Invoices { get; set; }
}