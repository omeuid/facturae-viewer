// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Xunit;
using FacturaeViewer.Core.IO;
using FacturaeViewer.Core.Validation;

namespace Facturae.Tests;

public class DocumentValidatorTests
{
    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    [Theory]
    [InlineData("Facturae-3.2-valid.xml")]
    [InlineData("Facturae-3.2.1-valid.xml")]
    [InlineData("Facturae-3.2.2-valid.xml")]
    [InlineData("Facturae-3.2.2-lote-valid.xml")]
    public void Fixture_valido_no_tiene_errores(string file)
    {
        var doc = FacturaeLoader.Load(Fixture(file));
        var report = DocumentValidator.Validate(doc);

        Assert.False(report.HasErrors, string.Join("\n", report.Checks.Select(c => c.ToString())));
        Assert.Contains(report.Checks, c => c.Code == "SCHEMA" && c.Status == CheckStatus.Passed);
        Assert.Contains(report.Checks, c => c.Code == "NIF" && c.Status == CheckStatus.Passed);
        Assert.Contains(report.Checks, c => c.Code == "TOT" && c.Status == CheckStatus.Passed);
        Assert.Contains(report.Checks, c => c.Code == "SIG-01" && c.Status == CheckStatus.Warning);
    }

    [Fact]
    public void Fixture_con_totales_incorrectos_reporta_error()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.2.2-totales-incorrectos.xml"));
        var report = DocumentValidator.Validate(doc);

        Assert.True(report.HasErrors);
        Assert.Contains(report.Checks, c => c.Code == "TOT-06" && c.Status == CheckStatus.Error);
    }

    [Fact]
    public void Documento_firmado_xades_incluye_las_comprobaciones_de_firma()
    {
        using var cert = TestSignature.CreateSelfSignedCertificate();
        var source = new XmlDocument { PreserveWhitespace = true };
        source.Load(Fixture("Facturae-3.2.2-valid.xml"));
        var xml = TestSignature.SignXades(source, cert);

        var document = FacturaeLoader.Parse(xml);
        var report = DocumentValidator.Validate(document);

        Assert.False(report.HasErrors, string.Join("\n", report.Checks.Select(c => c.ToString())));
        Assert.Contains(report.Checks, c => c.Code == "SIG-02" && c.Status == CheckStatus.Passed);
        Assert.Contains(report.Checks, c => c.Code == "SIG-06" && c.Status == CheckStatus.Info);
    }
}