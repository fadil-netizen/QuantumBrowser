@echo off
setlocal
echo [QuantumBrowser] Starting C# Build...

:: 0. Kill existing instances
taskkill /F /IM QuantumBrowser.exe 2>nul

:: 1. Setup Dependencies
if not exist "libs" (
    echo [QuantumBrowser] Downloading dependencies...
    powershell -ExecutionPolicy Bypass -File setup_deps.ps1
)

:: 2. Find C# Compiler (csc.exe) - Use version that works
set "CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC_PATH%" (
    echo [Error] C# Compiler not found
    pause
    exit /b 1
)

:: 3. Build
if not exist "bin" mkdir bin

echo [QuantumBrowser] Compiling...

:: Reference the WebView2 WinForms DLL from the extracted libs
:: Path in nuget: libs\webview2\lib\net462\Microsoft.Web.WebView2.WinForms.dll
:: Path in nuget: libs\webview2\lib\net462\Microsoft.Web.WebView2.Core.dll

"%CSC_PATH%" /target:winexe /platform:x64 /out:bin\QuantumBrowser.exe ^
    /r:libs\webview2\lib\net462\Microsoft.Web.WebView2.WinForms.dll ^
    /r:libs\webview2\lib\net462\Microsoft.Web.WebView2.Core.dll ^
    /r:System.dll ^
    /r:System.Windows.Forms.dll ^
    /r:System.Drawing.dll ^
    /r:System.Xml.dll ^
    /r:System.Security.dll ^
    /r:System.IO.Compression.dll ^
    /r:System.IO.Compression.FileSystem.dll ^
    /win32icon:logo.ico ^
    *.cs

if %ERRORLEVEL% NEQ 0 (
    echo [Error] Compilation Failed!
    exit /b %ERRORLEVEL%
)

:: 4. Copy Runtime DLLs
:: We need WebView2Loader.dll alongside the exe
if not exist "bin\runtimes\win-x64\native" mkdir "bin\runtimes\win-x64\native"
copy "libs\webview2\runtimes\win-x64\native\WebView2Loader.dll" "bin\runtimes\win-x64\native\" >nul
copy "libs\webview2\runtimes\win-x64\native\WebView2Loader.dll" "bin\" >nul
copy "libs\webview2\lib\net462\Microsoft.Web.WebView2.WinForms.dll" "bin\" >nul
copy "libs\webview2\lib\net462\Microsoft.Web.WebView2.Core.dll" "bin\" >nul

:: 5. Copy Assets (HTML/CSS)
if not exist "bin\assets" mkdir "bin\assets"
xcopy /s /y "assets" "bin\assets\" >nul

echo.
echo [Success] Build Complete!
echo Run bin\QuantumBrowser.exe to start.
echo.
