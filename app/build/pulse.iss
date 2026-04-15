; MGG Pulse — Inno Setup script
; Build with: iscc build\pulse.iss /DAppVersion=1.0.0 /DPublishDir=build\publish /DOutputDir=build\output
;
; Parameters (passed via /D on the command line or set by build.ps1):
;   AppVersion  — semantic version string, e.g. 1.2.0
;   PublishDir  — path to the dotnet publish output directory
;   OutputDir   — destination directory for the compiled .exe

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#ifndef PublishDir
  #define PublishDir "..\build\publish"
#endif

#ifndef OutputDir
  #define OutputDir "..\build\output"
#endif

[Setup]
AppName=MGG Pulse
AppVersion={#AppVersion}
AppPublisher=MGG Dev
AppPublisherURL=https://github.com/mgg-dev/mgg-pulse
AppSupportURL=https://github.com/mgg-dev/mgg-pulse/issues
AppUpdatesURL=https://github.com/mgg-dev/mgg-pulse/releases
DefaultDirName={localappdata}\MGG Pulse
DefaultGroupName=MGG Pulse
OutputDir={#OutputDir}
OutputBaseFilename=MGGPulse-Setup-{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\assets\branding\icon.ico
UninstallDisplayIcon={app}\MGG.Pulse.UI.exe
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon";  Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startupentry"; Description: "Launch MGG Pulse at Windows startup";               GroupDescription: "Startup:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\MGG Pulse";           Filename: "{app}\MGG.Pulse.UI.exe"
Name: "{group}\Uninstall MGG Pulse"; Filename: "{uninstallexe}"
Name: "{commondesktop}\MGG Pulse";   Filename: "{app}\MGG.Pulse.UI.exe"; Tasks: desktopicon

[Registry]
; Startup entry (optional — controlled by task)
Root: HKCU; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "MGGPulse"; ValueData: """{app}\MGG.Pulse.UI.exe"""; Tasks: startupentry; Flags: uninsdeletevalue

[Run]
Filename: "{app}\MGG.Pulse.UI.exe"; Description: "{cm:LaunchProgram,MGG Pulse}"; Flags: nowait postinstall skipifsilent

[Code]
// Silent installer support — used by the auto-updater (/SILENT flag).
// No custom code needed; Inno Setup handles /SILENT natively.
