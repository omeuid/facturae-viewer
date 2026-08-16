// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using Xunit;
using FacturaeViewer.Core.IO;
using FacturaeViewer.Core.Validation;

namespace Facturae.Tests;

public class BusinessRulesValidatorTests
{
    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    [Theory]
    [InlineData("Facturae-3.2-valid.xml")]
    [InlineData("Facturae-3.2.1-valid.xml")]
    [InlineData("Facturae-3.2.2-valid.xml")]
    [InlineData("Facturae-3.2.2-lote-valid.xml")]
    public void Fixtures_validos_no_generan_errores_de_reglas_de_negocio(string file)
    {
        var doc = FacturaeLoader.Load(Fixture(file));
        var report = BusinessRulesValidator.Validate(doc);

        Assert.False(report.HasErrors, string.Join("\n", report.Checks.Select(c => c.ToString())));
        Assert.Equal(0, report.ErrorCount);
    }

    [Fact]
    public void Fecha_de_operacion_posterior_a_la_emision_genera_error_FEC()
    {
        var xml = File.ReadAllText(Fixture("Facturae-3.2.2-valid.xml"));
        xml = ReplaceInXml(xml, "<IssueDate>2013-04-30</IssueDate>",
            "<IssueDate>2013-04-30</IssueDate><OperationDate>2025-12-31</OperationDate>");

        var doc = FacturaeLoader.Parse(xml);
        var report = BusinessRulesValidator.Validate(doc);

        var check = Assert.Single(report.Checks, c => c.Code == "FEC" && c.Status == CheckStatus.Error);
        Assert.Equal("InvoiceIssueData", check.TargetElement);
    }

    [Fact]
    public void Codigo_de_moneda_invalido_genera_error_COD()
    {
        var xml = File.ReadAllText(Fixture("Facturae-3.2.2-valid.xml"));
        xml = xml.Replace("<InvoiceCurrencyCode>EUR</InvoiceCurrencyCode>",
            "<InvoiceCurrencyCode>XXZ</InvoiceCurrencyCode>");

        var doc = FacturaeLoader.Parse(xml);
        var report = BusinessRulesValidator.Validate(doc);

        Assert.Contains(report.Checks, c => c.Code == "COD" && c.Status == CheckStatus.Error);
    }

    [Fact]
    public void Codigo_de_pais_invalido_genera_aviso_COD()
    {
        var xml = File.ReadAllText(Fixture("Facturae-3.2.2-valid.xml"));
        xml = ReplaceInXml(xml, "<CountryCode>ESP</CountryCode>",
            "<CountryCode>ZZZ</CountryCode>");

        var doc = FacturaeLoader.Parse(xml);
        var report = BusinessRulesValidator.Validate(doc);

        Assert.Contains(report.Checks, c => c.Code == "COD" && c.Status == CheckStatus.Warning);
    }

    [Fact]
    public void Provincia_invalida_en_direccion_espanola_genera_aviso_COD()
    {
        var xml = File.ReadAllText(Fixture("Facturae-3.2.2-valid.xml"));
        xml = ReplaceInXml(xml, "<Province>", "<Province>99");

        var doc = FacturaeLoader.Parse(xml);
        var report = BusinessRulesValidator.Validate(doc);

        Assert.Contains(report.Checks, c => c.Code == "COD" && c.Status == CheckStatus.Warning);
    }

    [Fact]
    public void Descuento_superior_al_coste_de_la_linea_genera_error_LIN()
    {
        var xml = File.ReadAllText(Fixture("Facturae-3.2.2-valid.xml"));
        xml = ReplaceInXml(xml, "<DiscountRate>", "<DiscountRate>999");

        var doc = FacturaeLoader.Parse(xml);
        var report = BusinessRulesValidator.Validate(doc);

        Assert.Contains(report.Checks, c => c.Code == "LIN" && c.Status == CheckStatus.Error);
    }

    private static string ReplaceInXml(string xml, string search, string replace)
    {
        int idx = xml.IndexOf(search, StringComparison.Ordinal);
        Assert.True(idx >= 0, $"No se encontró '{search}' en el fixture.");
        return xml.Remove(idx, search.Length).Insert(idx, replace);
    }

    [Fact]
    public void Solo_los_chequeos_con_error_o_aviso_son_navegables()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.2.2-valid.xml"));
        var report = BusinessRulesValidator.Validate(doc);

        Assert.All(report.Checks, c => Assert.False(c.CanNavigate, $"{c.Code} no debería ser navegable."));
    }
}