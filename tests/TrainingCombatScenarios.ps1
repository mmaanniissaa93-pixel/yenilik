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
$bot = $null
try {
    $window.StartPosition = 'Manual'
    $window.Location = New-Object Drawing.Point(-20000,-20000)
    $window.ShowInTaskbar = $false
    $window.Show()
    $window.GetType().GetMethod('TabPageV_Option_Click', $flags).Invoke($window,
        @((Field $window 'TabPageV_Control01_Training'), [EventArgs]::Empty)) | Out-Null
    $collisionCheck = $window.Controls.Find('PhBot_Collision_EnableInTraining', $true)[0]
    $bypassCheck = $window.Controls.Find('PhBot_Collision_NavigateAround', $true)[0]
    $collisionCheck.Checked = $false
    $bypassCheck.Checked = $false
    Check (![xBot.App.CollisionPolicy]::EnableCollisionInTrainingArea -and ![xBot.App.CollisionPolicy]::NavigateAroundObstacles) 'Collision checkboxes disable runtime policies'
    $collisionCheck.Checked = $true
    $bypassCheck.Checked = $true
    Check ([xBot.App.CollisionPolicy]::EnableCollisionInTrainingArea -and [xBot.App.CollisionPolicy]::NavigateAroundObstacles) 'Collision checkboxes enable runtime policies'
    $list = Field $window 'Training_lstvAreas'
    $info = New-Object xBot.App.TrainingAreaInfo
    $info.Name = 'Saved area'
    $info.Radius = 50
    $info.PickRadius = 50
    $item = New-Object Windows.Forms.ListViewItem($info.Name)
    $item.Name = $info.Name
    $item.Tag = $info
    foreach ($value in @('', '50', '50', 'Menzil')) { $item.SubItems.Add($value) | Out-Null }
    $list.Items.Clear()
    $list.Items.Add($item) | Out-Null
    $list.Tag = $item
    # Reproduce layout after a saved active area was restored.
    $list.ContextMenuStrip = $null
    $handler = [Delegate]::CreateDelegate([EventHandler], $window,
        $window.GetType().GetMethod('TrainingAreaList_DoubleClick', $flags))
    $list.remove_DoubleClick($handler)
    $window.GetType().GetMethod('LayoutTrainingAreaInner', $flags).Invoke($window, @()) | Out-Null
    Check ($list.ContextMenuStrip.Name -eq 'TrainingAreaEditMenu') 'Active area still initializes editing'
    $item.Selected = $true
    $item.Focused = $true
    [Windows.Forms.Application]::DoEvents()
    $script:dialogSeen = $false
    $script:dialogError = $null
    $timer = New-Object Windows.Forms.Timer
    $timer.Interval = 100
    $timer.add_Tick({
        $dlg = @([Windows.Forms.Application]::OpenForms | Where-Object { $_ -is [xBot.App.TrainingAreaEditForm] }) | Select-Object -First 1
        if ($null -eq $dlg) { return }
        $timer.Stop()
        try {
            $script:dialogSeen = $true
            (Field $dlg 'numRadius').Value = 120
            (Field $dlg 'numPickRadius').Value = 75
            (Field $dlg 'txtName').Text = 'Edited area'
            $bitmap = New-Object Drawing.Bitmap($dlg.Width, $dlg.Height)
            try {
                $dlg.DrawToBitmap($bitmap, (New-Object Drawing.Rectangle(0,0,$dlg.Width,$dlg.Height)))
                $bitmap.Save((Join-Path $outputPath 'training-area-edit.png'))
            } finally { $bitmap.Dispose() }
            $dlg.AcceptButton.PerformClick()
        } catch { $script:dialogError = $_; $dlg.Close() }
    })
    try {
        $timer.Start()
        [Windows.Forms.Control].GetMethod('OnDoubleClick', $flags).Invoke($list, @([EventArgs]::Empty)) | Out-Null
    } finally { $timer.Stop(); $timer.Dispose() }
    if ($script:dialogError) { throw $script:dialogError }
    Check $script:dialogSeen 'Double click opens the editor with an active saved area'
    Check ($item.SubItems[2].Text -eq '120' -and $item.SubItems[3].Text -eq '75') 'Both edited radii update the row'
    Check ($window.TrainingArea_GetRadius() -eq 120 -and $window.TrainingArea_GetPickRadius() -eq 75) 'Runtime reads both edited radii'
    Check ($item.Name -eq 'Edited area') 'Renamed active area keeps its persistence key in sync'
    foreach ($radius in @(1, 4, 1500)) {
        $info.Radius = $radius
        $info.PickRadius = $radius
        $dlg = New-Object xBot.App.TrainingAreaEditForm($info)
        try { Check ((Field $dlg 'numRadius').Value -eq $radius) "Existing radius $radius opens without throwing" }
        finally { $dlg.Dispose() }
    }

    $bot = [xBot.App.Bot]::Get
    $bot.GetType().GetField('tBotting', $flags).SetValue($bot, [Threading.Thread]::CurrentThread)
    $mob = [Runtime.Serialization.FormatterServices]::GetUninitializedObject([xBot.Game.Objects.Entity.SRMob])
    $mob.UniqueID = 123
    $mob.LifeStateType = [xBot.Game.Objects.Entity.SRModel+LifeState]::Alive
    [xBot.Game.InfoManager]::Mobs[$mob.UniqueID] = $mob
    $live = $bot.GetType().GetMethod('IsLiveCombatTarget', [Reflection.BindingFlags]'Static,NonPublic')
    $select = $bot.GetType().GetMethod('EnsureCombatTargetSelected', $flags)
    [xBot.Game.InfoManager].GetMethod('OnEntitySelected', [Reflection.BindingFlags]'Static,NonPublic').Invoke($null, @($mob.UniqueID)) | Out-Null
    Check ($select.Invoke($bot, @($mob))) 'Confirmed living target can attack'
    Add-Type -TypeDefinition @'
public static class DelayedCombatReply {
    public static System.Threading.Timer Signal(System.Threading.EventWaitHandle handle) {
        return new System.Threading.Timer(_ => handle.Set(), null, 400, System.Threading.Timeout.Infinite);
    }
}
'@
    $waitCast = $bot.GetType().GetMethod('WaitForCombatCast', $flags)
    [xBot.Game.InfoManager]::MonitorSkillCast.Reset() | Out-Null
    $delayedReply = [DelayedCombatReply]::Signal([xBot.Game.InfoManager]::MonitorSkillCast)
    try { Check ($waitCast.Invoke($bot, @($mob))) 'Cast reply delayed beyond 250 ms is still accepted' }
    finally { $delayedReply.Dispose() }
    $mob.LifeStateType = [xBot.Game.Objects.Entity.SRModel+LifeState]::Dead
    Check (!$live.Invoke($null, @($mob))) 'Dead target is rejected even before despawn'
    Check (!$select.Invoke($bot, @($mob))) 'Dead selected target cannot attack'
    [xBot.Game.InfoManager]::MonitorSkillCast.Reset() | Out-Null
    $elapsed = [Diagnostics.Stopwatch]::StartNew()
    Check (!$waitCast.Invoke($bot, @($mob))) 'Target death stops waiting for a cast reply'
    Check ($elapsed.ElapsedMilliseconds -lt 500) 'Target death does not incur the full reply timeout'
    $mob.LifeStateType = [xBot.Game.Objects.Entity.SRModel+LifeState]::Alive
    [xBot.Game.InfoManager]::Mobs.RemoveKey($mob.UniqueID)
    Check (!$live.Invoke($null, @($mob))) 'Removed target remaining in cache is rejected'
    Check (![xBot.App.CombatPolicy]::CanSendAttack($true, 123, 456)) 'Unrelated selection cannot authorize an attack'
    Check (![xBot.App.CombatPolicy]::CanSendAttack($true, 123, 0)) 'Failed selection cannot authorize an attack'
} finally {
    if ($bot) { $bot.GetType().GetField('tBotting', $flags).SetValue($bot, $null) }
    $window.Dispose()
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolveHandler)
}
