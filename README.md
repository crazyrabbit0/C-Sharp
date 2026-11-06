# C-Sharp

Small single-file C# utilities for Windows, compiled with the C# compiler that ships with the .NET Framework.

## CLI Proxy API Tray

`cli-proxy-api-tray.exe` runs [CLI Proxy API](https://github.com/router-for-me/CLIProxyAPI) (`cli-proxy-api.exe`) in the background and gives it a system tray icon.

- **Management Center** (or double-click the icon) opens `http://localhost:<port>/management.html`.
- **Containing Folder** opens the program's folder.
- The version item shows the running CLI Proxy API version and opens its releases page.
- **Exit** stops the tray and the CLI Proxy API process it started.

### Install

1. Download [`cli-proxy-api-tray.exe`](https://github.com/crazyrabbit0/C-Sharp/raw/main/cli-proxy-api-tray.exe) from this repository, or build it yourself (below).
2. Put it in the same folder as `cli-proxy-api.exe`.
3. Run it. Arguments are passed on to `cli-proxy-api.exe`, except the tray's own:
   - `--log` / `-l` writes `cli-proxy-api-tray.log`.
   - a path to an `.ico` file uses that tray icon.

Requires Windows with .NET Framework 4.0 or later (included in Windows 10 and 11).

### Build

Drag `cli-proxy-api-tray.cs` onto `compile.bat`, or run:

```bat
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:cli-proxy-api-tray.exe /win32icon:cli-proxy-api-tray.ico cli-proxy-api-tray.cs
```

`compile.bat` compiles any dropped `.cs` file and uses a matching `.ico` when one exists.

## Issues

Found a bug or have an idea? [Open an issue](https://github.com/crazyrabbit0/C-Sharp/issues/new/choose).

## License

[GPL-3.0](LICENSE) © CrazyRabbit
