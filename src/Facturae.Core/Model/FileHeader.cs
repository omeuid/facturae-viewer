// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Xml.Serialization;

namespace FacturaeViewer.Core.Model;

public class FileHeader
{
    /// <summary>Versión del esquema: "3.2", "3.2.1" o "3.2.2".</summary>
    [XmlElement("SchemaVersion", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? SchemaVersion { get; set; }

    /// <summary>Modalidad: "I" (individual) o "L" (lote).</summary>
    [XmlElement("Modality", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Modality { get; set; }

    /// <summary>Tipo de emisor: "EM" (emisor) o "RE" (receptor).</summary>
    [XmlElement("InvoiceIssuerType", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? InvoiceIssuerType { get; set; }

    [XmlElement("ThirdParty", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Party? ThirdParty { get; set; }

    [XmlElement("Batch", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Batch? Batch { get; set; }
}

public class Batch
{
    [XmlElement("BatchIdentifier", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? BatchIdentifier { get; set; }

    [XmlElement("InvoicesCount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public int InvoicesCount { get; set; }

    /// <summary>Suma de los InvoiceTotal del fichero (tipo AmountType: TotalAmount).</summary>
    [XmlElement("TotalInvoicesAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Amount? TotalInvoicesAmount { get; set; }

    [XmlElement("TotalOutstandingAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Amount? TotalOutstandingAmount { get; set; }

    [XmlElement("TotalExecutableAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Amount? TotalExecutableAmount { get; set; }

    [XmlElement("InvoiceCurrencyCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? InvoiceCurrencyCode { get; set; }
}