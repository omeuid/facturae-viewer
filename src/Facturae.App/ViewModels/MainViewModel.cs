// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
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
    private string _rawXml = string.Empty;

    /// <summary>Índice de línea (base 0) de cada elemento del XML formateado, por nombre local.</summary>
    private IReadOnlyDictionary<string, int> _xmlElementLines = new Dictionary<string, int>();

    [ObservableProperty]
    private int _selectedTabIndex;

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

    /// <summary>Índice de la factura mostrada en la navegación, en base 1 (0 cuando no hay documento).</summary>
    public int CurrentDisplayIndex => TotalInvoices > 0 ? CurrentIndex + 1 : 0;

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
            RawXml = FormatXml(document.Xml);
            _xmlElementLines = IndexElementLines(RawXml);
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
        RawXml = string.Empty;
        _xmlElementLines = new Dictionary<string, int>();
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

    partial void OnTotalInvoicesChanged(int value) => OnPropertyChanged(nameof(CurrentDisplayIndex));

    /// <summary>
    /// Asigna la factura actual según el índice y refresca los comandos.
    /// Se llama también desde Load/Reset porque asignar un índice repetido
    /// (p. ej. 0 → 0) no dispara OnCurrentIndexChanged.
    /// </summary>
    private void UpdateCurrentInvoice()
    {
        CurrentInvoice = _invoices.Count > CurrentIndex ? _invoices[CurrentIndex] : null;
        OnPropertyChanged(nameof(CurrentDisplayIndex));
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

    /// <summary>
    /// Cambia a la pestaña «XML» y desplaza el cursor al elemento que origina
    /// un chequeo de validación, si se conoce.
    /// </summary>
    [RelayCommand]
    private void NavigateToCheck(ValidationCheck check)
    {
        if (check is null || !check.CanNavigate || !HasDocument)
            return;

        if (check.TargetElement is string target && _xmlElementLines.TryGetValue(target, out int line))
        {
            SelectedTabIndex = 1;
            XmlScrollToLine?.Invoke(line);
        }
    }

    /// <summary>Petición de la vista para desplazarse a una línea del XML (base 0).</summary>
    public event Action<int>? XmlScrollToLine;

    /// <summary>
    /// Serializa el documento XML con indentación legible para mostrarlo en la
    /// pestaña «XML». Se parte del OuterXml para descartar el whitespace
    /// original (el documento cargado preserva el formato del fichero).
    /// </summary>
    private static string FormatXml(XmlDocument xml)
    {
        var clean = new XmlDocument { PreserveWhitespace = false };
        clean.LoadXml(xml.OuterXml);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = true,
            NewLineChars = Environment.NewLine,
        };

        using var sw = new StringWriter();
        using (var writer = XmlWriter.Create(sw, settings))
            clean.Save(writer);
        return sw.ToString();
    }

    /// <summary>
    /// Calcula la línea (base 0) en el XML formateado donde empieza cada
    /// elemento, indexada por su nombre local. Último ganador: en documentos
    /// con elementos repetidos (p. ej. varios Invoice) se guarda el último.
    /// </summary>
    private static IReadOnlyDictionary<string, int> IndexElementLines(string xml)
    {
        var lines = new Dictionary<string, int>();
        if (string.IsNullOrWhiteSpace(xml))
            return lines;

        var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit };
        using var reader = XmlReader.Create(new StringReader(xml), settings);
        if (reader is not System.Xml.IXmlLineInfo lineInfo || !lineInfo.HasLineInfo())
            return lines;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
                lines.TryAdd(reader.LocalName, lineInfo.LineNumber - 1);
        }

        return lines;
    }
}