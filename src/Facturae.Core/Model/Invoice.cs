// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Xml.Serialization;

namespace FacturaeViewer.Core.Model;

public class Invoice
{
    [XmlElement("InvoiceHeader", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public InvoiceHeader? InvoiceHeader { get; set; }

    [XmlElement("InvoiceIssueData", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public InvoiceIssueData? InvoiceIssueData { get; set; }

    [XmlArray("TaxesOutputs", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("Tax", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Tax[]? TaxesOutputs { get; set; }

    [XmlArray("TaxesWithheld", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("Tax", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Tax[]? TaxesWithheld { get; set; }

    [XmlElement("InvoiceTotals", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public InvoiceTotals? InvoiceTotals { get; set; }

    [XmlArray("Items", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("InvoiceLine", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public InvoiceLine[]? Items { get; set; }

    [XmlArray("PaymentDetails", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("Installment", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Installment[]? PaymentDetails { get; set; }

    [XmlElement("AdditionalData", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public AdditionalData? AdditionalData { get; set; }
}

public class InvoiceHeader
{
    [XmlElement("InvoiceNumber", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? InvoiceNumber { get; set; }

    [XmlElement("InvoiceSeriesCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? InvoiceSeriesCode { get; set; }

    /// <summary>Tipo de documento: "FC" completa, "FA" abreviada, "AF" autofactura, etc.</summary>
    [XmlElement("InvoiceDocumentType", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? InvoiceDocumentType { get; set; }

    /// <summary>Clase de factura: "OO" original, "OR" rectificativa por operación, "OC" rectificativa por artículo 80, "CO" recapitulativa.</summary>
    [XmlElement("InvoiceClass", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? InvoiceClass { get; set; }

    [XmlElement("Corrective", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Corrective? Corrective { get; set; }
}

public class Corrective
{
    [XmlElement("InvoiceNumber", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? InvoiceNumber { get; set; }

    [XmlElement("InvoiceSeriesCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? InvoiceSeriesCode { get; set; }

    [XmlElement("ReasonCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ReasonCode { get; set; }

    [XmlElement("ReasonDescription", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ReasonDescription { get; set; }

    [XmlElement("AdditionalReasonDescription", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? AdditionalReasonDescription { get; set; }

    [XmlElement("CorrectionMethod", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? CorrectionMethod { get; set; }

    [XmlElement("CorrectionMethodDescription", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? CorrectionMethodDescription { get; set; }
}

public class InvoiceIssueData
{
    [XmlElement("IssueDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? IssueDate { get; set; }

    [XmlElement("OperationDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? OperationDate { get; set; }

    [XmlElement("PlaceOfIssue", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public PlaceOfIssue? PlaceOfIssue { get; set; }

    [XmlElement("InvoicingPeriod", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public InvoicingPeriod? InvoicingPeriod { get; set; }

    [XmlElement("InvoiceCurrencyCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? InvoiceCurrencyCode { get; set; }

    [XmlElement("TaxCurrencyCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? TaxCurrencyCode { get; set; }

    [XmlElement("LanguageName", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? LanguageName { get; set; }

    [XmlElement("ExchangeRateDetails", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public ExchangeRateDetails? ExchangeRateDetails { get; set; }
}

public class PlaceOfIssue
{
    [XmlElement("PostCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? PostCode { get; set; }

    [XmlElement("PlaceOfIssueDescription", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? PlaceOfIssueDescription { get; set; }
}

public class InvoicingPeriod
{
    [XmlElement("StartDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? StartDate { get; set; }

    [XmlElement("EndDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? EndDate { get; set; }
}

public class ExchangeRateDetails
{
    [XmlElement("ExchangeRate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ExchangeRate { get; set; }

    [XmlElement("ExchangeRateDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ExchangeRateDate { get; set; }
}

public class Tax
{
    /// <summary>Tipo de impuesto: "01" IVA, "02" IPSI, "03" IGIC, "04" IRPF, "05" IRPF, "06" otro, etc.</summary>
    [XmlElement("TaxTypeCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? TaxTypeCode { get; set; }

    [XmlElement("TaxRate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TaxRate { get; set; }

    [XmlElement("TaxableBase", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Amount? TaxableBase { get; set; }

    [XmlElement("TaxAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Amount? TaxAmount { get; set; }

    [XmlElement("SpecialTaxableBase", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Amount? SpecialTaxableBase { get; set; }

    [XmlElement("SpecialTaxAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Amount? SpecialTaxAmount { get; set; }

    [XmlElement("EquivalenceSurcharge", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal EquivalenceSurcharge { get; set; }

    [XmlElement("EquivalenceSurchargeAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Amount? EquivalenceSurchargeAmount { get; set; }
}

public class Amount
{
    [XmlElement("TotalAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalAmount { get; set; }

    [XmlElement("EquivalentInEuros", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal EquivalentInEuros { get; set; }
}

public class InvoiceTotals
{
    [XmlElement("TotalGrossAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalGrossAmount { get; set; }

    [XmlElement("TotalGeneralDiscounts", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalGeneralDiscounts { get; set; }

    [XmlElement("TotalGeneralSurcharges", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalGeneralSurcharges { get; set; }

    [XmlElement("TotalGrossAmountBeforeTaxes", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalGrossAmountBeforeTaxes { get; set; }

    [XmlElement("TotalTaxOutputs", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalTaxOutputs { get; set; }

    [XmlElement("TotalTaxesWithheld", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalTaxesWithheld { get; set; }

    [XmlElement("InvoiceTotal", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal InvoiceTotal { get; set; }

    [XmlElement("TotalOutstandingAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalOutstandingAmount { get; set; }

    [XmlElement("TotalExecutableAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalExecutableAmount { get; set; }

    [XmlElement("TotalReimbursableExpenses", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalReimbursableExpenses { get; set; }

    [XmlElement("TotalPaymentsOnAccount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalPaymentsOnAccount { get; set; }

    [XmlElement("TotalFinancialExpenses", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalFinancialExpenses { get; set; }

    [XmlElement("TotalLiquidationAmounts", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal TotalLiquidationAmounts { get; set; }

    /// <summary>Anticipos y suplidos no incluidos en la base imponible.</summary>
    [XmlElement("PaymentsOnAccount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public PaymentsOnAccount? PaymentsOnAccount { get; set; }

    [XmlElement("ReimbursableExpenses", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public ReimbursableExpenses? ReimbursableExpenses { get; set; }

    [XmlElement("Subsidies", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Subsidies? Subsidies { get; set; }

    [XmlElement("AmountsWithheld", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public AmountsWithheld? AmountsWithheld { get; set; }

    [XmlElement("PaymentInKind", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public PaymentInKind? PaymentInKind { get; set; }
}

public class PaymentsOnAccount
{
    [XmlElement("PaymentOnAccountDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? PaymentOnAccountDate { get; set; }

    [XmlElement("PaymentOnAccountAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal PaymentOnAccountAmount { get; set; }

    [XmlElement("PaymentOnAccountDescription", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? PaymentOnAccountDescription { get; set; }
}

public class ReimbursableExpenses
{
    [XmlElement("ReimbursableExpenseDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ReimbursableExpenseDate { get; set; }

    [XmlElement("ReimbursableExpenseDescription", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ReimbursableExpenseDescription { get; set; }

    [XmlElement("ReimbursableExpenseAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal ReimbursableExpenseAmount { get; set; }
}

public class Subsidies
{
    [XmlArrayItem("Subsidy", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Subsidy[]? Items { get; set; }
}

public class Subsidy
{
    [XmlElement("SubsidyType", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? SubsidyType { get; set; }

    [XmlElement("SubsidyDescription", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? SubsidyDescription { get; set; }

    [XmlElement("SubsidyRate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal SubsidyRate { get; set; }

    [XmlElement("SubsidyAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal SubsidyAmount { get; set; }

    [XmlElement("DeductionPercentage", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal DeductionPercentage { get; set; }
}

public class AmountsWithheld
{
    [XmlElement("WithholdingDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? WithholdingDate { get; set; }

    [XmlElement("WithholdingDescription", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? WithholdingDescription { get; set; }

    [XmlElement("WithholdingAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal WithholdingAmount { get; set; }
}

public class PaymentInKind
{
    [XmlElement("PaymentInKindDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? PaymentInKindDate { get; set; }

    [XmlElement("PaymentInKindDescription", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? PaymentInKindDescription { get; set; }

    [XmlElement("PaymentInKindAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal PaymentInKindAmount { get; set; }
}

public class Installment
{
    [XmlElement("InstallmentDueDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? InstallmentDueDate { get; set; }

    [XmlElement("InstallmentAmount", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal InstallmentAmount { get; set; }

    [XmlElement("PaymentMeans", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? PaymentMeans { get; set; }

    [XmlElement("AccountToBeCredited", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Account? AccountToBeCredited { get; set; }

    [XmlElement("AccountToBeDebited", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Account? AccountToBeDebited { get; set; }

    [XmlElement("PaymentReconciliationReference", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? PaymentReconciliationReference { get; set; }

    [XmlElement("DebitReconciliationReference", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? DebitReconciliationReference { get; set; }

    [XmlElement("CollectionAdditionalInformation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? CollectionAdditionalInformation { get; set; }

    [XmlElement("RegulatoryReportingData", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? RegulatoryReportingData { get; set; }
}

public class Account
{
    [XmlElement("IBAN", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? IBAN { get; set; }

    [XmlElement("AccountNumber", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? AccountNumber { get; set; }

    [XmlElement("Suffix", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Suffix { get; set; }

    [XmlElement("BankCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? BankCode { get; set; }

    [XmlElement("BranchCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? BranchCode { get; set; }

    [XmlElement("BranchInSpain", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public AddressInSpain? BranchInSpain { get; set; }

    [XmlElement("OverseasBranchAddress", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public OverseasAddress? OverseasBranchAddress { get; set; }

    [XmlElement("BIC", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? BIC { get; set; }
}

public class AdditionalData
{
    [XmlArray("RelatedInvoice", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("Invoice", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public InvoiceNumberRef[]? RelatedInvoice { get; set; }

    [XmlElement("RelatedDocuments", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public RelatedDocuments? RelatedDocuments { get; set; }

    [XmlArray("InvoiceAdditionalInformation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("AdditionalInformation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string[]? InvoiceAdditionalInformation { get; set; }

    [XmlElement("Extensions", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Extensions { get; set; }
}

public class InvoiceNumberRef
{
    [XmlElement("InvoiceNumber", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? InvoiceNumber { get; set; }

    [XmlElement("InvoiceSeriesCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? InvoiceSeriesCode { get; set; }
}

public class RelatedDocuments
{
    [XmlArrayItem("Attachment", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Attachment[]? Items { get; set; }
}

public class Attachment
{
    [XmlElement("AttachmentCompressionAlgorithm", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? AttachmentCompressionAlgorithm { get; set; }

    [XmlElement("AttachmentFormat", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? AttachmentFormat { get; set; }

    [XmlElement("AttachmentEncoding", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? AttachmentEncoding { get; set; }

    [XmlElement("AttachmentDescription", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? AttachmentDescription { get; set; }

    [XmlElement("AttachmentData", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? AttachmentData { get; set; }

    [XmlElement("AttachmentDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? AttachmentDate { get; set; }

    [XmlElement("AttachmentExternalReference", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? AttachmentExternalReference { get; set; }
}