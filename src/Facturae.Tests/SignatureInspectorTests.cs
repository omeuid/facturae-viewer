// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Xunit;
using FacturaeViewer.Core.Model;

namespace Facturae.Tests;

public class SignatureInspectorTests
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
    public void Documento_sin_firma_no_tiene_detalles()
    {
        var details = SignatureInspector.Inspect(LoadFixture("Facturae-3.2.2-valid.xml"));

        Assert.False(details.HasSignature);
        Assert.Equal("Sin firma", details.TypeText);
        Assert.Equal(string.Empty, details.CertificateSubject);
        Assert.Equal(string.Empty, details.SigningTime);
    }

    [Fact]
    public void Firma_xades_extrae_fecha_politica_rol_y_certificado()
    {
        using var cert = CreateCertificate();
        var xml = TestSignature.SignXades(LoadFixture("Facturae-3.2.2-valid.xml"), cert);

        var details = SignatureInspector.Inspect(xml);

        Assert.True(details.HasSignature);
        Assert.True(details.IsXades);
        Assert.Contains("XAdES", details.TypeText);
        Assert.Matches(@"\d{2}/\d{2}/\d{4} \d{2}:\d{2}:\d{2}", details.SigningTime);
        Assert.Contains("Implícita", details.Policy);
        Assert.Equal("Cajero Pagador", details.ClaimedRole);
        Assert.Contains("Facturae Test", details.CertificateSubject);
        Assert.Equal("RSA-SHA256", details.SignatureMethod);
        Assert.Matches(@"\d{2}/\d{2}/\d{4}", details.CertificateValidFrom);
        Assert.Matches(@"\d{2}/\d{2}/\d{4}", details.CertificateValidTo);
        Assert.NotEqual(string.Empty, details.CertificateValidPeriod);
    }

    [Fact]
    public void Firma_xmldsig_pura_se_identifica_como_XMLDSig()
    {
        using var cert = CreateCertificate();
        var xml = TestSignature.SignPlainXmlDsig(LoadFixture("Facturae-3.2.2-valid.xml"), cert);

        var details = SignatureInspector.Inspect(xml);

        Assert.True(details.HasSignature);
        Assert.False(details.IsXades);
        Assert.Equal("XMLDSig", details.TypeText);
        Assert.Equal(string.Empty, details.SigningTime);
        Assert.Equal(string.Empty, details.Policy);
        Assert.Equal(string.Empty, details.ClaimedRole);
        Assert.Contains("Facturae Test", details.CertificateSubject);
        Assert.Equal("RSA-SHA256", details.SignatureMethod);
    }

    [Fact]
    public void Firma_real_extrae_politica_rol_y_certificado_del_fixture()
    {
        var details = SignatureInspector.Inspect(LoadFixture("Facturae-3.1-firmada-real.xsig.xml"));

        Assert.True(details.HasSignature);
        Assert.True(details.IsXades);
        Assert.Contains("facturae.es", details.Policy);
        Assert.Equal("emisor", details.ClaimedRole);
        Assert.NotEqual(string.Empty, details.CertificateSubject);
        Assert.NotEqual(string.Empty, details.CertificateIssuer);
    }
}