// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using FacturaeViewer.Core.Validation;

namespace Facturae.App.Converters;

/// <summary>
/// Convierte un <see cref="CheckStatus"/> en un pincel de color para la UI.
/// </summary>
public sealed class StatusToBrushConverter : IValueConverter
{
    public Brush PassedBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
    public Brush WarningBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0xEF, 0x6C, 0x00));
    public Brush ErrorBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
    public Brush InfoBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is CheckStatus status
            ? status switch
            {
                CheckStatus.Passed => PassedBrush,
                CheckStatus.Warning => WarningBrush,
                CheckStatus.Error => ErrorBrush,
                _ => InfoBrush,
            }
            : InfoBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}