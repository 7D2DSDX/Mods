Custom Categories for Custom Workstations
-----------------------------------------

This small SDX mod allows us to give custom workstations unique categories, allowing us to filter recipes and reduce lag.

Two new XML properties are available for Workstations, allowing up to 8 unique categories. The last category is a hard-coded "All", that will show all recipes for that workstation.

<property name="Categories" value="Basic,Iron Tools, Gifts" />
<property name="Categories_UI" value="ui_game_symbol_campfire,ui_game_symbol_tool,ui_game_symbol_science" />

This is a one-for-one match, and indicates which UI symbol to use for your Category.


For each of your items in the custom workstation, you'll need to populate the following property, to get them to filter:

<property name="Group" value="Basic"/>

or

<property name="Group" value="Iron Tools"/>
		

When implemented properly, it can drastically reduce the amount of lag for large-reciped mods.