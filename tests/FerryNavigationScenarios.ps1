param([string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
Push-Location $repoRoot
try {
    $buildTool = (Get-Command msbuild.exe -ErrorAction Stop).Source
    & $buildTool xBot/xBot.csproj /t:Build "/p:Configuration=$Configuration" /p:Platform=x86 /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw 'xBot build failed' }
    dotnet run --project tests/FerryNavigationScenarios
    if ($LASTEXITCODE -ne 0) { throw 'Navigation scenarios failed' }
    $compiler = Join-Path (Split-Path $buildTool -Parent) 'Roslyn/csc.exe'
    $outputDirectory = Join-Path $repoRoot "xBot/bin/x86/$Configuration"
    $assemblyPath = Join-Path $outputDirectory 'xBot.exe'
    $runnerPath = Join-Path $outputDirectory 'FerryRuntimeScenarios.exe'
    & $compiler /nologo /platform:x86 /target:exe "/reference:$assemblyPath" "/out:$runnerPath" tests/FerryRuntimeScenarios.cs
    if ($LASTEXITCODE -ne 0) { throw 'Production assembly scenario compilation failed' }
    & $runnerPath
    if ($LASTEXITCODE -ne 0) { throw 'Production assembly scenarios failed' }
} finally { Pop-Location }
