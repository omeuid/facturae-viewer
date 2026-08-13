// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using FacturaeViewer.Core.Model;

namespace FacturaeViewer.Core.Pdf;

/// <summary>
/// Genera la representación PDF de una factura (o lote) mediante QuestPDF.
/// Solo depende del modelo de visualización, no de la UI.
/// </summary>
public static class InvoicePdfDocument
{
    static InvoicePdfDocument()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
    }

    /// <summary>Devuelve los bytes del documento PDF para una factura.</summary>
    public static byte[] Generate(InvoiceDisplay invoice) => CreateDocument(invoice).GeneratePdf();

    /// <summary>Guarda el documento PDF para una factura en la ruta indicada.</summary>
    public static void Generate(InvoiceDisplay invoice, string path) => CreateDocument(invoice).GeneratePdf(path);

    /// <summary>Devuelve el documento QuestPDF (útil para pruebas).</summary>
    public static Document CreateDocument(InvoiceDisplay invoice)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(text => text.FontSize(9).FontColor(Colors.Black));

                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("FACTURA").FontSize(22).Bold();
                            left.Item()
                                .Text($"{invoice.DocumentTypeText} · {invoice.InvoiceClassText}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Medium);
                        });
                        row.ConstantItem(210).AlignRight().Column(right =>
                        {
                            right.Item().Text(HeaderNumber(invoice)).FontSize(14).SemiBold();
                            right.Item()
                                .Text("Emitida: " + invoice.IssueDate)
                                .FontColor(Colors.Grey.Medium);
                            if (!string.IsNullOrEmpty(invoice.OperationDate))
                                right.Item()
                                    .Text("Operación: " + invoice.OperationDate)
                                    .FontColor(Colors.Grey.Medium);
                        });
                    });
                    header.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().Column(content =>
                {
                    content.Item().PaddingTop(12).Row(row =>
                    {
                        row.RelativeItem().Element(c => PartyBox(c, "EMISOR", invoice.Seller));
                        row.ConstantItem(20);
                        row.RelativeItem().Element(c => PartyBox(c, "RECEPTOR", invoice.Buyer));
                    });

                    content.Item().PaddingTop(14).Text("Líneas").FontSize(11).Bold();

                    content.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2.4f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Descripción").Bold();
                            header.Cell().Element(HeaderCell).Text("Cantidad").Bold();
                            header.Cell().Element(HeaderCell).AlignRight().Text("Precio unitario").Bold();
                            header.Cell().Element(HeaderCell).AlignRight().Text("Importe").Bold();
                        });

                        foreach (var line in invoice.Lines)
                        {
                            table.Cell().Element(BodyCell).Text(line.Description);
                            table.Cell().Element(BodyCell).Text($"{line.Quantity:0.####} {line.UnitOfMeasureText}");
                            table.Cell().Element(BodyCell).AlignRight().Text(FacturaeProjector.FormatAmount(line.UnitPriceWithoutTax));
                            table.Cell().Element(BodyCell).AlignRight().Text(FacturaeProjector.FormatAmount(line.GrossAmount));
                        }

                        if (invoice.Lines.Count == 0)
                        {
                            table.Cell().ColumnSpan(4).Element(BodyCell)
                                .Text("Sin líneas de detalle").FontColor(Colors.Grey.Medium);
                        }
                    });

                    content.Item().PaddingTop(12).Row(row =>
                    {
                        row.RelativeItem().Column(taxes =>
                        {
                            taxes.Item().Text("Impuestos").FontSize(11).Bold();
                            if (invoice.TaxOutputs.Count == 0 && invoice.TaxesWithheld.Count == 0)
                            {
                                taxes.Item().PaddingTop(4).Text("Sin impuestos").FontColor(Colors.Grey.Medium);
                            }
                            else
                            {
                                foreach (var tax in invoice.TaxOutputs)
                                {
                                    taxes.Item().PaddingTop(4).Text(
                                        $"{tax.TaxTypeText} ({tax.Rate:0.##}%)  ·  base {FacturaeProjector.FormatAmount(tax.TaxableBase)}");
                                    if (tax.EquivalenceSurcharge > 0)
                                        taxes.Item().PaddingLeft(10).Text(
                                            $"Equivalencia: {FacturaeProjector.FormatAmount(tax.EquivalenceSurchargeAmount)}")
                                            .FontColor(Colors.Grey.Medium);
                                }
                                foreach (var tax in invoice.TaxesWithheld)
                                {
                                    taxes.Item().PaddingTop(4).Text(
                                        $"Retención {tax.TaxTypeText} ({tax.Rate:0.##}%): {FacturaeProjector.FormatAmount(tax.TaxAmount)}");
                                }
                            }
                        });

                        row.ConstantItem(20);

                        row.RelativeItem().Element(c =>
                            c.Padding(10).Border(1).BorderColor(Colors.Grey.Lighten2).Column(totals =>
                            {
                                totals.Item().Text("Totales").FontSize(11).Bold();
                                TotalRow(totals, "Base imponible", invoice.Totals.GrossAmount, invoice.CurrencyCode);
                                if (invoice.Totals.GeneralDiscounts != 0)
                                    TotalRow(totals, "Descuentos", -invoice.Totals.GeneralDiscounts, invoice.CurrencyCode);
                                if (invoice.Totals.GeneralSurcharges != 0)
                                    TotalRow(totals, "Recargos", invoice.Totals.GeneralSurcharges, invoice.CurrencyCode);
                                TotalRow(totals, "Cuotas repercutidas", TotalTaxOutputs(invoice), invoice.CurrencyCode);
                                TotalRow(totals, "Retenciones", -TotalTaxesWithheld(invoice), invoice.CurrencyCode);
                                totals.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                totals.Item().PaddingTop(4).Row(t =>
                                {
                                    t.RelativeItem().Text("Total factura").FontSize(13).Bold();
                                    t.ConstantItem(110).AlignRight().Text(FacturaeProjector.FormatAmount(invoice.Totals.InvoiceTotal))
                                        .FontSize(13).Bold();
                                });
                                if (invoice.Totals.TotalOutstandingAmount != 0 && invoice.Totals.TotalOutstandingAmount != invoice.Totals.InvoiceTotal)
                                    totals.Item().PaddingTop(2).AlignRight()
                                        .Text($"Pendiente: {FacturaeProjector.FormatAmount(invoice.Totals.TotalOutstandingAmount)}")
                                        .FontColor(Colors.Grey.Medium);
                            }));
                    });

                    if (invoice.Payments.Count > 0)
                    {
                        content.Item().PaddingTop(14).Text("Condiciones de pago").FontSize(11).Bold();
                        foreach (var payment in invoice.Payments)
                        {
                            content.Item().PaddingTop(4).Text(
                                $"{payment.PaymentMeansText} · vencimiento {payment.DueDate} · {FacturaeProjector.FormatAmount(payment.Amount)}");
                        }
                    }
                });

                page.Footer().PaddingTop(8).AlignCenter().Text(text =>
                {
                    text.Span("Página ").FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontColor(Colors.Grey.Medium);
                    text.Span(" de ").FontColor(Colors.Grey.Medium);
                    text.TotalPages().FontColor(Colors.Grey.Medium);
                });
            });
        });
    }

    private static string HeaderNumber(InvoiceDisplay invoice)
    {
        var number = string.IsNullOrEmpty(invoice.SeriesCode)
            ? invoice.InvoiceNumber
            : $"{invoice.SeriesCode} / {invoice.InvoiceNumber}";
        return string.IsNullOrEmpty(number) ? "Sin numeración" : number;
    }

    private static void PartyBox(IContainer container, string title, PartyDisplay? party)
        => container.Padding(10).Border(1).BorderColor(Colors.Grey.Lighten2).Column(column =>
        {
            column.Item().Text(title).FontSize(10).Bold().FontColor(Colors.Grey.Medium);
            if (party is null)
            {
                column.Item().PaddingTop(4).Text("No disponible").FontColor(Colors.Grey.Medium);
            }
            else
            {
                column.Item().PaddingTop(4).Text(party.Name).FontSize(10).SemiBold();
                if (!string.IsNullOrEmpty(party.TaxId))
                    column.Item().Text($"NIF: {party.TaxId}").FontColor(Colors.Grey.Medium);
                if (!string.IsNullOrEmpty(party.FullAddress))
                    column.Item().Text(party.FullAddress).FontColor(Colors.Grey.Medium);
            }
        });

    private static IContainer HeaderCell(IContainer container)
        => container.PaddingBottom(4).BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingLeft(2).PaddingRight(2);

    private static IContainer BodyCell(IContainer container)
        => container.PaddingVertical(4).PaddingLeft(2).PaddingRight(2).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

    private static void TotalRow(ColumnDescriptor column, string label, decimal amount, string currency)
        => column.Item().PaddingTop(2).Row(row =>
        {
            row.RelativeItem().Text(label);
            row.ConstantItem(110).AlignRight().Text(FacturaeProjector.FormatAmount(amount, currency));
        });

    private static decimal TotalTaxOutputs(InvoiceDisplay invoice)
        => invoice.TaxOutputs.Sum(t => t.TaxAmount + t.EquivalenceSurchargeAmount);

    private static decimal TotalTaxesWithheld(InvoiceDisplay invoice)
        => invoice.TaxesWithheld.Sum(t => t.TaxAmount);
}
