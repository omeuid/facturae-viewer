// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using FacturaeViewer.Core.Model;

namespace Facturae.App.Services;

/// <summary>
/// Exporta facturas a PDF (guardado en disco o fichero temporal para vista previa).
/// </summary>
public interface IPdfService
{
    /// <summary>Muestra el diálogo de guardado y exporta la factura. Devuelve la ruta o null si se cancela.</summary>
    string? SaveInvoicePdf(InvoiceDisplay invoice, string suggestedName);

    /// <summary>Genera la factura en un fichero temporal y devuelve su ruta.</summary>
    string CreateTempPdf(InvoiceDisplay invoice);
}