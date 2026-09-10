> For the complete documentation index, see [llms.txt](https://guide.phbot.org/llms.txt). Markdown versions of documentation pages are available by appending `.md` to page URLs; this page is available as [Markdown](https://guide.phbot.org/phbot/quest.md).

# Quest

## Active

Here you will find a list of every quest that is currently active on your character. You can abandon the quest or configure the auto quest turn in options.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2Fj4BvAggPVv5gwykhE5ku%2Fquest_1.png?alt=media&amp;token=e68f5450-4fdc-41fa-95a6-886646bac481" alt=""><figcaption></figcaption></figure>

## All

On the `All` tab you will find a list of every quest in the game. This allows you to configure quests that are not active on your character.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FB6NKo4GdKRXfVJetQANu%2Fquest_2.png?alt=media&amp;token=27900539-b507-4a91-8d0f-7b76fa27a1bd" alt=""><figcaption></figcaption></figure>

1. List of quests
   * Right click on a quest to enable it, use a return scroll when completed, or execute a script when completed.
   * You should execute a script for job quests and use a return scroll for non job quest related NPCs that are in town.
2. Search box

* Enter a quest name and press enter to search for it.

3. Search button
   * Enter a quest name into the search box and press enter or click the search button to search for it.
4. Clear button
   * Clears the window to free up some memory
5. Update button
   * Updates the quests. Your configuration will not be changed.
6. Reset button
   * Resets your quest configuration. Do not press this button unless you absolutely need to.

## View

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FmtLgfLrttEVBxDuhWPYL%2Fquest_3.png?alt=media&amp;token=4f534c8a-7a35-4ee1-a04e-3b68b51bee7d" alt="" width="375"><figcaption></figcaption></figure>

#### Options

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2F2YMYHP4yfToRHItMKJxR%2Fquest_4.png?alt=media&amp;token=72419f17-05a2-4864-b638-40fe8c97e8cc" alt=""><figcaption></figcaption></figure>

1. Retrieve quests above your level
   * Not all server types allow you to retrieve a quest above your current level so this option allows you to do that if allowed.
2. Do not turn in quests if EXP ratio is not 100%
   * This option will not turn in a completed quest if the EXP ratio is not 100%. This only applies to iSRO based servers.
3. Walk back to town from your training area to turn in quests
4. Do not turn in quests until all enabled quests are completed
5. Complete quests on the way to the training area
   * Turns in completed quests if the NPC is on the way to the training area. The character must walk near the NPC otherwise it will not turn the quest in.
6. Complete event quests while in town
   * If enabled the bot will walk to the So-Ok event NPCs and turn in event quests while in town.
7. Danger mode
   * Enables taking job trade quests in danger mode instead of safe mode.

## Auto Quest <a href="#autoquest" id="autoquest"></a>

Auto quest is done by enabling the quest and *not* setting a script.

1. Enable collision detection on the `Training Area` -> `Settings` tab
2. Enable the quest
3. If the quest requires you to return to town, enable the "Return" option for the quest
   * Once you are in town, the bot will first look for quests that can be accepted/turned in and then the town script will follow.
4. If the quest NPC is in your training area, the bot will walk to it and turn it in if it's close enough (eg. job cave quest)

## **Genie Lamp**

In order to turn the quest in, you must start the bot in the town of the Daily Quest Manager NPC. If you do not, the bot will skip over the quest because the NPC is not in your starting town.

1. Enable the Daily Quest
   * Set the quest to return when it is completed
2. Enable the Homeless Genie quest

## **Job Cave Quest**

1. Enable the quest
2. Start the bot in your training area

## Quest Script <a href="#questscript" id="questscript"></a>

This guide is for how you initially accept the quest.

1. Create a script that goes from town spawn to the quest NPC.
2. Add the `quest` command to the script.
3. If needed, add the `cast` command for "Merchant Pipe" (depends on the quest).
4. Walk to your training area.
5. If needed, add the `dismount` command.
6. Save the script and load it on the "Training Area" tab.

### Quest Return Script <a href="#questreturnscript" id="questreturnscript"></a>

This guide is for how you turn in the quest after it is completed.

1. Create a walk path that goes from your training area to the quest NPC.
2. Add the `quest` command to your script.
3. Set the script on the "Quest" tab and enable it.

The bot will execute this script in reverse after it finishes it, so there is no need to make it return to the training area.

