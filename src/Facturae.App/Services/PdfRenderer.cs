// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.IO;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Facturae.App.Services;

/// <summary>Página de un PDF renderizada como imagen WPF (WYSIWYG para vista previa e impresión).</summary>
public sealed record PdfPageImage(BitmapSource Image, double WidthDip, double HeightDip);

/// <summary>
/// Renderiza un documento PDF a imágenes usando Windows.Data.Pdf. El render
/// se hace a 96 DPI, por lo que el tamaño de la imagen coincide con el de la
/// página en DIPs (vista previa y salida impresa idénticas al PDF).
/// </summary>
public static class PdfRenderer
{
    public static async Task<IReadOnlyList<PdfPageImage>> RenderAsync(string pdfPath)
    {
        var file = await StorageFile.GetFileFromPathAsync(pdfPath);
        var document = await PdfDocument.LoadFromFileAsync(file);

        var pages = new List<PdfPageImage>((int)document.PageCount);
        for (uint i = 0; i < document.PageCount; i++)
        {
            using var page = document.GetPage(i);
            using var stream = new InMemoryRandomAccessStream();
            await page.RenderToStreamAsync(stream);

            var size = (int)stream.Size;
            using var reader = new DataReader(stream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)size);
            var bytes = new byte[size];
            reader.ReadBytes(bytes);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = new MemoryStream(bytes);
            bitmap.EndInit();
            bitmap.Freeze();

            pages.Add(new PdfPageImage(bitmap, page.Size.Width, page.Size.Height));
        }

        return pages;
    }
}