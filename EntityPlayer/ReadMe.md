Entity Player
-------------

The EntityPlayer SDX mod lets you over-ride the default player class, and add new functionality.

Base Features are:

- OneBlock Crouch: Allows the player to enter one-space openings
- Soft Hands: If this is set to true, then players will recieve 1 point of damage every time they hit with their bare hands
- JumpingDrain: If this is set to true, the players will take a stamina hit every time they jump.

The following Properties are set on the Player's entry in the entityclasses.xml:

      <!-- Turns on one block crouch -->
      <property name="OneBlockCrouch" value="true" />

      <!-- Turns on damage when hitting things with your bare hands -->
      <property name="SoftHands" value="true" />

      <!-- take a stamina hit for jumping -->
      <property name="JumpingDrain" value="true" />

	  
Experimental Features:
-----------------------------------------------

Encumbrance Update!
===================

The EntityPlayer Modlet has been expanded to include a new feature: Encumbrance!

Now, when your player is over-burdened, some buffs will kick in, depending on how badly you are overloaded, and the player will take a hit on movement speeds. Encumbrance is calculated on a per-item basis, updated everytime there's a change in the back pack items. ItemWeight is multiplied by the stack size.

By over-riding the player's class, we expose a few new XML properties for more control.

	In entityclasses.xml, new XML properties have been added:
	
		- Max Encumbrance is defined in the entityclasses.xml, in the player entry
			<property name="MaxEncumbrance" value="10000" />

		- Use Encumbrance to turn on and off the feature
			<property name="UseEncumbrance" value="false" />
	  
	In items.xml, new XML property has been added:
	
		- For each item that you want to adjust the weight too, add the following tag:
			<property name="ItemWeight" value="1" />
    
	Everything that does not have an ItemWeight will default to 0.1


To Do:
	Display an encumbrance total for the user to see
	
