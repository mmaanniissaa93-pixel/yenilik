param(
    [Parameter(Mandatory=$true)][string]$AssemblyPath,
    [Parameter(Mandatory=$true)][string]$OutputDirectory,
    [switch]$AuditDpiDesigner
)
# Run with 32-bit Windows PowerShell -STA. Render in an isolated working
# directory without Window_Load (accounts, proxy and auto-login).
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assemblyFile = (Resolve-Path -LiteralPath $AssemblyPath).Path
$outputPath = [IO.Path]::GetFullPath($OutputDirectory)
[IO.Directory]::CreateDirectory($outputPath) | Out-Null
Set-Location -LiteralPath $outputPath
[Environment]::CurrentDirectory = $outputPath
[Windows.Forms.Application]::EnableVisualStyles()
$binDir = Split-Path -Parent $assemblyFile
# powershell.exe AppDomain xBot.exe.config redirect'lerini uygulamaz;
# preserialized resx'in istediği SRE 4.0.0.0'ı bin klasöründeki 8.0.0.0 ile karşıla.
$resolveHandler = {
  param($s, $e)
  $simple = ($e.Name -split ',')[0].Trim()
  $map = @{
    'System.Resources.Extensions' = 'System.Resources.Extensions.dll'
    'System.Memory' = 'System.Memory.dll'
    'System.Buffers' = 'System.Buffers.dll'
    'System.Numerics.Vectors' = 'System.Numerics.Vectors.dll'
    'System.Runtime.CompilerServices.Unsafe' = 'System.Runtime.CompilerServices.Unsafe.dll'
  }
  if ($map.ContainsKey($simple)) {
    $candidate = Join-Path $binDir $map[$simple]
    if ([IO.File]::Exists($candidate)) { return [Reflection.Assembly]::LoadFrom($candidate) }
  }
  return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolveHandler) | Out-Null
[Reflection.Assembly]::LoadFrom($assemblyFile) | Out-Null
$windowType = [xBot.App.Window]
$getProp = $windowType.GetProperty('Get', [Reflection.BindingFlags]'Public,Static')
if ($getProp -eq $null) { throw 'xBot.App.Window::Get property not found' }
$window = $getProp.GetValue($null, $null)
if ($window -eq $null) { throw 'xBot.App.Window::Get returned null' }
$flags = [Reflection.BindingFlags]'Instance,NonPublic'
$loadMethod = $window.GetType().GetMethod('Window_Load', $flags)
$window.remove_Load([Delegate]::CreateDelegate([EventHandler], $window, $loadMethod))
$select = $window.GetType().GetMethod('TabPageV_Option_Click', $flags)
function Field($name) { $window.GetType().GetField($name, [Reflection.BindingFlags]'Instance,Public,NonPublic').GetValue($window) }
$boundPotion = Field 'Character_cbxUseHP'
function Capture($name, $form = $window) {
    [Windows.Forms.Application]::DoEvents()
    $bitmap = New-Object Drawing.Bitmap($form.Width, $form.Height)
    try {
        $form.DrawToBitmap($bitmap, (New-Object Drawing.Rectangle(0,0,$form.Width,$form.Height)))
        $bitmap.Save((Join-Path $outputPath ($name + '.png')), [Drawing.Imaging.ImageFormat]::Png)
    } finally { $bitmap.Dispose() }
}
try {
    $window.ApplyPhBotClassicTheme()
    $window.StartPosition = 'Manual'
    $window.Location = New-Object Drawing.Point(-20000,-20000)
    $window.ShowInTaskbar = $false
    $window.Show()
    $window.ClientSize = New-Object Drawing.Size(1440,880)
    $sidebar = Field 'TabPageV_Control01'
    $checks = 0
    foreach ($button in @($sidebar.Controls | Where-Object { $_ -is [Windows.Forms.Button] })) {
        $select.Invoke($window, @($button.psobject.BaseObject, [EventArgs]::Empty)) | Out-Null
        [Windows.Forms.Application]::DoEvents()
        $panel = (Field 'pnlWindow').Controls[$button.Name + '_Panel']
        if (!$panel.Visible) { throw ('Navigation failed: ' + $button.Name) }
        foreach ($tabs in @($panel.Controls | Where-Object { $_ -is [Windows.Forms.TabControl] })) {
            foreach ($page in @($tabs.TabPages)) {
                $tabs.SelectedTab = $page
                [Windows.Forms.Application]::DoEvents()
                if (!$page.Visible) { throw ('Tab failed: ' + $page.Text) }
                if (@($tabs.TabPages | Where-Object { $_.Visible }).Count -ne 1) { throw 'Overlapping tab pages' }
                $checks++
            }
            $tabs.SelectedIndex = 0
        }
        Capture ($button.Name.Replace('TabPageV_Control01_', ''))
        if ($button.Name -eq 'TabPageV_Control01_Character') {
            $tabs = @($panel.Controls | Where-Object { $_ -is [Windows.Forms.TabControl] })[0]
            $tabs.SelectedIndex = 1
            Capture 'Potions'
        }
    }
    $select.Invoke($window, @($sidebar.Controls['TabPageV_Control01_Skills'], [EventArgs]::Empty)) | Out-Null
    $window.ClientSize = New-Object Drawing.Size(1100,700)
    Capture 'Attack-compact'
    if ($sidebar.HorizontalScroll.Visible) { throw 'Sidebar has a horizontal scrollbar' }
    $window.ClientSize = New-Object Drawing.Size(1440,880)
    Capture 'Attack-restored'
    # Visual layout must not replace bound controls or change saved values.
    if ((Field 'Character_cbxUseHP').Parent -ne (Field 'TabPageH_Character_Option02_Panel')) { throw 'Potion control was detached' }
    Write-Output ('PASS: ' + $checks + ' tab transitions; all navigation entries; resize; bound potion control identity.')
    # Regressions: scroll to the end at minimum size, then enlarge the viewport.
    $failures = New-Object 'System.Collections.Generic.List[string]'
    $window.Size = $window.MinimumSize
    $wheel = $window.GetType().GetMethod('Sidebar_MouseWheel', $flags)
    $wheelArgs = New-Object Windows.Forms.MouseEventArgs('None', 0, 0, 0, -12000)
    $wheel.Invoke($window, @($sidebar.psobject.BaseObject, $wheelArgs.psobject.BaseObject)) | Out-Null
    $window.ClientSize = New-Object Drawing.Size(1440,880)
    [Windows.Forms.Application]::DoEvents()
    if ((Field '_sidebarScrollOffset') -ne 0) { $failures.Add('Sidebar remains scrolled after all entries fit.') }
    Capture 'Sidebar-after-grow'

    # Keyboard navigation must reveal the focused entry in the custom sidebar.
    $window.Size = $window.MinimumSize
    $wheelArgs = New-Object Windows.Forms.MouseEventArgs('None', 0, 0, 0, 12000)
    $wheel.Invoke($window, @($sidebar.psobject.BaseObject, $wheelArgs.psobject.BaseObject)) | Out-Null
    $lastNav = @($sidebar.Controls | Where-Object { $_ -is [Windows.Forms.Button] } | Sort-Object TabIndex)[-1]
    $lastNav.Focus() | Out-Null
    [Windows.Forms.Application]::DoEvents()
    if ($lastNav.Top -lt 0 -or $lastNav.Bottom -gt $sidebar.ClientSize.Height) {
        $failures.Add('Focused sidebar entry is outside the viewport.')
    }
    Capture 'Sidebar-keyboard'

    $scriptForm = New-Object xBot.App.ScriptCreatorForm -ArgumentList ([string]'')
    try {
        $scriptForm.StartPosition = 'Manual'
        $scriptForm.Location = New-Object Drawing.Point(-20000,-20000)
        $scriptForm.ShowInTaskbar = $false
        $scriptForm.Show()
        $group = @($scriptForm.Controls | Where-Object { $_ -is [Windows.Forms.GroupBox] })[0]
        foreach ($size in @($scriptForm.Size, $scriptForm.MinimumSize, (New-Object Drawing.Size(1000,800)))) {
            $scriptForm.Size = $size
            [Windows.Forms.Application]::DoEvents()
            foreach ($button in @($group.Controls | Where-Object { $_ -is [Windows.Forms.Button] })) {
                if (!$group.ClientRectangle.Contains($button.Bounds)) {
                    $failures.Add('Script command clipped by group at ' + $size + ': ' + $button.Text)
                }
            }
            # ScrollControlIntoView is also the path used when tabbing to a command.
            $lastCommand = @($group.Controls | Sort-Object Bottom)[-1]
            $scriptForm.ScrollControlIntoView($lastCommand)
            [Windows.Forms.Application]::DoEvents()
            $commandRect = $scriptForm.RectangleToClient($group.RectangleToScreen($lastCommand.Bounds))
            if (!$scriptForm.ClientRectangle.Contains($commandRect)) {
                $failures.Add('Last script command is unreachable at ' + $size)
            }
            Capture ('Script-' + $size.Width + 'x' + $size.Height) $scriptForm
        }
    } finally { $scriptForm.Dispose() }
    if ($failures.Count -gt 0) { throw ($failures -join [Environment]::NewLine) }
    Write-Output 'PASS: sidebar scroll resize; keyboard visibility; script command containment and scroll reachability.'
    if (![object]::ReferenceEquals($boundPotion, (Field 'Character_cbxUseHP'))) { throw 'Bound potion control instance changed' }
    if ($AuditDpiDesigner) {
        & (Join-Path $PSScriptRoot 'DpiDesignerChecks.ps1') -Window $window -OutputDirectory $outputPath -AssemblyPath $assemblyFile
    }
    $tree = New-Object 'System.Collections.Generic.List[string]'
    function Dump($control, $depth) {
        $tree.Add((' ' * $depth) + $control.Name + ' [' + $control.GetType().Name + '] ' + $control.Bounds + ' visible=' + $control.Visible + ' text=' + $control.Text)
        foreach ($child in $control.Controls) { Dump $child ($depth + 1) }
    }
    Dump $window 0
    $tree | Set-Content -LiteralPath (Join-Path $outputPath 'controls.txt')
} finally { $window.Dispose() }
