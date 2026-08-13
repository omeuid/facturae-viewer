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

public class SignatureValidatorTests
{
    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    private static XmlDocument LoadFixture(string name)
    {
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.Load(Fixture(name));
        return doc;
    }

    private static X509Certificate2 CreateCertificate() => TestSignature.CreateSelfSignedCertificate();

    [Fact]
    public void Documento_sin_firma_genera_aviso_SIG01()
    {
        var report = SignatureValidator.Validate(LoadFixture("Facturae-3.2.2-valid.xml"));

        Assert.Contains(report.Checks, c => c.Code == "SIG-01" && c.Status == CheckStatus.Warning);
        Assert.False(report.HasErrors);
    }

    [Fact]
    public void Firma_xmldsig_valida_pasa_la_validacion()
    {
        using var cert = CreateCertificate();
        var xml = TestSignature.SignPlainXmlDsig(LoadFixture("Facturae-3.2.2-valid.xml"), cert);
        var report = SignatureValidator.Validate(xml);

        Assert.False(report.HasErrors, string.Join("\n", report.Checks.Select(c => c.ToString())));
        Assert.Contains(report.Checks, c => c.Code == "SIG-02" && c.Status == CheckStatus.Passed);
    }

    [Fact]
    public void Firma_xades_valida_pasa_la_validacion()
    {
        using var cert = CreateCertificate();
        var xml = TestSignature.SignXades(LoadFixture("Facturae-3.2.2-valid.xml"), cert);
        var report = SignatureValidator.Validate(xml);

        Assert.False(report.HasErrors, string.Join("\n", report.Checks.Select(c => c.ToString())));
        Assert.Contains(report.Checks, c => c.Code == "SIG-02" && c.Status == CheckStatus.Passed);
        Assert.Contains(report.Checks, c => c.Code == "SIG-06" && c.Status == CheckStatus.Info);
        Assert.Contains(report.Checks, c => c.Code == "SIG-07" && c.Status == CheckStatus.Info);
        Assert.Contains(report.Checks, c => c.Code == "SIG-08" && c.Status == CheckStatus.Info);
        Assert.Contains(report.Checks, c => c.Code == "SIG-09" && c.Status == CheckStatus.Info);
        Assert.Contains(report.Checks, c => c.Code == "SIG-11" && c.Status == CheckStatus.Passed);
    }

    [Fact]
    public void Documento_firmado_sigue_validando_el_esquema_XSD()
    {
        using var cert = CreateCertificate();
        var xml = TestSignature.SignPlainXmlDsig(LoadFixture("Facturae-3.2.2-valid.xml"), cert);
        var report = SchemaValidator.Validate(xml);

        Assert.False(report.HasErrors, string.Join("\n", report.Checks.Select(c => c.ToString())));
    }

    [Fact]
    public void Firma_sobre_contenido_manipulado_genera_error_SIG03()
    {
        using var cert = CreateCertificate();
        var xml = TestSignature.SignPlainXmlDsig(LoadFixture("Facturae-3.2.2-valid.xml"), cert);

        // Se modifica un importe del documento después de firmarlo.
        var total = xml.SelectSingleNode("//*[local-name()='TotalAmount']")!;
        total.InnerText = "666.00";

        var report = SignatureValidator.Validate(xml);

        Assert.True(report.HasErrors);
        Assert.Contains(report.Checks, c => c.Code == "SIG-03" && c.Status == CheckStatus.Error);
    }

    [Fact]
    public void Firma_con_certificado_de_clave_distinta_genera_error_SIG03_y_SIG11()
    {
        using var certA = CreateCertificate();
        using var certB = CreateCertificate();
        var xml = TestSignature.SignXades(LoadFixture("Facturae-3.2.2-valid.xml"), certA);

        // Se sustituye el certificado de KeyInfo por otro de clave distinta.
        var certElement = xml.SelectSingleNode("//*[local-name()='X509Certificate']")!;
        certElement.InnerText = Convert.ToBase64String(certB.RawData);

        var report = SignatureValidator.Validate(xml);

        Assert.Contains(report.Checks, c => c.Code == "SIG-03" && c.Status == CheckStatus.Error);
        Assert.Contains(report.Checks, c => c.Code == "SIG-11" && c.Status == CheckStatus.Error);
    }

    [Fact]
    public void Bloque_xades_modificado_genera_error_SIG03()
    {
        using var cert = CreateCertificate();
        var xml = TestSignature.SignXades(LoadFixture("Facturae-3.2.2-valid.xml"), cert);

        // Se altera el rol declarado dentro de SignedProperties tras firmar.
        var role = xml.SelectSingleNode("//*[local-name()='ClaimedRole']")!;
        role.InnerText = "Rol alterado";

        var report = SignatureValidator.Validate(xml);

        Assert.True(report.HasErrors, string.Join("\n", report.Checks.Select(c => c.ToString())));
        Assert.Contains(report.Checks, c => c.Code == "SIG-03" && c.Status == CheckStatus.Error);
    }

    [Fact]
    public void Firma_sin_SHA1_no_genera_aviso_de_algortimo_debil()
    {
        using var cert = CreateCertificate();
        var xml = TestSignature.SignXades(LoadFixture("Facturae-3.2.2-valid.xml"), cert);
        var report = SignatureValidator.Validate(xml);

        Assert.DoesNotContain(report.Checks, c => c.Code == "SIG-04");
    }
}
