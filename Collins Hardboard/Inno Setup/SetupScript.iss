; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "HB Scheduler"
#define MyAppVersion "4.5"
#define MyAppPublisher "Caldwell Consulting"
#define MyAppExeName "HBScheduler.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{81A05F1A-4383-4C60-8D1F-38814C051196}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=HBSchedulerInstaller
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Dat Files\ConversionFile.csv"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Dat Files\FactoryValues.dat"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Dat Files\InventoryTrack.dat"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Dat Files\machines.dat"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Dat Files\Product Master File.xlsx"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Dat Files\productionshiftconfig.dat"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Dat Files\shiftconfig.dat"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\HBScheduler.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\CoatingScheduler.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\Configuration windows.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\ConversionFile.csv"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\ExtendedScheduleViewer.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\ImportLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\InventoryViewer.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\LumenWorks.Framework.IO.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\HBScheduler.vshost.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\ModelLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\ProductionScheduler.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\ScheduleGen.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\StaticHelpers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\WarehouseManager.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\Xceed.Wpf.AvalonDock.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\Xceed.Wpf.AvalonDock.Themes.Aero.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\Xceed.Wpf.AvalonDock.Themes.Metro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\Xceed.Wpf.AvalonDock.Themes.VS2010.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\Xceed.Wpf.DataGrid.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programming\Collins\CollinsHardboard\Collins Hardboard\Main Application\bin\Debug\Xceed.Wpf.Toolkit.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
