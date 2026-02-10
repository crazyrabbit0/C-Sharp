@echo off
setlocal

:: Check if a file was dropped
if "%~1"=="" (
    echo Usage: Drag and drop a .cs file onto this script.
    pause
    exit /b 1
) else (
    set "SOURCE=%~1"
    set "BASENAME=%~n1"
)

set "OUTPUT=%BASENAME%.exe"
set "ICON=%BASENAME%.ico"

:: Try to find the C# compiler (csc.exe)
if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" (
    set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
) else if exist "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" (
    set "CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
) else (
    echo Error: Could not find csc.exe. Please ensure .NET Framework is installed.
    pause
    exit /b 1
)

echo Found compiler at: %CSC%
echo Compiling %SOURCE%...
echo Output: %OUTPUT%

:: Check if icon exists
if exist "%ICON%" (
    echo Using icon: %ICON%
    "%CSC%" /target:winexe /out:"%OUTPUT%" /win32icon:"%ICON%" "%SOURCE%"
) else (
    echo Warning: Icon file %ICON% not found. Compiling without icon.
    "%CSC%" /target:winexe /out:"%OUTPUT%" "%SOURCE%"
)

if %ERRORLEVEL% equ 0 (
    echo Compilation successful!
) else (
    echo Compilation failed.
    pause
    exit /b 1
)

pause
endlocal
