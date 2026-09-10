> For the complete documentation index, see [llms.txt](https://guide.phbot.org/llms.txt). Markdown versions of documentation pages are available by appending `.md` to page URLs; this page is available as [Markdown](https://guide.phbot.org/phbot/attack.md).

# Attack

The "Attack" tab is your attack and buffs configuration. This is where you configure what skills to kill monsters with.

## Attack

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FIjy6nBikWB2OGybu9pD5%2Fattack.png?alt=media&amp;token=d7bbc26e-cca0-4f68-b524-0e9c5faeec2d" alt=""><figcaption></figcaption></figure>

1. Skill list
   * Here you'll find a list of all your characters skills. Imbue and passive skills will not be listed.
2. Add/Remove buttons
   * These two buttons allow you to add or remove skills from the attack list.
3. Type
   * This dropdown combo box contains a list of monster types. It allows you to add specific skills to specific monster types.
4. Attack list
   * List of attack skills you have added.
5. Imbue
   * Imbue skill. Only Chinese and European Rogue characters will be able to use this.
6. Up/Down buttons
   * These two buttons allow you to move skills up and down in the attack list.
7. Cast skills in order
   * Resets the skill use index when selecting a new monster. This allows for skills to be cast in order instead of what skill it left off at on the previous monster.
8. Kill steal monsters
   * Attack monsters that other players are attacking.
9. Don't attack monsters
   * Certain characters such as Clerics/Bards that only buff should not attack monsters and that is what this option is for.
10. Protect party members

* When another party member is attacked by a monster, switch to that monster and attack it instead of selecting the closest one. If a name is specified it will only protect that party member and will wait for them to be attacked.

11. Attack lower monsters first

* When a lower monster "type" attacks you, kill it before attacking the higher monster. eg. attacking party monster, normal monster attacks you, switch monsters to the normal one and kill it, then switch back to the party monster.

12. Switch monster after X DOT

* For Warlock characters. Cast X number of DOTs on a monster and then switch.

13. Use lower skills if skills for a specific monster type do not exist

* The default for when you have no skill configured for a monster is to go back down to "General". This option will go down from: "Giant" -> "Champion" instead of "Giant" -> "General" if you have no skills configured for Giant monsters but do for "Champion".

14. Slower attack mode

* Sends less packets when attacking/buffing/picking items.
* Not actually slower.

15. Lagtastic

* Send more packets on laggy private servers that do not always respond to attack packets.

16. Auto select nearby uniques (must not be botting)

* When not botting the bot can keep uniques selected so you only attack the unique and not the monsters that it spawns.

17. Auto select nearby titans (must not be botting)

* When not botting the bot can keep titan monsters selected so you only attack the titan.

18. Use teleport skills

* Use `Ghost Walk` and `Teleport` skills to get to a monster faster.

## Buffs <a href="#buffs" id="buffs"></a>

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FO8mrTYBLAF0Vzs6EmFUK%2Fbuffs.png?alt=media&amp;token=ea677e88-1ba8-49bb-beda-be6b4028a3d3" alt=""><figcaption></figcaption></figure>

You may add buffs for all weapons into this list. Weapons are automatically switched depending on the buff.

1. Buff while attacking monsters
   * When a buff ends, recast it even if it is attacking a monster.
2. Use Devil's Spirit / Angel's Spirit
   * Devil's Spirit is a mall item that increases movement speed and increases attack power. It can be used by the bot while botting.
3. Cast Mirror Reflect
   * Mirror Reflect is a weapon skill on D12 and up weapons. It has a chance to reflect damange done to you back at a monster/player. It is mostly useless on monsters.
4. Emergency buff HP < X
   * Emergency buffs will be cast when HP is less than the set amount.
5. Emergency buff MP < X
   * Emergency buffs will be cast when MP is less than the set amount.
6. Monsters attacking
   * Cast emergency buffs if more than the specified monsters are attacking.
7. Cast Force Cure on bad status
   * Instead of using a universal/purification pulls, cast as specific skill
   * Chinese only
8. Recast all buffs when one buff ends
   * Some buffs end very close together and may require multiple weapon switches. This allows you to recast all secondary weapon buffs when one buff ends.

## Party Buffs <a href="#partybuffs" id="partybuffs"></a>

Party buffs are buffs that go on another player in your party (or possibly outside the party). Please be sure to not have multiple characters of the same class buff the same player with the same buff.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FT8yRWAHxm5MCFL6C0WWs%2Fparty_buffs.png?alt=media&amp;token=e8c2f8b0-2a4e-4508-ad15-e3fdd08c9c4a" alt=""><figcaption></figcaption></figure>

1. Party Buffs list
   * A list of all available party buffs.
   * To add a player: select the buff name, select the player name from the right side, click on the arrow button pointing to the left.
2. Add/Remove buttons
   * Adds or removes a player from a particular party buff.
3. Player list
   * Party members will appear in this list.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2F18cWIS6uGg5Ef5mY5Cpu%2Fparty_buff_options.png?alt=media&amp;token=dfa48203-d796-40d7-9cf0-ed54eb63f3f3" alt=""><figcaption></figcaption></figure>

1. Party buffing radius
   * How far to let the bot walk to buff another party member.
2. Walk back to the center after buffing
   * After casting a party buff, walk back to the center of the training area.
3. Buff all nearby players
   * Probably useless, do not use.
   * Buffs all nearby players with all skills even if they are not in the buff list or in the party.
4. Start/stop party buffing if player is nearby
   * Allows you to toggle party buffs if a player is nearby.
   * If the list below (#5) is empty, party buffs will continue as usual no matter which option is selected.
5. Player names for #4

## Resurrect <a href="#resurrect" id="resurrect"></a>

The "Resurrect" tab allows you to configure which players should be revived.

* For reviving to work correctly you must add the resurrect skill to your buff lists.
* Group resurrect skills are used first if there is more than one person dead in the party otherwise single resurrect skills are used.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FrF5VRtedJZrf09VEcqM0%2Fresurrect.png?alt=media&amp;token=09e0a433-d4b5-4cfb-b252-82f2bb9d44f4" alt=""><figcaption></figcaption></figure>

1. Resurrect list
   * The current players in the list to be revived.
2. Add/Remove buttons
   * Adds or removes a player from the revive list.
3. Players near you
   * A list of players that are nearby.
4. Resurrect radius
   * The max distance the bot should travel to revive someone. If the player is not visible, the bot will not be able to walk to them to revive them.
5. Resurrect delay
   * How often to cast a resurrect skill when someone is dead.
6. Resurrect all party members
   * Instead of adding each party member to the revive list you can simply enable this and it will automatically revive dead party members while botting.
7. Resurrect try limit
   * After a certain number of resurrect attempts, give up on resurrecting the player.
8. Auto accept res from other players
   * In order for the bot to accept a revive you must enable this option.
9. Only accept res from party members
   * Do not accept revive requests from non-party members.

## Healing <a href="#healing" id="healing"></a>

The "Healing" tab allows you cast healing skills based on the health of the character or the party.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FeS3tGUDOzp5DkLLEaIEe%2Fhealing.png?alt=media&amp;token=757f13fd-6d3b-4819-b534-cc0ed925ca0d" alt=""><figcaption></figcaption></figure>

1. Heal party member when HP % drops below X
   * Cast a heal skill when a member of the party drops below a set percent.
2. Group heal all party members when average HP % drops below X
   * Cast a heal skill when the average HP of the entire party drops below a set percent.
3. Heal self when HP % drops below X
   * Cast a self healing skill when the character HP % drops below a set percent.
4. Heal skill list

## Lure <a href="#lure" id="lure"></a>

This tab allows you to configure a character to walk out to attract more monsters to the training area. This is only useful for characters in a party and only one character needs to lure.

There are two methods for luring:

1. Have the character randomly walk out X number of spaces and cast "Howling Shout".
2. Follow a walk script and add the "Howling Shout" skill to the script so it casts it at the perfect position in game.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2Fk1nG56y2TWFkiYhExCRO%2Flure.png?alt=media&amp;token=06cef983-4078-41e9-bd16-b0883e091dc7" alt=""><figcaption></figcaption></figure>

1. Walk X spaces then back to the center
   * This is the first option for luring monsters. It will walk out however many spaces you set, wait, and walk back.
2. Cast skill
   * Casts a skill you specify (preferably `Howling Shout`)
3. Buff only after lure script ends
   * This option will skip buffing until the entire lure script has finished.
   * Without this option it will walk to one of your coordinates then back into the training area to buff a player.
4. Stop luring if more than X party members are dead
   * See below for more.
5. Stop luring if more than X party giants are in the training area
   * See below for more.
6. Stop luring if more than X party gians are in the training area
   * See below for more.
7. Stop luring if more than X monsters in the training area
   * See below for more.
8. X ms delay at the edges
   * Waits a certain amount of milliseconds outside of the training area (not the center).
9. X ms delay at the center
   * Waits a certain amount of milliseconds in the training area center.
10. X ms half delay

* Waits a certain amount of milliseconds between the edge and the center.

11. Script

* Instead of walking randomly, you can record a walk path and set it up to cast "Howling Shout" at the perfect position.

12. Stop luring if party members are not near the training area

* This option will stop luring when party members are in town so the remaining characters do not die.

When the character is not luring, it is possible to have it attack monsters in the training area. If you do not wish for this to happen, you should enable the "Don't attack monsters" option on the "Attack" tab.

It is possible to add the `cast` command to your lure script and have it attack a nearby monster instead of using `Howling Shout`.

#### Attack Log <a href="#attacklog" id="attacklog"></a>

The "Attack Log" is a list of every player that has attacked you or killed you. This can be useful when in the "Job Cave" area or if someone PK's you.

