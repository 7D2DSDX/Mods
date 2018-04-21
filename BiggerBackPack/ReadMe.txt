Bigger Back Pack SDX Edition
============================

The Bigger Back Pack allows you to easily update the backpack size up to 45. The latest version also allows you to add additional tool belt slots too.

For different sized back packs,

Edit   PatchScripts/BiggerBackPack.cs:
	Update:     private sbyte NewIntenvotrySize = 45;

	
Edit Config/BiggerBackPack.xml
	Update: 	The rows and cols
	

For Different sized tool belts,

Edit 	PatchScripts/Toolbelt.cs	
	Update:		private byte NewToolBeltSize = 8;
	
	If you want a 9 slot, set that value to 8. If you want a 10 slot, set this value to 9, etc.