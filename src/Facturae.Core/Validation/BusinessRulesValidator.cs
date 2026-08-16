// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Globalization;
using FacturaeViewer.Core.Model;

namespace FacturaeViewer.Core.Validation;

/// <summary>
/// Comprueba reglas de negocio adicionales no cubiertas por los demás
/// validadores: coherencia de fechas (emisión/operación), validez de códigos
/// (país ISO 3166-1 alfa-3, moneda ISO 4217 y provincia española) y
/// coherencia de descuentos y cargos de línea.
/// </summary>
public static class BusinessRulesValidator
{
    /// <summary>Códigos de país ISO 3166-1 alfa-3 vigentes.</summary>
    private static readonly HashSet<string> IsoCountryCodes = new()
    {
        "AFG", "ALB", "DZA", "AND", "AGO", "ATG", "ARG", "ARM", "AUS", "AUT",
        "AZE", "BHS", "BHR", "BGD", "BRB", "BLR", "BEL", "BLZ", "BEN", "BTN",
        "BOL", "BIH", "BWA", "BRA", "BRN", "BGR", "BFA", "BDI", "CPV", "KHM",
        "CMR", "CAN", "CAF", "TCD", "CHL", "CHN", "COL", "COM", "COG", "COD",
        "CRI", "CIV", "HRV", "CUB", "CYP", "CZE", "DNK", "DJI", "DMA", "DOM",
        "ECU", "EGY", "SLV", "GNQ", "ERI", "EST", "SWZ", "ETH", "FJI", "FIN",
        "FRA", "GAB", "GMB", "GEO", "DEU", "GHA", "GRC", "GRD", "GTM", "GIN",
        "GNB", "GUY", "HTI", "HND", "HUN", "ISL", "IND", "IDN", "IRN", "IRQ",
        "IRL", "ISR", "ITA", "JAM", "JPN", "JOR", "KAZ", "KEN", "KIR", "PRK",
        "KOR", "KWT", "KGZ", "LAO", "LVA", "LBN", "LSO", "LBR", "LBY", "LIE",
        "LTU", "LUX", "MDG", "MWI", "MYS", "MDV", "MLI", "MLT", "MHL", "MRT",
        "MUS", "MEX", "FSM", "MDA", "MCO", "MNG", "MNE", "MAR", "MOZ", "MMR",
        "NAM", "NRU", "NPL", "NLD", "NZL", "NIC", "NER", "NGA", "MKD", "NOR",
        "OMN", "PAK", "PLW", "PAN", "PNG", "PRY", "PER", "PHL", "POL", "PRT",
        "QAT", "ROU", "RUS", "RWA", "KNA", "LCA", "VCT", "WSM", "SMR", "STP",
        "SAU", "SEN", "SRB", "SYC", "SLE", "SGP", "SVK", "SVN", "SLB", "SOM",
        "ZAF", "SSD", "ESP", "LKA", "SDN", "SUR", "SWE", "CHE", "SYR", "TWN",
        "TJK", "TZA", "THA", "TLS", "TGO", "TON", "TTO", "TUN", "TUR", "TKM",
        "TUV", "UGA", "UKR", "ARE", "GBR", "USA", "URY", "UZB", "VUT", "VAT",
        "VEN", "VNM", "YEM", "ZMB", "ZWE", "ALA", "BES", "CUW", "GGY", "IMN",
        "JEY", "MAF", "SXM", "XKX",
    };

    /// <summary>Códigos de moneda ISO 4217 vigentes.</summary>
    private static readonly HashSet<string> IsoCurrencyCodes = new()
    {
        "AED", "AFN", "ALL", "AMD", "ANG", "AOA", "ARS", "AUD", "AWG", "AZN",
        "BAM", "BBD", "BDT", "BGN", "BHD", "BIF", "BMD", "BND", "BOB", "BOV",
        "BRL", "BSD", "BTN", "BWP", "BYN", "BZD", "CAD", "CDF", "CHE", "CHF",
        "CHW", "CLF", "CLP", "CNY", "COP", "COU", "CRC", "CUC", "CUP", "CVE",
        "CZK", "DJF", "DKK", "DOP", "DZD", "EGP", "ERN", "ETB", "EUR", "FJD",
        "FKP", "GBP", "GEL", "GHS", "GIP", "GMD", "GNF", "GTQ", "GYD", "HKD",
        "HNL", "HRK", "HTG", "HUF", "IDR", "ILS", "INR", "IQD", "IRR", "ISK",
        "JMD", "JOD", "JPY", "KES", "KGS", "KHR", "KMF", "KPW", "KRW", "KWD",
        "KYD", "KZT", "LAK", "LBP", "LKR", "LRD", "LSL", "LYD", "MAD", "MDL",
        "MGA", "MKD", "MMK", "MNT", "MOP", "MRU", "MUR", "MVR", "MWK", "MXN",
        "MXV", "MYR", "MZN", "NAD", "NGN", "NIO", "NOK", "NPR", "NZD", "OMR",
        "PAB", "PEN", "PGK", "PHP", "PKR", "PLN", "PYG", "QAR", "RON", "RSD",
        "RUB", "RWF", "SAR", "SBD", "SCR", "SDG", "SEK", "SGD", "SHP", "SLE",
        "SOS", "SRD", "SSP", "STN", "SVC", "SYP", "SZL", "THB", "TJS", "TMT",
        "TND", "TOP", "TRY", "TTD", "TWD", "TZS", "UAH", "UGX", "USD", "USN",
        "UYI", "UYU", "UYW", "UZS", "VED", "VES", "VND", "VUV", "WST", "XAF",
        "XAG", "XAU", "XBA", "XBB", "XBC", "XBD", "XCD", "XDR", "XOF", "XPD",
        "XPF", "XPT", "XSU", "XTS", "XUA", "XXX", "YER", "ZAR", "ZMW", "ZWG",
    };

    /// <summary>Provincias españolas: código (01..52) o nombre canónico.</summary>
    private static readonly Dictionary<string, string> SpanishProvinces = new()
    {
        ["01"] = "Álava", ["02"] = "Albacete", ["03"] = "Alicante", ["04"] = "Almería",
        ["05"] = "Ávila", ["06"] = "Badajoz", ["07"] = "Islas Baleares", ["08"] = "Barcelona",
        ["09"] = "Burgos", ["10"] = "Cáceres", ["11"] = "Cádiz", ["12"] = "Castellón",
        ["13"] = "Ciudad Real", ["14"] = "Córdoba", ["15"] = "La Coruña", ["16"] = "Cuenca",
        ["17"] = "Gerona", ["18"] = "Granada", ["19"] = "Guadalajara", ["20"] = "Guipúzcoa",
        ["21"] = "Huelva", ["22"] = "Huesca", ["23"] = "Jaén", ["24"] = "León",
        ["25"] = "Lérida", ["26"] = "La Rioja", ["27"] = "Lugo", ["28"] = "Madrid",
        ["29"] = "Málaga", ["30"] = "Murcia", ["31"] = "Navarra", ["32"] = "Orense",
        ["33"] = "Asturias", ["34"] = "Palencia", ["35"] = "Las Palmas", ["36"] = "Pontevedra",
        ["37"] = "Salamanca", ["38"] = "Santa Cruz de Tenerife", ["39"] = "Cantabria",
        ["40"] = "Segovia", ["41"] = "Sevilla", ["42"] = "Soria", ["43"] = "Tarragona",
        ["44"] = "Teruel", ["45"] = "Toledo", ["46"] = "Valencia", ["47"] = "Valladolid",
        ["48"] = "Vizcaya", ["49"] = "Zamora", ["50"] = "Zaragoza", ["51"] = "Ceuta",
        ["52"] = "Melilla",
    };

    public static ValidationReport Validate(FacturaeDocument document)
        => Validate(document.Facturae);

    public static ValidationReport Validate(Facturae facturae)
    {
        var report = new ValidationReport();
        var invoices = facturae.Invoices ?? [];

        foreach (var invoice in invoices)
        {
            if (invoice is null)
                continue;

            ValidateDates(report, invoice);
            ValidateCurrency(report, invoice);
        }

        if (facturae.Parties is not null)
        {
            ValidateParty(report, "emisor", facturae.Parties.SellerParty);
            ValidateParty(report, "receptor", facturae.Parties.BuyerParty);
        }

        foreach (var invoice in invoices)
        {
            if (invoice is null)
                continue;
            foreach (var line in invoice.Items ?? [])
            {
                if (line is not null)
                    ValidateLine(report, invoice, line);
            }
        }

        if (!report.Checks.Any())
            report.AddPassed("BR", "Las reglas de negocio adicionales se cumplen.");

        return report;
    }

    /// <summary>La fecha de operación no puede ser posterior a la de emisión.</summary>
    private static void ValidateDates(ValidationReport report, Invoice invoice)
    {
        string id = invoice.InvoiceHeader?.InvoiceNumber?.Trim()
            ?? "factura sin número";

        var issue = TryParseDate(invoice.InvoiceIssueData?.IssueDate);
        var operation = TryParseDate(invoice.InvoiceIssueData?.OperationDate);

        if (issue is null && operation is null)
            return;

        if (issue is null)
        {
            report.AddWarning("FEC", $"En {id} la fecha de emisión no tiene un formato ISO 8601 válido.");
            return;
        }

        if (operation is null)
            return;

        if (operation > issue)
        {
            report.AddError("FEC",
                $"En {id} la fecha de operación ({invoice.InvoiceIssueData!.OperationDate}) es posterior a la de emisión " +
                $"({invoice.InvoiceIssueData.IssueDate}).");
        }
    }

    /// <summary>El código de moneda debe ser ISO 4217 (p. ej. EUR).</summary>
    private static void ValidateCurrency(ValidationReport report, Invoice invoice)
    {
        string id = invoice.InvoiceHeader?.InvoiceNumber?.Trim()
            ?? "factura sin número";

        foreach (var (campo, codigo) in new[]
        {
            ("InvoiceCurrencyCode", invoice.InvoiceIssueData?.InvoiceCurrencyCode),
            ("TaxCurrencyCode", invoice.InvoiceIssueData?.TaxCurrencyCode),
        })
        {
            if (string.IsNullOrWhiteSpace(codigo))
                continue;

            string code = codigo.Trim().ToUpperInvariant();
            if (!IsoCurrencyCodes.Contains(code))
                report.AddError("COD",
                    $"En {id} el código de moneda {campo} ({codigo.Trim()}) no es un código ISO 4217 válido.");
        }
    }

    /// <summary>Valida país (ISO 3166-1 alfa-3) y provincia de las direcciones de una parte.</summary>
    private static void ValidateParty(ValidationReport report, string rol, Party? party)
    {
        if (party is null)
            return;

        foreach (var (country, province) in new[]
        {
            (party.LegalEntity?.AddressInSpain?.CountryCode, party.LegalEntity?.AddressInSpain?.Province),
            (party.LegalEntity?.OverseasAddress?.CountryCode, party.LegalEntity?.OverseasAddress?.Province),
            (party.Individual?.AddressInSpain?.CountryCode, party.Individual?.AddressInSpain?.Province),
            (party.Individual?.OverseasAddress?.CountryCode, party.Individual?.OverseasAddress?.Province),
        })
        {
            ValidateCountry(report, rol, country);
            ValidateProvince(report, rol, country, province);
        }
    }

    /// <summary>El código de país de las direcciones debe ser ISO 3166-1 alfa-3.</summary>
    private static void ValidateCountry(ValidationReport report, string rol, string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return;

        string code = country.Trim().ToUpperInvariant();
        if (code == "ES")
            return; // abreviación habitual; no es un error grave

        if (!IsoCountryCodes.Contains(code))
            report.AddWarning("COD",
                $"El país del {rol} ({country.Trim()}) no es un código ISO 3166-1 alfa-3 válido.");
    }

    /// <summary>La provincia del {rol} debe ser un código o nombre válido de España.</summary>
    private static void ValidateProvince(ValidationReport report, string rol, string? country, string? province)
    {
        if (string.IsNullOrWhiteSpace(province))
            return;

        bool esEspana = country?.Trim().Equals("ESP", StringComparison.OrdinalIgnoreCase) == true
            || country?.Trim().Equals("ES", StringComparison.OrdinalIgnoreCase) == true;

        if (!esEspana)
            return;

        string p = province.Trim();
        if (SpanishProvinces.ContainsKey(p) || SpanishProvinces.Values.Any(v => v.Equals(p, StringComparison.OrdinalIgnoreCase)))
            return;

        report.AddWarning("COD", $"La provincia del {rol} ({province.Trim()}) no corresponde a una provincia española.");
    }

    /// <summary>Los descuentos/cargos de una línea no pueden superar su coste total.</summary>
    private static void ValidateLine(ValidationReport report, Invoice invoice, InvoiceLine line)
    {
        string id = invoice.InvoiceHeader?.InvoiceNumber?.Trim() ?? "factura sin número";
        string descripcion = string.IsNullOrWhiteSpace(line.ItemDescription)
            ? "línea sin descripción"
            : $"línea \"{line.ItemDescription.Trim()}\"";

        if (line.Quantity < 0 || line.UnitPriceWithoutTax < 0 || line.TotalCost < 0 || line.GrossAmount < 0)
            report.AddError("LIN", $"En {id}, {descripcion}: las cantidades e importes no pueden ser negativos.");

        foreach (var discount in line.DiscountsAndRebates ?? [])
        {
            if (discount.DiscountAmount < 0)
                report.AddError("LIN", $"En {id}, {descripcion}: el importe del descuento no puede ser negativo.");

            if (discount.DiscountRate is < 0 or > 100)
                report.AddError("LIN", $"En {id}, {descripcion}: el porcentaje de descuento ({discount.DiscountRate:0.##} %) debe estar entre 0 y 100.");
        }

        foreach (var charge in line.Charges ?? [])
        {
            if (charge.ChargeAmount < 0)
                report.AddError("LIN", $"En {id}, {descripcion}: el importe del cargo no puede ser negativo.");

            if (charge.ChargeRate is < 0 or > 100)
                report.AddError("LIN", $"En {id}, {descripcion}: el porcentaje del cargo ({charge.ChargeRate:0.##} %) debe estar entre 0 y 100.");
        }

        // El total de descuentos no puede dejar la línea en negativo.
        decimal discounts = (line.DiscountsAndRebates ?? []).Sum(d => d.DiscountAmount);
        decimal charges = (line.Charges ?? []).Sum(c => c.ChargeAmount);
        decimal net = line.TotalCost - discounts + charges;
        if (net < 0)
            report.AddError("LIN", $"En {id}, {descripcion}: los descuentos superan el coste total de la línea ({net:0.00}).");
    }

    /// <summary>Intenta parsear una fecha ISO 8601 (YYYY-MM-DD o con hora).</summary>
    private static DateTime? TryParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var text = value.Trim();
        var styles = DateTimeStyles.AllowWhiteSpaces;
        return DateTime.TryParseExact(
                text,
                ["yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssK"],
                CultureInfo.InvariantCulture,
                styles,
                out var result)
            ? result
            : DateTime.TryParse(text, CultureInfo.InvariantCulture, styles, out result)
                ? result
                : null;
    }
}