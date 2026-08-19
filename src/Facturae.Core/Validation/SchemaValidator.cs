// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Xml;
using System.Xml.Schema;
using FacturaeViewer.Core.Model;

namespace FacturaeViewer.Core.Validation;

/// <summary>
/// Valida un documento contra los esquemas XSD oficiales de Facturae
/// (3.2, 3.2.1 y 3.2.2), embebidos como recursos del ensamblado.
/// </summary>
public static class SchemaValidator
{
    private static readonly Lazy<XmlSchemaSet> SchemaSet = new(BuildSchemaSet);

    /// <summary>
    /// Nombres de los esquemas XSD embebidos (orden: xmldsig y XAdES primero
    /// para resolver los imports de los esquemas de Facturae).
    /// </summary>
    private static readonly string[] EmbeddedSchemas =
    [
        "xmldsig-core-schema.xsd",
        "XAdES_v1_3_2.xsd",
        "Facturaev3_1.xsd",
        "Facturaev3_2.xsd",
        "Facturaev3_2_1.xsd",
        "Facturaev3_2_2.xsd",
    ];

    public static ValidationReport Validate(FacturaeDocument document)
        => Validate(document.Xml);

    public static ValidationReport Validate(XmlDocument xml)
    {
        var report = new ValidationReport();

        void Handler(object? sender, ValidationEventArgs e)
        {
            var status = e.Severity == XmlSeverityType.Error ? CheckStatus.Error : CheckStatus.Warning;
            report.Add("SCHEMA", status, e.Message);
        }

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            ValidationType = ValidationType.Schema,
            ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings
                | XmlSchemaValidationFlags.ProcessIdentityConstraints,
        };
        settings.Schemas.Add(SchemaSet.Value);
        settings.ValidationEventHandler += Handler;

        try
        {
            using var reader = XmlReader.Create(new XmlNodeReader(xml), settings);
            while (reader.Read()) { }
        }
        catch (XmlSchemaValidationException ex)
        {
            report.Add("SCHEMA", CheckStatus.Error, ex.Message);
        }
        catch (XmlException ex)
        {
            report.Add("SCHEMA", CheckStatus.Error, $"Error de análisis XML: {ex.Message}");
        }

        if (report.ErrorCount == 0 && report.WarningCount == 0 && !report.Checks.Any())
            report.AddPassed("SCHEMA", "El documento es conforme al esquema XSD de Facturae.");

        return report;
    }

    private static XmlSchemaSet BuildSchemaSet()
    {
        var set = new XmlSchemaSet();
        foreach (var name in EmbeddedSchemas)
        {
            using var stream = OpenEmbeddedSchema(name);
            // El esquema xmldsig de la W3C lleva un DOCTYPE que hay que leer
            // (solo en los propios XSD, nunca en los documentos de facturas).
            using var reader = XmlReader.Create(stream,
                new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse });
            set.Add(null, reader);
        }

        try
        {
            set.Compile();
        }
        catch (XmlSchemaException ex)
        {
            throw new InvalidOperationException($"Error al compilar los esquemas XSD embebidos: {ex.Message}", ex);
        }

        return set;
    }

    private static Stream OpenEmbeddedSchema(string name)
    {
        var resourceName = $"FacturaeViewer.Core.Schemas.{name}";
        return typeof(SchemaValidator).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"No se encontró el esquema XSD embebido: {resourceName}");
    }
}