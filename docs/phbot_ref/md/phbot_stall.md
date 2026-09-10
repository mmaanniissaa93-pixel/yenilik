> For the complete documentation index, see [llms.txt](https://guide.phbot.org/llms.txt). Markdown versions of documentation pages are available by appending `.md` to page URLs; this page is available as [Markdown](https://guide.phbot.org/phbot/stall.md).

# Stall

Stalling allows you to sell items to other players in the game. phBot makes it super easy to do this automatically.

* The stall tab will show you the current items in your stall and the prices they are set at.
* To change the item you can right click on the slot and select what you would like to do.
* When an item is sold it will appear in the log.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2Ft1wBNvuRrjQ3mk71bW05%2Fstall.png?alt=media&amp;token=318b3861-029f-405a-8271-a83173a1a411" alt=""><figcaption></figcaption></figure>

* The "Inventory" tab shows you every item in your inventory. Items are de-duplicated and the prices are saved in your config file, so if you reconnect the prices will still be the same.
* To enable automatic stalling on relog you must enable "start bot on login" on the "Silkroad Login" tab. Automatic stalling is done before town loop (this can be toggled).
* This tab should be used when you want to stall a specific item like an armor piece or weapon.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FzR8b5fi0W11NT9R024Wh%2Fstall_inventory.png?alt=media&amp;token=c0f3d81c-c5bc-4abc-90d7-4d3c27de258e" alt=""><figcaption></figcaption></figure>

* The "Filter" tab shows you a list of all items in the game. This can be useful for automatically stalling items when you return to town to repair/rebuy items.
* The "Quantity" field allows you to set the amount the bot should sell at one time. The bot will automatically combine then split the items so they can be stalled individually.
* This tab should be used when you want to sell generic items like elixirs or global chatting items.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2F31Zsv9o2oFLShuHzZbXn%2Fstall_filter.png?alt=media&amp;token=d5f6db5e-de0e-4e6f-a0b7-e550c81b7aaf" alt=""><figcaption></figcaption></figure>

* The "Consignment" tab shows all of the items that are currently in consignment and their current status.
* To enable automatically adding items to consignment you must add the `DoConsignment` command to your town script or walk script.
* The `DoConsignment` command has the ability to settle, retrieve expired items, and add items.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FxILo9P7RVqUZ46C26WTW%2Fstall_consignment.png?alt=media&amp;token=522c7ca2-c87c-44c7-b876-a706e0c7a1e5" alt=""><figcaption></figcaption></figure>

* The "Options" tab gives you control over when the bot should stall based on how many items can be stalled. This can be set to "0" to always stall if there are items available.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FMk4fCZhnc2v5u9pSYweo%2Fstall_options.png?alt=media&amp;token=853210f1-c724-40b5-acc3-1d89b0476c64" alt=""><figcaption></figcaption></figure>

