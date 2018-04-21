using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;


public class ToolBeltPatch : IPatcherMod
{
    // This is a bit confusing, but it'll cascade through correctly. The newToolBeltSize is your desired size, minus 1.
    // The game uses many different style checks for the size of the backpacks, and the helper methods do the actual adjustments as needed.

    // If you want a 9 tool belt slot, which is default, this needs to be 8.
    // Want a 10 tool belt slot, set this to 9.
    private byte NewToolBeltSize = 8;
    private bool DebugLog = true;
    public bool Patch(ModuleDefinition module)
    {
        Console.WriteLine("==Tool Belt Patcher===");
        SetAccessLevels(module);
        SetToolBeltSize(module);
        return true;
    }

    private void SetAccessLevels(ModuleDefinition module)
    {
        // The EntityPlayerLocal is set to Private, so we can't make a change to it.
        var gm = module.Types.First(d => d.Name == "GameManager");
        var field = gm.Fields.First(d => d.FieldType.Name == "EntityPlayerLocal");
        SetFieldToPublic(field);
    }

    private void SetToolBeltSize(ModuleDefinition module)
    {

        // The constructor sets our array length, so we'll call this method for that
        AdjustConstructor(module, "Inventory", ".ctor", true);
        BumpToolBeltSize(module, "Inventory", ".ctor", false);
        BumpToolBeltSize(module, "Inventory", ".ctor", true);

        // General purpose method, taking all the loops from the default of 8, up to whatever number is specified at the top of the file.
        BumpToolBeltSize(module, "Inventory", "CanStack", false);
        BumpToolBeltSize(module, "Inventory", "CanStackNoEmpty", false);
        BumpToolBeltSize(module, "ItemActionEntryEquip", "RefreshEnabled", false);
        BumpToolBeltSize(module, "ItemActionEntryUse", "OnActivated", false);
        BumpToolBeltSize(module, "ItemActionEntryUse", "OnDisabledActivate", false);
        BumpToolBeltSize(module, "ItemActionEntryUse", "RefreshEnabled", false);


        BumpToolBeltSize(module, "NGUIContextMenuItemConsume", "ExecuteAction", false);
        BumpToolBeltSize(module, "NGUIContextMenuItemConsume", "Update", false);

        BumpToolBeltSize(module, "NGuiInvGridQuickAccess", "Update", false);
        BumpToolBeltSize(module, "XUiC_Toolbelt", "Update", false);

        BumpToolBeltSize(module, "EntityVehicle", "hasGasCan", false);
        BumpToolBeltSize(module, "EntityAlive", "SetAttachedTo", false);

        BumpToolBeltSize(module, "PlayerMoveController", "Start", true);

        // There's multiple values here we need to change, basically when we see a 7, we want to turn it to a the actual number of slots,
        // and if we see an 8, we want to bump the actual number
        BumpToolBeltSize(module, "PlayerMoveController", "Update", true);
        BumpToolBeltSize(module, "PlayerMoveController", "Update", false);

        // This updates the numeric keys for shortcuts
        BumpToolBeltSize(module, "PlayerActionsLocal", "get_InventorySlotIsPressed", false);
        BumpToolBeltSize(module, "PlayerActionsLocal", "get_InventorySlotWasReleased", false);
        BumpToolBeltSize(module, "PlayerActionsLocal", "get_InventorySlotWasPressed", false);

        // Lets us drop the items in the tool belt.
        BumpToolBeltSize(module, "EntityPlayerLocal", "dropBackpack", false);


    }
    private void Log(String strLogMessage)
    {
        if (this.DebugLog == true)
            SDX.Core.Logging.LogInfo(this.GetType().Name.ToString() + ": " + strLogMessage);

    }

    // This method bumps the baseline Toolbelt size from whatever you've set above, + 1, as the loop goes something like this:
    // if ( int i = 0; i < ToolBeltSize; i++ ), with ToolBeltSize being above
    private void AdjustConstructor(ModuleDefinition module, String strClass, String strMethod, bool IsActualNumber)
    {
        // Retrieve the Class, Method, and the instructions of that method.
        var varModule = module.Types.First(d => d.Name == strClass);
    
        var varMethod = varModule.Methods.First(d => d.Name == strMethod);
        var instructions = varMethod.Body.Instructions;
        // Loop around each instruction of the IL
        foreach (var i in instructions)
        {
            if (IsActualNumber)
            {
                if ((i.OpCode == OpCodes.Ldc_I4_S))
                {
                    i.OpCode = OpCodes.Ldc_I4;
                    i.Operand = (int)NewToolBeltSize + 2;
                }
            }
        }

    }
    // This method bumps the baseline Toolbelt size from whatever you've set above, + 1, as the loop goes something like this:
    // if ( int i = 0; i < ToolBeltSize; i++ ), with ToolBeltSize being above
    private void BumpToolBeltSize(ModuleDefinition module, String strClass, String strMethod, bool IsActualNumber)
    {
        // Retrieve the Class, Method, and the instructions of that method.
        var varModule = module.Types.First(d => d.Name == strClass);
   
        var varMethod = varModule.Methods.First(d => d.Name == strMethod);
        var instructions = varMethod.Body.Instructions;
        // Loop around each instruction of the IL
        foreach (var i in instructions)
        {
           // Log("Instruction: " + i.ToString());

            if (IsActualNumber)
            {
                if ((i.OpCode == OpCodes.Ldc_I4_7))
                {
                    i.OpCode = OpCodes.Ldc_I4;
                    i.Operand = (int)NewToolBeltSize;
                }
            }
            else
            {
                if ((i.OpCode == OpCodes.Ldc_I4_8))
                {
                    i.OpCode = OpCodes.Ldc_I4;
                    i.Operand = (int)NewToolBeltSize + 1;
                }

            }
        }

    }

 

  
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        var myClass = gameModule.Types.First(d => d.Name == "ItemActionEntryUse");
        var myMethod = myClass.Methods.First(d => d.Name == "OnTimerCompleted");
        var instructions = myMethod.Body.Instructions;

        // We want to replace the decInventoryLater with our own
        var myNewClass = modModule.Types.First(d => d.Name == "InventoryHelperSDX");
        var myNewMethod = gameModule.Import(myNewClass.Methods.First(d => d.Name == "decInventoryLater"));

        foreach (var i in instructions.Reverse())
        {
            if (i.OpCode == OpCodes.Call)
            {
                // We only want to replace the first call we find, when in reverse.
                i.Operand = myNewMethod;
                break;
            }
        }
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
