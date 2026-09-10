$ErrorActionPreference = 'Continue'
$headers = @{
  'Accept' = 'application/json, text/event-stream'
  'MCP-Protocol-Version' = '2025-03-26'
}
function Mcp-Call($method, $paramsJson, $sid) {
  $h = $headers.Clone()
  if ($sid) { $h['mcp-session-id'] = $sid }
  $body = '{"jsonrpc":"2.0","id":2,"method":"' + $method + '","params":' + $paramsJson + '}'
  $r = Invoke-WebRequest -Uri 'https://guide.phbot.org/~gitbook/mcp' -Method Post -Body $body -ContentType 'application/json' -Headers $h -TimeoutSec 60 -UseBasicParsing
  $newsid = $r.Headers['mcp-session-id']
  return @{ Content = $r.Content; Sid = $newsid }
}
try {
  $init = Mcp-Call 'initialize' '{"protocolVersion":"2025-03-26","capabilities":{},"clientInfo":{"name":"xbot","version":"1.0"}}' $null
  $sid = $init.Sid
  if ($sid -is [array]) { $sid = $sid[0] }
  Write-Output ('SID: ' + $sid)
  $tools = Mcp-Call 'tools/list' '{}' $sid
  $tools.Content | Out-File 'C:\Users\auguu\Desktop\xBot-WinForms\docs\phbot_ref\mcp_tools.json' -Encoding utf8
  $t = $tools.Content
  if ($t.Length -gt 4000) { $t = $t.Substring(0, 4000) }
  Write-Output $t
} catch {
  Write-Output ('FAIL: ' + $_.Exception.Message)
}
