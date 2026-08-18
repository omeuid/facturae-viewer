// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using FacturaeViewer.Core.Model;

namespace Facturae.App.Services;

/// <summary>
/// Comprueba actualizaciones consultando la API de releases de GitHub del
/// repositorio omeuid/facturae-viewer y descarga el instalador
/// (FacturaeViewer-Setup.exe) de la última release.
/// </summary>
public sealed class UpdateService : IUpdateService
{
    private const string ReleasesUrl = "https://api.github.com/repos/omeuid/facturae-viewer/releases/latest";
    private const string UserAgent = "FacturaeViewer/1.0";

    private readonly HttpClient _http;

    public UpdateService()
    {
        _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15),
        };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        _http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    }

    /// <summary>
    /// Versión instalada, leída del ensamblado (Directory.Build.props establece
    /// la propiedad Version del paquete).
    /// </summary>
    public ReleaseVersion CurrentVersion
        => ReleaseVersion.TryParse(
            Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion)
            ?? new ReleaseVersion(1, 0, 0);

    public async Task<ReleaseInfo?> CheckForUpdatesAsync()
    {
        using var response = await _http.GetAsync(ReleasesUrl);
        if (!response.IsSuccessStatusCode)
            return null;

        string json = await response.Content.ReadAsStringAsync();
        ReleaseInfo? release;
        try
        {
            release = ReleaseInfo.FromJson(json);
        }
        catch (JsonException)
        {
            return null;
        }

        return release is not null && release.Version.IsNewerThan(CurrentVersion) ? release : null;
    }

    public async Task<string> DownloadInstallerAsync(ReleaseInfo release)
    {
        if (release.InstallerUrl is null)
            throw new InvalidOperationException("La release no tiene instalador.");

        using var response = await _http.GetAsync(release.InstallerUrl);
        response.EnsureSuccessStatusCode();

        string tempPath = Path.Combine(Path.GetTempPath(), $"FacturaeViewer-Setup-{release.Version}.exe");
        using var stream = await response.Content.ReadAsStreamAsync();
        using var file = File.Create(tempPath);
        await stream.CopyToAsync(file);
        return tempPath;
    }
}