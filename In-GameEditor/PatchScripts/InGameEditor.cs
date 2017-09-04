using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;

public class CraftingLagFix : IPatcherMod
{
    private sbyte SearchLength = 3;
    
    public bool Patch(ModuleDefinition module)
    {
        Console.WriteLine("==Crafting Lag Patcher===");
        SetAccessLevels(module);
        ReduceRecipeLag(module);
        return true;
    }

    private void SetAccessLevels(ModuleDefinition module)
    {
        // The EntityPlayerLocal is set to Private, so we can't make a change to it.
        var gm = module.Types.First(d => d.Name == "GameManager");
        var field = gm.Fields.First(d => d.FieldType.Name == "EntityPlayerLocal");
        SetFieldToPublic(field);
    }

    // Set the filter on the search to be 2 or 3 characters
    private void ReduceRecipeLag( ModuleDefinition module )
    {
        var recipeClass = module.Types.First(d => d.Name == "XUiM_Recipes");
        var varMethod = recipeClass.Methods.First(d => d.Name == "FilterRecipesByName");
        var instructions = varMethod.Body.Instructions;
        var pro = varMethod.Body.GetILProcessor();

        // We need a reference to the GetLength call 
        var varGetLenth = module.Import(typeof(System.String).GetMethod("get_Length"));

        // Loop around each instruction of the IL
        foreach (var i in instructions.Reverse())
        {
            if ((i.OpCode == OpCodes.Newobj))
            {
                pro.InsertBefore(i, Instruction.Create(OpCodes.Ret));
                // 5	000A	newobj	instance void class [mscorlib]System.Collections.Generic.List`1<class Recipe>::.ctor()
               // var varNewInstance = module.Import(System.Collections.Generic{ }

               // pro.Body.Instructions.Add(Instruction.Create(OpCodes.Bge_S, "List<Recipes> alist = new List<Recipes>()" ));

               // pro.InsertBefore(i, Instruction.Create(OpCodes.Ldc_I4_0, SearchLength));

                //pro.InsertBefore(i, Instruction.Create(OpCodes.Callvirt, varGetLenth));
            }
        }

       // pro.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, varGetLenth));
       //pro.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0, SearchLength ));
       //pro.Body.Instructions.Add(Instruction.Create(OpCodes.Bge_S, "null    " ));
       // pro.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));


    }
     

    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
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
}