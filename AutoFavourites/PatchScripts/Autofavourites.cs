using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;
using SDX.Core;
public class AutoFavouritePatcher : IPatcherMod
{
  
    // Debug Logging
    private bool DebugLog = true;

    public bool Patch(ModuleDefinition module)
    {
        return true;
    }

    private void AutoFavourites(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        // We want to add our hook into the AddRecipeToCraft call
        var myClass = gameModule.Types.First(d => d.Name == "XUiC_CraftingQueue");
        var myMethod = myClass.Methods.First(d => d.Name == "AddRecipeToCraft");

        // Local reference to our new method to auto add the crafting item to the favourites
        var MyAutoFav = modModule.Types.First(d => d.Name == "MyAutoFavourites");
        var MyAutoFavHook = gameModule.Import(MyAutoFav.Methods.First(d => d.Name == "AddToFavourites"));

        // Grab all the IL instructions
        var instructions = myMethod.Body.Instructions;
                
        // The pro allows us to edit an manipulate new instructions.
        var pro = myMethod.Body.GetILProcessor();
        foreach (var i in instructions.Reverse())
        {
            // sdfld is unique, so we want to add the instructions  right after this
            if (i.OpCode == OpCodes.Stfld)
            {
                // Add a call to our AutoFavHook, passing in the first parameter (which is _recipe)
                pro.InsertAfter(i, Instruction.Create(OpCodes.Call, MyAutoFavHook));
                pro.InsertAfter(i, Instruction.Create(OpCodes.Ldarg_1));
            }
        }
    }


    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
		Log("Enabled the AutoFavourites Patch" );
        AutoFavourites(gameModule, modModule);
        return true;
    }

 
    // Helper functions to allow us to access and change variables that are otherwise unavailable.
    private void SetMethodToVirtual(MethodDefinition meth)
    {
        meth.IsVirtual = true;
    }

    private void SetFieldToPublic(FieldDefinition field)
    {
        field.IsFamily = false;
        field.IsPrivate = false;
        field.IsPublic = true;

    }
    private void SetMethodToPublic(MethodDefinition field)
    {
        field.IsFamily = false;
        field.IsPrivate = false;
        field.IsPublic = true;

    }

    private void Log( String strLogMessage )
    {
        if (this.DebugLog == true)
            SDX.Core.Logging.LogInfo( this.GetType().Name.ToString() + ": " + strLogMessage);

    }
}