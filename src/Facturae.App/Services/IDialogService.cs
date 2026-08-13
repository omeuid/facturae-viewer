// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

namespace Facturae.App.Services;

/// <summary>
/// Abstracción de los diálogos nativos para poder testear el ViewModel.
/// </summary>
public interface IDialogService
{
    /// <summary>Muestra un diálogo para elegir un fichero FacturaE y devuelve su ruta (o null si se cancela).</summary>
    string? OpenFacturaeFile();

    /// <summary>Muestra un mensaje de error al usuario.</summary>
    void ShowError(string title, string message);
}