param(
    [Parameter(Mandatory=$true)][string]$AssemblyPath,
    [Parameter(Mandatory=$true)][string]$OutputDirectory
)
# Isolated WinForms rendering with example data. No Window/Bot singleton or account.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
[Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path) | Out-Null
[IO.Directory]::CreateDirectory($OutputDirectory) | Out-Null
$form = New-Object Windows.Forms.Form
$form.ClientSize = New-Object Drawing.Size(1208, 660)
$form.StartPosition = 'Manual'
$form.Location = New-Object Drawing.Point(-20000, -20000)
$form.ShowInTaskbar = $false
$view = New-Object xBot.App.QuestPanel
$view.Dock = 'Fill'
$form.Controls.Add($view)
$active = New-Object 'System.Collections.Generic.List[xBot.App.QuestListEntry]'
$active.Add((New-Object xBot.App.QuestListEntry -Property @{ Id=559; Name='Becoming a Deity (1)'; State='Devam ediyor'; Objectives='Hunting 300 Unegs'; Enabled=$true }))
$catalog = New-Object 'System.Collections.Generic.List[xBot.App.QuestListEntry]'
$catalog.Add((New-Object xBot.App.QuestListEntry -Property @{ Id=604; Name='The Beginning of the Temple Dispute (Trader)'; Npc='Trader Union President Nanuakt'; Level=105; Enabled=$true; State='Alınmadı'; Completion="NPC'ye teslim et" }))
$catalog.Add((New-Object xBot.App.QuestListEntry -Property @{ Id=605; Name='The Beginning of the Temple Dispute (Hunter)'; Npc='Hunter Union President Namner'; Level=105; Enabled=$false; State='Alınmadı'; Completion="NPC'ye teslim et" }))
$catalog.Add((New-Object xBot.App.QuestListEntry -Property @{ Id=606; Name='The Beginning of the Temple Dispute (Thief)'; Npc='Thief Union President Tausert'; Level=105; Enabled=$false; State='Alınmadı'; Completion="NPC'ye teslim et" }))
try {
    $view.SetActive($active); $view.SetCatalog($catalog, $true)
    $view.SetStatus('Önizleme · Örnek görev verileri')
    $form.Show()
    foreach ($index in 0..2) {
        $view.SelectTab($index)
        [Windows.Forms.Application]::DoEvents()
        $bitmap = New-Object Drawing.Bitmap($view.Width, $view.Height)
        try {
            $view.DrawToBitmap($bitmap, (New-Object Drawing.Rectangle(0,0,$view.Width,$view.Height)))
            $path = Join-Path $OutputDirectory ('quest-' + @('active','all','options')[$index] + '.png')
            $bitmap.Save($path, [Drawing.Imaging.ImageFormat]::Png)
            Write-Output $path
        } finally { $bitmap.Dispose() }
    }
    $sections = New-Object 'System.Collections.Generic.List[System.Collections.Generic.KeyValuePair[string,string]]'
    $sections.Add((New-Object 'System.Collections.Generic.KeyValuePair[string,string]' ('Görev', 'Speak to Trader/Hunter Union Representative Kapado.')))
    $sections.Add((New-Object 'System.Collections.Generic.KeyValuePair[string,string]' ('Ödüller', 'EXP 5412921 / Skill Points 1100 / Gold 2340')))
    $sections.Add((New-Object 'System.Collections.Generic.KeyValuePair[string,string]' ('NPC', 'Trader Union President Nanuakt / Trader-Hunter Union Representative Kapado')))
    $sections.Add((New-Object 'System.Collections.Generic.KeyValuePair[string,string]' ('Teslim koşulu', 'Görev hedefini tamamlayıp NPC ile konuşun.')))
    $detail = [xBot.App.QuestPanel]::CreateDetailsDialog('The Beginning of the Temple Dispute (Trader)', $sections)
    try {
        $detail.StartPosition = 'Manual'; $detail.Location = New-Object Drawing.Point(-20000, -20000); $detail.ShowInTaskbar = $false
        $detail.Show(); [Windows.Forms.Application]::DoEvents()
        $bitmap = New-Object Drawing.Bitmap($detail.Width, $detail.Height)
        try { $detail.DrawToBitmap($bitmap, (New-Object Drawing.Rectangle(0,0,$detail.Width,$detail.Height))); $bitmap.Save((Join-Path $OutputDirectory 'quest-details.png'), [Drawing.Imaging.ImageFormat]::Png) }
        finally { $bitmap.Dispose() }
    } finally { $detail.Close(); $detail.Dispose() }
} finally { $form.Close(); $form.Dispose() }
