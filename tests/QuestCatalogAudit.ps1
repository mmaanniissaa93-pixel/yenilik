param(
    [Parameter(Mandatory = $true)][string]$MediaPath,
    [Parameter(Mandatory = $true)][string]$AssemblyPath
)

# Run with 32-bit Windows PowerShell for the .NET Framework/x86 bot assembly.
# Reads PK2 only; does not touch the selected character or generated database.
$ErrorActionPreference = 'Stop'
[Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path) | Out-Null
$pk = New-Object xBot.PK2Extractor.PK2ReaderAPI.Pk2Reader((Resolve-Path -LiteralPath $MediaPath).Path, '')
try {
    $positions = New-Object 'System.Collections.Generic.HashSet[string]'
    $modelsWithPositions = New-Object 'System.Collections.Generic.HashSet[uint32]'
    $rejected = 0
    foreach ($file in $pk.Files | Where-Object { $_.Name -eq 'npcpos.txt' }) {
        foreach ($line in ($pk.GetFileText($file) -split "`n")) {
            if (-not $line.Trim() -or $line.TrimStart().StartsWith('//')) { continue }
            $position = [xBot.App.QuestNpcCatalogPolicy]::ParsePosition($line)
            if ($null -eq $position) { $rejected++; continue }
            $positions.Add(('{0}:{1}:{2}:{3}:{4}' -f $position.ModelId, $position.Region, $position.X, $position.Z, $position.Y)) | Out-Null
            $modelsWithPositions.Add($position.ModelId) | Out-Null
        }
    }
    $languageIndex = 8
    $language = [regex]::Match($pk.GetFileText('type.txt'), 'Language\s*=\s*"([^"\r\n]+)"').Groups[1].Value
    if ($language -eq 'Vietnam') { $languageIndex = 9 }
    if ($language -eq 'Russia') { $languageIndex = 10 }
    $names = @{}
    $base = 'server_dep\silkroad\textdata\'
    foreach ($nameFile in (($pk.GetFileText($base + 'TextDataName.txt') + "`n" + $pk.GetFileText($base + 'textquest.txt')) -split "`n")) {
        if (-not $nameFile.Trim()) { continue }
        foreach ($line in ($pk.GetFileText($base + $nameFile.Trim()) -split "`n")) {
            $fields = $line -split "`t"
            if ($fields.Length -gt $languageIndex -and $fields[0] -eq '1') { $names[$fields[1]] = $fields[$languageIndex] }
        }
    }
    $npcs = @()
    foreach ($modelFile in ($pk.GetFileText($base + 'CharacterData.txt') -split "`n")) {
        if (-not $modelFile.Trim()) { continue }
        foreach ($line in ($pk.GetFileText($base + $modelFile.Trim()) -split "`n")) {
            $fields = $line -split "`t"
            if ($fields.Length -gt 12 -and $fields[0] -eq '1' -and $fields[10] -eq '2' -and $fields[11] -eq '2') {
                $npcs += [pscustomobject]@{ Id = [uint32]$fields[1]; Code = $fields[2]; Name = $names[$fields[5]] }
            }
        }
    }
    $npcs = @($npcs | Group-Object Id | ForEach-Object { $_.Group[-1] })
    $unresolvedExamples = @()
    $resolved = 0; $total = 0; $ambiguous = 0; $missingPosition = 0
    $examples = @()
    foreach ($line in ($pk.GetFileText($base + 'questdata.txt') -split "`n")) {
        $fields = $line -split "`t"
        if ($fields.Length -lt 10 -or $fields[0] -ne '1') { continue }
        $total++
        $notice = $names[$fields[9]]
        $matches = @($npcs | Where-Object { [xBot.App.QuestNpcCatalogPolicy]::MatchesQuestNpc($fields[2], $notice, $_.Code, $_.Name) })
        if ($matches.Count -eq 1) {
            if ($modelsWithPositions.Contains($matches[0].Id)) {
                $resolved++
                if ($examples.Count -lt 5) { $examples += ('{0} -> {1}' -f $fields[2], $matches[0].Code) }
            } else { $missingPosition++ }
        } elseif ($matches.Count -gt 1) { $ambiguous++ }
        elseif ($unresolvedExamples.Count -lt 12) { $unresolvedExamples += ('{0}: {1}' -f $fields[2], $notice) }
    }
    [pscustomobject]@{
        UniquePlacements = $positions.Count; PositionedModels = $modelsWithPositions.Count
        InvalidPositionRows = $rejected; QuestCount = $total; ResolvedQuestNpc = $resolved
        AmbiguousQuestNpc = $ambiguous; MatchedNpcWithoutPosition = $missingPosition
        UnresolvedQuestNpc = $total - $resolved - $ambiguous - $missingPosition
        Examples = $examples; UnresolvedExamples = $unresolvedExamples
    } | ConvertTo-Json
} finally { $pk.Dispose() }
