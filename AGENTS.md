# CLI Proxy API Tray - Agent Knowledge Base

> **Identity**: This is a Windows System Tray application (`cli-proxy-api-tray.exe`) for the CLI Proxy API.
> **Role**: It acts as a process wrapper that launches the backend API, manages its lifecycle, and provides a GUI context menu.
> **Stack**: C# (.NET Framework 4.0+), Windows Forms (WinForms), System.Drawing.

## 1. Build & Environment

### Build Commands
The project is a single-file C# application. It does not use `dotnet build` or `.csproj` files. It compiles directly with `csc.exe`.

**Standard Build:**
```cmd
:: Assumes .NET Framework 4.0+ is installed
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:cli-proxy-api-tray.exe /win32icon:cli-proxy-api-tray.ico cli-proxy-api-tray.cs
```

**Using Helper Script:**
You can also use the provided batch file:
```cmd
compile.bat cli-proxy-api-tray.cs
```

### Dependencies
- **System Requirements**: Windows OS, .NET Framework 4.0 or higher.
- **Runtime Assets**:
  - `cli-proxy-api-tray.ico`: Required for the executable icon.
  - `cli-proxy-api.exe`: The backend executable must be in the same directory at runtime.

### Testing
- **Status**: No automated unit tests exist (GUI/System Tray logic is hard to test automatically).
- **Manual Verification**:
  1. Build the executable.
  2. Run `cli-proxy-api-tray.exe`.
  3. Verify the icon appears in the system tray.
  4. Right-click to verify menu items (Management Center, Containing Folder, Version, Exit).
  5. Verify `cli-proxy-api.exe` is launched as a child process (check Task Manager).
  6. Click "Exit" and ensure both the tray app and the background API process terminate.

---

## 2. Code Style & Conventions

### Formatting (Allman Style)
Use **Allman style** for braces (opening brace on a new line).
*Note: The existing code has mixed styles. Future edits should standardize on Allman.*

**Correct:**
```csharp
if (condition)
{
    DoSomething();
}
```

**Incorrect:**
```csharp
if (condition) {
    DoSomething();
}
```

### Naming Conventions
- **Classes/Methods/Properties**: `PascalCase`
  - `TrayRunner`, `StartChildProcess`, `ManagementUrl`
- **Private Fields**: `camelCase`
  - `trayIcon`, `childProcess`, `targetExe`
- **Local Variables**: `camelCase`
  - `args`, `iconPath`

### Imports (Using Directives)
Keep imports sorted alphabetically (System first).
```csharp
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
```

### File Structure
Currently, the application is a **Single-File** executable (`cli-proxy-api-tray.cs`).
- **Main Class**: `TrayRunner` (inherits from `Form`).
- **Entry Point**: `public static void Main(string[] args)`.
- **Renderers**: `ModernRenderer` and `ModernColorTable` classes are defined at the bottom of the file.

*Guideline*: If the file exceeds 1000 lines, consider refactoring into multiple files, but for now, keep it self-contained for ease of compilation.

---

## 3. Architecture & Patterns

### Tray Icon Logic
- The app creates a hidden `Form` (`TrayRunner`) to handle the message loop.
- It instantiates `NotifyIcon` to display in the system tray.
- It uses `ContextMenuStrip` with a custom renderer (`ModernRenderer`) for a native-looking but styled menu.

### Process Management
- **Child Process**: The app spawns `cli-proxy-api.exe` using `System.Diagnostics.Process`.
- **Output Redirection**: It captures StdOut/StdErr to parse the version number and port.
- **Lifecycle**:
  - On Start: Launches child process.
  - On Exit: Kills child process (`childProcess.Kill()`).
  - On Crash: If the tray app crashes, the child process might become orphaned (known limitation).

### Dynamic UI Updates
- The menu items (Version, Port) are updated dynamically based on the stdout of the child process.
- **Thread Safety**: UI updates triggered by process events must use `this.Invoke` or `this.BeginInvoke`.

---

## 4. Implementation Guidelines for Agents

### Adding New Menu Items
1. Define a `ToolStripMenuItem` field in the class.
2. Initialize it in `InitializeApp`.
3. Add a helper method for the icon (e.g., `GetMyIcon()`) using `System.Drawing`.
4. Add the item to `trayMenu.Items`.
5. Implement the `Click` event handler.

### Handling Command Line Arguments
- Arguments passed to the tray app are forwarded to the child process (`cli-proxy-api.exe`), **EXCEPT** for tray-specific flags.
- **Tray Flags**:
  - `--log` / `-l`: Enables file logging to `cli-proxy-api-tray.log`.
  - `*.ico`: Uses the specified icon file instead of the default.

### Error Handling
- Wrap all `Process.Start` calls (e.g., opening URLs) in `try-catch` to prevent crashing if the browser or explorer fails to launch.
- Silently swallow errors in `Get...Icon` methods to prevent UI rendering crashes; return `null` or a default icon instead.

### Refactoring
- **Renaming**: If renaming variables, ensure consistency across the single file.
- **Splitting**: If instructed to split the file, you must update `compile.bat` to include all `.cs` files (e.g., `csc /out:main.exe *.cs`).

---

## 5. Common Tasks

### Task: Update Version Parsing
If the backend output format changes:
1. Locate `OnOutputDataReceived`.
2. Update the string parsing logic for "CLIProxyAPI Version:".
3. Ensure `UpdateTooltip()` is called to refresh the UI.

### Task: Change Icon
- The app attempts to extract the icon from the target executable if no specific icon is provided.
- Priority:
  1. Command line `.ico` argument.
  2. `cli-proxy-api-tray.ico` (if compiled with it).
  3. `favicon.ico` in the directory.
  4. Associated icon of `cli-proxy-api.exe`.
  5. System default application icon.

---

## 6. Checklist Before Committing
- [ ] Code compiles without errors using `csc.exe` or `compile.bat`.
- [ ] No new compiler warnings.
- [ ] Allman style braces used for new code.
- [ ] `try-catch` blocks present around external resource access (Files, Processes).
