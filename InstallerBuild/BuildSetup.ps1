
$sourceBin = "..\bin"
$buildDir = "..\bin\build"
$payloadZip = "$PWD\payload.zip"
$logoFile = "logo.png"
$outputExe = "$buildDir\quantumbrowsersetup.exe"

# 1. Kill Processes
Stop-Process -Name "QuantumBrowser" -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# 2. Prepare Build Directory
if (!(Test-Path $buildDir)) { New-Item -ItemType Directory -Path $buildDir -Force }

# 3. Prepare Payload Directory
$payloadDir = "$PWD\payload_temp"
if (Test-Path $payloadDir) { Remove-Item $payloadDir -Recurse -Force }
New-Item -ItemType Directory -Path $payloadDir -Force

Write-Host "Copying files..."
Copy-Item "$sourceBin\QuantumBrowser.exe" -Destination $payloadDir
Copy-Item "$sourceBin\WebView2Loader.dll" -Destination $payloadDir -ErrorAction SilentlyContinue
Copy-Item "$sourceBin\*.dll" -Destination $payloadDir -ErrorAction SilentlyContinue
if (Test-Path "$sourceBin\runtimes") { Copy-Item "$sourceBin\runtimes" -Destination $payloadDir -Recurse }
if (Test-Path "$sourceBin\assets") { Copy-Item "$sourceBin\assets" -Destination $payloadDir -Recurse }
if (Test-Path "..\MicrosoftEdgeWebview2Setup.exe") { Copy-Item "..\MicrosoftEdgeWebview2Setup.exe" -Destination $payloadDir }

# 4. Compress Payload using .NET API (More robust)
Write-Host "Compressing binaries..."
if (Test-Path $payloadZip) { Remove-Item $payloadZip }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($payloadDir, $payloadZip)

# 5. Compile
Write-Host "Compiling Installer..."
$csc = "$env:windir\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

# Use full paths for resources to avoid confusion
$cmd = "& '$csc' /target:winexe /out:'$outputExe' /win32manifest:'app.manifest' /resource:'$payloadZip','payload.zip' /resource:'$logoFile','logo.png' /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll Installer.cs"

Invoke-Expression $cmd

if (Test-Path $outputExe) {
    Write-Host "Success! Installer created at: $outputExe"
} else {
    Write-Host "Compilation failed."
    exit 1
}
