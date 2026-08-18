// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using FacturaeViewer.Core.Validation;

namespace FacturaeViewer.Core.Model;

/// <summary>
/// Datos descriptivos de la firma electrónica de un documento FacturaE,
/// extraídos del XML para mostrarlos en la interfaz: fecha, política, rol y
/// certificado. No valida la firma; la validación la hace
/// <see cref="SignatureValidator"/>.
/// </summary>
public sealed record SignatureDetails(
    bool HasSignature,
    bool IsXades,
    string SigningTime,
    string Policy,
    string ClaimedRole,
    string CertificateSubject,
    string CertificateIssuer,
    string CertificateValidFrom,
    string CertificateValidTo,
    string SignatureMethod)
{
    public string TypeText => !HasSignature ? "Sin firma"
        : IsXades ? "XAdES (XMLDSig con propiedades avanzadas)"
        : "XMLDSig";

    public string CertificateValidPeriod => string.IsNullOrEmpty(CertificateValidFrom) || string.IsNullOrEmpty(CertificateValidTo)
        ? string.Empty
        : $"{CertificateValidFrom} — {CertificateValidTo}";

    /// <summary>Texto legible del algoritmo de firma ("RSA-SHA256", ...).</summary>
    public static string AlgorithmToText(string algorithm) => algorithm switch
    {
        "http://www.w3.org/2000/09/xmldsig#rsa-sha1" => "RSA-SHA1",
        "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256" => "RSA-SHA256",
        "http://www.w3.org/2000/09/xmldsig#dsa-sha1" => "DSA-SHA1",
        _ => algorithm,
    };
}

/// <summary>
/// Extrae los detalles de la firma electrónica de un documento FacturaE
/// (fecha, política, rol y certificado) directamente del XML. Independiente de
/// la UI y de la validación.
/// </summary>
public static class SignatureInspector
{
    private const string XmlDsigNs = "http://www.w3.org/2000/09/xmldsig#";
    private const string XadesNs = "http://uri.etsi.org/01903/v1.3.2#";

    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-ES");

    public static SignatureDetails Inspect(XmlDocument xml)
    {
        var signature = FindSignature(xml);
        if (signature is null)
            return new SignatureDetails(
                HasSignature: false, IsXades: false,
                SigningTime: string.Empty, Policy: string.Empty, ClaimedRole: string.Empty,
                CertificateSubject: string.Empty, CertificateIssuer: string.Empty,
                CertificateValidFrom: string.Empty, CertificateValidTo: string.Empty,
                SignatureMethod: string.Empty);

        string signingTime = xml.SelectSingleNode($"//*[local-name()='SigningTime' and namespace-uri()='{XadesNs}']")
            ?.InnerText?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(signingTime) && DateTimeOffset.TryParse(signingTime, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed))
            signingTime = parsed.ToString("dd/MM/yyyy HH:mm:ss", Es);

        string policy = ExtractPolicy(xml);
        string role = xml.SelectSingleNode($"//*[local-name()='ClaimedRole' and namespace-uri()='{XadesNs}']")
            ?.InnerText?.Trim() ?? string.Empty;
        string signatureMethod = signature.SelectSingleNode(
            $"descendant::*[local-name()='SignatureMethod' and namespace-uri()='{XmlDsigNs}'][1]")
            ?.Attributes?["Algorithm"]?.Value ?? string.Empty;

        string subject = string.Empty, issuer = string.Empty, validFrom = string.Empty, validTo = string.Empty;
        using var cert = FindCertificate(signature);
        if (cert is not null)
        {
            subject = cert.Subject;
            issuer = cert.Issuer;
            validFrom = cert.NotBefore.ToLocalTime().ToString("dd/MM/yyyy", Es);
            validTo = cert.NotAfter.ToLocalTime().ToString("dd/MM/yyyy", Es);
        }

        return new SignatureDetails(
            HasSignature: true,
            IsXades: xml.SelectSingleNode($"//*[local-name()='QualifyingProperties' and namespace-uri()='{XadesNs}']") is not null,
            SigningTime: signingTime,
            Policy: policy,
            ClaimedRole: role,
            CertificateSubject: subject,
            CertificateIssuer: issuer,
            CertificateValidFrom: validFrom,
            CertificateValidTo: validTo,
            SignatureMethod: SignatureDetails.AlgorithmToText(signatureMethod));
    }

    private static string ExtractPolicy(XmlDocument xml)
    {
        var policyIdentifier = xml.SelectSingleNode(
            $"//*[local-name()='SignaturePolicyIdentifier' and namespace-uri()='{XadesNs}']");
        if (policyIdentifier is null)
            return string.Empty;

        if (policyIdentifier.SelectSingleNode("*[local-name()='SignaturePolicyImplied']") is not null)
            return "Implícita (SignaturePolicyImplied)";

        string? policyId = policyIdentifier.SelectSingleNode("descendant::*[local-name()='Identifier']")
            ?.InnerText?.Trim();
        return string.IsNullOrEmpty(policyId) ? "(sin identificador)" : policyId;
    }

    private static XmlElement? FindSignature(XmlDocument xml)
    {
        var nodes = xml.SelectNodes($"//*[local-name()='Signature' and namespace-uri()='{XmlDsigNs}']");
        return nodes?.Cast<XmlElement>().LastOrDefault();
    }

    private static X509Certificate2? FindCertificate(XmlElement signature)
    {
        var certEl = signature.SelectSingleNode("descendant::*[local-name()='X509Certificate'][1]") as XmlElement;
        var b64 = certEl?.InnerText?.Trim();
        if (string.IsNullOrEmpty(b64))
            return null;

        try
        {
            return X509CertificateLoader.LoadCertificate(Convert.FromBase64String(b64));
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}