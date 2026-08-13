// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.IO;
using System.Windows;
using System.Windows.Threading;
using Facturae.App.Services;
using Facturae.App.ViewModels;

namespace Facturae.App;

public partial class App : Application
{
    private const string HelpText =
        "Uso: FacturaeViewer [opciones] [fichero]\n" +
        "\n" +
        "Opciones:\n" +
        "  --help, -h   Muestra esta ayuda y sale.\n" +
        "  --clear      Borra la lista de ficheros recientes.\n" +
        "  [fichero]    Ruta de un fichero .xsig, .xpsig o .xml para abrir.\n" +
        "\n" +
        "Sin argumentos, abre la ventana vacía del visor.";

    private MainViewModel? _viewModel;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Opciones de consola sin abrir la ventana.
        if (e.Args.Any(a => a is "--help" or "-h"))
        {
            WriteLine(HelpText);
            Shutdown();
            return;
        }
        if (e.Args.Contains("--clear"))
        {
            new RecentFilesService().RemoveAll();
            WriteLine("Lista de ficheros recientes borrada.");
            Shutdown();
            return;
        }

        var fileArg = e.Args.FirstOrDefault(a => !a.StartsWith('-'));
        if (!SingleInstance.TryAcquire(fileArg, OnFileRequested))
        {
            // Otra instancia ya está abierta; la ruta se le ha enviado.
            Shutdown();
            return;
        }

        var dialogService = new DialogService();
        var pdfService = new PdfService();
        var recent = new RecentFilesService();

        _viewModel = new MainViewModel(dialogService, pdfService, recent)
        {
            RecentFiles = recent.Get().Select(RecentFilesService.SafePath).ToList(),
        };

        var window = new MainWindow(_viewModel);
        MainWindow = window;
        window.Show();

        if (fileArg is not null)
            _viewModel.Load(fileArg);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _viewModel?.Shutdown();
        base.OnExit(e);
    }

    private void OnFileRequested(string path)
    {
        if (_viewModel is not null && File.Exists(path))
            _viewModel.Load(path);
    }

    private static void WriteLine(string message)
    {
        Console.WriteLine(message);
    }
}