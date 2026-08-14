// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Facturae.App.Services;
using Facturae.App.Views;
using FacturaeViewer.Core.IO;
using FacturaeViewer.Core.Model;
using FacturaeViewer.Core.Validation;

namespace Facturae.App.ViewModels;

/// <summary>
/// ViewModel principal del visor: apertura de ficheros, ejecución de todas
/// las validaciones, navegación de lotes y estado del panel de resultados.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly IDialogService _dialogs;
    private readonly IPdfService _pdf;
    private readonly RecentFilesService _recent;
    private IReadOnlyList<InvoiceDisplay> _invoices = new List<InvoiceDisplay>();

    public MainViewModel(IDialogService dialogs, IPdfService pdf, RecentFilesService? recent = null)
    {
        _dialogs = dialogs;
        _pdf = pdf;
        _recent = recent ?? new RecentFilesService();
    }

    [ObservableProperty]
    private ObservableCollection<ValidationCheck> _checks = new();

    [ObservableProperty]
    private InvoiceDisplay? _currentInvoice;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _schemaVersion = string.Empty;

    [ObservableProperty]
    private string _documentStateText = "Sin documento";

    [ObservableProperty]
    private string _summaryText = "Arrastre un fichero .xsig, .xpsig o .xml aquí o use «Abrir…».";

    [ObservableProperty]
    private CheckStatus _overallStatus = CheckStatus.Info;

    [ObservableProperty]
    private bool _hasDocument;

    [ObservableProperty]
    private int _currentIndex;

    [ObservableProperty]
    private int _totalInvoices;

    [ObservableProperty]
    private bool _isDragOver;

    /// <summary>Rutas recientes (cadenas escapadas) mostradas en el menú.</summary>
    public IList<string> RecentFiles { get; set; } = new ObservableCollection<string>();

    [RelayCommand]
    private void Open()
    {
        var path = _dialogs.OpenFacturaeFile();
        if (path is not null)
            Load(path);
    }

    /// <summary>Carga y valida un fichero FacturaE (usado por el botón, el CLI y el drag & drop).</summary>
    public bool Load(string path)
    {
        try
        {
            var document = FacturaeLoader.Load(path);
            var report = DocumentValidator.Validate(document);
            _invoices = FacturaeProjector.Project(document.Facturae);

            Checks = new ObservableCollection<ValidationCheck>(report.Checks);
            FileName = Path.GetFileName(path);
            SchemaVersion = $"FacturaE {document.SchemaVersion}";
            TotalInvoices = _invoices.Count;
            HasDocument = true;

            OverallStatus = report.IsValid
                ? (report.HasWarnings ? CheckStatus.Warning : CheckStatus.Passed)
                : CheckStatus.Error;
            DocumentStateText = report.IsValid
                ? (report.HasWarnings ? "Válido con avisos" : "Válido")
                : "Inválido";
            SummaryText = $"{report.PassedCount} correctas · {report.WarningCount} avisos · {report.ErrorCount} errores";

            _recent.Add(path);
            RefreshRecentFiles();

            CurrentIndex = 0;
            UpdateCurrentInvoice();
            return true;
        }
        catch (Exception ex) when (ex is FacturaeParseException or IOException or UnauthorizedAccessException)
        {
            Reset();
            _dialogs.ShowError("No se pudo abrir el fichero", ex.Message);
            return false;
        }
    }

    public void Reset()
    {
        _invoices = new List<InvoiceDisplay>();
        Checks = new ObservableCollection<ValidationCheck>();
        FileName = string.Empty;
        SchemaVersion = string.Empty;
        DocumentStateText = "Sin documento";
        SummaryText = "Arrastre un fichero .xsig, .xpsig o .xml aquí o use «Abrir…».";
        OverallStatus = CheckStatus.Info;
        HasDocument = false;
        TotalInvoices = 0;
        CurrentIndex = 0;
        UpdateCurrentInvoice();
    }

    partial void OnCurrentIndexChanged(int value) => UpdateCurrentInvoice();

    /// <summary>
    /// Asigna la factura actual según el índice y refresca los comandos.
    /// Se llama también desde Load/Reset porque asignar un índice repetido
    /// (p. ej. 0 → 0) no dispara OnCurrentIndexChanged.
    /// </summary>
    private void UpdateCurrentInvoice()
    {
        CurrentInvoice = _invoices.Count > CurrentIndex ? _invoices[CurrentIndex] : null;
        GoPreviousCommand.NotifyCanExecuteChanged();
        GoNextCommand.NotifyCanExecuteChanged();
        ExportPdfCommand.NotifyCanExecuteChanged();
        PrintCommand.NotifyCanExecuteChanged();
    }

    private bool CanGoPrevious() => CurrentIndex > 0;

    private bool CanGoNext() => CurrentIndex < TotalInvoices - 1;

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void GoPrevious()
    {
        if (CanGoPrevious())
            CurrentIndex--;
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void GoNext()
    {
        if (CanGoNext())
            CurrentIndex++;
    }

    private bool CanExportOrPrint() => CurrentInvoice is not null;

    [RelayCommand(CanExecute = nameof(CanExportOrPrint))]
    private void ExportPdf()
    {
        if (CurrentInvoice is null)
            return;

        var suggested = string.IsNullOrEmpty(CurrentInvoice.SeriesCode)
            ? $"Factura_{CurrentInvoice.InvoiceNumber}.pdf"
            : $"Factura_{CurrentInvoice.SeriesCode}_{CurrentInvoice.InvoiceNumber}.pdf";

        try
        {
            var path = _pdf.SaveInvoicePdf(CurrentInvoice, suggested);
            if (path is not null)
                _dialogs.ShowInfo("Exportación completada", $"La factura se ha guardado en:\n{path}");
        }
        catch (Exception ex)
        {
            _dialogs.ShowError("No se pudo exportar el PDF", ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExportOrPrint))]
    private async Task Print()
    {
        if (CurrentInvoice is null)
            return;

        string? tempPath = null;
        try
        {
            tempPath = _pdf.CreateTempPdf(CurrentInvoice);
            var preview = new PdfPreviewWindow(tempPath) { Owner = System.Windows.Application.Current.MainWindow };
            preview.ShowDialog();
        }
        catch (Exception ex)
        {
            _dialogs.ShowError("No se pudo preparar la impresión", ex.Message);
        }
        finally
        {
            if (tempPath is not null)
            {
                try { File.Delete(tempPath); } catch (IOException) { }
            }
        }
    }

    private void RefreshRecentFiles()
    {
        RecentFiles = new ObservableCollection<string>(
            _recent.Get().Select(RecentFilesService.SafePath));
        OnPropertyChanged(nameof(RecentFiles));
    }

    /// <summary>Para el servicio de single-instance (Task de escucha) al salir.</summary>
    public void Shutdown()
    {
        SingleInstance.Stop();
    }

    /// <summary>Borra la lista de ficheros recientes (opción de línea de comandos).</summary>
    [RelayCommand]
    private void ClearRecentFiles()
    {
        _recent.RemoveAll();
        RefreshRecentFiles();
    }
}