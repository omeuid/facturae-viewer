// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

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

            PrintButton.IsEnabled = _pages.Count > 0;
            StatusText.Text = $"{_pages.Count} página(s)";
        }
        catch (Exception ex)
        {
            StatusText.Text = "No se pudo mostrar la vista previa.";
            MessageBox.Show(this, ex.Message, "Vista previa", MessageBoxButton.OK, MessageBoxImage.Error);
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