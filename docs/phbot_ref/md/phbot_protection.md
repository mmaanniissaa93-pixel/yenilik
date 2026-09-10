> For the complete documentation index, see [llms.txt](https://guide.phbot.org/llms.txt). Markdown versions of documentation pages are available by appending `.md` to page URLs; this page is available as [Markdown](https://guide.phbot.org/phbot/protection.md).

# Protection

The "Protection" tab contains your auto potion settings, return options, and monster attack preferences.

## Potions <a href="#potions" id="potions"></a>

To enable the potion type, check the box. Potions are used when your HP/MP stats drop below a percentage that you have set.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FiOoI4MEZdgvCBs5hoTCT%2Fprotection_potions.png?alt=media&amp;token=fb3934b8-ad29-4df4-95d3-c4edc11e7dbb" alt=""><figcaption></figcaption></figure>

## Return <a href="#return" id="return"></a>

The "Return" tab sets the options for returning to town.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FfozpXo3KOSr4oyLiBlUQ%2Fprotection_return.png?alt=media&amp;token=0ef70a71-ad4e-4e37-b874-7392084ae64f" alt=""><figcaption></figcaption></figure>

1. HP potions <= X
   * Return to town when you have less than a certain amount of HP potions.
2. <= 5 MP potions left
   * Return when you have <= 5 MP potions in your inventory.
3. Out of universal pills
   * Return when you have no more Universal Pills in your inventory.
4. Out of vigors
   * Return when you are out of Vigor potions.
5. Out of arrows/bolts
   * Return when you have no more arrows or bolts in your inventory.
   * The bot will count arrows for Chinese characters and Bolts for European characters if you have a Bow / Crossbow.
6. Out of speed scrolls
   * Return when you are out of movement speed scrolls.
7. Inventory is full
   * This option will return when you have 2 free slots empty.
   * It should be enabled so you always have room to pick up item drops and can also still switch weapons.
8. Weapon durability
   * Returns when the weapons durability is less than 4.
9. Shield durability
   * Returns when the shield durability is less than 4.
10. Armor durability

* Returns when any armor durability is less than 4.

11. Union Party Tickets

* Return when out of union party tickets.

12. Energy of Life

* Returns when out of Energy of Life items.

13. Return X minutes before the next hour

* This option can be used to return for certain in game events.

14. Return every X minutes

* This option can be used for Bards or Clerics that rarely return to town.

15. Return at ... every day

* This option can be used to get the Genie Lamp quest every day.

16. Return/disconnect ... minutes

* This option will return to town and disconnect every set minutes. It can be used to refresh the grey bar experience a certain number of times.

17. Styria

* Return for Styria.
* You must have the `styria` command in your script for it to register. The `styria` command is specifically tuned for iSRO/SilkroadR and will not work on private servers that have it at a different time. You can still register for Styria through the map tab though.

18. Return if less than X players are in the party

* Returns to town when the party does not have the set number of players.

19. Disconnect every X minutes

* This option will disconnect after being in game for X amount of minutes.

20. Reset play time (VTC)

* This option is for VTC only and will use the Silk mall to reset your play time so the character can gain EXP again.

21. Dead

* Return to town when dead.

22. Not resurrected within X minute(s)

* Waits for someone to resurrect you and then returns to town.

23. Return immediately if dead outside of the training area

* Instead of waiting, it will return immediately if you aren't inside the training area.

24. Use resurrection scroll

* Uses a resurrection scroll, if you have one, instead of returning to town.

25. Not attacked within X minute(s)

* Return to town if you are not attacked by a monster within a specified amount of time.

26. Unique spawns near you

* Return when a unique spawns near you.

27. Job transport dies

* Use a return scroll when the job transport dies.
* This is useful when doing job quests that require a transport.

28. Pink status

* Return if your character has pink murder status.

29. Murder status

* Disconnects if your character has murder status.

30. Change primary weapon if broken

* Switches the primary weapon to the same item type if it breaks instead of returning to town on weapon durability.

## Sockets <a href="#sockets" id="sockets"></a>

Use gear socket skills.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FvrTiJckkeUs7vyvGjlY1%2Fprotection_sockets.png?alt=media&amp;token=f9df5ec0-813e-4577-9017-97cfb6407a73" alt=""><figcaption></figcaption></figure>

## Pet Return <a href="#petreturn" id="petreturn"></a>

Pet return options.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FSKa6PVoSrWfH2iTC7xL1%2Fprotection_petreturn.png?alt=media&amp;token=885f8872-ec3d-4e60-b981-5f12a4a9e741" alt=""><figcaption></figcaption></figure>

1. Pet recovery kits
   * Return when out of recovery kits (fellow pet)
2. Pet revive
   * Return when out of pet revive items
3. Pet feed
   * Return when out of feed items
4. Abnormal state
   * Return when out of abnormal state potions for the pet
5. Transport recovery kits
   * Return when out of transport recovery kits (wolf pet)
6. Pick pet full
   * Return when the pick pet is full

## Berserk <a href="#monster" id="monster"></a>

The "Berserk" tab allows you to set preferences for berserk.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2F4JFOZMElqT7LadANNrDn%2Fprotection_berserk.png?alt=media&amp;token=701ee66f-8cc4-4f8a-ae9d-1a356a26421c" alt=""><figcaption></figcaption></figure>

## **Berserk**

These options control berserk for when the bot attacks a specific monster type, more than a certain number of monsters are attacking you, or while executing the script.

## Monster Preferences <a href="#monsterpreferences" id="monsterpreferences"></a>

"Monster Preferences" allow you to prefer one monster type or name over another.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2Fvb05bHqfhExNlyd668VB%2Fprotection_monster_preferences.png?alt=media&amp;token=c12bdb11-7d81-4340-abd6-b3b4c98e8c97" alt=""><figcaption></figcaption></figure>

1. List of your preferences
   * Right click on the window to add or change preferences.
2. Switch monster based on position in the list
   * If the bot is attacking a monster, switch to a more preferred monster based on its position in the list.
3. Up and down buttons
   * Select a preference from the list on the left and use the arrows to change their positions.
   * Monsters will be selected based on their position in the list. The higher the monster type or name is, the more of a priority it will be.

Selecting the "type" option gives you the ability to prefer a monster based on its type (eg. General, Champion, Giant, etc.).

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FzJttui4jrvah3L7GTp8c%2Fprotection_monster_preferences_name.png?alt=media&amp;token=46b594ad-6a85-4d2b-9e0c-6f126ce1ec3e" alt=""><figcaption></figcaption></figure>

Selecting the "name" option gives you the ability to prefer a monster based on its name (eg. Mangyang, Tiger, Chakji, etc.). This feature uses monster IDs which allows you to prefer an exact monster.

1. Search results
   * Right click on the window to add a monster to the preferences.
2. Search box
   * Enter in a monster name and the bot will look for it.
3. Search button
   * After entering the monster name you can either hit enter or click this button to search for the monster.
4. Add
   * Right click on the monster and click add to add the monster to the preferences list.

## Devil's Spirit <a href="#devilsspirit" id="devilsspirit"></a>

Uses Devil's Spirit / Angel's Spirit depending on the options selected.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FlUfq7S5Ya3GUcpDltMET%2Fprotection_devilsspirit.png?alt=media&amp;token=b886f966-2306-4234-8d7d-6e64ae61a495" alt=""><figcaption></figcaption></figure>

## Scrolls <a href="#scrolls" id="scrolls"></a>

The scrolls tab will allow you to select which scrolls should be automatically used by the bot. Scrolls are used while not botting, botting, and tracing unless otherwise set.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FeafuFvnMU6V71hmvv6jI%2Fscrolls.png?alt=media&amp;token=d5d6d80d-3747-4744-8d01-886af7a87f1b" alt=""><figcaption></figcaption></figure>

## Stats <a href="#stats" id="stats"></a>

The stats tab allows you to increase your Str/Int stat points and configure automatic stat point adding.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2F4i2KXTlfAqfq6utm9Rgu%2Fprotection_stats.png?alt=media&amp;token=7a3a1788-73ab-4b4b-95a2-b851a80a5088" alt=""><figcaption></figcaption></figure>

1. Allows you to manually increase your stat points
2. Enables automatic stat points
3. Set the number of stats to add for each level up

## Misc. <a href="#misc" id="misc"></a>

Additional miscellaneous options.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FIHoe5NRGXhE98PKYsljT%2Fprotection_misc.png?alt=media&amp;token=45e1dad5-1fc6-4abf-b8b1-8a06dd5c8dcb" alt=""><figcaption></figcaption></figure>

1. Stop bot when you die > X
   * Completely stop botting if you die more than a specified amount of times.
2. Stop if you die by a Statue of Justice
   * Joymax placed Statue's of Justice outside towns to kill botters but they removed them shortly after. This option is not necessary anymore.
3. Stop bot at 50% EXP (SilkroadR)
   * SilkroadR has a bar in the game that only allows you to gain EXP for a certain amount of time. The value starts at `100%` -> `50%` -> `0%`.
4. Stop bot at 0% EXP (SilkroadR)
   * Similar to #7. SilkroadR has a bar in the game that only allows you to gain EXP for a certain amount of time. After which, the bar turns grey and you do not gain anymore EXP.
5. Remove berserk from the game
   * Sometimes iSRO/SilkroadR will crash when someone uses berserk. This will disable the berserk animation and prevent the game from crashing.
   * This bug may have been fixed but there isn't really a way to know.
6. Remove invisible from the game
   * Allows you to see invisible players in the game.
   * iSRO/SilkroadR no longer send movement packets for invisible players so this option is useless there. However, on vSRO private servers it should still work fine.
7. Re-spawn the job transport if it dies
   * If the job transport dies, respawn it.
   * There is also an option to return when the job transport dies (on the Return tab).
8. Disconnect if a GM spawns
   * If the bot detects a GM in game, it has the ability to disconnect.
9. Disconnect if a player attacks you while botting.
10. Stop if a thief player is found in town

* This is useful if you are using the auto trade feature.

11. Only repair equipped items

* Normally the bot will repair all inventory items - even those that are not equipped. With this option, the bot will only repair the items that are equipped.

