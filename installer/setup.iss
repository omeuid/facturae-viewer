; Inno Setup script para el Visor de FacturaE
; Compilar con: iscc installer/setup.iss
; Asociación de ficheros en HKCU (sin permisos de administrador).

#define AppName "Visor de FacturaE"
#define AppExeName "FacturaeViewer.exe"
#define AppVersion "1.0.0"
#define Publisher "Facturae Viewer contributors"
#define AppURL "https://github.com/omeuid/facturae-viewer"

[Setup]
AppId={{7D3F5C4E-2E1A-4F8B-9C6A-3D2B1A0E9F5C}
AppName={#AppName}
AppVerName={#AppName} {#AppVersion}
AppVersion={#AppVersion}
AppPublisher={#Publisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
DefaultDirName={autopf}\FacturaeViewer
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
CloseApplicationsFilter={#AppExeName}
AppMutex=Local\FacturaeViewer.SingleInstance
MinVersion=10.0.17763
OutputDir=..\artifacts\installer
OutputBaseFilename=FacturaeViewer-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\src\Facturae.App\Assets\App.ico
UninstallDisplayIcon={app}\{#AppExeName}
LicenseFile=..\LICENSE
VersionInfoVersion={#AppVersion}
VersionInfoCompany={#Publisher}
VersionInfoDescription={#AppName}

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el escritorio"; GroupDescription: "Accesos directos:"

[Files]
; El publish single-file no incrusta las DLLs nativas (QuestPDF/WPF); deben
; desplegarse junto al exe. Se copia todo el contenido de artifacts\publish.
Source: "..\artifacts\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppExeName}"; Tasks: desktopicon

; --- Asociación de ficheros en HKCU (sin admin) ---
[Registry]
; Registro en "Abrir con...": sin esta clave, Explorer no muestra la app ni su
; icono en el diálogo "Abrir con...". Windows resuelve el icono desde
; Applications\<exe>\DefaultIcon y el comando desde shell\open\command.
Root: HKCU; Subkey: "Software\Classes\Applications\{#AppExeName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Applications\{#AppExeName}"; ValueType: string; ValueName: "FriendlyAppName"; ValueData: "{#AppName}"
Root: HKCU; Subkey: "Software\Classes\Applications\{#AppExeName}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#AppExeName},0"
Root: HKCU; Subkey: "Software\Classes\Applications\{#AppExeName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\Applications\{#AppExeName}\SupportedTypes"; ValueType: string; ValueName: ".xsig"; ValueData: ""
Root: HKCU; Subkey: "Software\Classes\Applications\{#AppExeName}\SupportedTypes"; ValueType: string; ValueName: ".xpsig"; ValueData: ""
Root: HKCU; Subkey: "Software\Classes\Applications\{#AppExeName}\SupportedTypes"; ValueType: string; ValueName: ".xml"; ValueData: ""

Root: HKCU; Subkey: "Software\Classes\.xsig"; ValueType: string; ValueName: ""; ValueData: "FacturaeViewer.xsig"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\FacturaeViewer.xsig"; ValueType: string; ValueName: ""; ValueData: "Factura electrónica FacturaE (firmada)"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\FacturaeViewer.xsig\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#AppExeName},0"
Root: HKCU; Subkey: "Software\Classes\FacturaeViewer.xsig\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" ""%1"""

Root: HKCU; Subkey: "Software\Classes\.xpsig"; ValueType: string; ValueName: ""; ValueData: "FacturaeViewer.xpsig"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\FacturaeViewer.xpsig"; ValueType: string; ValueName: ""; ValueData: "Factura electrónica FacturaE (firmada)"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\FacturaeViewer.xpsig\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#AppExeName},0"
Root: HKCU; Subkey: "Software\Classes\FacturaeViewer.xpsig\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" ""%1"""

Root: HKCU; Subkey: "Software\Classes\.xml"; ValueType: string; ValueName: ""; ValueData: "FacturaeViewer.xml"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\FacturaeViewer.xml"; ValueType: string; ValueName: ""; ValueData: "Documento XML"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\FacturaeViewer.xml\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#AppExeName},0"
Root: HKCU; Subkey: "Software\Classes\FacturaeViewer.xml\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" ""%1"""

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Ejecutar {#AppName}"; Flags: nowait postinstall skipifsilent