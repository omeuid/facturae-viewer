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
    private IReadOnlyList<InvoiceDisplay> _invoices = new List<InvoiceDisplay>();

    public MainViewModel(IDialogService dialogs)
    {
        _dialogs = dialogs;
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

            CurrentIndex = 0;
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
        CurrentInvoice = null;
        FileName = string.Empty;
        SchemaVersion = string.Empty;
        DocumentStateText = "Sin documento";
        SummaryText = "Arrastre un fichero .xsig, .xpsig o .xml aquí o use «Abrir…».";
        OverallStatus = CheckStatus.Info;
        HasDocument = false;
        TotalInvoices = 0;
        CurrentIndex = 0;
    }

    partial void OnCurrentIndexChanged(int value)
    {
        CurrentInvoice = _invoices.Count > value ? _invoices[value] : null;
        GoPreviousCommand.NotifyCanExecuteChanged();
        GoNextCommand.NotifyCanExecuteChanged();
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
}