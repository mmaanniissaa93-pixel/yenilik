param(
    [Parameter(Mandatory=$true)][string]$AssemblyPath,
    [Parameter(Mandatory=$true)][string]$OutputDirectory
)
# Run with 32-bit Windows PowerShell -STA; never log in or start a proxy.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assemblyFile = (Resolve-Path -LiteralPath $AssemblyPath).Path
$outputPath = [IO.Path]::GetFullPath($OutputDirectory)
[IO.Directory]::CreateDirectory($outputPath) | Out-Null
Set-Location -LiteralPath $outputPath
[Environment]::CurrentDirectory = $outputPath
$binDir = Split-Path -Parent $assemblyFile
$resolveHandler = {
    param($s, $e)
    $candidate = Join-Path $binDir (($e.Name -split ',')[0] + '.dll')
    if ([IO.File]::Exists($candidate)) { return [Reflection.Assembly]::LoadFrom($candidate) }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolveHandler)
[Reflection.Assembly]::LoadFrom($assemblyFile) | Out-Null
[Windows.Forms.Application]::EnableVisualStyles()
$flags = [Reflection.BindingFlags]'Instance,Public,NonPublic'
$window = [xBot.App.Window]::Get
$loadMethod = $window.GetType().GetMethod('Window_Load', $flags)
$window.remove_Load([Delegate]::CreateDelegate([EventHandler], $window, $loadMethod))
function Field($obj, $name) { $obj.GetType().GetField($name, $flags).GetValue($obj) }
function Check($condition, $message) {
    if (!$condition) { throw $message }
    Write-Output ('PASS: ' + $message)
}
function Find($name) { return $window.Controls.Find($name, $true)[0] }
function Capture($name) {
    [Windows.Forms.Application]::DoEvents()
    $bmp = New-Object Drawing.Bitmap($window.Width, $window.Height)
    try {
        $window.DrawToBitmap($bmp, (New-Object Drawing.Rectangle(0,0,$window.Width,$window.Height)))
        $bmp.Save((Join-Path $outputPath $name))
    } finally { $bmp.Dispose() }
}
try {
    Check (!$window.TopMost) 'Always on top is not permanently enabled at startup'
    $window.StartPosition = 'Manual'
    $window.Location = New-Object Drawing.Point(-20000,-20000)
    $window.Show()
    $window.GetType().GetMethod('TabPageV_Option_Click', $flags).Invoke($window,
        @((Field $window 'TabPageV_Control01_Login'), [EventArgs]::Empty)) | Out-Null
    $tabs = Find 'tabLoginRoot'
    $tabs.SelectedTab = $tabs.TabPages['pageConnect']

    (Find 'PhBot_ClientMode').Checked = $false
    Check ((Field $window 'Login_rbnClientless').Checked -and ![xBot.App.LoginStrategyManager]::UseClient) 'Client checkbox switches runtime to clientless mode'
    (Find 'PhBot_ClientMode').Checked = $true
    Check ((Field $window 'Login_rbnClient').Checked -and [xBot.App.LoginStrategyManager]::UseClient) 'Client checkbox restores client mode'
    (Find 'PhBot_NoClientless').Checked = $true
    Check (![xBot.App.LoginStrategyManager]::StayConnected) 'No clientless disables automatic failover'
    (Field $window 'Login_cbxRelogin').Checked = $true
    Check ([xBot.App.LoginStrategyManager]::AutoRelogin) 'Relog checkbox enables persisted reconnect policy'
    (Field $window 'Login_cbxRelogin').Checked = $false
    Check (![xBot.App.LoginStrategyManager]::AutoRelogin -and !(Field $window 'cbxGeneralAutoRelogin').Checked) 'Clearing relog cannot leave the hidden reconnect option enabled'
    (Find 'PhBot_HideLogin').Checked = $true
    Check ((Field $window 'Login_tbxUsername').UseSystemPasswordChar -and (Field $window 'Login_tbxPassword').UseSystemPasswordChar) 'Hide login masks username and password'
    (Field $window 'Login_cbxUseReturnScroll').Checked = $true
    Check ([xBot.App.LoginStrategyManager]::ReturnToTownOnLogin) 'Return on login updates the persisted setting'
    $saved = [xBot.App.LoginStrategyManager]::ToJson()
    [xBot.App.LoginStrategyManager]::HideLoginInfo = $false
    [xBot.App.LoginStrategyManager]::ReturnToTownOnLogin = $false
    [xBot.App.LoginStrategyManager]::FromJson($saved)
    $window.RefreshCustomSettingsWidgets()
    Check ((Find 'PhBot_HideLogin').Checked -and (Field $window 'Login_cbxUseReturnScroll').Checked) 'Login settings round-trip through persistence and refresh'
    foreach ($name in @('PhBot_LoginCheck','PhBot_AllowXTrap','PhBot_InstantAccess','PhBot_BlockAfter')) {
        Check (!(Find $name).Enabled) "$name is visibly unavailable instead of pretending to work"
    }

    $profile = New-Object Windows.Forms.ListViewItem('Test Profile')
    $profile.Name = 'Test Profile'
    $hosts = New-Object 'System.Collections.Generic.List[string]'
    $hosts.Add('127.0.0.1')
    foreach ($value in @([byte]22, [uint32]188, [uint16]15779, $hosts, $false, '', '')) {
        $sub = $profile.SubItems.Add('')
        $sub.Tag = $value
    }
    (Field $window 'Settings_lstvSilkroads').Items.Add($profile) | Out-Null
    (Field $window 'Login_cmbxSilkroad').Items.Add($profile.Name) | Out-Null
    (Field $window 'Login_cmbxSilkroad').Text = $profile.Name
    (Field $window 'Login_tbxUsername').Text = 'scenario-account'
    (Field $window 'Login_tbxPassword').Text = 'scenario-only'
    (Field $window 'Login_cmbxServer').Tag = 'Test Server'
    (Field $window 'Login_cmbxCharacter').Tag = 'Saved Character'
    (Field $window 'cbxGeneralAutoLogin').Checked = $true
    $window.GetType().GetMethod('LoadCommandLine', $flags).Invoke($window, @()) | Out-Null
    Check (![xBot.App.Bot]::Get.hasAutoLoginMode) 'Saved credentials without login CLI arguments do not create CLI auto-login mode'
    [xBot.App.Bot]::Get.hasAutoLoginMode = $true
    (Field $window 'cbxGeneralAutoLogin').Checked = $false
    Check (![xBot.App.Bot]::Get.hasAutoLoginMode -and ![xBot.App.LoginStrategyManager]::AutomatedLogin) 'Clearing auto login also clears stale CLI mode'
    $ports = New-Object 'System.Collections.Generic.List[uint16]'
    $ports.Add(15779)
    [xBot.App.Bot]::Get.Proxy = New-Object xBot.Network.Proxy($true, $hosts, $ports)
    [xBot.App.Bot]::Get.hasAutoLoginMode = $true
    (Field $window 'Settings_cbxSelectFirstChar').Checked = $true
    $characters = New-Object 'System.Collections.Generic.List[xBot.Game.Objects.Common.SRCharSelection]'
    $character = New-Object xBot.Game.Objects.Common.SRCharSelection
    $character.GetType().GetProperty('Name').SetValue($character, 'Saved Character', $null)
    $characters.Add($character)
    $beforeName = [xBot.Game.InfoManager]::CharName
    [xBot.App.Bot]::Get.OnCharacterListing($characters)
    Check ([xBot.Game.InfoManager]::CharName -eq $beforeName) 'Auto login off prevents character selection despite stale CLI and legacy first-character flags'
    [xBot.App.Bot]::Get.Proxy = $null

    $tabs.SelectedTab = $tabs.TabPages['pageSettings']
    [Windows.Forms.Application]::DoEvents()
    Check ((Find 'ClientPathDisplay').Visible -and (Find 'BrowseLoginClient').Visible) 'Client path display and browse button are reachable in login settings'
    $fakeClient = Join-Path $outputPath 'sro_client.exe'
    [IO.File]::WriteAllText($fakeClient, 'Test fixture only - never launched')
    $window.GetType().GetMethod('SetProfileClientPath', $flags).Invoke($window, [object[]]@($profile.psobject.BaseObject, [string]$fakeClient)) | Out-Null
    Check ($profile.SubItems[7].Tag -eq $fakeClient -and (Find 'ClientPathDisplay').Text -eq $fakeClient) 'Chosen client path updates the selected profile and visible field'
    # Force the settings debounce interval open, then inspect only this synthetic profile.
    [xBot.App.Settings].GetField('s_lastBotSaveTick', [Reflection.BindingFlags]'Static,NonPublic').SetValue($null, [long]0)
    [xBot.App.Settings]::SaveBotSettings()
    $json = Get-Content -LiteralPath (Join-Path $outputPath 'Settings.user.json') -Raw | ConvertFrom-Json
    Check ($json.Silkroads.'Test Profile'.ClientPath -eq $fakeClient) 'Client path persists in the same profile used by launch'
    Capture 'login-settings.png'
    $tabs.SelectedTab = $tabs.TabPages['pageConnect']
    Capture 'login-connect.png'

    $window.WindowState = 'Minimized'
    [Windows.Forms.Application]::DoEvents()
    Check (!$window.Visible -and !$window.ShowInTaskbar -and $window.NotifyIcon.Visible) 'Native minimize hides the taskbar window and keeps the tray icon'
    $clickArgs = (New-Object Windows.Forms.MouseEventArgs('Left',1,0,0,0)).psobject.BaseObject
    $window.GetType().GetMethod('NotifyIcon_MouseClick', $flags).Invoke($window, [object[]]@($window.NotifyIcon, $clickArgs)) | Out-Null
    Check ($window.Visible -and $window.ShowInTaskbar -and $window.WindowState -eq 'Normal') 'Tray single click restores the window'
    $window.GetType().GetMethod('Control_Click', $flags).Invoke($window,
        [object[]]@((Field $window 'btnWinMinimize'), [EventArgs]::Empty)) | Out-Null
    Check (!$window.Visible -and $window.NotifyIcon.Visible) 'Custom minimize also goes to the tray'
    $doubleClickArgs = (New-Object Windows.Forms.MouseEventArgs('Left',2,0,0,0)).psobject.BaseObject
    $window.GetType().GetMethod('NotifyIcon_MouseDoubleClick', $flags).Invoke($window, [object[]]@($window.NotifyIcon, $doubleClickArgs)) | Out-Null
    Check ($window.Visible -and $window.ShowInTaskbar -and $window.WindowState -eq 'Normal') 'Tray double click restores the window'
    (Field $window '_alwaysOnTopMenu').Checked = $true
    Check ($window.TopMost) 'Always-on-top can be enabled from tray menu'
    (Field $window '_alwaysOnTopMenu').Checked = $false
    Check (!$window.TopMost) 'Always-on-top can be disabled for the current session'
} finally {
    [xBot.App.Bot]::Get.Proxy = $null
    $window.Dispose()
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolveHandler)
}
