// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using Xunit;
using FacturaeViewer.Core.IO;
using FacturaeViewer.Core.Validation;

namespace Facturae.Tests;

public class TotalsValidatorTests
{
    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    [Theory]
    [InlineData("Facturae-3.2-valid.xml")]
    [InlineData("Facturae-3.2.1-valid.xml")]
    [InlineData("Facturae-3.2.2-valid.xml")]
    public void Fixtures_validos_tienen_totales_coherentes(string file)
    {
        var doc = FacturaeLoader.Load(Fixture(file));
        var report = TotalsValidator.Validate(doc);

        Assert.True(report.IsValid, string.Join("\n", report.Checks.Select(c => c.ToString())));
        Assert.Equal(0, report.ErrorCount);
    }

    [Fact]
    public void Lote_valido_tiene_totales_coherentes()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.2.2-lote-valid.xml"));
        var report = TotalsValidator.Validate(doc);

        Assert.True(report.IsValid, string.Join("\n", report.Checks.Select(c => c.ToString())));
        Assert.Equal(0, report.ErrorCount);
    }

    [Fact]
    public void Fixture_con_total_incorrecto_genera_error_TOT06()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.2.2-totales-incorrectos.xml"));
        var report = TotalsValidator.Validate(doc);

        Assert.True(report.HasErrors);
        Assert.Contains(report.Checks, c => c.Code == "TOT-06" && c.Status == CheckStatus.Error);
    }
}