# Quantum Browser - APK Builder (PowerShell)
# Auto-detects Java JDK and builds APK

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Quantum Browser - APK Builder" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Find Java JDK
Write-Host "[1/4] Searching for Java JDK..." -ForegroundColor Yellow

$javaPaths = @(
    "C:\Program Files\Android\Android Studio\jbr",
    "C:\Program Files\Android\Android Studio\jre",
    "C:\Android\Android Studio\jbr",
    "C:\Android\Android Studio\jre",
    "$env:LOCALAPPDATA\Android\Sdk\jbr",
    "$env:LOCALAPPDATA\Android\Sdk\jre"
)

$javaHome = $null

foreach ($path in $javaPaths) {
    if (Test-Path "$path\bin\java.exe") {
        $javaHome = $path
        break
    }
}

if ($null -eq $javaHome) {
    Write-Host ""
    Write-Host "ERROR: Java JDK not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install Android Studio or set JAVA_HOME manually." -ForegroundColor Yellow
    Write-Host "Expected locations:" -ForegroundColor Yellow
    Write-Host "  - C:\Program Files\Android\Android Studio\jbr" -ForegroundColor Gray
    Write-Host "  - C:\Program Files\Android\Android Studio\jre" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Or use Android Studio: Build -> Build APK(s)" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Found Java at: $javaHome" -ForegroundColor Green
Write-Host ""

# Step 2: Set environment
Write-Host "[2/4] Setting environment..." -ForegroundColor Yellow
$env:JAVA_HOME = $javaHome
$env:PATH = "$javaHome\bin;$env:PATH"

# Step 3: Navigate to project
Write-Host "[3/4] Navigating to project..." -ForegroundColor Yellow
Set-Location "C:\Users\fsl\AndroidStudioProjects\MyApplication"

# Step 4: Build APK
Write-Host "[4/4] Building APK..." -ForegroundColor Yellow
Write-Host "This may take 2-5 minutes..." -ForegroundColor Gray
Write-Host ""

try {
    # Clean and build
    & .\gradlew.bat clean assembleDebug
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "  BUILD SUCCESSFUL!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "APK Location:" -ForegroundColor Cyan
        Write-Host "  app\build\outputs\apk\debug\app-debug.apk" -ForegroundColor White
        Write-Host ""
        
        # Check file size
        $apkPath = "app\build\outputs\apk\debug\app-debug.apk"
        if (Test-Path $apkPath) {
            $fileSize = (Get-Item $apkPath).Length / 1MB
            Write-Host "File size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "You can now install this APK on your phone!" -ForegroundColor Green
            Write-Host ""
            
            # Offer to open folder
            $openFolder = Read-Host "Open APK folder? (Y/N)"
            if ($openFolder -eq "Y" -or $openFolder -eq "y") {
                explorer "app\build\outputs\apk\debug"
            }
        }
    } else {
        throw "Build failed with exit code $LASTEXITCODE"
    }
} catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  BUILD FAILED!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check the error messages above." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Alternative: Use Android Studio" -ForegroundColor Cyan
    Write-Host "  Build -> Build Bundle(s) / APK(s) -> Build APK(s)" -ForegroundColor Gray
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Read-Host "Press Enter to exit"
