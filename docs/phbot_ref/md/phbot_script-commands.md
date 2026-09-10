> For the complete documentation index, see [llms.txt](https://guide.phbot.org/llms.txt). Markdown versions of documentation pages are available by appending `.md` to page URLs; this page is available as [Markdown](https://guide.phbot.org/phbot/script-commands.md).

# Script Commands

All commands are case sensitive and the parameters must be separated by commas.

***

#### walk

This command causes the bot to make the character walk to a set of coordinates.

Parameters

* `region` (optional)
  * Caves use a different coordinate system which is what this first parameter is for. It is not required anywhere but caves.
* `x`
  * In game X coordinate.
* `y`
  * In game Y coordinate.
* `z`
  * In game Z coordinate. This value can be set to 0 and it will not affect anything.

Example

* `walk,6400,800,0` (outside cave)
* `walk,32775,-23232,-302,-76` (in cave)

#### teleport

This command uses an in game teleporter.

Parameters

* `source name`
  * Source NPC name or region name.
* `destination name`
  * Destination NPC name or region name.

Example

* `teleport,Jangan,Donwhang`
* `teleport,Ferry Ticket Seller Doji,Ferry Ticket Seller Tayun`

#### wait

Waits a specified amount of time in milliseconds. 1 second = 1000 milliseconds.

Parameters

* `time`
  * Amount of time to wait in milliseconds

Example

`wait,5000`

#### cast

Casts a skill.

Parameters

* `skill name`
  * Name of the skill to cast. It will first look for the exact name of the skill and then do a search looking for any skill that has the text in its name if it was not found.

Example

`cast,Invisible`

#### DoBlacksmith, DoHerbalist, DoStable, DoStorage, DoGuildStorage, DoGroceryTrader, DoProtectorTrader, DoJupiter

Enters the specified NPC to buy/sell items or store them depending on the NPC. `DoJupiter` is for only inside the Jupiter Temple. The Jupiter NPC is a combination of a Blacksmith and Herbalist.

#### DoStorageTake, DoGuildStorageTake

Takes an item from storage/guild storage. This command will not do anything more than take items from the NPC.

#### DoStorageStore, DoGuildStorageStore

Stores an item in storage/guild storage. This command will not do anything more than store items in the NPC.

#### DoConsignment

Enters the Consignment NPC to add items that are configured to be sold on the [Stall](broken://pages/APSwjR83kyiGdgFhUOdB) tab.

#### DoStall

Creates a stall if you have items to stall.

#### DoScript

Executes the walk script. This command can only be used in town scripts.

#### mount

Mounts the fellow/transport if you have one. If no arguments are supplied, it will mount the fellow pet.

Arguments

* `type`
  * `fellow` or `transport`

Example

* `mount,fellow`
* `mount,transport`

#### killhorse

Terminates the transport/horse. This will completely destroy the horse and drop items if it is a trade transport. See below for `dismount`.

Example

`killhorse`

#### terminate

Terminates a pet. If no arguments are supplied, it will terminate all.

Arguments

* `type`
  * `horse` or `transport`

Example

* `terminate,horse`
* `terminate,transport`

#### dismount

Dismounts the fellow/transport/horse. This is different from `killhorse` and `terminate` because it will get off the transport instead of killing it. For horses it will still terminate them.

#### quest

Accepts/gets the reward for a quest. Event quests from So-Ok are supported as well. Only repeatable quests are currently supported.

Parameters

* `npc`
  * The name of the NPC that has the quest.
* `quest name`
  * The name of the quest in the NPC.
* `trade type` (optional)
  * This last parameter is used for the new iSRO/SilkroadR job quests that require you to collect job items using a transport. The parameters for this are `safe` or `danger`.

Example

* `quest,Lost Soldier Chrom,Twisted (Unlimited Repeats)` (normal repeatable quest)
* `quest,Smuggler Chao,Collecting Trade Goods - Wild Ginseng! (Lvl 1-9),safe` (job quest)

### Job Cave Guide

1. Make a normal walk script that goes from the town spawn to the cave quest NPC.
2. Add all of the quests for that NPC to your walk script.
3. Continue to your training area.
4. Save the script like you normally would.
5. Create another script that goes from your training area back to the quest NPC.
6. Add the same quests again to this script.
7. Save it and do **not** walk back to your training area! The bot will execute the script in reverse after the script finishes.
8. Go to the quest tab in the bot and enable those quests and set the script.

#### use

Uses an item. Return scrolls are currently the only supported item.

Parameters

* `name`
  * Item name.

Example

`use,returnscroll`

#### begintargettrading

Begins target trading with the new Hunter/Thief trade NPCs that exist on iSRO and SilkroadR. This should be accompanied by the `settletargettrading` command after the target trade has completed.

Example

`begintargettrading`

#### settletargettrading

Settles the target trade.

Example

`settletargettrading`

#### styria

Registers for Styria when it is time. You must also have it set to return for Styria under the [Protection](broken://pages/APSwjR83kyiGdgFhUOdB) tab. The time is configured for iSRO and SilkroadR. If you wish to register for Styria on a private server, you may do so by using the [Map](broken://pages/APSwjR83kyiGdgFhUOdB) tab.

Example

`styria`

#### oldtrade

Buys, sells, and spawns a transport for the old job trade system on vSRO locales. For this to work, you must:

1. Create a script that goes from town spawn
2. To the Specialty Trader
3. Add `oldtrade,spawn`
4. Add `oldtrade,buy,1`
5. To another towns Specialty Trader
6. Add `oldtrade,sell`
7. Add `killhorse` to terminate the transport
8. Finally, `use,returnscroll` or teleport back to the original starting town

You may need to add a walk delay depending on the transport.

Paramters

* `buy`
  * Buys items at the Specialty Trader
  * `star` or `quantity`
    * Trade star count, 0 -> 5. A value of `0` will fill up the entire transport with items. A value greater than `5` will buy exactly that number of trade items.
  * `Item name` (optional)
    * Optional trade goods item name to buy
* `sell`
  * Sells items at the Specialty trader
* `spawn`
  * Spawns a trade transport
* `Item name` (optional)
  * Optional item scroll name to summon

Example

* `oldtrade,spawn`
* `oldtrade,buy,1`
* `oldtrade,sell`
* `oldtrade,spawn,Ironclad trade Horse`
* `oldtrade,buy,1,Horseshoe`
* `oldtrade,buy,250,Horsehoe`
* `oldtrade,buy,0`

#### stop

Stops the bot. This can be useful when you want to trade between towns and have the bot stop when it finishes.

Example

`stop`

#### disconnect

Disconnects from the server. This can be useful on certain private servers that have a trade ranking system that updates on reconnect.

Example

`disconnect`

#### recall

Recalls the pick pet. Can be useful in lure scripts.

Example

`recall`

#### profile

Loads a profile by name if it is not currently loaded.

Parameters

* `name`
  * Name of the profile (can be `Default` or empty to load the default profile)

Example

`profile,test`

