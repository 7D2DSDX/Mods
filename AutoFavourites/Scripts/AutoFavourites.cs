using System;

public class MyAutoFavourites : XUiC_CraftingWindowGroup 

{
    // Token: 0x0600474C RID: 18252
    public static void AddToFavourites(Recipe _recipe)
    {
        if (!CraftingManager.FavoriteRecipeList.Contains(_recipe.GetName()))
        {
            CraftingManager.ToggleFavoriteRecipe(_recipe);
        }
    }
}