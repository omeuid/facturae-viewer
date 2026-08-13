# Facturae Viewer

Visor y validador de facturas electrónicas españolas en formato **FacturaE** para
Windows 10/11. Permite abrir ficheros `.xsig` (facturas firmadas), `.xpsig` y `.xml`
(sin firmar), validar el formato (XSD) y la firma electrónica, visualizarlas con una
interfaz clara y exportarlas a PDF o imprimirlas.

> **Estado:** en desarrollo (fase 1 de 7 — ver [`docs/PLAN.md`](docs/PLAN.md)).

## Características (objetivo)

- Apertura por doble clic de ficheros `.xsig`, `.xpsig` y `.xml` (asociación de archivo).
- Validación del esquema XSD oficial (versiones 3.2, 3.2.1 y 3.2.2).
- Validación de firma XMLDSig/XAdES y de la cadena de confianza del certificado.
- Reglas de negocio: formato NIF/NIE/CIF y coherencia de totales e impuestos.
- Soporte de lotes (varias facturas por fichero).
- Vista de la factura con panel de validación por chequeos.
- Exportación a PDF y diálogo de impresión con vista previa.

## Requisitos previos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) o superior.
- Windows 10/11.
- *(Solo para el instalador)* [Inno Setup 6](https://jrsoftware.org/isdl.php) con `iscc` en el PATH.

## Compilar

```powershell
dotnet build
```

## Ejecutar

```powershell
dotnet run --project src/Facturae.App
```

## Ejecutar los tests

```powershell
dotnet test
```

## Generar el ejecutable portable

```powershell
dotnet publish src/Facturae.App -c Release -r win-x64 --self-contained /p:PublishSingleFile=true -o artifacts/publish
```

El ejecutable es `artifacts\publish\FacturaeViewer.exe` (autocontenido, no requiere
.NET instalado en el equipo destino).

## Generar el instalador

*Pendiente de la fase 7. Instrucciones detalladas se añadirán en `installer/README.md`.*

## Estructura del proyecto

```
src/Facturae.Core/    Núcleo: parseo XML, validación XSD, firma y reglas de negocio (sin UI)
src/Facturae.App/     Cliente WPF (Views, ViewModels, Services)
src/Facturae.Tests/   Tests xUnit
installer/            Instalador Inno Setup (setup.iss)
docs/PLAN.md          Plan de implementación
```

## Licencia

Apache License 2.0 — ver [LICENSE](LICENSE).
