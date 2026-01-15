@echo off
setlocal
echo [QuantumBrowser] Starting Build Process...

:: 1. Setup Dependencies
if not exist "libs\webview2" (
    echo [QuantumBrowser] Downloading dependencies...
    powershell -ExecutionPolicy Bypass -File setup_deps.ps1
)

:: 2. Find Visual Studio
set "VS_PATH="
for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do (
    set "VS_PATH=%%i"
)

if "%VS_PATH%"=="" (
    echo [Error] Visual Studio C++ Compiler not found!
    echo Please install Visual Studio 2019/2022 with "Desktop development with C++".
    pause
    exit /b 1
)

echo [QuantumBrowser] Found Visual Studio at: %VS_PATH%

:: 3. Initialize Environment
call "%VS_PATH%\VC\Auxiliary\Build\vcvars64.bat"

:: 4. Build
if not exist "bin" mkdir bin

echo [QuantumBrowser] Compiling...

cl.exe /nologo /EHsc /std:c++17 /Zi ^
    /I "src" ^
    /I "libs\webview2\build\native\include" ^
    src\main.cpp src\BrowserWindow.cpp ^
    /link /OUT:bin\QuantumBrowser.exe ^
    /LIBPATH:"libs\webview2\build\native\x64" ^
    WebView2Loader.dll.lib ^
    user32.lib gdi32.lib shlwapi.lib ole32.lib oleaut32.lib

if %ERRORLEVEL% NEQ 0 (
    echo [Error] Compilation Failed!
    pause
    exit /b %ERRORLEVEL%
)

:: 5. Copy Runtime DLL
copy "libs\webview2\build\native\x64\WebView2Loader.dll" "bin\" >nul

echo.
echo [Success] Build Complete!
echo Run bin\QuantumBrowser.exe to start.
echo.
pause
