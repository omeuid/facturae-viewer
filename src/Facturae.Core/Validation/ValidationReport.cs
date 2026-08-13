// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

namespace FacturaeViewer.Core.Validation;

/// <summary>Resultado de una comprobación individual de validación.</summary>
public enum CheckStatus
{
    /// <summary>La comprobación se cumplió correctamente.</summary>
    Passed,

    /// <summary>Aviso: no invalida el documento, pero conviene revisarlo.</summary>
    Warning,

    /// <summary>Error: el documento no es válido.</summary>
    Error,

    /// <summary>Informativo: no aplica veredicto.</summary>
    Info,
}

/// <summary>Resultado de una comprobación individual de validación.</summary>
public sealed record ValidationCheck(string Code, CheckStatus Status, string Message, string? Detail = null)
{
    public override string ToString() => $"[{Status}] {Code}: {Message}";
}

/// <summary>
/// Informe de validación: lista de comprobaciones y resumen.
/// </summary>
public sealed class ValidationReport
{
    public List<ValidationCheck> Checks { get; } = [];

    public bool HasErrors => Checks.Any(c => c.Status == CheckStatus.Error);

    public bool HasWarnings => Checks.Any(c => c.Status == CheckStatus.Warning);

    public bool IsValid => !HasErrors;

    public int ErrorCount => Checks.Count(c => c.Status == CheckStatus.Error);

    public int WarningCount => Checks.Count(c => c.Status == CheckStatus.Warning);

    public int PassedCount => Checks.Count(c => c.Status == CheckStatus.Passed);

    public void Add(string code, CheckStatus status, string message, string? detail = null)
        => Checks.Add(new ValidationCheck(code, status, message, detail));

    public void AddPassed(string code, string message, string? detail = null)
        => Add(code, CheckStatus.Passed, message, detail);

    public void AddWarning(string code, string message, string? detail = null)
        => Add(code, CheckStatus.Warning, message, detail);

    public void AddError(string code, string message, string? detail = null)
        => Add(code, CheckStatus.Error, message, detail);

    /// <summary>Fusiona los chequeos de otro informe en este.</summary>
    public void Merge(ValidationReport other)
        => Checks.AddRange(other.Checks);
}