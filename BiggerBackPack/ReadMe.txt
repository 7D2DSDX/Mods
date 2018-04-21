For different sized back packs,

Edit   PatchScripts/BiggerBackPack.cs:
	Update:     private sbyte NewIntenvotrySize = 45;

	
Edit Config/BiggerBackPack.xml
	Update: 	The rows and cols
	

For Different sized tool belts,

Edit 	PatchScripts/Toolbelt.cs	
	Update:		private byte NewToolBeltSize = 8;
	
	If you want a 9 slot, set that value to 8. If you want a 10 slot, set this value to 9, etc.