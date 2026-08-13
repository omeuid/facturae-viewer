// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.IO;
using Microsoft.Win32;
using FacturaeViewer.Core.Model;

namespace Facturae.App.Services;

/// <summary>
/// Gestión de la lista de ficheros recientes. Los datos se guardan en el
/// registro de la aplicación (HKCU), accesibles sin permisos de administrador.
/// </summary>
public sealed class RecentFilesService
{
    private const string RegistryPath = @"Software\FacturaeViewer";

    public const int MaxEntries = 10;

    /// <summary>Devuelve las rutas recientes más antiguas primero (más reciente la última).</summary>
    public IReadOnlyList<string> Get()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: false);
        if (key is null)
            return Array.Empty<string>();

        var values = key.GetValueNames()
            .Where(n => n.StartsWith("recent_", StringComparison.OrdinalIgnoreCase))
            .Select(n => key.GetValue(n)?.ToString() ?? string.Empty)
            .Where(v => !string.IsNullOrEmpty(v) && File.Exists(v))
            .ToList();

        return values;
    }

    /// <summary>Añade una ruta a la lista de recientes y poda las entradas antiguas.</summary>
    public void Add(string path)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            var values = Get().Where(v => !string.Equals(v, path, StringComparison.OrdinalIgnoreCase)).ToList();
            values.Add(path);
            if (values.Count > MaxEntries)
                values = values.Skip(values.Count - MaxEntries).ToList();

            for (int i = 0; i < values.Count; i++)
                key.SetValue($"recent_{i}", values[i]);
        }
        catch (Exception)
        {
            // El registro puede no estar disponible (entornos restringidos); no bloquea la app.
        }
    }

    /// <summary>Quita una ruta de la lista de recientes.</summary>
    public void Remove(string path)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
        var values = Get().Where(v => !string.Equals(v, path, StringComparison.OrdinalIgnoreCase)).ToList();
        for (int i = 0; i < values.Count; i++)
            key.SetValue($"recent_{i}", values[i]);
    }

    /// <summary>Borra todos los ficheros recientes.</summary>
    public void RemoveAll()
    {
        Registry.CurrentUser.DeleteSubKeyTree(RegistryPath, throwOnMissingSubKey: false);
    }

    public static string SafePath(string value) => value.Replace("_", "__").Replace(@"\", "/");
    public static string UnescapePath(string value) => value.Replace("/", @"\").Replace("__", "_");
}