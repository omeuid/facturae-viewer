// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using Xunit;
using FacturaeViewer.Core.IO;
using FacturaeViewer.Core.Model;

namespace Facturae.Tests;

public class FacturaeLoaderTests
{
    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    [Theory]
    [InlineData("Facturae-3.2-valid.xml", "3.2", FacturaeNamespaces.NamespaceV3_2)]
    [InlineData("Facturae-3.2.1-valid.xml", "3.2.1", FacturaeNamespaces.NamespaceV3_2_1)]
    [InlineData("Facturae-3.2.2-valid.xml", "3.2.2", FacturaeNamespaces.NamespaceV3_2_2)]
    public void Carga_por_fichero_detecta_version_y_namespace(string file, string version, string ns)
    {
        var doc = FacturaeLoader.Load(Fixture(file));

        Assert.Equal(version, doc.SchemaVersion);
        Assert.Equal(ns, doc.RootNamespace);
        Assert.NotNull(doc.Facturae);
        Assert.Single(doc.Facturae.Invoices!);
    }

    [Fact]
    public void Carga_fixture_32_es_individual_sin_lote()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.2-valid.xml"));

        Assert.Equal("I", doc.Facturae.FileHeader?.Modality);
        Assert.NotNull(doc.Facturae.FileHeader?.Batch);
        Assert.Equal(1, doc.Facturae.FileHeader?.Batch?.InvoicesCount);
    }

    [Fact]
    public void Carga_lote_con_dos_facturas()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.2.2-lote-valid.xml"));

        var batch = doc.Facturae.FileHeader?.Batch;
        Assert.NotNull(batch);
        Assert.Equal("L", doc.Facturae.FileHeader!.Modality);
        Assert.Equal("LOTE-2026-001", batch!.BatchIdentifier);
        Assert.Equal(2, batch.InvoicesCount);
        Assert.Equal(242.00m, batch.TotalInvoicesAmount?.TotalAmount);
        Assert.Equal(2, doc.Facturae.Invoices!.Length);
    }

    [Fact]
    public void Carga_deserializa_las_lineas_de_la_factura()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.2-valid.xml"));

        var lines = doc.Facturae.Invoices![0].Items!;
        Assert.Equal(2, lines.Length);
        Assert.Equal("Flores de jara y brezo", lines[0].ItemDescription);
        Assert.Equal(33.75m, lines[0].GrossAmount);
        Assert.Equal(26.00m, lines[1].GrossAmount);
        Assert.Equal(66.03m, doc.Facturae.Invoices[0].InvoiceTotals!.InvoiceTotal);
    }

    [Fact]
    public void Carga_fichero_inexistente_lanza_excepcion()
    {
        Assert.Throws<FileNotFoundException>(() =>
            FacturaeLoader.Load(Fixture("no-existe.xml")));
    }

    [Fact]
    public void Carga_fixture_31_firmado_detecta_version_y_namespace()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.1-firmada-real.xsig.xml"));

        Assert.Equal("3.1", doc.SchemaVersion);
        Assert.Equal(FacturaeNamespaces.NamespaceV3_1, doc.RootNamespace);
        Assert.NotNull(doc.Facturae);
        Assert.Single(doc.Facturae.Invoices!);
    }
}