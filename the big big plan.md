These are my ideas and plans for the new version
this will require a complete rewrite of the old mod and also some parts of the game
0 Prerequisite: unpack the old mod V0 ( inside the zip folder ) and check how it worked
=> the goal is to have all the features inside it at-minimum

0.5 i have made many attempts at decompiling the game, and all failed. now it is your turn.
for efficient mod development we need a stable api & understanding of the game, hence a decompilation will be nessecary.

step 1000 is building an api
i do not want to do this in one step but rather along the way.
As we work on this mod we will build this API by forcing all code to interact through this api.
if the modding api is missing a feature we need => we will add said feature and cuntinue building the mod right after
the api should allow easy access to the game's core features, whilst preventing conflicts
e.g. adding a Player class that holds all Data and acessible fields and functions you might want
e.g. Player.get_local().noclip(true)
e.g. Player.get_online_players[0].Inventory.setSlot( Items.Scissors , Slot.Slot0 )
you get the idea i think
1. change :
when mod is active => game should launch in windowed mode and disable fullscreen => this will allow us to add a dedicated mapeditor window that can be also moved to another window

2. change : build our own mapeditor window and gui that spawns as long as you are inside the map-editing part of the game
this will be a very large and complicated task and take alot of time
for the

3. change instead of changing the actual ingame-editors spawnlist => we should instead move all spawnable items and things into our custom ui
=> our first step to this is to copy all the features that the exising mappingui of the basegame has to our custom ui with full functionality ( ui = square window not linked to game but rendered and made using unity, with tabs on the top the first tab is mod-settings for now, the second is our mappingui )
=> the mappingui should also display the icons of the mapobjects that we are trying to use and have full functionality


4. when the custom-mappingui is fully functional => here comes the hard part everybody keeps nagging about:

everyone wants all these available inside the new version:
electric fences
buttons triggers linking things together
custom assets inside the maps
all Dlc-map-assets

and many more
basically everybody wants this mod to add all the features that the developers had at hand when they made their own maps
this should be not that hard to make, if we managed to actually geta working fully featured decompilation.
