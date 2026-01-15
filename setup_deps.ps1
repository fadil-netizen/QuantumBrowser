$nugetUrl = "https://www.nuget.org/api/v2/package/Microsoft.Web.WebView2/1.0.2903.40"
$outputFile = "webview2.zip"
$extractPath = "libs"

Write-Host "Downloading WebView2 NuGet Package..."
Invoke-WebRequest -Uri $nugetUrl -OutFile $outputFile

Write-Host "Extracting..."
Expand-Archive -Path $outputFile -DestinationPath $extractPath -Force

Write-Host "Cleaning up..."
Remove-Item $outputFile


Write-Host "WebView2 Dependencies Ready."

$bootstrapperUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703"
$bootstrapperFile = "MicrosoftEdgeWebview2Setup.exe"

Write-Host "Downloading WebView2 Bootstrapper..."
try {
    Invoke-WebRequest -Uri $bootstrapperUrl -OutFile $bootstrapperFile
    Write-Host "Bootstrapper downloaded."
} catch {
    Write-Host "Failed to download Bootstrapper. Setup will continue without it."
}

