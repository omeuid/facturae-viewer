// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using FacturaeViewer.Core.Model;

namespace FacturaeViewer.Core.Validation;

/// <summary>
/// Comprueba la coherencia aritmética de importes, bases y cuotas de una
/// factura FacturaE (reglas del plan de implementación). Las líneas se
/// comparan con 6 decimales y los totales con 2 decimales.
/// </summary>
public static class TotalsValidator
{
    private const decimal MoneyTolerance = 0.01m;
    private const decimal LineTolerance = 0.000001m;

    public static ValidationReport Validate(FacturaeDocument document)
        => Validate(document.Facturae);

    public static ValidationReport Validate(Facturae facturae)
    {
        var report = new ValidationReport();
        var invoices = facturae.Invoices ?? [];

        if (invoices.Length == 0)
        {
            report.AddError("TOT", "El documento no contiene ninguna factura.");
            return report;
        }

        for (int i = 0; i < invoices.Length; i++)
            ValidateInvoice(report, invoices[i], i);

        ValidateBatch(report, facturae.FileHeader, invoices);

        if (!report.Checks.Any())
            report.AddPassed("TOT", "Los importes de la factura son coherentes.");

        return report;
    }

    private static void ValidateInvoice(ValidationReport report, Invoice invoice, int index)
    {
        string id = invoice.InvoiceHeader?.InvoiceNumber?.Trim()
            ?? $"factura {index + 1}";

        foreach (var line in invoice.Items ?? [])
        {
            if (line is null)
                continue;

            string linea = string.IsNullOrWhiteSpace(line.ItemDescription)
                ? "línea sin descripción"
                : $"línea \"{line.ItemDescription.Trim()}\"";

            decimal totalCost = line.Quantity * line.UnitPriceWithoutTax;
            if (line.Quantity != 0 && !Approx(totalCost, line.TotalCost, LineTolerance))
            {
                report.AddError("TOT-LINE-01", $"En {linea} de {id} el coste total ({line.TotalCost:0.######}) no coincide con cantidad × precio ({totalCost:0.######}).");
                continue;
            }

            decimal discounts = (line.DiscountsAndRebates ?? []).Sum(d => d?.DiscountAmount ?? 0m);
            decimal charges = (line.Charges ?? []).Sum(c => c?.ChargeAmount ?? 0m);
            decimal gross = totalCost - discounts + charges;

            if (!Approx(gross, line.GrossAmount, LineTolerance))
                report.AddError("TOT-LINE-02", $"En {linea} de {id} el importe bruto ({line.GrossAmount:0.######}) no coincide con el coste total menos descuentos más recargos ({gross:0.######}).");
        }

        var totals = invoice.InvoiceTotals;
        if (totals is null)
        {
            report.AddError("TOT", $"La factura {id} no declara InvoiceTotals.");
            return;
        }

        decimal grossSum = (invoice.Items ?? [])
            .Where(l => l is not null)
            .Sum(l => l.GrossAmount);

        if (!Approx(grossSum, totals.TotalGrossAmount, MoneyTolerance))
            report.AddError("TOT-01", $"En {id} la suma de los importes brutos de las líneas ({grossSum:0.00}) no coincide con TotalGrossAmount ({totals.TotalGrossAmount:0.00}).");

        decimal beforeTaxes = totals.TotalGrossAmount - totals.TotalGeneralDiscounts + totals.TotalGeneralSurcharges;
        if (!Approx(beforeTaxes, totals.TotalGrossAmountBeforeTaxes, MoneyTolerance))
            report.AddError("TOT-02", $"En {id} TotalGrossAmountBeforeTaxes ({totals.TotalGrossAmountBeforeTaxes:0.00}) no coincide con TotalGrossAmount − descuentos + recargos ({beforeTaxes:0.00}).");

        // Cuotas repercutidas: base × tipo, y recargo de equivalencia por separado.
        foreach (var tax in invoice.TaxesOutputs ?? [])
        {
            if (tax is null || tax.TaxAmount is null)
                continue;

            decimal baseImponible = tax.TaxableBase?.TotalAmount ?? 0m;
            decimal expected = baseImponible * tax.TaxRate / 100m;
            if (!Approx(expected, tax.TaxAmount.TotalAmount, MoneyTolerance))
                report.AddError("TOT-03", $"En {id} la cuota de {tax.TaxTypeCode} ({tax.TaxAmount.TotalAmount:0.00}) no coincide con base {baseImponible:0.00} × tipo {tax.TaxRate:0.00}% ({expected:0.00}).");
        }

        decimal taxOutputSum = (invoice.TaxesOutputs ?? [])
            .Where(t => t is not null)
            .Sum(t => (t.TaxAmount?.TotalAmount ?? 0m) + (t.EquivalenceSurchargeAmount?.TotalAmount ?? 0m));

        if (!Approx(taxOutputSum, totals.TotalTaxOutputs, MoneyTolerance))
            report.AddError("TOT-04", $"En {id} la suma de cuotas repercutidas y recargo de equivalencia ({taxOutputSum:0.00}) no coincide con TotalTaxOutputs ({totals.TotalTaxOutputs:0.00}).");

        decimal withheldSum = (invoice.TaxesWithheld ?? [])
            .Where(t => t is not null)
            .Sum(t => t.TaxAmount?.TotalAmount ?? 0m);

        if (!Approx(withheldSum, totals.TotalTaxesWithheld, MoneyTolerance))
            report.AddError("TOT-05", $"En {id} la suma de cuotas retenidas ({withheldSum:0.00}) no coincide con TotalTaxesWithheld ({totals.TotalTaxesWithheld:0.00}).");

        decimal invoiceTotal = totals.TotalGrossAmountBeforeTaxes + totals.TotalTaxOutputs - totals.TotalTaxesWithheld;
        if (!Approx(invoiceTotal, totals.InvoiceTotal, MoneyTolerance))
            report.AddError("TOT-06", $"En {id} el importe total ({totals.InvoiceTotal:0.00}) no coincide con bruto + repercutidas − retenidas ({invoiceTotal:0.00}).");

        // Si no hay pagos anticipados, el pendiente debe coincidir con el total.
        if (totals.TotalPaymentsOnAccount == 0m)
        {
            decimal outstanding = totals.InvoiceTotal - totals.TotalPaymentsOnAccount;
            if (!Approx(outstanding, totals.TotalOutstandingAmount, MoneyTolerance))
                report.AddWarning("TOT-07", $"En {id} el importe pendiente ({totals.TotalOutstandingAmount:0.00}) no coincide con el total ({outstanding:0.00}).");
        }
    }

    private static void ValidateBatch(ValidationReport report, FileHeader? fileHeader, Invoice[] invoices)
    {
        if (fileHeader?.Batch is null)
        {
            if (fileHeader?.Modality == "I" && invoices.Length > 1)
                report.AddError("TOT-08", $"La modalidad es individual (I) pero el documento contiene {invoices.Length} facturas.");
            return;
        }

        var batch = fileHeader.Batch;
        if (batch.InvoicesCount != invoices.Length)
            report.AddError("TOT-08", $"El lote declara {batch.InvoicesCount} facturas pero contiene {invoices.Length}.");

        decimal sum = invoices.Sum(i => i.InvoiceTotals?.InvoiceTotal ?? 0m);
        decimal batchTotal = batch.TotalInvoicesAmount?.TotalAmount ?? 0m;
        if (!Approx(sum, batchTotal, MoneyTolerance))
            report.AddError("TOT-09", $"La suma de los importes de las facturas ({sum:0.00}) no coincide con el total del lote ({batchTotal:0.00}).");

        if (string.IsNullOrWhiteSpace(batch.BatchIdentifier))
            report.AddWarning("TOT-10", "El lote no declara un identificador de lote (BatchIdentifier).");
    }

    private static bool Approx(decimal a, decimal b, decimal tolerance)
        => Math.Abs(a - b) <= tolerance;
}