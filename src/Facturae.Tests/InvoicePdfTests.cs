// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using Xunit;
using FacturaeViewer.Core.IO;
using FacturaeViewer.Core.Model;
using FacturaeViewer.Core.Pdf;

namespace Facturae.Tests;

public class InvoicePdfTests
{
    private static InvoiceDisplay ProjectFirst(string fixture)
    {
        var doc = FacturaeLoader.Load(Path.Combine(AppContext.BaseDirectory, "Fixtures", fixture));
        return FacturaeProjector.Project(doc.Facturae).First();
    }

    [Fact]
    public void Generate_produce_un_documento_pdf_valido()
    {
        var bytes = InvoicePdfDocument.Generate(ProjectFirst("Facturae-3.2.2-valid.xml"));

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 1000, "El PDF generado es demasiado pequeño.");
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public void Generate_guarda_el_documento_en_disco()
    {
        var path = Path.Combine(Path.GetTempPath(), $"facturae_test_{Guid.NewGuid():N}.pdf");
        try
        {
            InvoicePdfDocument.Generate(ProjectFirst("Facturae-3.2.2-lote-valid.xml"), path);

            Assert.True(File.Exists(path));
            Assert.True(new FileInfo(path).Length > 1000);
        }
        finally
        {
            File.Delete(path);
        }
    }
}