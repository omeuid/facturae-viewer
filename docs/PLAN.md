# Plan de implementación — Visor FacturaE (.xsig) para Windows

## Alcance y decisiones acordadas
- App de escritorio Windows (10/11) para visualizar y validar facturas electrónicas FacturaE.
- Formatos: `.xsig` (firmadas, primario), `.xpsig` y `.xml` sin firmar. Un fichero puede contener un lote de facturas.
- Stack: C# / .NET 10 (LTS), WPF (MVVM con CommunityToolkit.Mvvm). UI solo en español.
- Instalador: Inno Setup + `.exe` portable vía `dotnet publish`. Licencia Apache 2.0.

## Requisitos funcionales
1. Validar formato: XML bien formado, XSD de la versión detectada (3.2 / 3.2.1 / 3.2.2), reglas de negocio (NIF/NIE/CIF, coherencia de totales).
2. Validar firma: XMLDSig (digest + RSA, SHA-1/SHA-256), XAdES (policy, certificado firmante, rol), cadena de confianza y revocación (OCSP/CRL) reportando "no verificado" si no hay red.
3. Mostrar la factura gráficamente con panel de validación por chequeos (OK/aviso/error) y navegación de lotes.
4. Exportar a PDF (QuestPDF) e imprimir con vista previa WYSIWYG (Windows.Data.Pdf + PrintDialog).
5. Integración Windows: doble clic, single-instance con ruta CLI, drag & drop, recientes.

## Estructura del proyecto
```
facturae-viewer/
├── README.md
├── FacturaeViewer.sln
├── Directory.Build.props
├── .github/workflows/build.yml      # CI: build, tests, publish, instalador
├── src/
│   ├── Facturae.Core/               # sin dependencias de UI
│   │   ├── Model/                   # clases XmlSerializer (v3.2/3.2.1/3.2.2)
│   │   ├── Schemas/                 # XSD oficiales embebidos
│   │   ├── Validation/              # Schema, Signature, Nif, Totals
│   │   └── Pdf/                     # documento QuestPDF
│   ├── Facturae.App/                # WPF: Views, ViewModels, Services
│   └── Facturae.Tests/              # xUnit + fixtures oficiales
└── installer/setup.iss              # Inno Setup
```

## Fases
1. Esqueleto: solución .NET 10 + WPF, CI, README base.
2. Core FacturaE: XSDs embebidos, modelo generado, validación de esquema y reglas de negocio. Tests con muestras oficiales.
3. Firma: SignedXml + XAdES + cadena/revocación. Tests con fixtures buenas/malas.
4. UI: shell MVVM, apertura/arrastre, panel de validación, layout visual.
5. PDF e impresión: QuestPDF + vista previa/impresión WYSIWYG.
6. Integración Windows: asociación de archivo (HKCU), single-instance, CLI, recientes.
7. Empaquetado: publish self-contained, instalador Inno Setup, CI con artefactos, README con pasos exactos.

## Stack técnico
- Firma XMLDSig: System.Security.Cryptography.Xml (SignedXml).
- XSD: System.Xml.Schema con esquemas embebidos; modelo con XmlSerializer.
- PDF: QuestPDF (licencia Community, gratuita para OSS).
- MVVM: CommunityToolkit.Mvvm. Tests: xUnit.
- Instalador: Inno Setup (asociación de archivo en HKCU, sin admin).

## Riesgos y notas
- Regenerar el modelo al cambiar XSD (documentar).
- Soportar SHA-1 en validación de firmas antiguas (no en creación).
- Revocación sin red: reportar "no verificado" como aviso, nunca bloqueante.
- SmartScreen: .exe sin firmar muestra advertencia; opciones futuras: firma de código, Azure Trusted Signing o Microsoft Store.