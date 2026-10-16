<#
.SYNOPSIS
    Signs xBot binaries with Authenticode signature using SHA-256.
.DESCRIPTION
    Generates a development Code Signing certificate if one is not already present,
    signs target executables and DLLs, and verifies the resulting signature.
#>
param(
    [string[]]$BinaryPaths = @(
        "xBot/bin/x86/Release/xBot.exe",
        "xBot/bin/Release/Client.Library.dll",
        "xBot/bin/Debug/xBot.exe"
    ),
    [string]$CertSubject = "CN=xBot Development Code Signing",
    [switch]$ForceNewCert
)

$ErrorActionPreference = "Stop"
Write-Host "=== xBot Authenticode Signing Pipeline ===" -ForegroundColor Cyan

# 1. Locate or create Code Signing Certificate
$cert = $null
if (-not $ForceNewCert) {
    $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
            Where-Object { $_.Subject -match "xBot" } |
            Select-Object -First 1
}

if ($null -eq $cert) {
    Write-Host "Creating new development Code Signing certificate ($CertSubject)..." -ForegroundColor Yellow
    $cert = New-SelfSignedCertificate -Type CodeSigningCert `
                                      -Subject $CertSubject `
                                      -CertStoreLocation "Cert:\CurrentUser\My" `
                                      -NotAfter (Get-Date).AddYears(5) `
                                      -HashAlgorithm "SHA256"
    Write-Host "Certificate created: Thumbprint=$($cert.Thumbprint)" -ForegroundColor Green
} else {
    Write-Host "Found existing certificate: Thumbprint=$($cert.Thumbprint) ($($cert.Subject))" -ForegroundColor Green
}

# 2. Sign each target binary
$results = [System.Collections.Generic.List[PSObject]]::new()
$success = $true

foreach ($path in $BinaryPaths) {
    if (-not (Test-Path $path)) {
        Write-Host "Skipping (not found): $path" -ForegroundColor DarkGray
        continue
    }

    $fullPath = (Resolve-Path $path).Path
    Write-Host "Signing $fullPath..." -NoNewline

    try {
        # Try timestamping first; fallback to untimestamped local signature if offline
        $sig = $null
        try {
            $sig = Set-AuthenticodeSignature -FilePath $fullPath `
                                             -Certificate $cert `
                                             -HashAlgorithm "SHA256" `
                                             -TimestampServer "http://timestamp.digicert.com" `
                                             -ErrorAction Stop
        } catch {
            $sig = Set-AuthenticodeSignature -FilePath $fullPath `
                                             -Certificate $cert `
                                             -HashAlgorithm "SHA256" `
                                             -ErrorAction Stop
        }

        if ($sig.Status -eq "Valid" -or $sig.Status -eq "UnknownError") {
            # Note: Self-signed certs report UnknownError when root cert is not in Trusted Root CA,
            # but the binary IS successfully signed and verifiable via Authenticode.
            Write-Host " [OK - $($sig.Status)]" -ForegroundColor Green
        } else {
            Write-Host " [FAILED: $($sig.StatusMessage)]" -ForegroundColor Red
            $success = $false
        }

        $verify = Get-AuthenticodeSignature -FilePath $fullPath
        $results.Add([PSCustomObject]@{
            File = [System.IO.Path]::GetFileName($fullPath)
            Path = $fullPath
            Status = $verify.Status
            Signer = $verify.SignerCertificate.Subject
            Thumbprint = $verify.SignerCertificate.Thumbprint
        })
    } catch {
        Write-Host " [EXCEPTION: $($_.Exception.Message)]" -ForegroundColor Red
        $success = $false
    }
}

Write-Host "`n=== Signature Verification Results ===" -ForegroundColor Cyan
$results | Format-Table File, Status, Signer, Thumbprint -AutoSize

if (-not $success) {
    Write-Error "Authenticode signing completed with one or more errors."
    exit 1
}

Write-Host "All existing binaries successfully signed with SHA-256 Authenticode signature.`n" -ForegroundColor Green
exit 0
