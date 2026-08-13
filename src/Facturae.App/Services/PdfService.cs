// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.IO;
using Microsoft.Win32;
using FacturaeViewer.Core.Model;
using FacturaeViewer.Core.Pdf;

namespace Facturae.App.Services;

/// <summary>Exportación a PDF mediante QuestPDF.</summary>
public sealed class PdfService : IPdfService
{
    public string? SaveInvoicePdf(InvoiceDisplay invoice, string suggestedName)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Exportar a PDF",
            Filter = "Documento PDF (*.pdf)|*.pdf",
            FileName = suggestedName,
            DefaultExt = ".pdf",
            AddExtension = true,
        };

        if (dialog.ShowDialog() != true)
            return null;

        InvoicePdfDocument.Generate(invoice, dialog.FileName);
        return dialog.FileName;
    }

    public string CreateTempPdf(InvoiceDisplay invoice)
    {
        var path = Path.Combine(Path.GetTempPath(), $"facturae_{Guid.NewGuid():N}.pdf");
        InvoicePdfDocument.Generate(invoice, path);
        return path;
    }
}