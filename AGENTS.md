# AGENTS.md

## Estado del repositorio
Proyecto completo: las 7 fases del plan (`docs/PLAN.md`) están implementadas,
con README, guía de uso (`docs/USO.md`) e instalador terminados. La release
`v1.0.0` está publicada en GitHub con el portable y el instalador.

## Decisiones de diseño acordadas (no renegociar sin el usuario)
- App de escritorio Windows para visualizar/validar facturas electrónicas
  FacturaE: `.xsig` (firmadas, primario), `.xpsig` y `.xml` sin firmar.
  Un fichero puede contener un lote de facturas.
- Stack: **C# / .NET 10 (LTS)** + **WPF** (MVVM con CommunityToolkit.Mvvm).
  Sin WinUI 3 ni Avalonia.
- Interfaz y mensajes de validación **solo en español** (sin i18n por ahora).
- Estructura:
  - `src/Facturae.Core/` — parseo XML (`XmlSerializer` desde XSD oficiales),
    validación XSD 3.1/3.2/3.2.1/3.2.2 (esquemas embebidos), firma
    XMLDSig/XAdES (`System.Security.Cryptography.Xml` + `X509Chain`),
    reglas de negocio (NIF/NIE/CIF, coherencia de totales), PDF (QuestPDF),
    modelo de datos compartido con la app (`Model/ReleaseInfo`).
    Sin dependencias de UI.
  - `src/Facturae.App/` — cliente WPF (Views/ViewModels/Services).
  - `src/Facturae.Tests/` — xUnit con fixtures oficiales de facturae.gob.es.
  - `installer/` — Inno Setup (`setup.iss`), asociación de archivo en HKCU.
- Licencia del repo: Apache 2.0 (mantener en todos los archivos nuevos).

## Quirks
- El CI real es `.github/workflows/build.yml`.
- El CI descarga xmlsec1 win64 desde las releases de GitHub
  (`https://github.com/lsh123/xmlsec/releases/download/1.3.12/...`) y define
  `XMLSEC_BIN` para la verificación cruzada de firmas. Sin esa variable, 4
  tests de firma cruzada se omiten.
- El instalador se genera con nombre versionado:
  `FacturaeViewer-Setup-<versión>.exe`. La app (`UpdateService`) lo detecta
  por el prefijo `FacturaeViewer-Setup` + `.exe` en los assets de la release.
- El usuario trabaja en español: respuestas y mensajes de commit en español.
- `Directory.Build.props` centraliza la configuración común (target
  `net10.0-windows`, nullable, WPF habilitado solo donde se necesita).
- Las muestras de validación (fixtures) deben venir de facturae.gob.es y
  de los ejemplos oficiales; no inventar XML de prueba.

## Verificación
- Build/test: `dotnet build` y `dotnet test` desde la raíz.
- Ejecutable portable:
  `dotnet publish src/Facturae.App -c Release -r win-x64 --self-contained /p:PublishSingleFile=true -o artifacts/publish`
- Instalador: compilar con `iscc installer/setup.iss` (Inno Setup) sobre el
  publish anterior; el instalador queda en `artifacts/installer`.