// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

namespace FacturaeViewer.Core.Model;

/// <summary>
/// Espacios de nombres del formato FacturaE y de la firma XMLDSig/XAdES.
/// </summary>
public static class FacturaeNamespaces
{
    /// <summary>Versión 3.1.</summary>
    public const string NamespaceV3_1 = "http://www.facturae.es/Facturae/2007/v3.1/Facturae";

    /// <summary>Versión 3.2.</summary>
    public const string NamespaceV3_2 = "http://www.facturae.es/Facturae/2009/v3.2/Facturae";

    /// <summary>Versión 3.2.1.</summary>
    public const string NamespaceV3_2_1 = "http://www.facturae.es/Facturae/2014/v3.2.1/Facturae";

    /// <summary>Versión 3.2.2.</summary>
    public const string NamespaceV3_2_2 = "http://www.facturae.gob.es/formato/Versiones/Facturaev3_2_2.xml";

    /// <summary>Firma XML Digital Signature (XMLDSig).</summary>
    public const string XmlDsig = "http://www.w3.org/2000/09/xmldsig#";

    /// <summary>Espacio de nombres XAdES (1.3.2).</summary>
    public const string XAdES = "http://uri.etsi.org/01903/v1.3.2#";

    /// <summary>Espacio de nombres XAdES (1.1.1).</summary>
    public const string XAdES111 = "http://uri.etsi.org/01903/v1.1.1#";
}