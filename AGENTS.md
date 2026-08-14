# AGENTS.md

## Estado del repositorio
Proyecto nuevo en scaffolding. Ya existe `LICENSE` (Apache 2.0),
`docs/PLAN.md` (plan de implementación acordado) y la estructura base de
la solución. El README y el instalador aún no están terminados.

## Decisiones de diseño acordadas (no renegociar sin el usuario)
- App de escritorio Windows para visualizar/validar facturas electrónicas
  FacturaE: `.xsig` (firmadas, primario), `.xpsig` y `.xml` sin firmar.
  Un fichero puede contener un lote de facturas.
- Stack: **C# / .NET 10 (LTS)** + **WPF** (MVVM con CommunityToolkit.Mvvm).
  Sin WinUI 3 ni Avalonia.
- Interfaz y mensajes de validación **solo en español** (sin i18n por ahora).
- Estructura:
  - `src/Facturae.Core/` — parseo XML (`XmlSerializer` desde XSD oficiales),
    validación XSD 3.2/3.2.1/3.2.2 (esquemas embebidos), firma
    XMLDSig/XAdES (`System.Security.Cryptography.Xml` + `X509Chain`),
    reglas de negocio (NIF/NIE/CIF, coherencia de totales), PDF (QuestPDF).
    Sin dependencias de UI.
  - `src/Facturae.App/` — cliente WPF (Views/ViewModels/Services).
  - `src/Facturae.Tests/` — xUnit con fixtures oficiales de facturae.gob.es.
  - `installer/` — Inno Setup (`setup.iss`), asociación de archivo en HKCU.
- Licencia del repo: Apache 2.0 (mantener en todos los archivos nuevos).

## Quirks
- El CI real es `.github/workflows/build.yml`.
- El usuario trabaja en español: respuestas y mensajes de commit en español.
- `Directory.Build.props` centraliza la configuración común (target
  `net10.0-windows`, nullable, WPF habilitado solo donde se necesita).
- Las muestras de validación (fixtures) deben venir de facturae.gob.es y
  de los ejemplos oficiales; no inventar XML de prueba.

## Verificación
- Build/test: `dotnet build` y `dotnet test` desde la raíz.
- Ejecutable portable:
  `dotnet publish src/Facturae.App -c Release -r win-x64 --self-contained /p:PublishSingleFile=true`
- Instalador: compilar con `iscc` (Inno Setup) sobre `installer/setup.iss`.