// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics;
using System.Windows;
using Facturae.App.Services;
using FacturaeViewer.Core.Model;

namespace Facturae.App.Views;

/// <summary>
/// Diálogo que informa de una actualización disponible y permite descargar e
/// ejecutar el instalador de la nueva versión.
/// </summary>
public partial class UpdateDialog : Window
{
    private readonly IUpdateService _updates;
    private readonly ReleaseInfo _release;

    public UpdateDialog(IUpdateService updates, ReleaseInfo release, string currentVersion)
    {
        InitializeComponent();
        _updates = updates;
        _release = release;
        VersionText.Text = $"Versión instalada: {currentVersion}   →   Nueva versión: {release.Version}";
        NotesTextBox.Text = string.IsNullOrWhiteSpace(release.Notes)
            ? "(La release no incluye notas.)"
            : release.Notes;
        NotesTextBox.Height = 0;
    }

    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        DownloadButton.IsEnabled = false;
        StatusText.Text = "Descargando el instalador…";

        try
        {
            string installerPath = await _updates.DownloadInstallerAsync(_release);
            StatusText.Text = "Descargado. Cerrando el visor para instalar…";

            // El instalador de Inno Setup se ejecuta y cierra la app si está abierta.
            Process.Start(new ProcessStartInfo(installerPath) { UseShellExecute = true });

            // Se cierra el diálogo; la aplicación principal seguirá abierta hasta
            // que el instalador la detenga al reemplazar los ficheros.
            Close();
        }
        catch (Exception ex)
        {
            DownloadButton.IsEnabled = true;
            StatusText.Text = "No se pudo descargar la actualización.";
            MessageBox.Show(this, ex.Message, "Actualización", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}