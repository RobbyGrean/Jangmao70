# Khet70

Khet70 is a separate build of the Jangmao70 application workflow. The source was copied from `H:\CodexProjects\346` and adapted surgically for the Khet70 identity and storage paths.

The Khet70 package contains the real Procurement (45 DOCX) and Payroll (4 DOCX) templates, the position catalog, and the three shared salary rates from the Khet70 source files.

## Build

From PowerShell in this folder:

```powershell
.\build-exe-Khet70.ps1
.\build-installer-Khet70.ps1
```

Outputs:

- `dist\Khet70\Khet70.exe`
- `release\Khet70-Installer\Khet70-Setup.exe`
- `release\Khet70-Installer.zip`

`verify-manifest.ps1` checks the Khet70 identity, manifest containment, and hashes for all 49 packaged DOCX files.

## Installed paths

- `%LOCALAPPDATA%\Khet70\Khet70.exe`
- `%LOCALAPPDATA%\Khet70\Data`
- `%LOCALAPPDATA%\Khet70\Config`
- `%LOCALAPPDATA%\Khet70\Template`
- `%LOCALAPPDATA%\Khet70\Output`
- Desktop shortcut: `Khet70.lnk`
- Start Menu folder: `Khet70`

The installer and uninstaller operate only on the Khet70 install root and shortcuts targeting Khet70.
