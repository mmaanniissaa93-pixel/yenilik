param(
    [string]$AssemblyPath = "xBot\bin\x86\Debug\xBot.exe"
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$assemblyFile = (Resolve-Path -LiteralPath $AssemblyPath).Path
$binDir = Split-Path -Parent $assemblyFile

$resolveHandler = {
    param($s, $e)
    $candidate = Join-Path $binDir (($e.Name -split ',')[0] + '.dll')
    if ([IO.File]::Exists($candidate)) { return [Reflection.Assembly]::LoadFrom($candidate) }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolveHandler)
$assembly = [Reflection.Assembly]::LoadFrom($assemblyFile)

$flags = [Reflection.BindingFlags]'Instance,Static,Public,NonPublic'

# Initialize Window instance without showing UI / message loop
$window = [xBot.App.Window]::Get
$loadMethod = $window.GetType().GetMethod('Window_Load', $flags)
if ($null -ne $loadMethod) {
    $window.remove_Load([Delegate]::CreateDelegate([EventHandler], $window, $loadMethod))
}

$bot = [xBot.App.Bot]::Get

$compiler = New-Object Microsoft.CSharp.CSharpCodeProvider
try {
    $parameters = New-Object CodeDom.Compiler.CompilerParameters
    $parameters.GenerateInMemory = $true
    foreach ($reference in @($assemblyFile, 'System.dll', 'System.Core.dll', 'System.Windows.Forms.dll', 'System.Drawing.dll')) {
        $parameters.ReferencedAssemblies.Add($reference) | Out-Null
    }
    $scenariosFile = Join-Path $PSScriptRoot 'Phase1StabilityScenarios.cs'
    $sourceCode = [IO.File]::ReadAllText($scenariosFile)
    $compiled = $compiler.CompileAssemblyFromSource($parameters, $sourceCode)
    if ($compiled.Errors.HasErrors) {
        throw ($compiled.Errors | Out-String)
    }
    $scenarioType = $compiled.CompiledAssembly.GetType('Phase1StabilityScenarios')
    $runMethod = $scenarioType.GetMethod('Run', [Reflection.BindingFlags]'Static,Public')
    $runMethod.Invoke($null, @($bot, $window))
}
finally {
    $compiler.Dispose()
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolveHandler)
}
