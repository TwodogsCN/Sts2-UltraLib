@echo off
REM UltraLib CHM build script (Windows)
REM 1. Installs/uses Node.js to convert Markdown -> HTML help sources
REM 2. Compiles the .hhp with HTML Help Workshop's hhc.exe -> UltraLib.chm
REM
REM Prerequisites:
REM   - Node.js (https://nodejs.org) on PATH
REM   - HTML Help Workshop (https://learn.microsoft.com/en-us/previous-versions/windows/desktop/htmlhelp/microsoft-html-help-workshop)
REM     -> ensures hhc.exe is available (install to default path, or add to PATH)
setlocal

set ROOT=%~dp0..\..
set CHMDIR=%~dp0
set OUT=%CHMDIR%out

echo === [1/3] Converting Markdown -> HTML help sources ===
node "%CHMDIR%build-chm.js" --source "%ROOT%\docs" --out "%OUT%"
if errorlevel 1 ( echo ERROR: Markdown conversion failed & exit /b 1 )

echo === [2/3] Locating hhc.exe ===
set HHC=hhc.exe
where hhc.exe >nul 2>nul
if errorlevel 1 (
  if exist "%ProgramFiles(x86)%\HTML Help Workshop\hhc.exe" (
    set "HHC=%ProgramFiles(x86)%\HTML Help Workshop\hhc.exe"
  ) else if exist "%ProgramFiles%\HTML Help Workshop\hhc.exe" (
    set "HHC=%ProgramFiles%\HTML Help Workshop\hhc.exe"
  ) else (
    echo WARNING: hhc.exe not found. Install HTML Help Workshop or add it to PATH.
    echo The HTML sources in %OUT% are ready; compile UltraLib.hhp manually.
    exit /b 2
  )
)

echo === [3/3] Compiling UltraLib.chm ===
"%HHC%" "%OUT%\UltraLib.hhp"
if errorlevel 1 ( echo ERROR: CHM compile failed & exit /b 1 )

echo.
echo Done. CHM built at: %OUT%\UltraLib.chm
endlocal
