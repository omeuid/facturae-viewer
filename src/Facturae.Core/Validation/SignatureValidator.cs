// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using FacturaeViewer.Core.Model;

namespace FacturaeViewer.Core.Validation;

/// <summary>
/// Valida la firma electrónica de un documento FacturaE: XMLDSig
/// (digest + RSA con SHA-1/SHA-256) y XAdES (fecha, certificado firmante,
/// política y rol), además de la cadena de confianza y la revocación.
/// </summary>
public static class SignatureValidator
{
    private const string XmlDsigNs = "http://www.w3.org/2000/09/xmldsig#";

    private const string EnvelopedTransform = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";
    private const string C14NTransform = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
    private const string ExclC14NTransform = "http://www.w3.org/2001/10/xml-exc-c14n#";
    private const string SignedPropertiesTransform = "http://uri.etsi.org/01903#SignedProperties";

    private const string RsaSha1 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
    private const string RsaSha256 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

    private const string DigestSha1 = "http://www.w3.org/2000/09/xmldsig#sha1";
    private const string DigestSha256 = "http://www.w3.org/2001/04/xmlenc#sha256";

    public static ValidationReport Validate(FacturaeDocument document)
        => Validate(document.Xml);

    public static ValidationReport Validate(XmlDocument xml)
    {
        var report = new ValidationReport();

        var signature = FindSignature(xml);
        if (signature is null)
        {
            report.AddWarning("SIG-01",
                "El documento no contiene firma electrónica XMLDSig/XAdES.");
            return report;
        }

        bool cryptoOk;
        string detail;
        try
        {
            var signedXmlResult = CheckWithSignedXml(xml, signature);
            if (signedXmlResult is not null)
            {
                cryptoOk = signedXmlResult.Value;
                detail = cryptoOk
                    ? "Firma XMLDSig verificada (digest y RSA sobre SignedInfo)."
                    : "La verificación con SignedXml determinó que la firma no es válida.";
            }
            else
            {
                cryptoOk = VerifyManually(xml, signature, out detail);
            }
        }
        catch (Exception ex)
        {
            report.AddError("SIG-12",
                $"No se pudo procesar la firma: {ex.Message}");
            return report;
        }

        if (cryptoOk)
        {
            report.AddPassed("SIG-02",
                detail.Contains("manual", StringComparison.OrdinalIgnoreCase)
                    ? "La firma XMLDSig/XAdES se verificó correctamente (verificación manual de la referencia XAdES)."
                    : "La firma XMLDSig/XAdES se verificó correctamente.",
                detail);
        }
        else
        {
            report.AddError("SIG-03",
                $"La firma no es válida: {detail}");
        }

        ReportAlgorithmWarning(signature, report);
        VerifyXades(xml, signature, report);
        VerifyChainOfTrust(signature, report);

        return report;
    }

    private static XmlElement? FindSignature(XmlDocument xml)
    {
        var nodes = xml.SelectNodes($"//*[local-name()='Signature' and namespace-uri()='{XmlDsigNs}']");
        return nodes?.Cast<XmlElement>().LastOrDefault();
    }

    /// <summary>
    /// Intenta la verificación completa con SignedXml. Devuelve true/false si
    /// SignedXml pudo verificar (firmas XMLDSig puras y XAdES con transform
    /// XPath), o null si SignedXml no soporta un transform (p. ej. el de
    /// XAdES SignedProperties) y hay que verificar manualmente.
    /// </summary>
    private static bool? CheckWithSignedXml(XmlDocument xml, XmlElement signature)
    {
        try
        {
            var signedXml = new SignedXml(xml);
            signedXml.LoadXml(signature);
            return signedXml.CheckSignature();
        }
        catch (CryptographicException)
        {
            return null;
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException or XmlException)
        {
            return null;
        }
    }

    /// <summary>
    /// Verificación manual de XMLDSig/XAdES: valida la firma RSA sobre el
    /// SignedInfo canonizado, el digest de las referencias de contenido y el
    /// digest de la referencia XAdES <c>SignedProperties</c> (transform que
    /// SignedXml de .NET no implementa).
    /// </summary>
    private static bool VerifyManually(XmlDocument xml, XmlElement signature, out string detail)
    {
        detail = string.Empty;

        var signatureValue = FindDirectChild(signature, "SignatureValue")?.InnerText?.Trim();
        if (signatureValue is null)
        {
            detail = "no se encontró el elemento SignatureValue.";
            return false;
        }

        var signedInfo = FindDirectChild(signature, "SignedInfo");
        if (signedInfo is null)
        {
            detail = "no se encontró el elemento SignedInfo.";
            return false;
        }

        string canonicalization = FindDirectChild(signedInfo, "CanonicalizationMethod")
            ?.GetAttribute("Algorithm") ?? C14NTransform;
        string signatureMethod = FindDirectChild(signedInfo, "SignatureMethod")
            ?.GetAttribute("Algorithm") ?? RsaSha256;

        HashAlgorithmName rsaHash;
        try
        {
            rsaHash = ResolveSignatureHash(signatureMethod);
        }
        catch (NotSupportedException ex)
        {
            detail = ex.Message;
            return false;
        }

        using var publicKey = ResolvePublicKey(signature);
        if (publicKey is null)
        {
            detail = "no se encontró una clave pública en KeyInfo para comprobar la firma.";
            return false;
        }

        byte[] signedInfoBytes;
        try
        {
            signedInfoBytes = Canonicalize(signedInfo, canonicalization);
        }
        catch (Exception ex) when (ex is XmlException or ArgumentException)
        {
            detail = $"no se pudo canonizar SignedInfo: {ex.Message}";
            return false;
        }

        byte[] signatureValueBytes;
        try
        {
            signatureValueBytes = Convert.FromBase64String(signatureValue);
        }
        catch (FormatException)
        {
            detail = "SignatureValue no es Base64 válido.";
            return false;
        }

        if (!publicKey.VerifyData(signedInfoBytes, signatureValueBytes, rsaHash, RSASignaturePadding.Pkcs1))
        {
            detail = "la firma RSA sobre SignedInfo no coincide con la clave pública del documento.";
            return false;
        }

        foreach (XmlElement reference in FindChildren(signedInfo, "Reference"))
        {
            if (IsSignedPropertiesReference(reference))
                continue;

            if (!VerifyContentReference(xml, signature, reference, canonicalization, out string refDetail))
            {
                detail = refDetail;
                return false;
            }
        }

        foreach (XmlElement reference in FindChildren(signedInfo, "Reference"))
        {
            if (IsSignedPropertiesReference(reference)
                && !VerifySignedPropertiesReference(xml, reference, canonicalization, out string spDetail))
            {
                detail = spDetail;
                return false;
            }
        }

        detail = "verificación manual correcta: RSA sobre SignedInfo y digests de las referencias de contenido y XAdES.";
        return true;
    }

    private static bool VerifyContentReference(
        XmlDocument xml, XmlElement signature, XmlElement reference, string defaultCanonicalization, out string detail)
    {
        detail = string.Empty;

        string uri = reference.GetAttribute("URI");
        string digestMethod = FindDirectChild(reference, "DigestMethod")?.GetAttribute("Algorithm") ?? DigestSha256;
        string? digestValueB64 = FindDirectChild(reference, "DigestValue")?.InnerText?.Trim();
        if (digestValueB64 is null)
        {
            detail = $"la referencia '{uri}' no tiene DigestValue.";
            return false;
        }

        var transforms = FindDescendants(reference, "Transform");
        bool removeSignature = transforms.Any(t => t.GetAttribute("Algorithm") == EnvelopedTransform);
        string canonicalization = transforms
            .FirstOrDefault(t => t.GetAttribute("Algorithm") is C14NTransform or ExclC14NTransform)
            ?.GetAttribute("Algorithm") ?? defaultCanonicalization;

        // El transform enveloped-signature excluye el elemento Signature de la
        // verificación; trabajamos sobre una copia para no mutar el documento.
        var working = (XmlDocument)xml.CloneNode(true);
        if (removeSignature)
        {
            foreach (var sig in working.SelectNodes($"//*[local-name()='Signature' and namespace-uri()='{XmlDsigNs}']")!
                         .Cast<XmlElement>().ToList())
                sig.ParentNode?.RemoveChild(sig);
        }

        XmlNode target;
        if (string.IsNullOrEmpty(uri) || uri == "#")
        {
            target = working;
        }
        else
        {
            string id = uri.TrimStart('#');
            target = working.SelectSingleNode($"//*[@Id='{id}' or @id='{id}' or @ID='{id}']")
                ?? throw new InvalidOperationException($"No se encontró la referencia '{uri}' en el documento.");
        }

        byte[] canonicalBytes;
        try
        {
            canonicalBytes = Canonicalize(target, canonicalization);
        }
        catch (Exception ex) when (ex is XmlException or ArgumentException)
        {
            detail = $"no se pudo canonizar la referencia '{uri}': {ex.Message}";
            return false;
        }

        if (!DigestMatches(canonicalBytes, digestMethod, digestValueB64))
        {
            detail = $"el digest de la referencia '{uri}' no coincide (el contenido fue modificado).";
            return false;
        }

        return true;
    }

    private static bool VerifySignedPropertiesReference(
        XmlDocument xml, XmlElement reference, string defaultCanonicalization, out string detail)
    {
        detail = string.Empty;

        string uri = reference.GetAttribute("URI");
        string id = uri.TrimStart('#');
        string digestMethod = FindDirectChild(reference, "DigestMethod")?.GetAttribute("Algorithm") ?? DigestSha256;
        string? digestValueB64 = FindDirectChild(reference, "DigestValue")?.InnerText?.Trim();
        if (digestValueB64 is null)
        {
            detail = $"la referencia XAdES '{uri}' no tiene DigestValue.";
            return false;
        }

        // El transform http://uri.etsi.org/01903#SignedProperties selecciona el
        // elemento SignedProperties y lo canoniza con el método de SignedInfo.
        var signedProperties = xml.SelectSingleNode($"//*[local-name()='SignedProperties' and @Id='{id}']");
        if (signedProperties is null)
        {
            detail = $"no se encontró el elemento XAdES SignedProperties '{id}'.";
            return false;
        }

        byte[] canonicalBytes;
        try
        {
            canonicalBytes = Canonicalize(signedProperties, defaultCanonicalization);
        }
        catch (Exception ex) when (ex is XmlException or ArgumentException)
        {
            detail = $"no se pudo canonizar SignedProperties: {ex.Message}";
            return false;
        }

        if (!DigestMatches(canonicalBytes, digestMethod, digestValueB64))
        {
            detail = $"el digest de SignedProperties '{id}' no coincide (el bloque XAdES fue modificado).";
            return false;
        }

        return true;
    }

    private static bool DigestMatches(byte[] data, string digestMethod, string digestValueB64)
    {
        byte[] expected;
        try
        {
            expected = Convert.FromBase64String(digestValueB64);
        }
        catch (FormatException)
        {
            return false;
        }

        return ComputeDigest(data, digestMethod).SequenceEqual(expected);
    }

    private static bool IsSignedPropertiesReference(XmlElement reference)
        => FindDescendants(reference, "Transform")
            .Any(t => t.GetAttribute("Algorithm") == SignedPropertiesTransform);

    private static byte[] Canonicalize(XmlNode node, string algorithm)
    {
        Transform transform = algorithm == ExclC14NTransform
            ? new XmlDsigExcC14NTransform()
            : new XmlDsigC14NTransform(false);

        // La canonización debe cubrir todo el subárbol del nodo (elemento y
        // descendientes), no solo el propio nodo: C14N solo serializa los
        // nodos que pertenecen al conjunto.
        var root = node is XmlDocument document ? document.DocumentElement! : node;
        transform.LoadInput(root.SelectNodes(". | .//* | .//text()")!);

        using var output = (Stream)transform.GetOutput(typeof(Stream));
        using var memory = new MemoryStream();
        output.CopyTo(memory);
        return memory.ToArray();
    }

    private static byte[] ComputeDigest(byte[] data, string algorithm) => algorithm switch
    {
        DigestSha1 => SHA1.HashData(data),
        DigestSha256 => SHA256.HashData(data),
        "http://www.w3.org/2001/04/xmldsig-more#sha384" => SHA384.HashData(data),
        "http://www.w3.org/2001/04/xmlenc#sha512" => SHA512.HashData(data),
        _ => throw new NotSupportedException($"Algoritmo de resumen no soportado: {algorithm}"),
    };

    private static HashAlgorithmName ResolveSignatureHash(string signatureMethod) => signatureMethod switch
    {
        RsaSha1 => HashAlgorithmName.SHA1,
        RsaSha256 => HashAlgorithmName.SHA256,
        _ => throw new NotSupportedException($"Algoritmo de firma no soportado: {signatureMethod}"),
    };

    private static RSA? ResolvePublicKey(XmlElement signature)
    {
        var cert = FindCertificate(signature);
        if (cert is not null)
        {
            using (cert)
                return cert.GetRSAPublicKey();
        }

        var rsaKeyValue = signature.SelectSingleNode($"descendant::*[local-name()='RSAKeyValue'][1]");
        if (rsaKeyValue is XmlElement keyValue)
        {
            var modulus = keyValue.SelectSingleNode("descendant::*[local-name()='Modulus'][1]")?.InnerText?.Trim();
            var exponent = keyValue.SelectSingleNode("descendant::*[local-name()='Exponent'][1]")?.InnerText?.Trim();
            if (modulus is not null && exponent is not null)
            {
                try
                {
                    var rsa = RSA.Create();
                    rsa.ImportParameters(new RSAParameters
                    {
                        Modulus = Convert.FromBase64String(modulus),
                        Exponent = Convert.FromBase64String(exponent),
                    });
                    return rsa;
                }
                catch (CryptographicException)
                {
                    return null;
                }
            }
        }

        return null;
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

    private static void ReportAlgorithmWarning(XmlElement signature, ValidationReport report)
    {
        string signatureMethod = signature.SelectSingleNode(
            $"descendant::*[local-name()='SignatureMethod'][1]")?.Attributes?["Algorithm"]?.Value ?? string.Empty;
        bool usesSha1Digest = signature.SelectNodes("descendant::*[local-name()='DigestMethod']")!
            .Cast<XmlElement>()
            .Any(m => m.GetAttribute("Algorithm") == DigestSha1);

        if (signatureMethod == RsaSha1 || usesSha1Digest)
        {
            report.AddWarning("SIG-04",
                "La firma usa el algoritmo SHA-1 (deprecado). Se recomienda firmar con SHA-256.");
        }
    }

    private static void VerifyXades(XmlDocument xml, XmlElement signature, ValidationReport report)
    {
        var qualifyingProperties = xml.SelectSingleNode("//*[local-name()='QualifyingProperties']") as XmlElement;
        if (qualifyingProperties is null)
            return;

        report.Add("SIG-06", CheckStatus.Info,
            $"El documento incluye una firma XAdES ({qualifyingProperties.NamespaceURI}).");

        var signingTime = xml.SelectSingleNode("//*[local-name()='SigningTime']")?.InnerText?.Trim();
        if (!string.IsNullOrEmpty(signingTime))
            report.Add("SIG-07", CheckStatus.Info, $"Fecha y hora de firma declarada: {signingTime}.");

        var policyIdentifier = xml.SelectSingleNode("//*[local-name()='SignaturePolicyIdentifier']");
        if (policyIdentifier is not null)
        {
            if (policyIdentifier.SelectSingleNode("*[local-name()='SignaturePolicyImplied']") is not null)
            {
                report.Add("SIG-08", CheckStatus.Info, "Política de firma: implícita (SignaturePolicyImplied).");
            }
            else
            {
                string? policyId = policyIdentifier.SelectSingleNode(
                    "descendant::*[local-name()='Identifier']")?.InnerText?.Trim();
                report.Add("SIG-08", CheckStatus.Info,
                    $"Política de firma: {policyId ?? "(sin identificador)"}.");
            }
        }

        var claimedRole = xml.SelectSingleNode("//*[local-name()='ClaimedRole']")?.InnerText?.Trim();
        if (!string.IsNullOrEmpty(claimedRole))
            report.Add("SIG-09", CheckStatus.Info, $"Rol declarado: {claimedRole}.");

        VerifySigningCertificateDigest(xml, signature, report);
    }

    private static void VerifySigningCertificateDigest(XmlDocument xml, XmlElement signature, ValidationReport report)
    {
        var certDigest = xml.SelectSingleNode("//*[local-name()='CertDigest']") as XmlElement;
        if (certDigest is null)
            return;

        using var cert = FindCertificate(signature);
        if (cert is null)
            return;

        string digestMethod = FindDirectChild(certDigest, "DigestMethod")?.GetAttribute("Algorithm") ?? DigestSha256;
        string? digestValueB64 = FindDirectChild(certDigest, "DigestValue")?.InnerText?.Trim();
        if (digestValueB64 is null)
            return;

        byte[] expected;
        try
        {
            expected = Convert.FromBase64String(digestValueB64);
        }
        catch (FormatException)
        {
            return;
        }

        var actual = ComputeDigest(cert.RawData, digestMethod);
        if (actual.SequenceEqual(expected))
        {
            report.AddPassed("SIG-11",
                "El certificado firmante declarado en XAdES coincide con el de la firma.");
        }
        else
        {
            report.AddError("SIG-11",
                "El certificado firmante declarado en XAdES no coincide con el de la firma.");
        }
    }

    private static void VerifyChainOfTrust(XmlElement signature, ValidationReport report)
    {
        using var cert = FindCertificate(signature);
        if (cert is null)
        {
            report.AddWarning("SIG-10",
                "No se encontró un certificado en la firma para validar la cadena de confianza.");
            return;
        }

        string subject = cert.Subject;

        bool validated = TryBuildChain(cert, X509RevocationMode.Online, out string onlineDetail);
        if (validated)
        {
            report.AddPassed("SIG-10",
                $"Cadena de confianza verificada con comprobación de revocación (OCSP/CRL). Certificado: {subject}.");
            return;
        }

        validated = TryBuildChain(cert, X509RevocationMode.Offline, out string offlineDetail);
        if (validated)
        {
            report.AddPassed("SIG-10",
                $"Cadena de confianza verificada (sin comprobación de revocación por falta de red). Certificado: {subject}.",
                onlineDetail);
            return;
        }

        report.AddWarning("SIG-10",
            $"La cadena de confianza no se pudo validar ({offlineDetail}); el estado de revocación queda no verificado. Certificado: {subject}.",
            onlineDetail);
    }

    private static bool TryBuildChain(X509Certificate2 cert, X509RevocationMode revocationMode, out string detail)
    {
        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = revocationMode;
        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

        bool ok;
        try
        {
            ok = chain.Build(cert);
        }
        catch (Exception ex)
        {
            detail = ex.Message;
            return false;
        }

        if (ok)
        {
            detail = "cadena de confianza correcta.";
            return true;
        }

        var reasons = chain.ChainStatus
            .Select(s => s.StatusInformation?.Trim())
            .Where(r => !string.IsNullOrEmpty(r))
            .Distinct();
        detail = reasons.Any() ? string.Join("; ", reasons) : "certificado no confiable.";
        return false;
    }

    private static XmlElement? FindDirectChild(XmlElement parent, string localName)
    {
        foreach (XmlNode child in parent.ChildNodes)
        {
            if (child is XmlElement e && e.LocalName == localName)
                return e;
        }
        return null;
    }

    private static List<XmlElement> FindChildren(XmlElement parent, string localName)
    {
        var result = new List<XmlElement>();
        foreach (XmlNode child in parent.ChildNodes)
        {
            if (child is XmlElement e && e.LocalName == localName)
                result.Add(e);
        }
        return result;
    }

    private static List<XmlElement> FindDescendants(XmlElement parent, string localName)
        => parent.SelectNodes($".//*[local-name()='{localName}']")!
            .Cast<XmlElement>()
            .ToList();
}
