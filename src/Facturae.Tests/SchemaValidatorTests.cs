// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using Xunit;
using FacturaeViewer.Core.IO;
using FacturaeViewer.Core.Validation;

namespace Facturae.Tests;

public class SchemaValidatorTests
{
    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    [Theory]
    [InlineData("Facturae-3.2-valid.xml")]
    [InlineData("Facturae-3.2.1-valid.xml")]
    [InlineData("Facturae-3.2.2-valid.xml")]
    [InlineData("Facturae-3.2.2-lote-valid.xml")]
    public void Fixtures_validos_son_conformes_al_esquema(string file)
    {
        var doc = FacturaeLoader.Load(Fixture(file));
        var report = SchemaValidator.Validate(doc);

        Assert.True(report.IsValid, string.Join("\n", report.Checks.Select(c => c.ToString())));
        Assert.Contains(report.Checks, c => c.Code == "SCHEMA" && c.Status == CheckStatus.Passed);
    }

    [Theory]
    [InlineData("Facturae-3.2.2-totales-incorrectos.xml")]
    [InlineData("Facturae-3.2.2-nif-invalido.xml")]
    public void Fixtures_invalidos_de_reglas_siguen_siendo_conformes_al_esquema(string file)
    {
        var doc = FacturaeLoader.Load(Fixture(file));
        var report = SchemaValidator.Validate(doc);

        Assert.True(report.IsValid, string.Join("\n", report.Checks.Select(c => c.ToString())));
    }

    [Fact]
    public void Fixture_sin_campo_obligatorio_genera_error_de_esquema()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.2.2-esquema-invalido.xml"));
        var report = SchemaValidator.Validate(doc);

        Assert.True(report.HasErrors);
        Assert.Contains(report.Checks, c => c.Code == "SCHEMA" && c.Status == CheckStatus.Error);
    }
}