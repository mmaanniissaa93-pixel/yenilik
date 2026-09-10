> For the complete documentation index, see [llms.txt](https://guide.phbot.org/llms.txt). Markdown versions of documentation pages are available by appending `.md` to page URLs; this page is available as [Markdown](https://guide.phbot.org/phbot/party.md).

# Party

The main party tab gives you a list of all the members currently in your party. There are more options when you right click on a party member.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2F9eeSXl32ocDk16YP75OC%2Fparty.png?alt=media&amp;token=31aee914-5f11-4277-baa5-4d7519ceb282" alt=""><figcaption></figcaption></figure>

1. Lists all players in the party
2. Show Forgotten World summons
   * If enabled, the bot will show a popup window with the Forgotten World summon information asking you to accept or decline.
   * This option does not save.

## Accept / Invite <a href="#party_accept_invite" id="party_accept_invite"></a>

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2F6aq5LHz30psiWI0oepUX%2Fparty_options.png?alt=media&amp;token=c9b7cac3-fde6-4569-9ca6-de6df65f0427" alt=""><figcaption></figcaption></figure>

The radio buttons near the top for accepting / inviting do not enable it. You must tick the check boxes below it for accepting / inviting.

1. Accept all party invites
   * Does not use the list and accepts all join invitations.
2. Accept party invites from list
   * Uses the Accept / Invite list and only accepts from players in the list.
3. Invite all players
   * Does not use the list and invites all nearby players to the party.
4. Invite only from the list
   * Uses the Accept / Invite list and only invites players from the list.
5. Leave if leader is not in the list
   * Leaves the party when the party master is not in the "Party Leader List".
   * If the list is empty it will not leave the party.
6. Leave if leader is in the training area
   * Leaves the party when the party master is not in the "Party Leader List" but is also in the training area.
   * This can be used to keep the party together if the party master has gone offline. They will leave when the master returns.
   * If the list is empty it will not leave the party.
7. Invite other players to the party
   * Enables inviting other players. This should be on by default.
   * If this option is off the bot will only accept invites and not send invites.
8. Only accept invites at the training area
   * Do not accept party invitations from anyone unless you are in the training area.
9. Accept party invites from other players
   * Enables accepting party join invitations.
   * If this option is off the bot will not accept any party invitations.
10. Allow other party members to invite players

* This is an in game feature that can disallow other players in the party from inviting.
* It is recommended to keep this feature enabled.

11. Refuse invites from players not in the list.

* If enabled, the bot will reject party invites similar to declining them in game.
* If not enabled, the invites will eventually expire on their own.

12. Party type

* The type is exactly the same as in the game.

13. Party Leader List

* Controls if the bot should leave the party when the party master is not found in this list. Leave the list empty if you do not want the bot to leave a party.

14. Accept / Invite List

* List of player names to invite to a party or accept party invitations from.

15. Accept delay

* Delay in seconds before accepting the party invite.

## Matching <a href="#partymatching" id="partymatching"></a>

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FUuyy1qIT9hLsjLfPr2Cu%2Fparty_matching.png?alt=media&amp;token=4d6326ed-2e1c-473f-b6b6-c93e52ec6abd" alt=""><figcaption></figcaption></figure>

1. Title
   * Party matching title for auto form party.
2. Level
   * Party matching level range for auto form party.
3. View Party Matching

   * Shows a list of the current parties in the matching system.

   <figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2F8YhcCiAbL2DFFbsFrujy%2Fparty_matching_list.png?alt=media&amp;token=479dd28e-ef18-4993-83fb-194a3b7780f4" alt=""><figcaption></figcaption></figure>
4. Join party #
   * Allows you to join a party via its party matching number.
5. Auto form party matching
   * Automatically add the party to the party matching system. The party will be auto reformed every 10.
6. Join by name
   * Join a party matching based on a player name.
7. Join by title
   * Join a party matching based on its title.

## Taxi <a href="#taxi" id="taxi"></a>

Party taxi timer allows you to collect gold from players in your party. It will automatically send private messages to them asking for payment when their time is up and can also kick them if they do not pay.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2FyqdyHm44sp2MYu3lCoDt%2Fparty_taxi.png?alt=media&amp;token=21b6977f-7d71-459b-80be-112f3a4d1309" alt=""><figcaption></figcaption></figure>

1. Taxi list
   * List of players currently in the taxi and their remaining time.
   * You can right click on their name and add more time.

<figure><img src="https://2872462270-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F2m4okFXMkt87rLgVVbFZ%2Fuploads%2F6yOYRt5fYg5y6DACnK5d%2Fparty_taxi_options.png?alt=media&amp;token=6a5d3c17-e57d-48b8-8aea-d8f0e87b07a6" alt=""><figcaption></figcaption></figure>

1. Cost per hour
   * The amount of gold you wish to charge for the taxi.
   * Players can pay in multiples of this and time will be added accordingly.
2. Kick players from party when time is up
   * Kick the player when they fail to pay after their time has expired.
3. Add party matching joins to taxi
   * Automatically add new players to the taxi when they join through party matching
4. Blacklist after X kicks
   * If a player has been kicked X number of times, do not accept them to the party anymore.
5. Blacklist
   * List of players that are not allowed to join the taxi.
6. Exchange
   * Message to ask the player for gold (the %0 is where the gold amount will be placed in the message)
7. Kick
   * Message when a player is about to be kicked for not sending the gold (the %0 is where the number of minutes will be placed in the message)
8. Delay in minutes
   * Delay before kicking the player from the party
   * This is the value for the "Kick" message

