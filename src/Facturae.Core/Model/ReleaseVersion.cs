// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

namespace FacturaeViewer.Core.Model;

/// <summary>
/// Versión semántica simplificada (Major.Minor.Patch), usada para comparar la
/// versión instalada con la de la última release de GitHub. Acepta el prefijo
/// "v" de los tags ("v1.0.0") y compara numéricamente sin considerar
/// pre-releases (una versión con pre-release se trata como la versión base).
/// </summary>
public sealed record ReleaseVersion(int Major, int Minor, int Patch) : IComparable<ReleaseVersion>
{
    public int CompareTo(ReleaseVersion? other)
    {
        if (other is null)
            return 1;

        int compare = Major.CompareTo(other.Major);
        if (compare != 0)
            return compare;

        compare = Minor.CompareTo(other.Minor);
        if (compare != 0)
            return compare;

        return Patch.CompareTo(other.Patch);
    }

    /// <summary>Devuelve true si esta versión es más reciente que la dada.</summary>
    public bool IsNewerThan(ReleaseVersion? other) => other is null || CompareTo(other) > 0;

    /// <summary>
    /// Intenta analizar una cadena de versión como "1.2.3" o "v1.2.3".
    /// Devuelve null si no es un número de versión válido.
    /// </summary>
    public static ReleaseVersion? TryParse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        string candidate = text.Trim();
        if (candidate.StartsWith('v') || candidate.StartsWith('V'))
            candidate = candidate[1..];

        // Se descarta el sufijo de pre-release (p. ej. "1.0.0-rc1").
        int separator = candidate.IndexOfAny(['-', '+']);
        if (separator >= 0)
            candidate = candidate[..separator];

        string[] parts = candidate.Split('.');
        if (parts.Length < 1 || parts.Length > 3)
            return null;

        int[] numbers = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out int component) || component < 0)
                return null;
            numbers[i] = component;
        }

        return new ReleaseVersion(
            numbers[0],
            numbers.Length > 1 ? numbers[1] : 0,
            numbers.Length > 2 ? numbers[2] : 0);
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}