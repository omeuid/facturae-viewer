// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using Xunit;
using FacturaeViewer.Core.IO;
using FacturaeViewer.Core.Validation;

namespace Facturae.Tests;

public class NifValidatorTests
{
    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    [Theory]
    [InlineData("12345678Z")]
    [InlineData("X0000000T")]
    [InlineData("K0000000T")]
    public void IsValidNif_acepta_identificadores_correctos(string nif)
    {
        Assert.True(NifValidator.IsValidNif(nif));
    }

    [Theory]
    [InlineData("12345678T")]
    [InlineData("1234567Z")]
    [InlineData("ABCDEFGHZ")]
    public void IsValidNif_rechaza_identificadores_incorrectos(string nif)
    {
        Assert.False(NifValidator.IsValidNif(nif));
    }

    [Theory]
    [InlineData("B28015865")]
    [InlineData("A28015865")]
    [InlineData("B12345674")]
    public void IsValidCif_acepta_cifs_correctos(string cif)
    {
        Assert.True(NifValidator.IsValidCif(cif));
    }

    [Theory]
    [InlineData("B28015866")]
    [InlineData("B12345679")]
    [InlineData("12345678Z")]
    [InlineData("X0000000T")]
    public void IsValidCif_rechaza_cifs_incorrectos(string cif)
    {
        Assert.False(NifValidator.IsValidCif(cif));
    }

    [Theory]
    [InlineData("Facturae-3.2-valid.xml")]
    [InlineData("Facturae-3.2.1-valid.xml")]
    [InlineData("Facturae-3.2.2-valid.xml")]
    [InlineData("Facturae-3.2.2-lote-valid.xml")]
    public void Fixtures_validos_tienen_nifs_validos(string file)
    {
        var doc = FacturaeLoader.Load(Fixture(file));
        var report = NifValidator.Validate(doc);

        Assert.True(report.IsValid, string.Join("\n", report.Checks.Select(c => c.ToString())));
        Assert.DoesNotContain(report.Checks, c => c.Code == "NIF" && c.Status == CheckStatus.Error);
    }

    [Fact]
    public void Fixture_con_cif_invalido_genera_error()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.2.2-nif-invalido.xml"));
        var report = NifValidator.Validate(doc);

        Assert.True(report.HasErrors);
        Assert.Contains(report.Checks, c => c.Code == "NIF" && c.Status == CheckStatus.Error);
    }
}