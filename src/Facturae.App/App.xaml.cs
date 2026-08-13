// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Windows;
using Facturae.App.Services;
using Facturae.App.ViewModels;

namespace Facturae.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var viewModel = new MainViewModel(new DialogService(), new PdfService());
        var window = new MainWindow(viewModel);
        MainWindow = window;
        window.Show();

        // Apertura opcional desde línea de comandos (uso completo en la fase 6).
        if (e.Args.Length > 0)
            viewModel.Load(e.Args[0]);
    }
}