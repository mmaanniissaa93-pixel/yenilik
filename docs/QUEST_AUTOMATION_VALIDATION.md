# Quest Automation v2: NPC discovery and server confirmation

Normal quest acceptance and turn-in now share one pending NPC conversation. Sending a menu selection or reward packet does not mark the quest successful.

| State | Trigger |
| --- | --- |
| Inactive | Configured quest is absent from the character's quest list |
| Going to NPC | Resolving/walking to the selected NPC identity |
| Accepting | Waiting for the menu and then a matching `0x30D5` add |
| Active | Server quest list contains an unfinished quest |
| Completed | Server quest state is 2 or 8 |
| Turning in | Reward request sent; matching `0x30D5` remove still required |
| Waiting to repeat | Confirmed removal; five-second cooldown before another acceptance |
| Failed | Timeout, unavailable menu, unresolved NPC, ambiguous reward, interrupted travel, or abandonment |

Each dialog/confirmation stage has a 12-second timeout. Transient failures allow at most three attempts, with 5- and 10-second retry delays. Saving a rule clears its exhausted retry budget. Duplicate menus/reward-ID responses cannot send the same action twice. Updates to other quest IDs and abandon updates cannot confirm delivery. A late removal can confirm a timed-out reward request before another transaction replaces it. Stopping the bot or teleporting releases the conversation; disconnect clears runtime state.

The NPC importer reads every `npcpos.txt` in Media.pk2, preserving multiple placements and signed dungeon region IDs. Its field order is **model, region, X, Z (height), Y**. Quest text references come from `textquest.txt` as well as `TextDataName.txt`; translated `NoticeNPC` text identifies the NPC. A quest-family code can corroborate a personal name when the notice omits a title. Different matching model identities remain ambiguous. Navigation chooses a placement in the current world and requires a navmesh path and a nearby live NPC with the same identity. The known QEV milestone family retains its potion-merchant fallback.

Reward preferences are saved per quest: equipped weapon, selected weapon type, exact item name, or a reward chosen from the catalog. Multiple matches require an explicit selection. Weapon matching checks item class as well as TID4. A missing reward table is an error, rather than evidence that a quest has no selectable reward. The existing verified fixed-reward format for zero/one catalog item and selectable-reward format are preserved; other server reward layouts still require live verification.

## Local verification

```powershell
MSBuild xBot/xBot.csproj /p:Configuration=Debug /p:Platform=x86 /v:minimal /nologo
dotnet run --project tests/ProtectionScenarios/ProtectionScenarios.csproj
dotnet run --project tests/QuestAutomationScenarios/QuestAutomationScenarios.csproj
```

`QuestAutomationScenarios` compiles the production manager and policy with fake game/network boundaries. It checks profile roundtripping, conversation ownership, duplicate responses, matching server updates, repeat cooldown, timeout, unavailable menus, wrong quest IDs, and disconnect. These simulations do not verify the server's wire layout or quest scripts.

Run the read-only catalog audit using 32-bit Windows PowerShell:

```powershell
& "$env:WINDIR/SysWOW64/WindowsPowerShell/v1.0/powershell.exe" -NoProfile `
  -File tests/QuestCatalogAudit.ps1 -MediaPath '<client>/Media.pk2' `
  -AssemblyPath xBot/bin/x86/Debug/xBot.exe
```

The tested SevarOnline media contains 12,367 unique placements for 589 model IDs, with no rejected position rows. The final audit resolves 682 of 734 quest NPC mappings; 7 are ambiguous and 45 remain unresolved. Some notices are empty, generic, misspelled, or identify multiple models. Discovery is not a claim that every catalog quest can run automatically.

## Live validation still required

Rebuild the PK2 database to populate `npc_positions` and `quests.notice_npc_text`. Existing profiles remain readable. Select a normal repeatable quest with a resolved NPC and configure its reward before running the bot.

1. With the quest absent, enable automation. Verify the bot reaches the intended NPC and only changes from Accepting after the matching server add.
2. Complete the objectives and verify Completed, then Turning in. Keep the quest pending until the server removes it; record both quest ID and update type.
3. Verify Waiting to repeat after removal, followed by a new acceptance and a fresh matching add. With repeat disabled, verify no new acceptance.
4. Exercise an unavailable quest menu and an unacknowledged request; verify the retry bound. Exercise stop/teleport during the conversation and verify no later dialog/reward is sent for the canceled conversation.
5. Validate a quest with different accepting/delivering NPCs separately: NoticeNPC alone does not encode a universal per-stage NPC mapping. Confirm server-specific selectable reward semantics with an actual multiple-reward quest.

No live normal quest or repeat cycle was executed during this implementation. Quest Automation remains open until those checks and unresolved NPC mappings are addressed. The next feature order remains Dismantle, guild storage/gold, consignment, party buffs, mastery/skills.
