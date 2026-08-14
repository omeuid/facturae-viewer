// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Facturae.Tests;

/// <summary>
/// Ayudante de pruebas para generar certificados autofirmados y firmar un
/// documento FacturaE: firma XMLDSig pura (con SignedXml) y firma XAdES
/// (con el transform oficial http://uri.etsi.org/01903#SignedProperties,
/// que SignedXml de .NET no genera y se construye manualmente).
/// </summary>
internal static class TestSignature
{
    private const string DsNs = "http://www.w3.org/2000/09/xmldsig#";
    private const string XadesNs = "http://uri.etsi.org/01903/v1.3.2#";
    private const string C14N = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
    private const string Enveloped = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";
    private const string SignedPropsTransform = "http://uri.etsi.org/01903#SignedProperties";
    private const string DigestSha256 = "http://www.w3.org/2001/04/xmlenc#sha256";
    private const string RsaSha256 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

    public static X509Certificate2 CreateSelfSignedCertificate(string subject = "CN=Facturae Test, O=Facturae Viewer, C=ES")
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        return request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(730));
    }

    public static XmlDocument SignPlainXmlDsig(XmlDocument source, X509Certificate2 cert)
    {
        var xml = (XmlDocument)source.CloneNode(true);

        using var rsa = cert.GetRSAPrivateKey()!;
        var signedXml = new SignedXml(xml)
        {
            SigningKey = rsa,
        };
        signedXml.SignedInfo!.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
        signedXml.SignedInfo!.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

        var reference = new Reference("");
        reference.DigestMethod = DigestSha256;
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigC14NTransform());
        signedXml.AddReference(reference);

        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(cert));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();

        xml.DocumentElement!.AppendChild(xml.ImportNode(signedXml.GetXml()!, true));
        return xml;
    }

    public static XmlDocument SignXades(XmlDocument source, X509Certificate2 cert)
    {
        var xml = (XmlDocument)source.CloneNode(true);
        using var rsa = cert.GetRSAPrivateKey()!;

        var signature = xml.CreateElement("ds", "Signature", DsNs);
        signature.SetAttribute("Id", "Signature1");

        var signedInfo = xml.CreateElement("ds", "SignedInfo", DsNs);
        signature.AppendChild(signedInfo);

        var canonicalization = xml.CreateElement("ds", "CanonicalizationMethod", DsNs);
        canonicalization.SetAttribute("Algorithm", C14N);
        signedInfo.AppendChild(canonicalization);

        var signatureMethod = xml.CreateElement("ds", "SignatureMethod", DsNs);
        signatureMethod.SetAttribute("Algorithm", RsaSha256);
        signedInfo.AppendChild(signatureMethod);

        // Referencia al contenido completo de la factura (URI vacía).
        var contentReference = CreateReference(xml, "", DigestSha256, C14N, Enveloped);
        signedInfo.AppendChild(contentReference);

        // Referencia XAdES a SignedProperties con el transform oficial.
        var propsReference = xml.CreateElement("ds", "Reference", DsNs);
        propsReference.SetAttribute("URI", "#SignedProperties1");
        var transforms = xml.CreateElement("ds", "Transforms", DsNs);
        var propsTransform = xml.CreateElement("ds", "Transform", DsNs);
        propsTransform.SetAttribute("Algorithm", SignedPropsTransform);
        transforms.AppendChild(propsTransform);
        propsReference.AppendChild(transforms);
        var propsDigestMethod = xml.CreateElement("ds", "DigestMethod", DsNs);
        propsDigestMethod.SetAttribute("Algorithm", DigestSha256);
        propsReference.AppendChild(propsDigestMethod);
        var propsDigestValue = xml.CreateElement("ds", "DigestValue", DsNs);
        propsReference.AppendChild(propsDigestValue);
        signedInfo.AppendChild(propsReference);

        var signatureValue = xml.CreateElement("ds", "SignatureValue", DsNs);
        signature.AppendChild(signatureValue);

        var keyInfo = xml.CreateElement("ds", "KeyInfo", DsNs);
        var x509Data = xml.CreateElement("ds", "X509Data", DsNs);
        var certElement = xml.CreateElement("ds", "X509Certificate", DsNs);
        certElement.InnerText = Convert.ToBase64String(cert.RawData);
        x509Data.AppendChild(certElement);
        keyInfo.AppendChild(x509Data);
        signature.AppendChild(keyInfo);

        var objectEl = xml.CreateElement("ds", "Object", DsNs);
        objectEl.SetAttribute("Id", "Object1");
        var qualifyingProperties = xml.CreateElement("xades", "QualifyingProperties", XadesNs);
        qualifyingProperties.SetAttribute("Target", "#Signature1");
        var signedProperties = xml.CreateElement("xades", "SignedProperties", XadesNs);
        signedProperties.SetAttribute("Id", "SignedProperties1");

        var signedSignatureProperties = xml.CreateElement("xades", "SignedSignatureProperties", XadesNs);
        var signingTime = xml.CreateElement("xades", "SigningTime", XadesNs);
        signingTime.InnerText = XmlConvert.ToString(DateTimeOffset.UtcNow);
        signedSignatureProperties.AppendChild(signingTime);

        var signingCertificate = xml.CreateElement("xades", "SigningCertificate", XadesNs);
        var certRef = xml.CreateElement("xades", "Cert", XadesNs);
        var certDigest = xml.CreateElement("xades", "CertDigest", XadesNs);
        var certDigestMethod = xml.CreateElement("xades", "DigestMethod", XadesNs);
        certDigestMethod.SetAttribute("Algorithm", DigestSha256);
        certDigest.AppendChild(certDigestMethod);
        var certDigestValue = xml.CreateElement("xades", "DigestValue", XadesNs);
        certDigestValue.InnerText = Convert.ToBase64String(SHA256.HashData(cert.RawData));
        certDigest.AppendChild(certDigestValue);
        certRef.AppendChild(certDigest);
        signingCertificate.AppendChild(certRef);
        signedSignatureProperties.AppendChild(signingCertificate);

        var policyIdentifier = xml.CreateElement("xades", "SignaturePolicyIdentifier", XadesNs);
        var policyImplied = xml.CreateElement("xades", "SignaturePolicyImplied", XadesNs);
        policyIdentifier.AppendChild(policyImplied);
        signedSignatureProperties.AppendChild(policyIdentifier);

        var signerRole = xml.CreateElement("xades", "SignerRole", XadesNs);
        var claimedRoles = xml.CreateElement("xades", "ClaimedRoles", XadesNs);
        var claimedRole = xml.CreateElement("xades", "ClaimedRole", XadesNs);
        claimedRole.InnerText = "Cajero Pagador";
        claimedRoles.AppendChild(claimedRole);
        signerRole.AppendChild(claimedRoles);
        signedSignatureProperties.AppendChild(signerRole);

        signedProperties.AppendChild(signedSignatureProperties);
        qualifyingProperties.AppendChild(signedProperties);
        objectEl.AppendChild(qualifyingProperties);
        signature.AppendChild(objectEl);

        xml.DocumentElement!.AppendChild(signature);

        // 1) Digest del contenido: documento completo sin la firma, C14N.
        var working = (XmlDocument)xml.CloneNode(true);
        foreach (var sig in working.SelectNodes($"//*[local-name()='Signature' and namespace-uri()='{DsNs}']")!
                     .Cast<XmlElement>().ToList())
            sig.ParentNode!.RemoveChild(sig);
        var contentTransform = new XmlDsigC14NTransform(false);
        contentTransform.LoadInput(working.DocumentElement!.SelectNodes(". | .//* | .//@* | .//text()")!);
        using var contentStream = (Stream)contentTransform.GetOutput(typeof(Stream));
        using var contentMemory = new MemoryStream();
        contentStream.CopyTo(contentMemory);
        SetDigestValue(contentReference, SHA256.HashData(contentMemory.ToArray()));

        // 2) Digest de SignedProperties: elemento canonizado con C14N.
        var signedPropsEl = xml.SelectSingleNode("//*[local-name()='SignedProperties']")!;
        var propsC14N = new XmlDsigC14NTransform(false);
        propsC14N.LoadInput(signedPropsEl.SelectNodes(". | .//* | .//@* | .//text()")!);
        using var propsStream = (Stream)propsC14N.GetOutput(typeof(Stream));
        using var propsMemory = new MemoryStream();
        propsStream.CopyTo(propsMemory);
        SetDigestValue(propsReference, SHA256.HashData(propsMemory.ToArray()));

        // 3) Firma RSA sobre SignedInfo canonizado.
        var signedInfoEl = xml.SelectSingleNode("//*[local-name()='SignedInfo']")!;
        var signedInfoTransform = new XmlDsigC14NTransform(false);
        signedInfoTransform.LoadInput(signedInfoEl.SelectNodes(". | .//* | .//@* | .//text()")!);
        using var signedInfoStream = (Stream)signedInfoTransform.GetOutput(typeof(Stream));
        using var signedInfoMemory = new MemoryStream();
        signedInfoStream.CopyTo(signedInfoMemory);
        var signatureBytes = rsa.SignData(signedInfoMemory.ToArray(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        signatureValue.InnerText = Convert.ToBase64String(signatureBytes);

        return xml;
    }

    private static XmlElement CreateReference(
        XmlDocument xml, string uri, string digestMethod, string c14n, string enveloped)
    {
        var reference = xml.CreateElement("ds", "Reference", DsNs);
        if (!string.IsNullOrEmpty(uri))
            reference.SetAttribute("URI", uri);

        var transforms = xml.CreateElement("ds", "Transforms", DsNs);
        var envelopedTransform = xml.CreateElement("ds", "Transform", DsNs);
        envelopedTransform.SetAttribute("Algorithm", enveloped);
        transforms.AppendChild(envelopedTransform);
        var c14nTransform = xml.CreateElement("ds", "Transform", DsNs);
        c14nTransform.SetAttribute("Algorithm", c14n);
        transforms.AppendChild(c14nTransform);
        reference.AppendChild(transforms);

        var digestMethodEl = xml.CreateElement("ds", "DigestMethod", DsNs);
        digestMethodEl.SetAttribute("Algorithm", digestMethod);
        reference.AppendChild(digestMethodEl);

        reference.AppendChild(xml.CreateElement("ds", "DigestValue", DsNs));
        return reference;
    }

    private static void SetDigestValue(XmlElement reference, byte[] digest)
        => reference.SelectSingleNode("descendant::*[local-name()='DigestValue']")!.InnerText
            = Convert.ToBase64String(digest);
}