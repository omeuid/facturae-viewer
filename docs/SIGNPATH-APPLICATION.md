# Solicitud de firma de código — SignPath Foundation

Ficha con la información necesaria para rellenar el formulario de solicitud del
programa de firma gratuita para proyectos open source de SignPath Foundation
(<https://signpath.org/apply>). Permite completar la solicitud en una sola
sentada y deja constancia en el repositorio para futuras renovaciones.

## Datos del proyecto

| Campo | Valor |
|---|---|
| Nombre del proyecto | Facturae Viewer |
| URL del repositorio | <https://github.com/omeuid/facturae-viewer> |
| Licencia | Apache License 2.0 (ver `LICENSE` en la raíz, aprobada por OSI, sin dual-licensing) |
| Lenguaje principal | C# / .NET 10 (WPF) |
| Descripción | Visor y validador de facturas electrónicas españolas en formato FacturaE (.xsig, .xpsig, .xml) para Windows 10/11 |
| URL de descarga | <https://github.com/omeuid/facturae-viewer/releases> |
| Artefacto a firmar | `FacturaeViewer-Setup-1.0.0.exe` (instalador Inno Setup, x64) |
| Flujo de build | CI público en GitHub Actions (`.github/workflows/build.yml`), compila desde el source, ejecuta los tests y genera el instalador |
| Release ya publicada | Sí, `v1.0.0` con el instalador y el ejecutable portable |
| MFA | Activado en GitHub (obligatorio para todos los miembros del equipo) |
| Política de firma | Ver sección "Code signing policy" en el `README.md` |

## Cumplimiento de condiciones (signpath.org/terms)

- [x] Sin malware ni funciones que comprometan la seguridad del usuario.
- [x] Licencia OSI aprobada, sin dual-licensing comercial (Apache 2.0).
- [x] Sin código propietario del proyecto: todo el código del repo es Apache 2.0.
- [x] Proyecto mantenido activamente.
- [x] Release ya publicada en la forma que se va a firmar (v1.0.0).
- [x] Funcionalidad descrita en la página de descarga (README y docs/USO.md).
- [x] Repositorio público en GitHub.
- [x] Política de firma publicada en la home del proyecto (README).
- [x] Build verificable: el instalador se genera en CI desde el source.
- [x] Metadatos de binarios: nombre y versión de producto consistentes en cada build (ver `installer/setup.iss`).

## Dependencias incluidas en el instalador

| Dependencia | Licencia | Notas |
|---|---|---|
| CommunityToolkit.Mvvm | MIT | ✅ Sin problema |
| System.Security.Cryptography.Xml | MIT (parte de .NET) | ✅ Sin problema |
| QuestPDF | Propietaria de QuestPDF (no OSI) | ⚠️ Licencia gratuita solo por debajo de 1 M$ de ingresos anuales; se incluye como DLL de librería upstream en el paquete. Mencionar en la solicitud |

## Mensaje requerido en la home

"Free code signing provided by SignPath.io, certificate by SignPath Foundation"
(publicado en la sección "Code signing policy" del README).