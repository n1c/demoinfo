# DemoInfo

This is a C#-Library that makes reading CS:GO-Demos and analyzing them easier.

## Usage

Refer to [this guide](https://github.com/moritzuehling/demostatistics-generator/blob/master/README.md#usage-of-demoinfo-public). There is also an example-project where you can see the parser in action!

## Features

* Get Informations about each player at any point in time:
 * Name
 * SteamID
 * Team
 * Clantag
 * Position
 * View-Direction
 * HP
 * Whether he is alive
 * The players team (CT / T / Spectator)
 * The players weapons
 * Kills
 * Deaths
 * Assists
 * MVPs
 * Score
 * Money
    * Current money
    * Current equipment value
* Scores
* Team-names
* The following game-events:
 * Player was attacked (for GOTV demos newer than July 1st 2015)
 * Exploding / starting / stopping of the following nades:
    * Grenade (position, throwing player)
    * Smoke (position, throwing player, when did it start, when did it stop)
    * Fire (position, ~~throwing player~~[1], when did it start, when did it stop)
    * Flash (position, throwing player, flashed players)
 * Weapon fired (who fired, what weapon, position)
 * Player died (weapon, killer, victim, weapon, position)
 * Round start
 * Match start
 * End of Freezetime
 * Bomb-Events

[1] This is not networked for some odd reason.
