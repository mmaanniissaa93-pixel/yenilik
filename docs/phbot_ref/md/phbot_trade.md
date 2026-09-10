> For the complete documentation index, see [llms.txt](https://guide.phbot.org/llms.txt). Markdown versions of documentation pages are available by appending `.md` to page URLs; this page is available as [Markdown](https://guide.phbot.org/phbot/trade.md).

# Trade

The "Trade" tab allows you to configure the trade loop and have the bot automatically walk to different towns to earn gold..

## Loop <a href="#tradeloop" id="tradeloop"></a>

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FkhwUNbmFxiV7nLpNunTQ%2Ftrade_loop.png?alt=media&amp;token=006ecf96-ff3d-4613-9d62-80a496f61ef5" alt=""><figcaption></figcaption></figure>

1. Start
   * Starting town for the trade loop.
2. End
   * End town for the trade loop.
   * If you want to buy trade goods in Jangan then sell in Donwhang and rebuy in Donwhang then sell in Hotan your loop would look like this:
   * Jangan -> Donwhang
   * Donwhang -> Hotan
3. Transport
   * Name of the transport to use for the trade.
4. Star
   * Number of trade goods to buy. You can choose by start, quantity, or fill the transport.
5. Add
   * Add the loop to the list.
6. Start
   * Starts auto trade.
7. Stop
   * Stops auto trade.
8. Shows the trade loop information.

## Items <a href="#tradeitems" id="tradeitems"></a>

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2Fq0I7PxgJaEL0nrRNqysL%2Ftrade_items.png?alt=media&amp;token=1e0f37ef-e586-466d-aed8-d7fd14136fd7" alt=""><figcaption></figcaption></figure>

1. Name of the item.
2. Enable the item for purchasing.
3. Quantity of the item to buy. This option would be used with the "Use quantity" option on the Loop tab.

## Options <a href="#tradeoptions" id="tradeoptions"></a>

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2Fz1MpF01CdLhyNlBRIrGo%2Ftrade_options.png?alt=media&amp;token=00f66a37-484d-477b-a9e7-c4dec8b9347b" alt=""><figcaption></figcaption></figure>

1. Repeat trade loop
   * Repeat the loop X number of items.
2. Loop count
3. Attack spawned thieves
   * Attack thieves in a specified radius around the transport.
   * In order for this option to work you must use "Stay off transport" or "Remount transport".
4. Attack radius
5. Use return scroll after loop completes
   * This option can be used to loop the trade infinitely if you only specify a single loop.
6. Skip town loop
   * Don't walk around the town to repair/purchase items.
7. Stay on transport
   * Always stays on the transport and will not attack. You will need to have enough recovery kits to out-pot the attacks.
8. Stay off transport
   * Always dismount the transport.
9. Remount transport
   * If you are not being attacked, the bot will attempt to remount the transport.

