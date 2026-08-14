# Fixtures de validación

Muestras XML usadas por los tests de `Facturae.Tests`.

## Procedencia

- `Facturae-3.2-valid.xml`, `Facturae-3.2.1-valid.xml` y
  `Facturae-3.2.2-valid.xml` reproducen el ejemplo oficial "Flores & Mate"
  publicado por facturae.gob.es en la documentación del formato FacturaE
  (v3.2 y posteriores). facturae.gob.es no distribuye el XML de ejemplo
  como fichero descargable (solo XSD y PDF), por lo que la muestra se ha
  reconstruido a partir de ese ejemplo oficial: línea 1 "Flores de jara y
  brezo" (1 × 25,00, descuento 5 %, recargo 40 %) y línea 2 "Mate de erizo"
  (2 × 13,00). Las bases y cuotas de IVA (16 %), recargo de equivalencia
  (1 %) e IRPF (4 %) siguen las del ejemplo oficial.

  Total: bruto 59,75 + repercutidas 8,67 (4,00 + 0,25 + 4,16 + 0,26)
  − retenidas 2,39 = **66,03**.

- `Facturae-3.2.2-lote-valid.xml` es una variante de lote (modalidad L) con
  dos facturas de 121,00 EUR cada una y total de lote **242,00** — construida
  a partir del ejemplo oficial solo en su estructura.

Los fixtures siguen las restricciones del esquema: el grupo `Batch` es
obligatorio en `FileHeader` (incluso en modalidad individual, con
`InvoicesCount` = 1), los importes de línea usan 6 decimales
(`DoubleSixDecimalType`), los tipos de descuento/recargo 4 decimales, los
tipos impositivos 2 decimales y `UnitOfMeasure` usa el código
(`01` unidades, `02` horas). Se han omitido `RegistrationData` y
`ContactDetails` (opcionales) por simplicidad.

- `Facturae-3.1-firmada-real.xsig.xml` es una factura real firmada con
  XAdES (firma de un particular con DNIe, formato FacturaE 3.1, año 2010),
  publicada por el Centro de Transferencia de Tecnología del Gobierno de
  España (repo `ctt-gob-es/clienteafirma`, fichero de ejemplo
  `sample-facturae-firmada.xsig.xml`). Se usa para comprobar que la
  verificación de firma funciona con firmas reales y no solo con las
  generadas en tests: la firma pasa la validación (SIG-02), con avisos por
  usar SHA-1 (SIG-04) y por no poder validar la cadena de confianza del
  DNIe de 2010 (SIG-10, caducado y sin raíz instalada).

- Los fixtures `*-incorrecto.xml` / `-invalido.xml` son mutaciones
  deliberadas del fixture válido 3.2.2 para ejercitar los validadores:
  - `Facturae-3.2.2-totales-incorrectos.xml`: `InvoiceTotal` = 99,99
    (rompe la coherencia aritmética, código `TOT-06`).
  - `Facturae-3.2.2-nif-invalido.xml`: CIF del emisor `B28015866`
    (dígito de control incorrecto, código `NIF`).
  - `Facturae-3.2.2-esquema-invalido.xml`: se elimina el `InvoiceNumber`
    obligatorio (error de esquema, código `SCHEMA`).

## Verificación

Los fixtures válidos deben pasar la validación XSD y los validadores de
reglas de negocio. Los fixtures inválidos deben producir exactamente los
errores indicados.