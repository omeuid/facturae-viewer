// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using Xunit;
using FacturaeViewer.Core.IO;
using FacturaeViewer.Core.Model;

namespace Facturae.Tests;

public class InvoiceDisplayTests
{
    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    [Fact]
    public void Fixture_valido_se_proyecta_con_los_datos_correctos()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.2.2-valid.xml"));
        var invoices = FacturaeProjector.Project(doc.Facturae);

        var invoice = Assert.Single(invoices);
        Assert.Equal("1", invoice.InvoiceNumber);
        Assert.Equal("FC", invoice.DocumentType);
        Assert.Equal("OO", invoice.InvoiceClass);
        Assert.Equal("2013-04-30", invoice.IssueDate);
        Assert.Equal("EUR", invoice.CurrencyCode);

        Assert.NotNull(invoice.Seller);
        Assert.Equal("Empresa de Pruebas y Ejemplos, S.L.", invoice.Seller.Name);
        Assert.Equal("B28015865", invoice.Seller.TaxId);

        Assert.NotNull(invoice.Buyer);
        Assert.Equal("Comercial de Ejemplo, S.A.", invoice.Buyer.Name);
        Assert.Equal("B12345674", invoice.Buyer.TaxId);

        Assert.Equal(2, invoice.Lines.Count);
        Assert.Equal("Flores de jara y brezo", invoice.Lines[0].Description);
        Assert.Equal(1m, invoice.Lines[0].Quantity);
        Assert.Equal(25m, invoice.Lines[0].UnitPriceWithoutTax);
        Assert.Equal(33.75m, invoice.Lines[0].GrossAmount);

        Assert.Contains(invoice.TaxOutputs, t => t.Code == "01" && t.Rate == 16m && t.TaxAmount == 4m);
        Assert.Contains(invoice.TaxesWithheld, t => t.Code == "04" && t.Rate == 4m && t.TaxAmount == 2.39m);

        Assert.Equal(59.75m, invoice.Totals.GrossAmount);
        Assert.Equal(66.03m, invoice.Totals.InvoiceTotal);
    }

    [Fact]
    public void Lote_se_proyecta_como_varias_facturas()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.2.2-lote-valid.xml"));
        var invoices = FacturaeProjector.Project(doc.Facturae);

        Assert.Equal(2, invoices.Count);
        Assert.Equal("1", invoices[0].InvoiceNumber);
        Assert.Equal(121.00m, invoices[0].Totals.InvoiceTotal);
        Assert.Equal("2", invoices[1].InvoiceNumber);
        Assert.Equal("Servicio de consultoría", invoices[1].Lines.Single().Description);
        Assert.Equal(121.00m, invoices[1].Totals.InvoiceTotal);
    }

    [Theory]
    [InlineData("01", "IVA")]
    [InlineData("04", "IRPF")]
    [InlineData("99", "99")]
    public void TaxTypeToText_devuelve_nombres_legibles(string code, string expected)
    {
        Assert.Equal(expected, FacturaeProjector.TaxTypeToText(code));
    }

    [Theory]
    [InlineData("01", "Unidades")]
    [InlineData("02", "Horas")]
    [InlineData("99", "99")]
    public void UnitOfMeasureToText_devuelve_nombres_legibles(string code, string expected)
    {
        Assert.Equal(expected, FacturaeProjector.UnitOfMeasureToText(code));
    }

    [Fact]
    public void DocumentTypeToText_traduce_los_codigos_comunes()
    {
        Assert.Equal("Factura completa", FacturaeProjector.DocumentTypeToText("FC"));
        Assert.Equal("Factura abreviada", FacturaeProjector.DocumentTypeToText("FA"));
    }

    [Fact]
    public void FullAddress_combina_los_datos_de_direccion()
    {
        var party = new PartyDisplay(
            "Empresa", "B28015865", "J", "R",
            "Calle Mayor 1", "28001", "Madrid", "Madrid", "ESP");

        Assert.Equal("Calle Mayor 1, Madrid, Madrid, ESP", party.FullAddress);
    }
}