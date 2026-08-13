// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Xml.Serialization;

namespace FacturaeViewer.Core.Model;

public class InvoiceLine
{
    [XmlElement("IssuerContractReference", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? IssuerContractReference { get; set; }

    [XmlElement("IssuerContractDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? IssuerContractDate { get; set; }

    [XmlElement("IssuerTransactionReference", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? IssuerTransactionReference { get; set; }

    [XmlElement("IssuerTransactionDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? IssuerTransactionDate { get; set; }

    [XmlElement("ReceiverContractReference", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ReceiverContractReference { get; set; }

    [XmlElement("ReceiverContractDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ReceiverContractDate { get; set; }

    [XmlElement("ReceiverTransactionReference", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ReceiverTransactionReference { get; set; }

    [XmlElement("ReceiverTransactionDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ReceiverTransactionDate { get; set; }

    [XmlElement("FileReference", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? FileReference { get; set; }

    [XmlElement("FileDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? FileDate { get; set; }

    [XmlElement("SequenceNumber", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal SequenceNumber { get; set; }

    [XmlArray("DeliveryNotesReferences", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("DeliveryNote", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public DeliveryNote[]? DeliveryNotesReferences { get; set; }

    [XmlElement("ItemDescription", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ItemDescription { get; set; }

    [XmlElement("Quantity", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal Quantity { get; set; }

    [XmlElement("UnitOfMeasure", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? UnitOfMeasure { get; set; }

    [XmlElement("UnitPriceWithoutTax", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal UnitPriceWithoutTax { get; set; }

    [XmlElement("TotalCost", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalCost { get; set; }

    [XmlArray("DiscountsAndRebates", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("Discount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Discount[]? DiscountsAndRebates { get; set; }

    [XmlArray("Charges", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("Charge", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Charge[]? Charges { get; set; }

    [XmlElement("GrossAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal GrossAmount { get; set; }

    [XmlArray("TaxesOutputs", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("Tax", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Tax[]? TaxesOutputs { get; set; }

    [XmlArray("TaxesWithheld", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("Tax", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Tax[]? TaxesWithheld { get; set; }

    [XmlElement("AdditionalLineItemInformation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? AdditionalLineItemInformation { get; set; }

    [XmlElement("SpecialTaxableEvent", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public SpecialTaxableEvent? SpecialTaxableEvent { get; set; }

    [XmlElement("ArticleCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ArticleCode { get; set; }

    [XmlElement("Extension", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Extension { get; set; }
}

public class DeliveryNote
{
    [XmlElement("DeliveryNoteNumber", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? DeliveryNoteNumber { get; set; }

    [XmlElement("DeliveryNoteDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? DeliveryNoteDate { get; set; }
}

public class Discount
{
    [XmlElement("DiscountReason", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? DiscountReason { get; set; }

    [XmlElement("DiscountRate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal DiscountRate { get; set; }

    [XmlElement("DiscountAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal DiscountAmount { get; set; }
}

public class Charge
{
    [XmlElement("ChargeReason", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ChargeReason { get; set; }

    [XmlElement("ChargeRate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal ChargeRate { get; set; }

    [XmlElement("ChargeAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal ChargeAmount { get; set; }
}

public class SpecialTaxableEvent
{
    [XmlElement("SpecialTaxableEventCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? SpecialTaxableEventCode { get; set; }

    [XmlElement("SpecialTaxableEventReason", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? SpecialTaxableEventReason { get; set; }
}