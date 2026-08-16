// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using FacturaeViewer.Core.Model;

namespace FacturaeViewer.Core.Validation;

/// <summary>
/// Ejecuta todas las comprobaciones de validación de un documento FacturaE
/// (esquema XSD, NIF/CIF, coherencia de totales y firma) y fusiona los
/// resultados en un único informe.
/// </summary>
public static class DocumentValidator
{
    public static ValidationReport Validate(FacturaeDocument document)
    {
        var report = new ValidationReport();
        report.Merge(SchemaValidator.Validate(document));
        report.Merge(NifValidator.Validate(document));
        report.Merge(TotalsValidator.Validate(document));
        report.Merge(SignatureValidator.Validate(document));
        report.Merge(BusinessRulesValidator.Validate(document));
        return report;
    }
}