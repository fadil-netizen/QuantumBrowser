@echo off
setlocal
echo [QuantumBrowser] Starting Modern C# Build...

:: 0. Kill existing instances
taskkill /F /IM QuantumBrowser.exe 2>nul

:: 1. Setup Dependencies
if not exist "libs" (
    echo [QuantumBrowser] Downloading dependencies...
    powershell -ExecutionPolicy Bypass -File setup_deps.ps1
)

:: 2. Check if we have .NET SDK (preferred for modern C#)
where dotnet >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [QuantumBrowser] Using .NET SDK...
    goto :use_dotnet_sdk
)

:: 3. Fallback to existing build (with compatibility mode)
echo [QuantumBrowser] .NET SDK not found, using framework compiler...
call build_csharp.bat
goto :end

:use_dotnet_sdk
:: Create a temporary csproj for compilation
echo [QuantumBrowser] Creating project file...
(
echo ^<Project Sdk="Microsoft.NET.Sdk"^>
echo   ^<PropertyGroup^>
echo     ^<OutputType^>WinExe^</OutputType^>
echo     ^<TargetFramework^>net6.0-windows^</TargetFramework^>
echo     ^<UseWindowsForms^>true^</UseWindowsForms^>
echo     ^<LangVersion^>latest^</LangVersion^>
echo     ^<Nullable^>disable^</Nullable^>
echo   ^</PropertyGroup^>
echo   ^<ItemGroup^>
echo     ^<PackageReference Include="Microsoft.Web.WebView2" Version="1.0.2792.45" /^>
echo     ^<PackageReference Include="System.Text.Json" Version="8.0.0" /^>
echo   ^</ItemGroup^>
echo ^</Project^>
) > QuantumBrowser_temp.csproj

echo [QuantumBrowser] Building with .NET SDK...
dotnet build QuantumBrowser_temp.csproj -c Release -o bin

if %ERRORLEVEL% NEQ 0 (
    echo [Error] Build Failed!
    del QuantumBrowser_temp.csproj
    pause
    exit /b %ERRORLEVEL%
)

:: Copy assets
if not exist "bin\assets" mkdir "bin\assets"
xcopy /s /y "assets" "bin\assets\" >nul

del QuantumBrowser_temp.csproj
echo.
echo [Success] Build Complete!
echo Run bin\QuantumBrowser.exe to start.
echo.

:end
