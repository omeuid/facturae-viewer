// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using Xunit;
using FacturaeViewer.Core.Model;

namespace Facturae.Tests;

public class ReleaseInfoTests
{
    [Theory]
    [InlineData("1.2.3", 1, 2, 3)]
    [InlineData("v1.2.3", 1, 2, 3)]
    [InlineData("V1.2.3", 1, 2, 3)]
    [InlineData("1.2", 1, 2, 0)]
    [InlineData("1", 1, 0, 0)]
    [InlineData("1.0.0-rc1", 1, 0, 0)]
    [InlineData("1.0.0+build.5", 1, 0, 0)]
    public void TryParse_acepta_formatos_validos(string text, int major, int minor, int patch)
    {
        var version = ReleaseVersion.TryParse(text);

        Assert.NotNull(version);
        Assert.Equal(new ReleaseVersion(major, minor, patch), version);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    [InlineData("abc")]
    [InlineData("1.2.3.4")]
    [InlineData("1.x.3")]
    public void TryParse_rechaza_formatos_invalidos(string? text)
    {
        Assert.Null(ReleaseVersion.TryParse(text));
    }

    [Fact]
    public void IsNewerThan_compara_por_mayor_menor_y_patch()
    {
        Assert.True(new ReleaseVersion(2, 0, 0).IsNewerThan(new ReleaseVersion(1, 9, 9)));
        Assert.True(new ReleaseVersion(1, 1, 0).IsNewerThan(new ReleaseVersion(1, 0, 9)));
        Assert.True(new ReleaseVersion(1, 0, 1).IsNewerThan(new ReleaseVersion(1, 0, 0)));
        Assert.False(new ReleaseVersion(1, 0, 0).IsNewerThan(new ReleaseVersion(1, 0, 0)));
        Assert.False(new ReleaseVersion(1, 0, 0).IsNewerThan(new ReleaseVersion(1, 0, 1)));
        Assert.True(new ReleaseVersion(1, 0, 0).IsNewerThan(null));
    }

    private const string ReleaseJson = """
    {
      "tag_name": "v1.1.0",
      "name": "v1.1.0",
      "body": "Novedades de la versión 1.1.\n\n- Mejora A\n- Mejora B",
      "html_url": "https://github.com/omeuid/facturae-viewer/releases/tag/v1.1.0",
      "assets": [
        {
          "name": "FacturaeViewer.exe",
          "browser_download_url": "https://github.com/omeuid/facturae-viewer/releases/download/v1.1.0/FacturaeViewer.exe"
        },
        {
          "name": "FacturaeViewer-Setup-1.1.0.exe",
          "browser_download_url": "https://github.com/omeuid/facturae-viewer/releases/download/v1.1.0/FacturaeViewer-Setup-1.1.0.exe"
        }
      ]
    }
    """;

    [Fact]
    public void FromJson_extrae_version_notas_e_instalador()
    {
        var release = ReleaseInfo.FromJson(ReleaseJson);

        Assert.NotNull(release);
        Assert.Equal(new ReleaseVersion(1, 1, 0), release.Version);
        Assert.Contains("Mejora A", release.Notes);
        Assert.Equal("https://github.com/omeuid/facturae-viewer/releases/download/v1.1.0/FacturaeViewer-Setup-1.1.0.exe",
            release.InstallerUrl);
        Assert.EndsWith("releases/tag/v1.1.0", release.HtmlUrl);
    }

    [Fact]
    public void FromJson_devuelve_null_sin_tag_valido()
    {
        Assert.Null(ReleaseInfo.FromJson("""{ "name": "sin versión" }"""));
    }

    [Fact]
    public void FromJson_devuelve_null_si_la_release_no_tiene_instalador()
    {
        const string json = """
        {
          "tag_name": "v1.1.0",
          "assets": [ { "name": "FacturaeViewer.exe", "browser_download_url": "..." } ]
        }
        """;

        Assert.Null(ReleaseInfo.FromJson(json));
    }
}