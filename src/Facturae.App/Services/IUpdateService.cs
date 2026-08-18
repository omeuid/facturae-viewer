// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using FacturaeViewer.Core.Model;

namespace Facturae.App.Services;

/// <summary>
/// Comprueba y descarga actualizaciones de la aplicación desde las releases
/// de GitHub. Abstraído como interfaz para poder probar el ViewModel.
/// </summary>
public interface IUpdateService
{
    /// <summary>Versión de la aplicación instalada.</summary>
    ReleaseVersion CurrentVersion { get; }

    /// <summary>
    /// Consulta la última release publicada. Devuelve la información de la
    /// release si es más reciente que la versión instalada, o null si no hay
    /// actualización disponible.
    /// </summary>
    Task<ReleaseInfo?> CheckForUpdatesAsync();

    /// <summary>Descarga el instalador de la release a un fichero temporal y devuelve su ruta.</summary>
    Task<string> DownloadInstallerAsync(ReleaseInfo release);
}
