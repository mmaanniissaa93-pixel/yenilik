param(
    [string]$AssemblyPath = "xBot\bin\x86\Debug\xBot.exe"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $AssemblyPath)) {
    Write-Error "Assembly not found: $AssemblyPath"
    exit 1
}

$fullAssemblyPath = (Resolve-Path $AssemblyPath).Path
$binDir = Split-Path -Parent $fullAssemblyPath

$resolveHandler = {
    param($s, $e)
    $candidate = Join-Path $binDir (($e.Name -split ',')[0] + '.dll')
    if ([IO.File]::Exists($candidate)) { return [Reflection.Assembly]::LoadFrom($candidate) }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolveHandler)

try {
    # Load xBot assembly
    $assembly = [System.Reflection.Assembly]::LoadFrom($fullAssemblyPath)

    # Compile Phase2AStabilityScenarios
    $scenarioSource = Get-Content -Raw "tests\Phase2AStabilityScenarios.cs"

    $provider = New-Object Microsoft.CSharp.CSharpCodeProvider
    $params = New-Object System.CodeDom.Compiler.CompilerParameters
    $params.GenerateInMemory = $true
    $params.GenerateExecutable = $false
    $params.CompilerOptions = "/platform:x86"

    $params.ReferencedAssemblies.Add("System.dll") | Out-Null
    $params.ReferencedAssemblies.Add("System.Core.dll") | Out-Null
    $params.ReferencedAssemblies.Add("System.Windows.Forms.dll") | Out-Null
    $params.ReferencedAssemblies.Add("System.Drawing.dll") | Out-Null
    $params.ReferencedAssemblies.Add($fullAssemblyPath) | Out-Null

    $results = $provider.CompileAssemblyFromSource($params, $scenarioSource)

    if ($results.Errors.HasErrors) {
        Write-Host "Compilation errors:" -ForegroundColor Red
        foreach ($err in $results.Errors) {
            Write-Host $err.ToString() -ForegroundColor Red
        }
        exit 1
    }

    $testType = $results.CompiledAssembly.GetType("xBot.Tests.Phase2AStabilityScenarios")
    $runMethod = $testType.GetMethod("RunAll", [System.Reflection.BindingFlags]"Public,Static")

    $exitCode = [int]$runMethod.Invoke($null, @())
    exit $exitCode
}
catch {
    Write-Host "Exception during execution:" -ForegroundColor Red
    Write-Host $_.Exception.ToString() -ForegroundColor Red
    if ($_.Exception.InnerException) {
        Write-Host "InnerException:" -ForegroundColor Red
        Write-Host $_.Exception.InnerException.ToString() -ForegroundColor Red
    }
    exit 1
}
finally {
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolveHandler)
}
