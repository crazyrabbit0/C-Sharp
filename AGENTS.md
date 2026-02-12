# CLI Proxy API Tray - Agent Knowledge Base

## Project Overview
This is a Windows System Tray application (`cli-proxy-api-tray.exe`) for the CLI Proxy API.
It launches the API backend (`cli-proxy-api.exe`), manages its lifecycle, and provides a system tray menu for interaction.

## Build Instructions

### Manual Compilation
To compile manually using the C# compiler (`csc.exe`):
```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:cli-proxy-api-tray.exe /win32icon:cli-proxy-api-tray.ico cli-proxy-api-tray.cs
```

**Requirements:**
- .NET Framework 4.0 or newer.
- `cli-proxy-api-tray.ico` must be present in the directory.

## File Structure
- `cli-proxy-api-tray.cs`: Main source code (single-file application).
- `cli-proxy-api-tray.ico`: Application icon.
- `cli-proxy-api.exe`: The backend executable (expected to be in the same folder at runtime).

## Key Features
- **System Tray Icon**: Minimizes to tray.
- **Context Menu**:
  - **Management Center**: Opens `http://localhost:{port}/management.html`.
  - **Containing Folder**: Opens the application directory.
  - **v{version}**: Displays current version (custom drawn tag icon) and links to GitHub Releases.
  - **Exit**: Terminates the tray app and the child API process.
- **Dynamic Versioning**: Parses `CLIProxyAPI Version:` output from the backend to update the menu.
