<#
.SYNOPSIS
    Packs separate splash animation PNG frames into a single sprite sheet atlas.
.DESCRIPTION
    Scans the specified folder for sequential frame PNGs (e.g., frame_001.png ... frame_048.png),
    stitches them into a single 32-bit transparent atlas image, and reports the storage savings.
#>
param(
    [string]$SourceFolder = "xbot_splash_transparent_frames",
    [string]$OutputFile = "splash_atlas.png",
    [int]$Columns = 8
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $SourceFolder)) {
    Write-Error "Source frames folder '$SourceFolder' does not exist."
    exit 1
}

$files = Get-ChildItem -Path $SourceFolder -Filter "*.png" | Sort-Object Name
if ($files.Count -eq 0) {
    Write-Error "No PNG frames found in '$SourceFolder'."
    exit 1
}

Write-Host "Found $($files.Count) frames in '$SourceFolder'." -ForegroundColor Cyan

# Read first frame dimensions
$sample = [System.Drawing.Image]::FromFile($files[0].FullName)
$frameWidth = $sample.Width
$frameHeight = $sample.Height
$sample.Dispose()

$frameCount = $files.Count
$cols = [Math]::Min($Columns, $frameCount)
$rows = [Math]::Ceiling($frameCount / $cols)
$atlasWidth = $cols * $frameWidth
$atlasHeight = $rows * $frameHeight

Write-Host "Atlas layout: $cols cols x $rows rows ($atlasWidth x $atlasHeight px)..." -ForegroundColor Yellow

$atlas = New-Object System.Drawing.Bitmap($atlasWidth, $atlasHeight, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($atlas)
$graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
$graphics.Clear([System.Drawing.Color]::Transparent)

for ($i = 0; $i -lt $frameCount; $i++) {
    $col = $i % $cols
    $row = [Math]::Floor($i / $cols)
    $destX = $col * $frameWidth
    $destY = $row * $frameHeight

    $frameImg = [System.Drawing.Image]::FromFile($files[$i].FullName)
    $graphics.DrawImage($frameImg, $destX, $destY, $frameWidth, $frameHeight)
    $frameImg.Dispose()
}

$graphics.Dispose()

# Save as PNG
$atlas.Save($OutputFile, [System.Drawing.Imaging.ImageFormat]::Png)
$atlas.Dispose()

$originalSizeMb = ($files | Measure-Object -Property Length -Sum).Sum / 1MB
$atlasSizeMb = (Get-Item $OutputFile).Length / 1MB

Write-Host "Successfully generated '$OutputFile'!" -ForegroundColor Green
Write-Host ("Original frames total: {0:N2} MB" -f $originalSizeMb)
Write-Host ("Single atlas size:      {0:N2} MB" -f $atlasSizeMb)
