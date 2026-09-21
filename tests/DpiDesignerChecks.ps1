param(
    [Parameter(Mandatory=$true)][Windows.Forms.Form]$Window,
    [Parameter(Mandatory=$true)][string]$OutputDirectory,
    [Parameter(Mandatory=$true)][string]$AssemblyPath
)
# Invoked by ReferenceLayoutPreview.ps1 in its isolated, 32-bit STA process.
# Does NOT change Windows display settings or the application's scaling mode.
# Raster enlargement + reduced logical viewport != a real Windows DPI test.
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$assembly = [Reflection.Assembly]::LoadFrom($AssemblyPath)
$flags = [Reflection.BindingFlags]'Instance,Public,NonPublic'
$lines = New-Object 'System.Collections.Generic.List[string]'
function Record([string]$message) { $lines.Add($message); Write-Output $message }
function GetField([string]$name) { $Window.GetType().GetField($name, $flags).GetValue($Window) }
function SavePreview($form, [string]$name, [double]$factor) {
    $bitmap = New-Object Drawing.Bitmap($form.Width, $form.Height)
    try {
        $form.DrawToBitmap($bitmap, (New-Object Drawing.Rectangle(0,0,$form.Width,$form.Height)))
        $scaled = New-Object Drawing.Bitmap([int][Math]::Round($bitmap.Width * $factor), [int][Math]::Round($bitmap.Height * $factor))
        try {
            $graphics = [Drawing.Graphics]::FromImage($scaled)
            try {
                $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.DrawImage($bitmap, (New-Object Drawing.Rectangle(0,0,$scaled.Width,$scaled.Height)))
            } finally { $graphics.Dispose() }
            $scaled.Save((Join-Path $OutputDirectory ($name + '.png')))
        } finally { $scaled.Dispose() }
    } finally { $bitmap.Dispose() }
}
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class DpiAuditNative {
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern int GetAwarenessFromDpiAwarenessContext(IntPtr context);
    [DllImport("user32.dll")] public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);
    [DllImport("shcore.dll")] public static extern int GetScaleFactorForMonitor(IntPtr monitor, out int scale);
}
'@
$monitorScale = 0
$monitor = [DpiAuditNative]::MonitorFromWindow($Window.Handle, 2)
$scaleResult = [DpiAuditNative]::GetScaleFactorForMonitor($monitor, [ref]$monitorScale)
$awareness = [DpiAuditNative]::GetAwarenessFromDpiAwarenessContext([DpiAuditNative]::GetWindowDpiAwarenessContext($Window.Handle))
Record ('HOST ONLY: window DPI={0}; awareness={1}; nearest monitor scale={2}%; query HRESULT={3}; AutoScaleMode={4}' -f [DpiAuditNative]::GetDpiForWindow($Window.Handle), $awareness, $monitorScale, $scaleResult, $Window.AutoScaleMode)
Record 'The host is PowerShell, not xBot.exe. Monitor settings are read only. 125/150 outputs are simulations, not Windows DPI switches.'
$sidebar = GetField 'TabPageV_Control01'
$root = GetField 'pnlWindow'
$select = $Window.GetType().GetMethod('TabPageV_Option_Click', $flags)
$bound = GetField 'Character_cbxUseHP'
foreach ($factor in @(1.0, 1.25, 1.5)) {
    $percent = [int]($factor * 100)
    # Approximate usable client area on a fixed 1440x880 physical-pixel viewport.
    $logical = New-Object Drawing.Size([int][Math]::Floor(1440/$factor), [int][Math]::Floor(880/$factor))
    $Window.ClientSize = $logical
    $actual = $Window.ClientSize
    $count = 0
    foreach ($button in @($sidebar.Controls | Where-Object { $_ -is [Windows.Forms.Button] })) {
        $select.Invoke($Window, @($button.psobject.BaseObject, [EventArgs]::Empty)) | Out-Null
        [Windows.Forms.Application]::DoEvents()
        $panel = $root.Controls[$button.Name + '_Panel']
        if (!$panel.Visible) { throw ('Navigation failed at simulated scale ' + $percent) }
        foreach ($tabs in @($panel.Controls | Where-Object { $_ -is [Windows.Forms.TabControl] })) {
            foreach ($page in @($tabs.TabPages)) {
                $tabs.SelectedTab = $page
                [Windows.Forms.Application]::DoEvents()
                if (!$page.Visible -or @($tabs.TabPages | Where-Object Visible).Count -ne 1) { throw 'Tab visibility regression' }
                $count++
            }
        }
    }
    if (![object]::ReferenceEquals($bound, (GetField 'Character_cbxUseHP'))) { throw 'Bound control replaced' }
    if ($sidebar.Bounds.IntersectsWith((GetField 'rtbxLogs').Bounds)) { throw 'Sidebar overlaps logs' }
    if ((GetField 'rtbxLogs').Bounds.IntersectsWith((GetField '_phBotActionsPanel').Bounds)) { throw 'Logs overlap action buttons' }
    $select.Invoke($Window, @($sidebar.Controls['TabPageV_Control01_Skills'], [EventArgs]::Empty)) | Out-Null
    $attack = GetField 'TabPageH_Skills_Option01_Panel'
    $attack.Parent.Parent.SelectedTab = $attack.Parent
    [Windows.Forms.Application]::DoEvents()
    SavePreview $Window ('Simulated-' + $percent + '-attack') $factor
    $clipped = @($attack.Controls | Where-Object { $_.Visible -and ($_.Right -gt $attack.ClientSize.Width -or $_.Bottom -gt $attack.ClientSize.Height) })
    Record ('SIMULATION {0}%: logical requested={1}; actual={2}; tabs={3}; navigation/identity/shell overlap checks PASS; attack edge overflow count={4}; AutoScroll={5}' -f $percent, $logical, $actual, $count, $clipped.Count, $attack.AutoScroll)
    foreach ($control in $clipped) { Record ('  EDGE: ' + $control.Name + ' ' + $control.Bounds) }
    $select.Invoke($Window, @($sidebar.Controls['TabPageV_Control01_Login'], [EventArgs]::Empty)) | Out-Null
    SavePreview $Window ('Simulated-' + $percent + '-login') $factor
    $scriptForm = New-Object xBot.App.ScriptCreatorForm -ArgumentList ([string]'')
    try {
        $scriptForm.StartPosition = 'Manual'
        $scriptForm.Location = New-Object Drawing.Point(-20000,-20000)
        $scriptForm.ShowInTaskbar = $false
        $scriptForm.Show()
        $scriptForm.ClientSize = New-Object Drawing.Size([int][Math]::Floor(1000/$factor), [int][Math]::Floor(800/$factor))
        $group = @($scriptForm.Controls | Where-Object { $_ -is [Windows.Forms.GroupBox] })[0]
        $buttons = @($group.Controls | Where-Object { $_ -is [Windows.Forms.Button] } | Sort-Object Bottom)
        foreach ($button in $buttons) { if (!$group.ClientRectangle.Contains($button.Bounds)) { throw 'Script command clipped' } }
        foreach ($button in @($buttons[-1], $buttons[0])) {
            $scriptForm.ScrollControlIntoView($button)
            [Windows.Forms.Application]::DoEvents()
            $rect = $scriptForm.RectangleToClient($group.RectangleToScreen($button.Bounds))
            if (!$scriptForm.ClientRectangle.Contains($rect)) { throw 'Script command not reachable' }
        }
        SavePreview $scriptForm ('Simulated-' + $percent + '-script') $factor
        Record ('SIMULATION ' + $percent + '%: script command containment; scroll to last and first command PASS')
    } finally { $scriptForm.Dispose() }
    # Record fit limitations without overriding the existing minimum window size.
    Record ('FIT at 1024x768 physical pixels, {0}%: minimum outer size {1}; available logical size {2}x{3}' -f $percent, $Window.MinimumSize, [Math]::Floor(1024/$factor), [Math]::Floor(768/$factor))
}

# Resource and field validation is separate from actual Visual Studio Designer loading.
$forms = @{
    'xBot.App.Window'='xBot/App/Window.Designer.cs'
    'xBot.App.About'='xBot/App/About.Designer.cs'
    'xBot.App.Ads'='xBot/App/Ads.Designer.cs'
    'xBot.PK2Extractor.Pk2Extractor'='xBot/PK2Extractor/Pk2Extractor.Designer.cs'
}
foreach ($name in $forms.Keys) {
    $type = $assembly.GetType($name, $true)
    $source = [IO.File]::ReadAllText((Join-Path $repo $forms[$name]))
    $manager = New-Object ComponentModel.ComponentResourceManager($type)
    $keys = @([regex]::Matches($source, 'resources\.Get(?:Object|String)\("([^"]+)"\)') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique)
    foreach ($key in $keys) { if ($null -eq $manager.GetObject($key, [Globalization.CultureInfo]::InvariantCulture)) { throw ('Missing resource ' + $name + ':' + $key) } }
    $fields = @([regex]::Matches($source, 'this\.(\w+)\s*=\s*new\s') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique)
    foreach ($field in $fields) { if (!$type.GetField($field, $flags) -and !$type.GetProperty($field, $flags)) { throw ('Missing member ' + $name + ':' + $field) } }
    if (!$type.GetMethod('InitializeComponent', $flags)) { throw ('Missing InitializeComponent: ' + $name) }
    Record ('SOURCE/RESOURCE PASS: {0}; {1} initialized members; {2} resource keys' -f $name, $fields.Count, $keys.Count)
}

Add-Type -AssemblyName System.Design
Add-Type -ReferencedAssemblies System.Design,System.Windows.Forms,System.Drawing -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Windows.Forms;
public static class DesignerAudit {
    public static string[] Run(Type[] types) {
        var results = new List<string>();
        using (var surface = new DesignSurface(typeof(Form))) {
            var designerHost = (IDesignerHost)surface.GetService(typeof(IDesignerHost));
            foreach (Type type in types) {
                var control = (Control)designerHost.CreateComponent(type);
                ((Form)designerHost.RootComponent).Controls.Add(control);
                var designer = designerHost.GetDesigner(control);
                if (!control.Site.DesignMode || designer == null)
                    throw new InvalidOperationException("No designer for " + type.FullName);
                results.Add("DESIGN HOST SMOKE PASS: " + type.FullName + "; designer=" + designer.GetType().FullName);
            }
        }
        return results.ToArray();
    }
}
'@
$customTypes = @($Window.GetType().GetFields($flags) | ForEach-Object FieldType | Where-Object { $_.Namespace -eq 'xGraphics' -and [Windows.Forms.Control].IsAssignableFrom($_) } | Select-Object -Unique)
foreach ($message in [DesignerAudit]::Run([Type[]]$customTypes)) { Record $message }
Record 'This is an independent .NET DesignSurface smoke test, NOT a Visual Studio source designer open/serialize/save test.'
$lines | Set-Content -LiteralPath (Join-Path $OutputDirectory 'dpi-designer-results.txt') -Encoding UTF8
