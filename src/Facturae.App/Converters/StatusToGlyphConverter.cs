// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Globalization;
using System.Windows.Data;
using FacturaeViewer.Core.Validation;

namespace Facturae.App.Converters;

/// <summary>
/// Convierte un <see cref="CheckStatus"/> en un símbolo legible.
/// </summary>
public sealed class StatusToGlyphConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is CheckStatus status
            ? status switch
            {
                CheckStatus.Passed => "✓",
                CheckStatus.Warning => "⚠",
                CheckStatus.Error => "✕",
                _ => "ℹ",
            }
            : "ℹ";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}