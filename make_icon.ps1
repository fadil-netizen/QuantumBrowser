
Add-Type -AssemblyName System.Drawing
$source = "logo.png"
$dest = "logo.ico"

if (Test-Path $source) {
    echo "Converting $source to $dest..."
    $bitmap = [System.Drawing.Bitmap]::FromFile($source)
    # Resize to 256x256 if needed or just use as is (Icon from handle usually defaults to standard sizes)
    # But GetHicon might produce a cursor or limited size.
    # Let's try the simple method first.
    $icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
    $stream = New-Object System.IO.FileStream($dest, 'Create')
    $icon.Save($stream)
    $stream.Close()
    $icon.Dispose()
    $bitmap.Dispose()
    echo "Done."
} else {
    echo "Source file $source not found."
}
