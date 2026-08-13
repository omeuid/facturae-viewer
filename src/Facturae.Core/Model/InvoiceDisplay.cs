// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Globalization;

namespace FacturaeViewer.Core.Model;

/// <summary>
/// Proyección del modelo FacturaE en objetos de visualización, listos para
/// enlazar en la interfaz o para exportar. No depende de la UI.
/// </summary>
public static class FacturaeProjector
{
    public static IReadOnlyList<InvoiceDisplay> Project(Facturae facturae)
    {
        var invoices = new List<InvoiceDisplay>();
        foreach (var invoice in facturae.Invoices ?? [])
        {
            invoices.Add(new InvoiceDisplay(
                InvoiceNumber: invoice.InvoiceHeader?.InvoiceNumber ?? string.Empty,
                SeriesCode: invoice.InvoiceHeader?.InvoiceSeriesCode ?? string.Empty,
                DocumentType: invoice.InvoiceHeader?.InvoiceDocumentType ?? string.Empty,
                InvoiceClass: invoice.InvoiceHeader?.InvoiceClass ?? string.Empty,
                IssueDate: invoice.InvoiceIssueData?.IssueDate ?? string.Empty,
                OperationDate: invoice.InvoiceIssueData?.OperationDate ?? string.Empty,
                PlaceOfIssue: invoice.InvoiceIssueData?.PlaceOfIssue?.PlaceOfIssueDescription ?? string.Empty,
                CurrencyCode: invoice.InvoiceIssueData?.InvoiceCurrencyCode ?? string.Empty,
                Seller: ProjectParty(facturae.Parties?.SellerParty),
                Buyer: ProjectParty(facturae.Parties?.BuyerParty),
                Lines: (invoice.Items ?? []).Select(ProjectLine).ToList(),
                TaxOutputs: (invoice.TaxesOutputs ?? []).Select(ProjectTax).ToList(),
                TaxesWithheld: (invoice.TaxesWithheld ?? []).Select(ProjectTax).ToList(),
                Payments: (invoice.PaymentDetails ?? []).Select(p => new PaymentDisplay(
                    DueDate: p.InstallmentDueDate ?? string.Empty,
                    Amount: p.InstallmentAmount,
                    PaymentMeans: p.PaymentMeans ?? string.Empty)).ToList(),
                Totals: new InvoiceTotalsDisplay(
                    GrossAmount: invoice.InvoiceTotals?.TotalGrossAmount ?? 0m,
                    GeneralDiscounts: invoice.InvoiceTotals?.TotalGeneralDiscounts ?? 0m,
                    GeneralSurcharges: invoice.InvoiceTotals?.TotalGeneralSurcharges ?? 0m,
                    TaxOutputs: invoice.InvoiceTotals?.TotalTaxOutputs ?? 0m,
                    TaxesWithheld: invoice.InvoiceTotals?.TotalTaxesWithheld ?? 0m,
                    InvoiceTotal: invoice.InvoiceTotals?.InvoiceTotal ?? 0m,
                    TotalOutstandingAmount: invoice.InvoiceTotals?.TotalOutstandingAmount ?? 0m,
                    TotalExecutableAmount: invoice.InvoiceTotals?.TotalExecutableAmount ?? 0m)));
        }
        return invoices;
    }

    private static PartyDisplay? ProjectParty(Party? party)
    {
        if (party is null)
            return null;

        string name = party.LegalEntity?.CorporateName ?? string.Empty;
        if (string.IsNullOrEmpty(name) && party.Individual is not null)
        {
            name = string.Join(' ',
                new[] { party.Individual.Name, party.Individual.FirstSurname, party.Individual.SecondSurname }
                    .Where(s => !string.IsNullOrEmpty(s)));
        }

        string address = string.Empty, postCode = string.Empty, town = string.Empty;
        string province = string.Empty, countryCode = string.Empty;

        var inSpain = party.LegalEntity?.AddressInSpain ?? party.Individual?.AddressInSpain;
        var overseas = party.LegalEntity?.OverseasAddress ?? party.Individual?.OverseasAddress;
        if (inSpain is not null)
        {
            address = inSpain.Address ?? string.Empty;
            postCode = inSpain.PostCode ?? string.Empty;
            town = inSpain.Town ?? string.Empty;
            province = inSpain.Province ?? string.Empty;
            countryCode = inSpain.CountryCode ?? string.Empty;
        }
        else if (overseas is not null)
        {
            address = overseas.Address ?? string.Empty;
            town = overseas.PostCodeAndTown ?? string.Empty;
            province = overseas.Province ?? string.Empty;
            countryCode = overseas.CountryCode ?? string.Empty;
        }

        return new PartyDisplay(
            Name: name,
            TaxId: party.TaxIdentification?.TaxIdentificationNumber ?? string.Empty,
            PersonTypeCode: party.TaxIdentification?.PersonTypeCode ?? string.Empty,
            ResidenceTypeCode: party.TaxIdentification?.ResidenceTypeCode ?? string.Empty,
            Address: address,
            PostCode: postCode,
            Town: town,
            Province: province,
            CountryCode: countryCode);
    }

    private static InvoiceLineDisplay ProjectLine(InvoiceLine line) => new(
        Description: line.ItemDescription ?? string.Empty,
        Quantity: line.Quantity,
        UnitOfMeasure: line.UnitOfMeasure ?? string.Empty,
        UnitPriceWithoutTax: line.UnitPriceWithoutTax,
        TotalCost: line.TotalCost,
        GrossAmount: line.GrossAmount);

    private static TaxDisplay ProjectTax(Tax tax) => new(
        Code: tax.TaxTypeCode ?? string.Empty,
        Rate: tax.TaxRate,
        TaxableBase: tax.TaxableBase?.TotalAmount ?? 0m,
        TaxAmount: tax.TaxAmount?.TotalAmount ?? 0m,
        EquivalenceSurcharge: tax.EquivalenceSurcharge,
        EquivalenceSurchargeAmount: tax.EquivalenceSurchargeAmount?.TotalAmount ?? 0m);

    /// <summary>Texto legible de un código de unidad de medida FacturaE.</summary>
    public static string UnitOfMeasureToText(string code) => code switch
    {
        "01" => "Unidades",
        "02" => "Horas",
        "03" => "Kilogramos",
        "04" => "Litros",
        "05" => "Metros",
        "06" => "Metros cuadrados",
        "07" => "Metros cúbicos",
        "08" => "Pares",
        _ => code,
    };

    /// <summary>Texto legible de un tipo de impuesto FacturaE.</summary>
    public static string TaxTypeToText(string code) => code switch
    {
        "01" => "IVA",
        "02" => "IPSI",
        "03" => "IGIC",
        "04" => "IRPF",
        "05" => "IRPF",
        "06" => "Otro impuesto",
        "07" => "ITPAJD",
        "08" => "IE",
        "09" => "RA",
        "10" => "IGTECM",
        "11" => "IECDPAC",
        "12" => "IIIMAB",
        "13" => "ICIO",
        "14" => "IMVDN",
        "15" => "IMSN",
        "16" => "IMPN",
        "17" => "REIVA",
        "18" => "REIGIC",
        "19" => "REIPSI",
        "20" => "IPCN",
        "21" => "Otro impuesto",
        "22" => "Impuesto cesión carburantes",
        "23" => "IGFEI",
        _ => code,
    };

    /// <summary>Texto legible del tipo de documento FacturaE ("FC", "FA", ...).</summary>
    public static string DocumentTypeToText(string code) => code switch
    {
        "FC" => "Factura completa",
        "FA" => "Factura abreviada",
        "AF" => "Autofactura",
        "AC" => "Factura del arrendatario",
        "FE" => "Factura del expedidor",
        "TE" => "Factura del transportista",
        "TA" => "Factura del transitario",
        "FF" => "Factura del proveedor de servicios de facturación",
        "A1" => "Rentas arrendamiento",
        "A2" => "Rentas de inmuebles (céntimos)",
        "A3" => "Rentas de inmuebles (otros)",
        _ => code,
    };

    /// <summary>Texto legible de la clase de factura ("OO", "OR", "OC", "CO", ...).</summary>
    public static string InvoiceClassToText(string code) => code switch
    {
        "OO" => "Original",
        "OR" => "Rectificativa (por operación)",
        "OC" => "Rectificativa (artículo 80)",
        "CO" => "Recapitulativa",
        "CR" => "Rectificativa de facturas recapitulativas",
        _ => code,
    };

    /// <summary>Texto legible de un medio de pago FacturaE.</summary>
    public static string PaymentMeansToText(string code) => code switch
    {
        "01" => "Efectivo",
        "02" => "Transferencia bancaria",
        "03" => "Tarjeta de crédito",
        "04" => "Cheque",
        "05" => "Crédito documentario",
        "06" => "Letra de cambio",
        "07" => "Pagaré",
        "08" => "Recibo domiciliado",
        "09" => "Acuerdo de cobro",
        "10" => "Facturación",
        "11" => "Vale",
        "12" => "Compensación de deudas",
        "13" => "Pago mediante datos de tarjeta (TPE)",
        "14" => "Cobro en caja",
        _ => code,
    };

    /// <summary>Formatea un importe decimal en el idioma de la aplicación.</summary>
    public static string FormatAmount(decimal value, string currencyCode = "EUR")
        => value.ToString("N2", CultureInfo.GetCultureInfo("es-ES")) + " " + currencyCode;
}

public sealed record PartyDisplay(
    string Name,
    string TaxId,
    string PersonTypeCode,
    string ResidenceTypeCode,
    string Address,
    string PostCode,
    string Town,
    string Province,
    string CountryCode)
{
    public string FullAddress
        => string.Join(", ", new[] { Address, Town, Province, CountryCode }.Where(s => !string.IsNullOrEmpty(s)));
}

public sealed record InvoiceLineDisplay(
    string Description,
    decimal Quantity,
    string UnitOfMeasure,
    decimal UnitPriceWithoutTax,
    decimal TotalCost,
    decimal GrossAmount)
{
    public string UnitOfMeasureText => FacturaeProjector.UnitOfMeasureToText(UnitOfMeasure);
}

public sealed record TaxDisplay(
    string Code,
    decimal Rate,
    decimal TaxableBase,
    decimal TaxAmount,
    decimal EquivalenceSurcharge,
    decimal EquivalenceSurchargeAmount)
{
    public string TaxTypeText => FacturaeProjector.TaxTypeToText(Code);
}

public sealed record PaymentDisplay(string DueDate, decimal Amount, string PaymentMeans)
{
    public string PaymentMeansText => FacturaeProjector.PaymentMeansToText(PaymentMeans);
}

public sealed record InvoiceTotalsDisplay(
    decimal GrossAmount,
    decimal GeneralDiscounts,
    decimal GeneralSurcharges,
    decimal TaxOutputs,
    decimal TaxesWithheld,
    decimal InvoiceTotal,
    decimal TotalOutstandingAmount,
    decimal TotalExecutableAmount);

public sealed record InvoiceDisplay(
    string InvoiceNumber,
    string SeriesCode,
    string DocumentType,
    string InvoiceClass,
    string IssueDate,
    string OperationDate,
    string PlaceOfIssue,
    string CurrencyCode,
    PartyDisplay? Seller,
    PartyDisplay? Buyer,
    IReadOnlyList<InvoiceLineDisplay> Lines,
    IReadOnlyList<TaxDisplay> TaxOutputs,
    IReadOnlyList<TaxDisplay> TaxesWithheld,
    IReadOnlyList<PaymentDisplay> Payments,
    InvoiceTotalsDisplay Totals)
{
    public string DocumentTypeText => FacturaeProjector.DocumentTypeToText(DocumentType);
    public string InvoiceClassText => FacturaeProjector.InvoiceClassToText(InvoiceClass);
}