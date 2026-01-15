
Add-Type -AssemblyName System.Drawing

$source = "logo.png"
$dest = "logo.ico"
$tempPng = "temp_logo.png"

if (Test-Path $source) {
    echo "Processing $source..."
    
    # 1. Resize to 256x256
    $img = [System.Drawing.Image]::FromFile($source)
    $bitmap = New-Object System.Drawing.Bitmap 256, 256
    $g = [System.Drawing.Graphics]::FromImage($bitmap)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.DrawImage($img, 0, 0, 256, 256)
    $g.Dispose()
    $img.Dispose()
    
    # Save as PNG
    $bitmap.Save($tempPng, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
    
    # 2. Wrap in ICO
    $pngBytes = [System.IO.File]::ReadAllBytes($tempPng)
    
    # Header: Reserved(2), Type(2)=1 (Icon), Count(2)=1
    $header = [byte[]]@(0,0, 1,0, 1,0)
    
    # Entry: W(1), H(1), Colors(1), Res(1), Planes(2), BPP(2), Size(4), Offset(4)
    # 0 means 256 for W/H
    $entry = New-Object byte[] 16
    $entry[0] = 0 # W
    $entry[1] = 0 # H
    $entry[2] = 0 # Colors
    $entry[3] = 0 # Res
    $entry[4] = 1 # Planes
    $entry[5] = 0
    $entry[6] = 32 # BPP
    $entry[7] = 0
    
    $lenBytes = [BitConverter]::GetBytes([int]$pngBytes.Length)
    [Array]::Copy($lenBytes, 0, $entry, 8, 4)
    
    $offsetBytes = [BitConverter]::GetBytes([int]22) # 6+16
    [Array]::Copy($offsetBytes, 0, $entry, 12, 4)
    
    [System.IO.File]::WriteAllBytes($dest, $header + $entry + $pngBytes)
    
    Remove-Item $tempPng
    echo "Created logo.ico (PNG format)"
} else {
    echo "Source not found"
}
