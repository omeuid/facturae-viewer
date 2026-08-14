// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Facturae.App.Services;

namespace Facturae.App.Views;

/// <summary>
/// Vista previa WYSIWYG de un PDF (Windows.Data.Pdf) con impresión mediante
/// PrintDialog sobre un FixedDocument construido a partir de las páginas.
/// </summary>
public partial class PdfPreviewWindow : Window
{
    private readonly string _pdfPath;
    private IReadOnlyList<PdfPageImage> _pages = [];
    private double _scale = 1.0;

    public PdfPreviewWindow(string pdfPath)
    {
        InitializeComponent();
        _pdfPath = pdfPath;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        try
        {
            _pages = await PdfRenderer.RenderAsync(_pdfPath);
            foreach (var page in _pages)
                PagesPanel.Children.Add(CreatePageView(page));

            PageScroller.SizeChanged += OnPageScrollerSizeChanged;
            RecomputeScale();

            PrintButton.IsEnabled = _pages.Count > 0;
            StatusText.Text = $"{_pages.Count} página(s)";
        }
        catch (Exception ex)
        {
            StatusText.Text = "No se pudo mostrar la vista previa.";
            MessageBox.Show(this, ex.Message, "Vista previa", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnPageScrollerSizeChanged(object sender, SizeChangedEventArgs e) => RecomputeScale();

    /// <summary>
    /// Ajusta el zoom de las páginas para que todo el folio quepa en el área
    /// visible del ScrollViewer, sin necesidad de redimensionar la ventana.
    /// </summary>
    private void RecomputeScale()
    {
        if (_pages.Count == 0 || PagesPanel.Children.Count == 0)
            return;

        var maxWidth = _pages.Max(p => p.WidthDip);
        var maxHeight = _pages.Max(p => p.HeightDip);
        var availWidth = PageScroller.ViewportWidth - 32;
        var availHeight = PageScroller.ViewportHeight - 16;

        var scale = Math.Min(1.0, availWidth > 0 ? availWidth / maxWidth : 1.0);
        scale = Math.Min(scale, availHeight > 0 ? availHeight / maxHeight : scale);
        _scale = scale > 0 ? scale : 1.0;

        var transform = _scale < 1.0 ? new ScaleTransform(_scale, _scale) : Transform.Identity;
        foreach (var child in PagesPanel.Children)
        {
            if (child is Border page)
                page.LayoutTransform = transform;
        }
    }

    private static UIElement CreatePageView(PdfPageImage page)
        => new Border
        {
            Margin = new Thickness(0, 0, 0, 16),
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
            BorderThickness = new Thickness(1),
            Child = new Image
            {
                Source = page.Image,
                Width = page.WidthDip,
                Height = page.HeightDip,
                Stretch = Stretch.Fill,
            },
        };

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new PrintDialog();
        if (dialog.ShowDialog() != true)
            return;

        var document = new FixedDocument();
        foreach (var page in _pages)
        {
            var fixedPage = new FixedPage { Width = page.WidthDip, Height = page.HeightDip };
            fixedPage.Children.Add(new Image
            {
                Source = page.Image,
                Width = page.WidthDip,
                Height = page.HeightDip,
                Stretch = Stretch.Fill,
            });

            var pageContent = new PageContent { Child = fixedPage };
            document.Pages.Add(pageContent);
        }

        dialog.PrintDocument(document.DocumentPaginator, "Factura");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}