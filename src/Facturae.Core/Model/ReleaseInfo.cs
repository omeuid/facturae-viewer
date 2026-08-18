// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Text.Json;

namespace FacturaeViewer.Core.Model;

/// <summary>
/// Información de la última release de Facturae Viewer disponible en GitHub,
/// con los datos necesarios para ofrecer una actualización: versión, notas,
/// dirección del instalador y página de la release.
/// </summary>
public sealed record ReleaseInfo(
    ReleaseVersion Version,
    string Name,
    string Notes,
    string? InstallerUrl,
    string? HtmlUrl)
{
    /// <summary>
    /// Analiza la respuesta JSON de la API "GET /repos/.../releases/latest" de
    /// GitHub y devuelve los datos de la release, o null si la respuesta no es
    /// una release válida o no tiene instalador.
    /// </summary>
    public static ReleaseInfo? FromJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("tag_name", out var tag))
            return null;

        var version = ReleaseVersion.TryParse(tag.GetString());
        if (version is null)
            return null;

        string name = root.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? string.Empty : string.Empty;
        string notes = root.TryGetProperty("body", out var bodyElement) ? bodyElement.GetString() ?? string.Empty : string.Empty;
        string? htmlUrl = root.TryGetProperty("html_url", out var urlElement) ? urlElement.GetString() : null;

        string? installerUrl = null;
        if (root.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
        {
            foreach (var asset in assets.EnumerateArray())
            {
                if (!asset.TryGetProperty("name", out var assetName))
                    continue;

                string assetNameText = assetName.GetString() ?? string.Empty;
                if (!assetNameText.EndsWith("-Setup.exe", StringComparison.OrdinalIgnoreCase))
                    continue;

                installerUrl = asset.TryGetProperty("browser_download_url", out var downloadUrl)
                    ? downloadUrl.GetString()
                    : null;
                break;
            }
        }

        if (string.IsNullOrEmpty(installerUrl))
            return null;

        return new ReleaseInfo(version, name, notes, installerUrl, htmlUrl);
    }
}