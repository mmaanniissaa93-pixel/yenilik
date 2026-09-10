> For the complete documentation index, see [llms.txt](https://guide.phbot.org/llms.txt). Markdown versions of documentation pages are available by appending `.md` to page URLs; this page is available as [Markdown](https://guide.phbot.org/phbot.ini.md).

# phBot.ini

Configurable options in phBot.ini

The `phBot.ini` file is located in the same folder as the `phBot.exe` file. It contains global configuration settings for all characters that you open.

***

**Username**

ProjectHax login username.

**Password**

ProjectHax login password.

**UpdateSkip**

Skips a specific bot version. Once a new version is released you will be asked to update again.

**BindIP**

Binds outgoing Joymax connections to a specific IP address.

**white**

Disables the dark theme when set to `1`.

**iSRO\_Gateway**

Sets the iSRO gateway server to use. It can also be set to `Random` to use a random server.

**SilkroadR\_Gateway**

Sets the SilkroadR gateway server to use. It can also be set to `Random` to use a random server.

**X-Trap**

Sets the X-Trap server to use. Can be set to `server` to use ProjectHax or an IP address if you wish to run your own server on your own machine or in a virtual machine.

**SilkroadServer**

Sets the last clientless logged into Silkroad server name.

**Language**

Sets the bot language. The default is `en`.

**SwitchGatewayAttempts**

Sets the value for switching the gateway server after a certain number of failed login attempts.

**iSROPath**

iSRO game path.

**SilkroadRPath**

SilkroadR game path.

**cSRORPath**

Official cSRO SilkroadR game path.

**SocksIP**

SOCKS IP or hostname.

**SocksPort**

SOCKS port.

**SocksUser**

SOCKS username. Only used with SOCKS v5.

**SocksPass**

SOCKS password. Only used with SOCKS v5.

**SocksVersion**

SOCKS version. `4` or `5`.

**ReduceSilkroad**

Reduce Silkroad memory usage by periodically emptying the processes working set.

**Title**

Changes the phBot window title. The default is `phBot v%0 - %1 - %2`.

**Passcode**

Sets the default passcode for iSRO for easier login.

**DisableTray**

Can be set to `1` to disable the tray icon. This does the same thing as the `--disabletray` command line argument.

**AllowXTrap**

Allows X-Trap to run on servers that would normally disconnect you if X-Trap is disabled.

