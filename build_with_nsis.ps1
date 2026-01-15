$nsisVersion = "3.09"
$nsisZip = "nsis-$nsisVersion.zip"
# Direct link to SourceForge mostly works with curl -L
$nsisUrl = "https://sourceforge.net/projects/nsis/files/NSIS%203/$nsisVersion/$nsisZip/download"
$toolsDir = "$PWD\tools_nsis"
$nsisDir = "$toolsDir\nsis-$nsisVersion"
$makensis = "$nsisDir\makensis.exe"

# Enable TLS 1.2
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# 1. Prepare Tools Directory
if (!(Test-Path $toolsDir)) { New-Item -ItemType Directory -Path $toolsDir -Force }

# 2. Check if NSIS is present
if (!(Test-Path $makensis)) {
    Write-Host "Downloading NSIS Portable..."
    $zipPath = "$toolsDir\$nsisZip"
    
    # Try using curl.exe if available (Window 10+), otherwise Invoke-WebRequest
    if (Get-Command "curl.exe" -ErrorAction SilentlyContinue) {
        Write-Host "Using curl.exe..."
        # -L to follow redirects, -o to output file
        & curl.exe -L $nsisUrl -o $zipPath
    } else {
        Write-Host "Using Invoke-WebRequest..."
        # Fake User Agent
        Invoke-WebRequest -Uri $nsisUrl -OutFile $zipPath -UserAgent "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
    }
    
    if (Test-Path $zipPath) {
        if ((Get-Item $zipPath).Length -gt 1000000) {
            Write-Host "Extracting NSIS..."
            Expand-Archive -Path $zipPath -DestinationPath $toolsDir -Force
            Remove-Item $zipPath
        } else {
            Write-Host "Error: Downloaded file is too small (probably HTML page). Aborting."
            exit 1
        }
    } else {
        Write-Host "Error: Download failed."
        exit 1
    }
}

# 3. Compile
if (Test-Path $makensis) {
    Write-Host "Compiling setup.nsi..."
    & $makensis "setup.nsi"
} else {
    Write-Host "Error: makensis.exe not found after extraction."
    exit 1
}
