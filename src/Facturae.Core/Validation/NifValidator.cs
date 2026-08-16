// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using FacturaeViewer.Core.Model;

namespace FacturaeViewer.Core.Validation;

/// <summary>
/// Valida el formato de los identificadores fiscales (NIF, NIE y CIF) de
/// emisor y receptor, según las reglas del Ministerio de Hacienda
/// (Orden HAP/1650/2015). Solo comprueba la forma del identificador: la
/// existencia del NIF en el censo de la AEAT no es comprobable offline.
/// </summary>
public static class NifValidator
{
    private const string ControlLetters = "TRWAGMYFPDXBNJZSQVHLCKE";

    /// <summary>Primeras letras válidas de un CIF.</summary>
    private static readonly HashSet<char> CifLetters = new("ABCDEFGHJNPQRSUVW");

    /// <summary>Letras de CIF cuyo carácter de control debe ser numérico.</summary>
    private static readonly HashSet<char> CifDigitControlLetters = new("ABCDEFGHJUV");

    private static readonly HashSet<char> NifSpecialPrefixes = new("KLMXYZ");

    public static ValidationReport Validate(FacturaeDocument document)
        => Validate(document.Facturae);

    public static ValidationReport Validate(Facturae facturae)
    {
        var report = new ValidationReport();
        var parties = facturae.Parties;

        if (parties?.SellerParty is not null)
            AddPartyCheck(report, "emisor", parties.SellerParty);

        if (parties?.BuyerParty is not null)
            AddPartyCheck(report, "receptor", parties.BuyerParty);

        if (parties?.SellerParty is null && parties?.BuyerParty is null)
            report.AddWarning("NIF", "El documento no declara partes intervinientes.");

        return report;
    }

    private static void AddPartyCheck(ValidationReport report, string rol, Party party)
    {
        var taxId = party.TaxIdentification;
        string nombre = party.LegalEntity?.CorporateName?.Trim()
            ?? ConcatName(party.Individual)
            ?? "<sin nombre>";

        if (taxId is null || string.IsNullOrWhiteSpace(taxId.TaxIdentificationNumber))
        {
            report.AddWarning("NIF", $"El {rol} ({nombre}) no declara un número de identificación fiscal.",
                targetElement: "TaxIdentification");
            return;
        }

        string countryCode = party.LegalEntity?.AddressInSpain?.CountryCode
            ?? party.LegalEntity?.OverseasAddress?.CountryCode
            ?? party.Individual?.AddressInSpain?.CountryCode
            ?? party.Individual?.OverseasAddress?.CountryCode
            ?? "ESP";

        var (status, detail) = Evaluate(taxId, countryCode);
        report.Add("NIF", status,
            status == CheckStatus.Passed
                ? $"El NIF del {rol} ({taxId.TaxIdentificationNumber.Trim()}) tiene un formato válido."
                : $"El NIF del {rol} ({taxId.TaxIdentificationNumber.Trim()}) no es válido.",
            detail,
            targetElement: "TaxIdentification");
    }

    private static (CheckStatus, string?) Evaluate(TaxIdentification taxId, string countryCode)
    {
        string raw = taxId.TaxIdentificationNumber?.Trim() ?? string.Empty;
        bool isCompany = string.Equals(taxId.PersonTypeCode, "J", StringComparison.OrdinalIgnoreCase);
        bool isForeignPerson = string.Equals(taxId.PersonTypeCode, "I", StringComparison.OrdinalIgnoreCase);
        bool esSpain = countryCode.Equals("ESP", StringComparison.OrdinalIgnoreCase)
            || raw.StartsWith("ES", StringComparison.OrdinalIgnoreCase);

        string id = raw.StartsWith("ES", StringComparison.OrdinalIgnoreCase) ? raw[2..].Trim() : raw;

        if (esSpain && !isForeignPerson)
        {
            bool ok = isCompany ? IsValidCif(id) : IsValidNif(id);
            return ok
                ? (CheckStatus.Passed, null)
                : (CheckStatus.Error, "El identificador no cumple las reglas de formación del NIF/CIF.");
        }

        // Identificador extranjero: si empieza por dos letras se asume que
        // son el código de país y el resto el NIF (Orden HAP/1650/2015).
        if (id.Length >= 2 && char.IsLetter(id[0]) && char.IsLetter(id[1]))
        {
            string rest = id[2..];
            if (rest.Length >= 3 && rest.All(char.IsLetterOrDigit))
                return (CheckStatus.Passed, null);
            return (CheckStatus.Warning, "Formato de NIF extranjero no reconocido.");
        }

        return (CheckStatus.Warning,
            "NIF de persona no residente o identificador extranjero: no comprobable sin el censo de la AEAT.");
    }

    /// <summary>
    /// Comprueba un NIF/NIE español (persona física): 8 dígitos + letra de
    /// control, o letra X/Y/Z/K/L/M + 7 dígitos + letra de control.
    /// </summary>
    public static bool IsValidNif(string nif)
    {
        nif = (nif ?? string.Empty).Trim().ToUpperInvariant();
        if (nif.Length != 9)
            return false;

        if (NifSpecialPrefixes.Contains(nif[0]) && nif.AsSpan(1, 7).IndexOfAnyExceptInDigitRange() is -1)
        {
            int baseValue = nif[0] switch { 'K' or 'X' => 0, 'L' or 'Y' => 1, _ => 2 };
            long num = baseValue * 10_000_000L + long.Parse(nif.AsSpan(1, 7));
            return nif[8] == ControlLetters[(int)(num % 23)];
        }

        if (nif.AsSpan(0, 8).IndexOfAnyExceptInDigitRange() is -1)
        {
            long num = long.Parse(nif.AsSpan(0, 8));
            return nif[8] == ControlLetters[(int)(num % 23)];
        }

        return false;
    }

    /// <summary>
    /// Comprueba un CIF (persona jurídica): letra + 7 dígitos + carácter de
    /// control (numérico o alfabético según la letra inicial).
    /// </summary>
    public static bool IsValidCif(string cif)
    {
        cif = (cif ?? string.Empty).Trim().ToUpperInvariant();
        if (cif.Length != 9 || !CifLetters.Contains(cif[0]) || cif.AsSpan(1, 7).IndexOfAnyExceptInDigitRange() is not -1)
            return false;

        int oddSum = 0, evenSum = 0;
        for (int i = 1; i <= 7; i++)
        {
            int digit = cif[i] - '0';
            if (i % 2 == 1)
            {
                int doubled = digit * 2;
                oddSum += doubled / 10 + doubled % 10;
            }
            else
            {
                evenSum += digit;
            }
        }

        int control = (10 - ((oddSum + evenSum) % 10)) % 10;
        char last = cif[8];

        if (CifDigitControlLetters.Contains(cif[0]))
            return last is >= '0' and <= '9' && (last - '0') == control;

        // Para N/P/Q/R/S/W el control puede ser letra o dígito.
        if (last is >= 'A' and <= 'J')
            return ControlLetterToDigit(last) == control;
        return last is >= '0' and <= '9' && (last - '0') == control;
    }

    /// <summary>Convierte una letra de control CIF (A=1..J=0) en su dígito.</summary>
    private static int ControlLetterToDigit(char letter) => letter == 'J' ? 0 : letter - 'A' + 1;

    private static string? ConcatName(Individual? individual)
    {
        if (individual is null)
            return null;

        var joined = string.Join(" ", new[]
        {
            individual.Name?.Trim() ?? string.Empty,
            individual.FirstSurname?.Trim() ?? string.Empty,
            individual.SecondSurname?.Trim() ?? string.Empty,
        }).Trim();

        return joined.Length == 0 ? null : joined;
    }
}

/// <summary>Ayuda para localizar el primer carácter fuera del rango '0'..'9'.</summary>
internal static class SpanDigitExtensions
{
    public static int IndexOfAnyExceptInDigitRange(this ReadOnlySpan<char> span)
    {
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] is < '0' or > '9')
                return i;
        }
        return -1;
    }
}