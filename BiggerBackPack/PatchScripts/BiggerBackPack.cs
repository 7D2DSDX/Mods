using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;


public class BiggerBackPack : IPatcherMod
{
    private sbyte OldInventorySize = 32;
    private sbyte NewInventorySize = 45;

    public bool Patch(ModuleDefinition module)
    {
        Console.WriteLine("==BiggerBack Pack Patcher===");
        SetAccessLevels(module);
        SetBackpackSize(module);
        return true;
    }

    private void SetAccessLevels(ModuleDefinition module)
    {
        // The EntityPlayerLocal is set to Private, so we can't make a change to it.
        var gm = module.Types.First(d => d.Name == "GameManager");
        var field = gm.Fields.First(d => d.FieldType.Name == "EntityPlayerLocal");
        SetFieldToPublic(field);
    }

    private void SetBackpackSize( ModuleDefinition module)
    {

        // Use the UpdateModule() helper method to adjust the GetSlots method in the Bag class, but only the first value.
        UpdateBackPack(module, "Bag", "GetSlots", 1);
        UpdateBackPack(module, "Bag", "SetSlots", 1);

        // PlayerDataFile is different, since we want to update it in the first three spots.
        UpdateBackPack(module, "PlayerDataFile", "Read", 3);

    }

    // Helper function to update the backPack module
    public void UpdateBackPack(ModuleDefinition module, String strModuleName, String strMethodName, int maxCounter)
    {
        // Retrieve the Class, Method, and the instructions of that method.
        var varModule = module.Types.First(d => d.Name == strModuleName);
        var varMethod = varModule.Methods.First(d => d.Name == strMethodName );
        var instructions = varMethod.Body.Instructions;

        int count = 0;
 
        // Loop around each instruction of the IL
        foreach (var i in instructions)
        {
            // If the IL code matches what we want, and has the default value we want (old inventory size is 32 ), update it with the new value.
            if ((i.OpCode == OpCodes.Ldc_I4_S) && ((sbyte)i.Operand == OldInventorySize))
            {
                // Update the value with the new backpack size, and break. There's nothing else we want to do in this method.
                i.Operand = NewInventorySize;

                // When adjusting the PlaterDataFile, there's 3 places where the back pack size is set. We want to go through this loop, grabbing each one
                // However, there are other "32" sizes that match, so we'll want to break after the third one.
                if ( ++count == maxCounter )
                    break;
            }
        }
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
