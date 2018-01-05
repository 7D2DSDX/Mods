using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class WorkstationCategories : XUiC_CategoryList 
{
    /*************
     * The Workstation Categories allows us to add our own categories to new custom work benches.
     * 
     * In the XML for the work station, a new Categories tag is set. This is a comma-sepearted value for each category you want to display.
     * 
     * For example, the following adds 3 new cateogires, Basic, Iron Tools, and Gifts
     * 		<property name="Categories" value="Basic,Iron Tools, Gifts" />
     *
     * In addition to the Categories, another node Categories_UI is also exposted. It's a comma-seperated value, that specifies the UI sprite
     * that is to be displayed, in the same order as listed as the Categories node.
     * 
     * 		<property name="Categories_UI" value="ui_game_symbol_campfire,ui_game_symbol_tool,ui_game_symbol_science" />
     * 	
     * 	This would assign the ui_game_symbol_campfire to "Basic", while ui_game_symbol_science is assigned as "Gifts". If the UI value isnt' available, the campfire is defaulted.
     *
     */

    // Reserving last category for "All". 
    int MaxCategories = 8;
   
    bool blDebug = true;
    public void Log( String strMessage)
    {
        if (blDebug)
            Debug.Log("WorkstationCategories: " + strMessage);
    }
    
    public void GetExtendedCategories(String strWorkstation)
    {
        // We wnat to store the categories in a list to reference them in a loop.
        List<String> lstCategories = new List<String>();
        List<String> lstCategories_UI = new List<String>();

  


        // Grab a reference to the block, so we can identify the workstation
        BlockValue block = Block.GetBlockValue(strWorkstation);
        Block workBenchBlock = block.Block;


        // See if we have cateogires UI element tag, and read them in.
        if (workBenchBlock.Properties.Values.ContainsKey("Categories_UI"))
            lstCategories_UI = workBenchBlock.Properties.Values["Categories_UI"].Split(',').ToList();
    

        // If we have our own categories, then we'll use them. Otherwise, we'll set the categories specified in the xml
        if ( workBenchBlock.Properties.Values.ContainsKey("Categories") )
        {
            Log("Reading Categories...");
            lstCategories = workBenchBlock.Properties.Values["Categories"].Split(',').ToList();

            // Clear the default categories that we have set above
            for ( int x = 0; x < 9; x++ )
                SetCategoryEmpty(x);

            // Loop around for each cateogiry, and assigning its UI match. 
            // If there's no UI symbol set, then use the default campfire.
            for (int x = 0; x < lstCategories.Count; x++ )
            {
                String strCategory = lstCategories[x].ToString();
                String strCategoryUI = "ui_game_symbol_campfire";

                // If there's a category set, then use it.
                if (lstCategories_UI[x] != null)
                    strCategoryUI = lstCategories_UI[x].ToString();

                // Set the new category
                SetCategoryEntry(x, strCategory, strCategoryUI, strCategory);
            }

            Log("Adding Default Category");
            // we add the fourth parameter "all", to show that it includes everything, while String.Empty sets the "filter", and will show everything.
            SetCategoryEntry(8, string.Empty, "ui_game_symbol_campfire","All");

            return;
           
        }
        // Set up default Cateogires, in case the work bench isn't set up correctly.
        SetCategoryEntry(0, "Basics", "ui_game_symbol_campfire");
        SetCategoryEntry(1, "Building", "ui_game_symbol_map_house");
        SetCategoryEntry(2, "Resources", "ui_game_symbol_resource");
        SetCategoryEntry(3, "Ammo/Weapons", "ui_game_symbol_knife");
        SetCategoryEntry(4, "Tools/Traps", "ui_game_symbol_tool");
        SetCategoryEntry(5, "Food/Cooking", "ui_game_symbol_fork");
        SetCategoryEntry(6, "Decor/Miscellaneous", "ui_game_symbol_chair");
        SetCategoryEntry(7, "Clothing", "ui_game_symbol_shirt");
        SetCategoryEntry(8, string.Empty, "ui_game_symbol_campfire", "All");

    }

 
}

