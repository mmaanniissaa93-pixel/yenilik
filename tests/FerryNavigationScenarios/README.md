# Ferry final navigation

Run from the repository root (requires MSBuild / .NET Framework 4.8 and .NET 8 SDK):

```powershell
./tests/FerryNavigationScenarios.ps1
```

The console suite links the actual navigation and ferry executor source files; only UI / teleport DB dependencies are stubbed. The second suite exercises `SRModel` and `WaitMovement` from the built x86 application assembly. Neither is a live game/server test.

Verified offline fixture: player `(-4636,-31)`, board `(-4661.8,-36.2)`, `nav06.dat`: `11720 -> 11717 -> 11716`. Every leg is a graph edge, all wire regions are `23406`, and the endpoint is 4.3909 m from the board. Fixture coordinates are used only in tests.

Production approach chooses the closest reachable node on the player's mesh component, preserves the raw A* chain and Z, and scans live entities during each waypoint. Arrival tolerance is 0.75 m, independent of the attempt budget. Two replans are allowed; failed nodes are excluded. A normal route waypoint failure aborts its route. The original candidate scoring and teleport interaction remain unchanged.

For a cross-file approach, strict multi-region routing requires shared physical nodes including height. Bounding-box proximity alone does not prove a walkable connection. Routes containing teleport segments, missing legs, or unproven crossings are rejected rather than flattened or filled with direct movement.

## Live validation still required

No xBot / sro_client session was running while this change was implemented. Collision versus rejected/unacknowledged movement, actual entity streaming, and successful Nouth -> Roc transit have NOT been verified.

Normal runs emit `[FERRY-NAV]` path, waypoint, before/after, movement result, candidate and failure logs. `MoveTo queued` means injection was queued; it is not proof of server movement. Separate `server movement` and `server movement-stuck` logs report parsed server responses for the character or mount. `GetRealtimePosition` remains a server-destination-based interpolation, not independent position telemetry.

For the requested one-time A/B/C comparison, launch the built application with:

```powershell
$env:XBOT_FERRY_COMPARE_MOVES = '1'
& ./xBot/bin/x86/Release/xBot.exe
```

Start the ferry route while at the actual stall point (within 40 m of the board). This opt-in mode tries A: the former StandPointNear step, B: a 4 m geometric step, C: the next actual nav node. All use the same production WaitMovement -> Bot.MoveTo -> PacketBuilder path, including mounted movement, with 0.75 m tolerance and no geometric bypass. Each trial reads fresh runtime coordinates. Trials are sequential and do not rewind the character: compare their logged before positions, and do not treat them as identical-origin experiments if an earlier trial moved. Logs include whether the old 3 m tolerance would have skipped the target. Normal execution then follows the mesh approach.

Clear the environment variable before launching later normal runs:

```powershell
Remove-Item Env:XBOT_FERRY_COMPARE_MOVES
```
