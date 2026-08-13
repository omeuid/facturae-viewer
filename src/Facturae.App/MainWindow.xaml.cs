// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Windows;
using Facturae.App.ViewModels;

namespace Facturae.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    protected override void OnDragOver(DragEventArgs e)
    {
        _viewModel.IsDragOver = e.Data.GetDataPresent(DataFormats.FileDrop);
        e.Effects = _viewModel.IsDragOver ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    protected override void OnDragLeave(DragEventArgs e)
    {
        _viewModel.IsDragOver = false;
        e.Handled = true;
    }

    protected override void OnDrop(DragEventArgs e)
    {
        _viewModel.IsDragOver = false;
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            _viewModel.Load(files[0]);
        e.Handled = true;
    }
}