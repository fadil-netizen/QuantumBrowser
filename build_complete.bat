@echo off
echo [QuantumBrowser] Starting COMPLETE Build Process...

:: 1. Build the Main Application
echo.
echo ==========================================
echo PHASE 1: Building Application
echo ==========================================
call build_csharp.bat
if %ERRORLEVEL% NEQ 0 (
    echo [Error] Application Build Failed!
    pause
    exit /b %ERRORLEVEL%
)

:: 2. Build the Installer
echo.
echo ==========================================
echo PHASE 2: Building Installer
echo ==========================================
cd InstallerBuild
powershell -ExecutionPolicy Bypass -File BuildSetup.ps1
if %ERRORLEVEL% NEQ 0 (
    echo [Error] Installer Build Failed!
    cd ..
    pause
    exit /b %ERRORLEVEL%
)
cd ..

echo.
echo ==========================================
echo [SUCCESS] COMPLETE BUILD FINISHED
echo ==========================================
echo Application: bin\QuantumBrowser.exe
echo Installer:   bin\build\quantumbrowsersetup.exe
echo.
pause
