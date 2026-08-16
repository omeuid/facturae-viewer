// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Facturae.App.Converters;

/// <summary>
/// Convierte un booleano en cursor: true → <see cref="Cursors.Hand"/>,
/// false → <see cref="Cursors.Arrow"/>.
/// </summary>
public sealed class BoolToCursorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Cursors.Hand : Cursors.Arrow;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}