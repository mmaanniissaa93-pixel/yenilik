param(
    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$resolvedOutput = (Resolve-Path -LiteralPath $OutputDirectory).Path
$managedAssembly = Join-Path $resolvedOutput 'System.Data.SQLite.dll'
$nativeAssembly = Join-Path $resolvedOutput 'x86\SQLite.Interop.dll'

if (-not (Test-Path -LiteralPath $managedAssembly)) {
    throw "System.Data.SQLite.dll bulunamadı: $managedAssembly"
}
if (-not (Test-Path -LiteralPath $nativeAssembly)) {
    throw "x86 SQLite.Interop.dll bulunamadı: $nativeAssembly"
}

$databasePath = Join-Path ([IO.Path]::GetTempPath()) ("xbot-sqlite-smoke-{0}.sqlite3" -f [Guid]::NewGuid().ToString('N'))
$connection = $null
try {
    [Reflection.Assembly]::LoadFrom($managedAssembly) | Out-Null
    [System.Data.SQLite.SQLiteConnection]::CreateFile($databasePath)
    $connection = New-Object System.Data.SQLite.SQLiteConnection("Data Source=$databasePath;Version=3;")
    $connection.Open()
    $command = $connection.CreateCommand()
    try {
        $command.CommandText = "CREATE TABLE smoke(id INTEGER PRIMARY KEY, value TEXT); INSERT INTO smoke(value) VALUES ('ok'); SELECT COUNT(*) FROM smoke;"
        $count = [Convert]::ToInt32($command.ExecuteScalar())
        if ($count -ne 1) {
            throw "SQLite doğrulama sonucu beklenmiyordu: $count"
        }
        Write-Output "SQLite x86 smoke test başarılı: count=$count"
    }
    finally {
        $command.Dispose()
    }
}
finally {
    if ($null -ne $connection) {
        $connection.Dispose()
    }
    if (Test-Path -LiteralPath $databasePath) {
        Remove-Item -LiteralPath $databasePath -Force
    }
}
