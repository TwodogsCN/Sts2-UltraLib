@echo off
REM UltraLib CHM build script (Windows)
REM 1. Uses Node.js to convert Markdown -> HTML help sources
REM 2. Compiles the .hhp with a Microsoft HTML Help compiler (hhc.exe) -> UltraLib.chm
REM 3. Copies the built .chm to the project dist/ folder
REM
REM Prerequisites:
REM   - Node.js (https://nodejs.org) on PATH
REM   - A Microsoft HTML Help compiler, e.g.:
REM       * HTML Help Workshop (https://learn.microsoft.com/en-us/previous-versions/windows/desktop/htmlhelp/microsoft-html-help-workshop)
REM       * EasyCHM's bundled HHC.EXE (C:\Program Files (x86)\EasyCHM\HHC.EXE)
REM     or any hhc.exe on PATH.
setlocal

set ROOT=%~dp0..\..
set CHMDIR=%~dp0
set OUT=%CHMDIR%out
set DIST=%ROOT%\dist

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
  ) else if exist "%ProgramFiles(x86)%\EasyCHM\HHC.EXE" (
    set "HHC=%ProgramFiles(x86)%\EasyCHM\HHC.EXE"
  ) else if exist "%ProgramFiles%\EasyCHM\HHC.EXE" (
    set "HHC=%ProgramFiles%\EasyCHM\HHC.EXE"
  ) else (
    echo WARNING: hhc.exe not found. Install HTML Help Workshop / EasyCHM or add it to PATH.
    echo The HTML sources in %OUT% are ready; compile UltraLib.hhp manually.
    exit /b 2
  )
)

echo === [3/3] Compiling UltraLib.chm ===
"%HHC%" "%OUT%\UltraLib.hhp"
if errorlevel 1 ( echo ERROR: CHM compile failed & exit /b 1 )

if not exist "%DIST%" mkdir "%DIST%"
copy /Y "%OUT%\UltraLib.chm" "%DIST%\UltraLib.chm" >nul
echo.
echo Done. CHM built at: %OUT%\UltraLib.chm
echo Copied to: %DIST%\UltraLib.chm
endlocal
