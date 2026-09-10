> For the complete documentation index, see [llms.txt](https://guide.phbot.org/llms.txt). Markdown versions of documentation pages are available by appending `.md` to page URLs; this page is available as [Markdown](https://guide.phbot.org/supported-operating-systems.md).

# Supported Operating Systems

Supported Operating Systems

* Windows Vista (x86/x86-64) to Windows 11 (x86/x86-64)
* Arch, Ubuntu 20.04, Debian 10, and Fedora 33
* Any OS that supports Wine 1.9 or higher

Windows XP is no longer supported. If you are using Vista you should upgrade to at least Windows 7.

## Wine <a href="#wine" id="wine"></a>

**Linux**

It **might** be possible to install the VC++ 2013 redistributable binary but if not I've included the required DLLs below.

**You now need to override the MSVCR libraries inside Wine.**

1. `winecfg`
2. Libraries -> New override for library
3. Add `msvcr120.dll`, `msvcp120.dll`, and `vcomp120.dll`
4. OK

You should now not see any errors related to `moneypunct` when opening the bot or manager.

**Ubuntu**

1. Add the Wine PPA. <https://www.winehq.org/download/ubuntu>
2. Install Wine.

   [Click here (launches apt)](apt://winehq-devel)
3. Download the VC++ runtime libraries to your `phBot.exe` folder.
   * [msvcr120.dll](http://cdn.phbot.org/msvcr120.dll)
   * [msvcp120.dll](http://cdn.phbot.org/msvcp120.dll)
   * [vcomp120.dll](http://cdn.phbot.org/vcomp120.dll)
4. Start the bot.

   `wine phBot.exe`

**Debian**

1. Install Wine.

   `apt-get install wine-development`
2. Download the VC++ runtime libraries to your `phBot.exe` folder.
   * [msvcr120.dll](http://cdn.phbot.org/msvcr120.dll)
   * [msvcp120.dll](http://cdn.phbot.org/msvcp120.dll)
   * [vcomp120.dll](http://cdn.phbot.org/vcomp120.dll)
3. Start the bot.

   `wine phBot.exe`

**Arch**

1. Install Wine.

   `sudo pacman -S wine`
2. Download the VC++ runtime libraries to your `phBot.exe` folder.
   * [msvcr120.dll](http://cdn.phbot.org/msvcr120.dll)
   * [msvcp120.dll](http://cdn.phbot.org/msvcp120.dll)
   * [vcomp120.dll](http://cdn.phbot.org/vcomp120.dll)
3. Start the bot.

   `wine phBot.exe`

**Fedora**

1. Install Wine.

   `sudo yum install wine`
2. Download the VC++ runtime libraries to your `phBot.exe` folder.
   * [msvcr120.dll](http://cdn.phbot.org/msvcr120.dll)
   * [msvcp120.dll](http://cdn.phbot.org/msvcp120.dll)
   * [vcomp120.dll](http://cdn.phbot.org/vcomp120.dll)
3. Start the bot.

   `wine phBot.exe`

**macOS**

1. Download Wine Bottler
   * The stable release of Wine Bottler should work fine but if not use the development release.
   * [Wine Bottler](http://winebottler.kronenberg.org/)
2. Download the VC++ runtime libraries to your `phBot.exe` folder.
   * [msvcr120.dll](http://cdn.phbot.org/msvcr120.dll)
   * [msvcp120.dll](http://cdn.phbot.org/msvcp120.dll)
   * [vcomp120.dll](http://cdn.phbot.org/vcomp120.dll)
3. Double clicking `phBot.exe` should start the bot.

