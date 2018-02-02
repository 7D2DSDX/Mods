This mod adds instructions to allow any console commands compiled into Mods.dll to be added to the registered commands at server bootup.

ConsoleCmdLoader can be modified if you change the name of Mods.dll or have additional dll's to load commands from.

ConsoleCmdTest is included as an example command to check that all is working. As long as your command extends ConsoleCmdAbstract (and by extension IConsoleCommand) it will be added to the available commands in game.

Huge thanks to TormentedEmu and SphereII for the technical advice on IL code insertion, and to the people who tolerated my rage at it not working :P
