$ErrorActionPreference = 'Continue'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$base = 'https://guide.phbot.org'
$pages = @(
  'introduction.md','purchasing.md','download.md','manager.md',
  'supported-operating-systems.md','command-line.md','phbot.ini.md',
  'initial-startup.md','phbot/auto-configure.md','phbot/notifications.md',
  'phbot/protection.md','phbot/town.md','phbot/training-area.md',
  'phbot/script-commands.md','phbot/attack.md','phbot/pet.md',
  'phbot/party.md','phbot/union-party.md','phbot/pick-filter.md',
  'phbot/quest.md','phbot/players.md','phbot/guild.md','phbot/academy.md',
  'phbot/inventory.md','phbot/stall.md','phbot/alchemy.md','phbot/trade.md',
  'phbot/mastery.md','phbot/map.md','phbot/sound.md','phbot/key-bindings.md',
  'phbot/conditions.md'
)
$mdDir = 'C:\Users\auguu\Desktop\xBot-WinForms\docs\phbot_ref\md'
$imgDir = 'C:\Users\auguu\Desktop\xBot-WinForms\docs\phbot_ref\guide'
New-Item -ItemType Directory -Path $mdDir -Force | Out-Null
New-Item -ItemType Directory -Path $imgDir -Force | Out-Null
$manifest = @()
foreach ($p in $pages) {
  $slug = $p -replace '\.md$','' -replace '/','_'
  try {
    $md = Invoke-WebRequest -Uri ($base + '/' + $p) -UseBasicParsing -TimeoutSec 30
    $text = $md.Content
    $text | Out-File (Join-Path $mdDir ($slug + '.md')) -Encoding utf8
    Write-Output ('MD OK: ' + $p + ' (' + $text.Length + ')')
    $urls = @()
    foreach ($m in [regex]::Matches($text, '!\[[^\]]*\]\(([^)]+)\)')) { $urls += $m.Groups[1].Value }
    foreach ($m in [regex]::Matches($text, '<img[^>]+src="([^"]+)"')) { $urls += $m.Groups[1].Value }
    $urls = $urls | Sort-Object -Unique
    $i = 0
    foreach ($u in $urls) {
      $u = $u -replace '&amp;','&'
      $i++
      $ext = '.png'
      if ($u -match '\.(jpg|jpeg|gif|webp)(\?|$)') { $ext = '.img' }
      $fname = ($slug + '_' + $i.ToString('00') + $ext)
      try {
        Invoke-WebRequest -Uri $u -OutFile (Join-Path $imgDir $fname) -UseBasicParsing -TimeoutSec 60
        $sz = (Get-Item (Join-Path $imgDir $fname)).Length
        $manifest += ($fname + ' <= ' + $p)
        Write-Output ('  IMG OK: ' + $fname + ' (' + $sz + ')')
      } catch { Write-Output ('  IMG FAIL: ' + $u + ' :: ' + $_.Exception.Message) }
    }
  } catch { Write-Output ('MD FAIL: ' + $p + ' :: ' + $_.Exception.Message) }
}
$manifest | Out-File (Join-Path $imgDir 'MANIFEST.txt') -Encoding utf8
Write-Output 'DONE'
