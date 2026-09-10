> For the complete documentation index, see [llms.txt](https://guide.phbot.org/llms.txt). Markdown versions of documentation pages are available by appending `.md` to page URLs; this page is available as [Markdown](https://guide.phbot.org/phbot/town.md).

# Town

The "Town" tab allows you to setup item buying on loop.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FzqDBRg4t3gbZsaXvHVLn%2Ftown_buy.png?alt=media&amp;token=feef61d3-3c78-4038-8a61-9d7db850b00a" alt=""><figcaption></figcaption></figure>

1. This is the area where town items will appear.
2. Search box
   * Type the item name you are looking to buy and press enter.
3. Search button
   * After typing the item name you can either press enter or click this button to search for items.
4. Clear button
   * Clears the window and frees up some memory.
5. Refresh button
   * Shows you all NPC items that can be purchased.
6. Reset button
   * Resets all of your town buy settings.
7. Enables buying the item.
8. Skip guild storage if a guild member is nearby
   * Only one person can access guild storage at a time. This option will skip guild storage if another guild member is nearby.

* After you have found the item you would like to edit, double click on the "Quantity" area to change the quantity and then mark the box next to the icon to enable it. This way is much better than having a bunch of combo boxes because you can set it to buy any item in any supported NPC.
* The free low level potions will be sold automatically so they can be re-bought to get a full stack.

***

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FLP4bSmSlEWmF7qOnz3e1%2Ftown_options.png?alt=media&amp;token=8867c34a-46ef-4980-8154-53a432c4defd" alt=""><figcaption></figcaption></figure>

1. Skip guild storage if a guild member is nearby
   * This option will not try to enter guild storage if another guild member is nearby.
2. Guild storage enter retry
   * Occasionally a game bug will occur where another member is stuck in guild storage. This value is the number of times to retry when that occurs.
3. Stop bot if the inventory is full when buying items
4. Do not cast speed or noise during the script
   * This option will disable the default behavior of automatically casting moving speed and noise if available.
5. Sell excess potions
   * Sells potions/arrows/bolts that are in excess of the amount set to buy
6. Drop items in town
   * Items that are set to "Drop" in the pick filter will drop in town
7. Do not use low level town scripts
   * This option can be used on iSRO based servers to disable using the `_r.txt` scripts where they do not have the free town NPCs or token items

