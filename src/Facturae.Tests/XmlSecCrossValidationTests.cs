// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Xunit;
using FacturaeViewer.Core.Validation;

namespace Facturae.Tests;

/// <summary>
/// Ejecuta el test solo si xmlsec está disponible (variable XMLSEC_BIN o
/// xmlsec.exe/xmlsec1.exe en el PATH); si no, el test se omite. Los tests se
/// ejecutan en el CI, donde se instala xmlsec.
/// </summary>
public sealed class XmlSecFactAttribute : FactAttribute
{
    public XmlSecFactAttribute()
    {
        if (!XmlSecCrossValidationTests.IsXmlSecAvailable())
            Skip = "xmlsec no está instalado; se omite la verificación cruzada.";
    }
}

/// <summary>
/// Verificación cruzada de la firma contra xmlsec (XML Security Library),
/// implementación de referencia independiente de la nuestra.
/// </summary>
public class XmlSecCrossValidationTests
{
    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    public static bool IsXmlSecAvailable() => FindXmlSec() is not null;

    private static string? FindXmlSec()
    {
        string? configured = Environment.GetEnvironmentVariable("XMLSEC_BIN");
        if (!string.IsNullOrEmpty(configured) && File.Exists(configured))
            return configured;

        foreach (var candidate in new[] { "xmlsec.exe", "xmlsec1.exe" })
        {
            string? found = FindOnPath(candidate);
            if (found is not null)
                return found;
        }
        return null;
    }

    private static string? FindOnPath(string fileName)
    {
        string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (string dir in path.Split(Path.PathSeparator))
        {
            if (dir.Length == 0)
                continue;
            string candidate = Path.Combine(dir, fileName);
            if (File.Exists(candidate))
                return candidate;
        }
        return null;
    }

    /// <summary>Ejecuta xmlsec --verify y devuelve true si la firma es válida.</summary>
    private static bool XmlSecVerifies(string xmlPath)
    {
        string? xmlsec = FindXmlSec();
        if (xmlsec is null)
            return false;

        var psi = new ProcessStartInfo
        {
            FileName = xmlsec!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add("--verify");
        psi.ArgumentList.Add("--enabled-key-data");
        psi.ArgumentList.Add("rsa,key-value,x509");
        psi.ArgumentList.Add(xmlPath);

        // xmlsec win64 necesita las DLLs de libxml2/libxslt/openssl, que viven en
        // directorios hermanos del ejecutable; se añaden al PATH del proceso hijo.
        string? xmlsecDir = Path.GetDirectoryName(xmlsec);
        if (xmlsecDir is not null)
        {
            var extra = new List<string> { xmlsecDir };
            var root = Path.GetDirectoryName(Path.GetDirectoryName(xmlsecDir));
            if (root is not null)
            {
                foreach (string dir in Directory.GetDirectories(root))
                    extra.Add(Path.Combine(dir, "bin"));
            }
            psi.Environment["PATH"] = string.Join(Path.PathSeparator, extra) + Path.PathSeparator
                + Environment.GetEnvironmentVariable("PATH");
        }

        using var process = Process.Start(psi)!;
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        // xmlsec escribe "Verification status: OK" en stderr.
        string combined = output + error;
        return combined.Contains("status: OK", StringComparison.OrdinalIgnoreCase);
    }

    private static XmlDocument LoadFixture(string name)
    {
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.Load(Fixture(name));
        return doc;
    }

    private static X509Certificate2 CreateCertificate() => TestSignature.CreateSelfSignedCertificate();

    private static string WriteTemp(XmlDocument xml, string name)
    {
        // Se escribe el OuterXml tal cual: XmlDocument.Save() reindenta el
        // documento, y el digest de la firma se calculó sobre el árbol en
        // memoria (que puede estar compactado por TestSignature).
        string path = Path.Combine(Path.GetTempPath(), $"xmlsec-{Guid.NewGuid():N}-{name}");
        File.WriteAllText(path, xml.OuterXml);
        return path;
    }

    [XmlSecFact]
    public void Firma_real_de_facturae_gob_es_es_valida_para_xmlsec_y_para_nosotros()
    {
        string path = Fixture("Facturae-3.1-firmada-real.xsig.xml");

        Assert.True(XmlSecVerifies(path), "xmlsec debe validar la firma real.");
        var report = SignatureValidator.Validate(LoadFixture("Facturae-3.1-firmada-real.xsig.xml"));
        Assert.False(report.HasErrors, string.Join("\n", report.Checks.Select(c => c.ToString())));
        Assert.Contains(report.Checks, c => c.Code == "SIG-02" && c.Status == CheckStatus.Passed);
    }

    [XmlSecFact]
    public void Firma_xmldsig_generada_es_valida_para_xmlsec_y_para_nosotros()
    {
        using var cert = CreateCertificate();
        var xml = TestSignature.SignPlainXmlDsig(LoadFixture("Facturae-3.2.2-valid.xml"), cert);
        string path = WriteTemp(xml, "xmldsig.xml");
        try
        {
            Assert.True(XmlSecVerifies(path), "xmlsec debe validar la firma XMLDSig generada.");
            var report = SignatureValidator.Validate(xml);
            Assert.False(report.HasErrors, string.Join("\n", report.Checks.Select(c => c.ToString())));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [XmlSecFact]
    public void Firma_xades_generada_es_valida_para_xmlsec_y_para_nosotros()
    {
        using var cert = CreateCertificate();
        var xml = TestSignature.SignXades(LoadFixture("Facturae-3.2.2-valid.xml"), cert);
        string path = WriteTemp(xml, "xades.xml");
        try
        {
            Assert.True(XmlSecVerifies(path), "xmlsec debe validar la firma XAdES generada.");
            var report = SignatureValidator.Validate(xml);
            Assert.False(report.HasErrors, string.Join("\n", report.Checks.Select(c => c.ToString())));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [XmlSecFact]
    public void Firma_sobre_contenido_manipulado_es_invalida_para_xmlsec_y_para_nosotros()
    {
        using var cert = CreateCertificate();
        var xml = TestSignature.SignPlainXmlDsig(LoadFixture("Facturae-3.2.2-valid.xml"), cert);

        var total = xml.SelectSingleNode("//*[local-name()='TotalAmount']")!;
        total.InnerText = "666.00";

        string path = WriteTemp(xml, "manipulada.xml");
        try
        {
            Assert.False(XmlSecVerifies(path), "xmlsec debe rechazar la firma manipulada.");
            var report = SignatureValidator.Validate(xml);
            Assert.True(report.HasErrors);
            Assert.Contains(report.Checks, c => c.Code == "SIG-03" && c.Status == CheckStatus.Error);
        }
        finally
        {
            File.Delete(path);
        }
    }
}