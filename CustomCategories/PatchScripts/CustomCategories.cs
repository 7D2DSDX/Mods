using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;
using SDX.Core;
public class CustomCateogires : IPatcherMod
{
  
    // Debug Logging
    private bool DebugLog = true;

    public bool Patch(ModuleDefinition module)
    {
        Log("=== Custom Categories Patcher ===");
        return true;
    }

   
    private void AddDefaultCategories(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        Log("Adding Default Categories");
        // By default, there's no categories on Custom Workstations. This can cause a lot of inventory lag as it loads up all the avialable 
        // recipes, rather than just the category selected.
        var XUiC_CategoryList = gameModule.Types.First(d => d.Name == "XUiC_CategoryList");
        var method = XUiC_CategoryList.Methods.First(d => d.Name == "SetupCategoriesByWorkstation");

        // We created a new method that we are going to call. Let's find a reference to that new one.
        Log("Finding Reference to the new Method");
        var WorkstationCategoriesClass = modModule.Types.First(d => d.Name == "WorkstationCategories");
        var WorkstationCategoriesMethod = gameModule.Import(WorkstationCategoriesClass.Methods.First(d => d.Name == "GetExtendedCategories"));

        Log("Workstation Categories: " + WorkstationCategoriesMethod.ToString());
        // Grab the insturctiosn and loop around reverse. We want to inject our method call right before the last return true statement, which is called
        // right after it sets the default category.
        var instructions = method.Body.Instructions;
        var pro = method.Body.GetILProcessor();
        foreach (var i in instructions.Reverse())
        {

            // We want the first opcode for ldc_i4 and change it from 0 to 3
            if (i.OpCode == OpCodes.Ldc_I4_1)
            {
                pro.InsertBefore(i, Instruction.Create(OpCodes.Ldarg_0));
                pro.InsertBefore(i, Instruction.Create(OpCodes.Ldarg_1));

                pro.InsertBefore( i, Instruction.Create(OpCodes.Callvirt, WorkstationCategoriesMethod));


                break;
            }
        }

    }


  
    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        AddDefaultCategories(gameModule, modModule);
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