// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Windows;
using Microsoft.Win32;

namespace Facturae.App.Services;

/// <summary>Diálogos nativos de la aplicación.</summary>
public sealed class DialogService : IDialogService
{
    public string? OpenFacturaeFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Abrir factura electrónica FacturaE",
            Filter = "Ficheros FacturaE (*.xsig;*.xpsig;*.xml)|*.xsig;*.xpsig;*.xml|Todos los archivos (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public void ShowError(string title, string message)
        => MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public void ShowInfo(string title, string message)
        => MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
}