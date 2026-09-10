> For the complete documentation index, see [llms.txt](https://guide.phbot.org/llms.txt). Markdown versions of documentation pages are available by appending `.md` to page URLs; this page is available as [Markdown](https://guide.phbot.org/initial-startup.md).

# Initial Startup

## Logging In <a href="#loggingin" id="loggingin"></a>

At first launch you will be asked to login to the bot using your ProjectHax username and password.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FO8nD2FRFJG9szF1TsOec%2Fprojecthax.png?alt=media&amp;token=50e02a72-f1a4-4134-a82c-6b2f5eb50254" alt=""><figcaption></figcaption></figure>

Enter your login details and click "Login". You will now logged into the bot if you have time on your account. This information is saved in `phBot.ini` so you will not need to enter it again.

You can change the language and theme in the top right corner of this tab. It will require you to restart the bot for the changes to take effect.

### Reset Hardware ID <a href="#resethwid" id="resethwid"></a>

Each ProjectHax account has three hardware IDs. They automatically reset at 00:00 UTC every single day. If you change your hardware you may get a message in the bot saying that three hardware IDs have been used. To mitigate this issue, you may reset them once per day, manually.

[Click here to reset hardware IDs](https://phbot.org/reset/hwid/)

### Choose Locale <a href="#chooselocale" id="chooselocale"></a>

Directly after logging in, a dialog will appear asking you to select your locale. Three options can be chosen.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2Fn20fBvX4V1ryL7kphKs5%2Fchooselocale.png?alt=media&amp;token=139a6d44-8aab-4bd6-ae10-6d244f745132" alt=""><figcaption></figcaption></figure>

1. International Silkroad
   * Including merged SilkroadR servers
2. Korean Silkroad
3. cSRO SilkroadR (official)
4. jSRO GameCom
5. DIGEAM
6. TRSRO
7. Gzone
8. VTC
9. Russian Silkroad
10. Private Server
    * vSRO 1.188/1.193/1.274, thSRO, Black Rogue, ECSRO, and cSRO SilkroadR are all supported.
    * iSRO private servers are supported (RIGID).

Selecting "Private Server" will bring up another dialog asking for more information. You only need to do this if you plan on playing on a Silkroad private server. iSRO and SilkroadR are completely separate from this.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FpryLY2jSn6OMH7ry3uyQ%2Feditprivateserver.png?alt=media&amp;token=a18a9d5d-840a-423f-b8da-2c89161db9c9" alt="" width="375"><figcaption></figcaption></figure>

1. List of private servers you have added
2. Name
   * This field allows you to give the private server a name that will show up in the bot. It does not affect anything else.
3. Division
   * Silkroad game division. Certain locales have multiple divisions but use the same Silkroad.exe launcher.
4. Hosts
   * A list of gateway servers. It will be automatically populated after selecting the game path.
5. Version
   * Game version
6. Servers
   * A list of Silkroad servers that show up after you launch the game. You will need to manually add them to the list.
7. Captcha
   * Some private servers have a static captcha character (or no character). This allows you to specify the captcha character to be sent automatically.
8. Path
   * Silkroad game path. Instead of selecting the path when you click "Launch Client", you must select it here so the server data can be saved for later use.
9. Type
   * This is the vSRO server type. vSRO is separated into several variants such as Black Rogue, thSRO, v1.193, and v1.274. Unfortunately, there isn't a good way to detect the server type automatically, so you must do this on your own. If you do not select the correct type you will have packet parsing errors when you get in game.
   * You do not need to select any options here for cSRO SilkroadR private servers / vSRO 2 job / iSRO private servers.

## Launch Game <a href="#launchgame" id="launchgame"></a>

Launching the client will start the Silkroad game and allow you to login as normal.

1. Start by clicking "Launch Client".
2.

```
<figure><img src="/files/Q5XiQQUD39s0OGOtRcm0" alt=""><figcaption></figcaption></figure>
```

3. You will be asked to set the path for the game. This is so it knows how to start Silkroad. You will only need to do this once per game (iSRO/SilkroadR/private server).

   <figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FQLhAuKIlhzQ3up1iSVug%2Flaunchgame_2.png?alt=media&amp;token=b914c19a-6c36-4da3-a24a-a07c7d898fdf" alt=""><figcaption></figcaption></figure>
4. Navigate to your Silkroad folder and click "Select Folder".

   <figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FILIjVMhBZlK1pHenjACW%2Flaunchgame_3.png?alt=media&amp;token=3489e76d-29e6-49aa-8b15-9c33a8eebfa1" alt=""><figcaption></figcaption></figure>
5. The game will now start and you can login as normal.

## Clientless Login <a href="#clientlesslogin" id="clientlesslogin"></a>

First, I should start by explaining what "clientless" actually is. Client-less means that the Silkroad game doesn't need to be running while you are using the bot. It uses less resources than the game so you can run more characters. phBot gives you the option to login either with the game or without it. For client login see [Launch Game](broken://pages/APSwjR83kyiGdgFhUOdB).

You will need to have the game installed so the bot can generate a database containing data from the games `Media.pk2` and `Data.pk2` files. After that, you may copy the `iSRO.db3` and `SilkroadR.db3` files to a VPS and launch the bot. If the game updates, you'll need to copy the newly generated databases to the VPS.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FnW2Fjwhrw6FLWM4WTp0X%2Fsilkroadlogin.png?alt=media&amp;token=072553e6-8559-4671-9c3d-e9f0be94cb4b" alt=""><figcaption></figcaption></figure>

1. Username
   * This is the username you use to login to the game.
2. Password
   * This is the password you use to login to the game.
3. Server
   * Silkroad server name.
4. Gateway Server
   * The Silkroad server you are playing on may have multiple IP addresses. This box can be used to select a gateway server that has less traffic or if one goes down to choose another working one.
5. Switch gateway after X attempts
   * Change the gateway server after X failed connection attempts.
6. SOCKS IP/Port
   * SOCKS is a proxy protocol. If you have a SOCKS proxy server you can have the bot use it to login to the game. This can be used to bypass IP limits on private servers.
   * Only SOCKS v5 is supported.
7. SOCKS Username
8. SOCKS Password
9. Reduce Memory
   * This feature allows you to lower the memory usage of Silkroad. Do NOT enable it unless you know what you are doing! It can cause performance issues and possibly crash the game.
10. Return to town on login

* Use a return scroll after loading into the game

11. Start bot on login

* Start botting on login. This should be used with the "Relog on disconnect" feature

12. Relog on disconnect

* If you disconnect from the game server, reconnect.

13. Login

* After you enter your Silkroad login details, click this button to start logging into Silkroad without using the client.

14. Logout

* After you get in game with clientless this button will be enabled. It allows you to logout of the game.

15. JCP

* Allows you to login to iSRO with a JC Planet account.
* You must check this box if you're logging into the game with your email address.

16. Client

* Starts the client with clientless login. This is different from launching the client and typing in your account info.

17. Queue

* Prevents multiple bots from logging into the game at one time.
* Only allows one bot to connect to the server to prevent temporary IP bans on iSRO.

18. Hide login

* Hides your Silkroad login details. This mainly for streamers.

19. Captcha

* Allow the bot to automatically resolve the captcha on its own.

20. Allow X-Trap

* Allows X-Trap to run on servers that would normally disconnect you if X-Trap is disabled.

21. Server Capacity

* Once you connect to the gateway server this will be populated with a list of servers and their capacities.

## Go Client / Go Clientless <a href="#goclient" id="goclient"></a>

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FvWvCkbPBYXegJtrQs0lp%2Fgoclient.png?alt=media&amp;token=0a0128b4-5301-4d93-9e7b-7f4dcd880cec" alt=""><figcaption></figcaption></figure>

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FwUIhpMOnhPKWkZBUK1pH%2Fgoclientless.png?alt=media&amp;token=ded08d0c-bacb-4145-a557-e8c08e7bcf73" alt=""><figcaption></figcaption></figure>

Depending on what state the game is in, you will either see "Go Client" or "Go Clientless". If the game is not running or has crashed you will see "Go Client" otherwise the button will show "Go Clientless". If it is greyed out then you have not logged into Silkroad yet.

### **Go Clientless**

* After clicking this, the game client will be terminated.
* You will still be in game until you close the bot.

### **Go Client**

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FHlMvPe3ilxfdM7HDDKaH%2Fgoclient_options.png?alt=media&amp;token=2d2269c8-766a-4589-8f65-bab0b4c41f92" alt=""><figcaption></figcaption></figure>

* Clicking this button will allow you to start the client without needing to exit or restart the bot.
* `Return/Teleport` will use a return scroll or a nearby teleporter after starting the game. This is required because the client has to sync up at a teleport.
* `Reconnect` will disconnect, start the game, and log you back in at the position you left off at in the game.

