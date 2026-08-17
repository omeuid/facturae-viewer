// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Xml;
using Xunit;
using FacturaeViewer.Core.IO;
using FacturaeViewer.Core.Xml;

namespace Facturae.Tests;

public class XmlFormattingTests
{
    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    [Theory]
    [InlineData("Facturae-3.2-valid.xml")]
    [InlineData("Facturae-3.2.2-lote-valid.xml")]
    [InlineData("Facturae-3.1-firmada-real.xsig.xml")]
    public void El_formateo_produce_un_XML_bien_formado_con_mismo_contenido(string file)
    {
        var original = new XmlDocument { PreserveWhitespace = true };
        original.Load(Fixture(file));

        string formatted = XmlFormatting.Format(original);
        Assert.False(string.IsNullOrWhiteSpace(formatted));
        Assert.Contains("Facturae", formatted);

        var reparsed = new XmlDocument();
        reparsed.LoadXml(formatted);

        Assert.Equal(original.DocumentElement!.LocalName, reparsed.DocumentElement!.LocalName);
        Assert.Equal(original.DocumentElement.NamespaceURI, reparsed.DocumentElement.NamespaceURI);
        Assert.Equal(CountElements(original.DocumentElement), CountElements(reparsed.DocumentElement));
    }

    private static int CountElements(XmlElement element)
        => element.ChildNodes.OfType<XmlElement>().Count();

    [Fact]
    public void El_formateo_indenta_el_contenido()
    {
        var original = new XmlDocument { PreserveWhitespace = true };
        original.Load(Fixture("Facturae-3.2.2-valid.xml"));

        string formatted = XmlFormatting.Format(original);

        Assert.Contains("\r\n  ", formatted);
        Assert.Contains("<Invoice>", formatted);
    }

    [Fact]
    public void El_indice_de_lineas_localiza_los_elementos()
    {
        var doc = FacturaeLoader.Load(Fixture("Facturae-3.2.2-valid.xml"));
        string formatted = XmlFormatting.Format(doc.Xml);
        var lines = XmlFormatting.IndexElementLines(formatted);

        Assert.True(lines.TryGetValue("InvoiceHeader", out int line));
        Assert.True(line >= 0 && line < formatted.Split('\n').Length);
        Assert.True(lines.ContainsKey("InvoiceTotals"));
        Assert.True(lines.ContainsKey("TaxIdentification"));
    }

    [Fact]
    public void El_indice_de_lineas_devuelve_coleccion_vacia_para_vacio()
    {
        Assert.Empty(XmlFormatting.IndexElementLines(string.Empty));
    }
}