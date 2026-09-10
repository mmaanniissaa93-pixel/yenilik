> For the complete documentation index, see [llms.txt](https://guide.phbot.org/llms.txt). Markdown versions of documentation pages are available by appending `.md` to page URLs; this page is available as [Markdown](https://guide.phbot.org/phbot/training-area.md).

# Training Area

## Script <a href="#trainingarea" id="trainingarea"></a>

**Add Training Area**

Adding a new training area is relatively simple. This method does not require a script because it uses path finding.

1. Right click on the Training Area list.

2. Click "Add".

3. Give it a name.

   <figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FkBe7QF0WKAIwvUR9N0Ol%2Fadd_training_area.png?alt=media&amp;token=8e051265-d28e-4c28-ae3b-65db4e0198ef" alt=""><figcaption></figcaption></figure>

4. You will now see it appear in the list.

5. Click "Get Position" to get your characters current coordinates.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FWtxtgZR0ZiZBrk0YzeK5%2Fnew_training_area.png?alt=media&amp;token=e89c8be1-9e0b-498a-88cd-be464b99b155" alt=""><figcaption></figcaption></figure>

* To edit the "Radius" or "Pick Radius", simply double click on the values.
* For more options right click on your training area.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FET7VGC7xEazg5Lvtge9d%2Ftraining_area_menu.png?alt=media&amp;token=7583a8d0-4f3a-45d5-91db-9a46d585468c" alt=""><figcaption></figcaption></figure>

1. Add
   * Adds a new training area.
2. Enable
   * Enables the training area.
3. Walk
   * Walks to the training area coordinates. The walk button will turn to "Stop" after choosing this option.
4. Set Coordinates
   * Allows you specify coordinates for the training area.
5. Set Polygon
   * Allows you to set a polygon for the training area. This is only used when copying a polygon from another bot. You will still need to set the training coordinates if using a polygon.
6. Use Radius
   * Use a simple radius for the attack area.
7. Use polygon
   * Use a polygon for the attack area. If choosing this option, you must draw a polygon on the map tab or set one by choosing "Set polygon".
8. Copy Coordinates
   * Copies the training area coordinates to the clipboard.
9. Copy Polygon
   * Copies the polygon data to the clipboard.
10. Change Script

* Allows you to set a walk script for the training area.

11. Choose Monster

* Allows you to choose a monster close to your level for the training area.

12. Edit Script

* Opens the script creator and lets you edit the walk script for this training area.

13. Reload

* Reloads the script from disk.

14. Clear

* Clears the script from memory.

15. Delete

* Deletes the training area.

**Create Walk Script**

A script is essentially a path that the bot will follow after completing the town loop. Scripts are very simple to create just make sure your first click is in the town spawn area otherwise bad things will happen!

1. Click "Create". This will bring up a new dialog that will allow you to record a script as well as add custom commands to it.

   <figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FjycnmsC51qqXJ8kns8qx%2Ftraining_area_create.png?alt=media&amp;token=1ed93d1f-c50b-4b45-ba6a-d8e8fdf2d837" alt=""><figcaption></figcaption></figure>
2. Click "Record". This will begin logging your characters movements as well as add "wait" commands after a teleport.

   <figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2Flys0iWdUJLI0G7ZMvKWf%2Fscript_recorder.png?alt=media&amp;token=415ff596-a19f-4a59-a3df-9f41709d6e45" alt=""><figcaption></figcaption></figure>
3. When you have finished recording your script, click "Stop" and "Save As".
4. Save your script and close the "Script Creator".
5. Right click on your training area and click "Change Script".
6. Choose the script you just created and you are done.

## Conditions <a href="#conditions" id="conditions"></a>

Training Area Conditions allow you to change the current Training Area to another when your character reaches a specific level that you set. This tab is easy to understand by following the dialogs that show up when you right click in the list and go to "Add".

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FXSHFprT2cfgz5zpZPH83%2Ftraining_conditions.png?alt=media&amp;token=357025dc-89a8-4cff-a2d9-e42638b0258c" alt=""><figcaption></figcaption></figure>

## Collision <a href="#collision" id="collision"></a>

[Download](https://forum.projecthax.com/t/latest-testing-release/24)

Collision detection allows the bot to navigate around obstacles to get to monsters and walk to nearby NPCs. These options can be resources intensive.

Occasionally there will be navmesh updates which require you to re-run the Easy Installer or download and extract the ZIP from the latest release thread on the forum.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FnpyN3w4M35oChvJZSSu5%2Ftraining_area_collision.png?alt=media&amp;token=861a2077-dba8-4f3a-b7b6-45a089f13abe" alt=""><figcaption></figcaption></figure>

1. Enable collision detection in the training area
   * Allows the bot to ignore monsters and items that are behind obstacles.
   * Requires you to download the navmesh data. It is linked in the release thread.
2. Navigate around obstacles
   * Allows the bot to navigate around obstacles to get to monsters.
3. Navigate to item drops
   * Uses path finding to get to items that dropped behind an obstacle.
4. Disable Samarkand
   * Disables teleporting to Samarkand (useful on servers that do not have Samarkand enabled)
5. Disable Alexandria
   * Disables teleporting to Alexandria (useful on servers that do not have Alexandria enabled)
6. Disable Guide/Advice NPCs
   * Disables using Guide teleporter NPCs in towns
7. Ignore teleport level
   * Allows the bot to teleport to an area above the characters level (useful if the teleporters level isn't correct)

## Settings <a href="#settings" id="settings"></a>

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FZ3AWrcHFzNQqL7YFqN2o%2Ftraining_area_settings.png?alt=media&amp;token=1533afa8-e124-4774-947d-7859e7c0ce50" alt=""><figcaption></figcaption></figure>

1. Don't walk around
   * By default the bot will randomly walk around in your training area. This option will disable that.
2. Use Treasure Boxes
   * Use Treasure Box NPCs in the Jangan cave.
3. Use Easter Egg Event NPCs
   * This option allows you to use the Easter Egg NPCs when the Easter event is active.
4. Equip better items
   * Equips better items that have been picked up. This should not be used on high level characters where gear is important.
5. Summon flowers in the training area
   * Flowers are buff monsters that can be summoned.
6. Use a repair hammer
   * Instead of returning on durability, the bot can use a repair hammer if you have one.
7. Use berserker regeneration potions
   * If your berserk is not full, the bot can use a berserker regen potion to regenerate your berserk.
8. Use Energy of Life potions
   * Energy of Life potions extend berserk or can completely regenerate your berserk just like a berserker regeneration potion except they can be purchased in an NPC.
9. Use Energy of Life berserk regeneration
   * Uses Energy of Life items.
   * The berserk restoration is used based on your berserk options; otherwise just the Energy of Life item is used.
10. Use a reverse return scroll when you die

* After completing the town loop the bot will use a reverse return to get back to your training area.
* Requires that you die while the bot was started. If you restart the bot it will not use it.

11. Use a reverse return scroll after returning to town

* Same as #15 except it will also use reverse if you used a return scroll to get back to town.

12. Use monster summon scrolls & Pandora's Box in the training area

* Uses monster summon scrolls in the training area

13. Wait for all strong monsters to be killed before summoning another

* To prevent your character or party from dying, the bot can wait for the strong monsters that spawn from summon scrolls to die before using another scroll.

14. Use speed drugs

* Use movement speed drugs

15. Only use speed drugs in the script

* This option can be used if you have a Bard in your party casting a speed buff. It can save some gold.

## Script <a href="#settings" id="settings"></a>

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FAVoBxtVjF8X26UMSzA1H%2Ftraining_area_script.png?alt=media&amp;token=b8a35a0b-b592-4deb-a12f-0a7b6f87bd2c" alt=""><figcaption></figcaption></figure>

1. Skip town script entirely
   * Skips doing the town script and immediately goes to the training area.
2. Continue town scripts
   * If you disconnect halfway through your town script the bot can continue it by enabling this option.
3. Return when can't continue script
   * Uses a return scroll if the script cannot be continued
4. Avoid Statue of Justice in the script
   * Uses path finding to navigate around Statue's of Justice when walking to the training area.
5. Script walk delay X milliseconds
   * Adds a delay to each walk command. This can be useful if your character is getting stuck some of the time.
6. Go back a coordinate if stuck after X seconds
   * Walks back to the previous coordinate if the character has not been able to move.
7. Return if stuck in script after X seconds
   * Uses a return scroll if the character has been stuck for the specified number of seconds.
8. Ride fellow pet to training area
   * Ride the fellow pet to the training area
9. Remount in caves
   * Remount the fellow pet after entering the cave. It is not possible to enter a cave when a pet is mounted so it must be dismounted first.
10. Use town NPC teleport for last recall

* Uses the Guide/Advice NPCs in towns to go back to the training area if a return scroll was used there.

11. Use town NPC teleporter for last death

* Uses the Guide/Advice NPCs in town to go back to the training area if the character died there.

