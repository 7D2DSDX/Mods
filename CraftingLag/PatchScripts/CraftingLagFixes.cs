using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;
using SDX.Core;
public class CraftingLagFix : IPatcherMod
{
  
    // Debug Logging
    private bool DebugLog = true;

    public bool Patch(ModuleDefinition module)
    {
        Log("=== Crafting Lag Patcher ===");
        ReduceRecipeLag(module);
        RemoveSetAllChildrenDirty(module);
        ReduceWorkstationGridLag(module);

        Log("Attempt fix at Forge Grid ==");




        // The HandleSlotChangedEvent in the XuC_ItemStackGrid makes a call to GetUISlots, and updates the BackEnd.
        // however, the UpdateBackend does nothing, and the GetUISlot does all the work. Let's clean it up.
        var gm = module.Types.First(d => d.Name == "XUiC_ItemStackGrid");
        var method = gm.Methods.First(d => d.Name == "HandleSlotChangedEvent");

        var instructions = method.Body.Instructions;
        var pro = method.Body.GetILProcessor();
        var updateBack = gm.Methods.First(d => d.Name == "UpdateBackend");


        
        int delNextLines = 0;
        foreach (var i in instructions.Reverse())
        {
            if (i.OpCode == OpCodes.Callvirt && i.Operand == updateBack)
            {
                Log("Removing UpdateBack");
                delNextLines = 5;
                instructions.Remove(i);
                continue;
            }

            // We want to delete the virtCall, plus the "this" attached to it, so enable the flag.
            if (delNextLines > 0)
            {
                instructions.Remove(i);
                delNextLines--;
            }
        }

        return true;
    }

 
    private void ReduceWorkstationGridLag( ModuleDefinition module )
    {
        List<String> Workstations = new List<String>();
        Workstations.Add("XUiC_WorkstationOutputGrid");
        Workstations.Add("XUiC_WorkstationInputGrid");
        Workstations.Add("XUiC_WorkstationFuelGrid");
       // Workstations.Add("XUiC_WorkstationToolGrid");

        // We want to remove the SetAllChildrenDirty from most calls, so find the reference in the XUIController class, then the method.
        var XUIControllerClass = module.Types.First(d => d.Name == "XUiController");
        var childDirty = XUIControllerClass.Methods.First(d => d.Name == "SetAllChildrenDirty");
        var isDirty = XUIControllerClass.Fields.First(d => d.Name == "IsDirty");
        Log("Found the SetAllChildrenDirty Method ");

        foreach (String strWorkstation in Workstations)
        {
            Log("Reducing Workstation Grid Lag: " + strWorkstation);

            var WorkstationClass = module.Types.First(d => d.Name == strWorkstation);
            var method = WorkstationClass.Methods.First(d => d.Name == "UpdateBackend");

 
            int delNextLines = 0;
            var instructions = method.Body.Instructions;
            var pro = method.Body.GetILProcessor();
            foreach (var i in instructions.Reverse())
            {
              
                // We want the first opcode for ldc_i4 and change it from 0 to 3
                if (i.OpCode == OpCodes.Callvirt && i.Operand == childDirty)
                {

                    Log("Removing call to " + childDirty.Name.ToString() + " in " + method.Name.ToString());
                    instructions.Remove(i);
                    delNextLines = 3;
                    continue;
                }

                // We want to delete the virtCall, plus the "this" attached to it, so enable the flag.
                if (delNextLines > 0)
                {
                    instructions.Remove(i);
                    delNextLines--;
                }

                // We want to add the opcodes as the last thing in the method.
                if (i.OpCode == OpCodes.Ret)
                {
                    pro.InsertBefore(i, Instruction.Create(OpCodes.Ldarg_0));
                    pro.InsertBefore(i, Instruction.Create(OpCodes.Ldc_I4_1));
                    pro.InsertBefore(i, Instruction.Create(OpCodes.Stfld, isDirty));
                }
            }
     
            
        }

    }
    // Set the filter on the search to be 2 or 3 characters
    private void ReduceRecipeLag( ModuleDefinition module )
    {
        // Grab the XUiC_Recipe Class
        var recipeClass = module.Types.First(d => d.Name == "XUiC_RecipeList");
        var classMethods = recipeClass.Methods;
        // The method we want may be obfuscated, so we need to be creative to find it. Find all the methods in this class.

        // For each of these methods, we want to loop around, looking at the instructions for a method call
        // that's inside of them.
        foreach (var method in classMethods)
        {

            var body = method.Body.Instructions
                    .Where(instruction => instruction.OpCode == OpCodes.Call)
                    .Select(instruction => (MethodReference)instruction.Operand)
                    .Where(methodReference => methodReference.FullName.Contains("FilterRecipesByName"));

            // If the method search didn't find it, continue to the next one.
            if (body.Count() == 0)
                continue;

            Log("Found reference to FilterRecipesByName in " + method.FullName.ToString());
            // Now that we have our method, we want to get the IL instructions, and update the first LDC_I4
            var instructions = method.Body.Instructions;
            var pro = method.Body.GetILProcessor();
            foreach (var i in instructions)
            {
                // Since we are looking for the ble branch op code, we want to change the value of the *previous* opcode.
                if (i.OpCode == OpCodes.Ldc_I4_0 )
                {
                    Log("Updating Search Parameter.");
                    // The Op Code we want to change is two 
                    //i.Previous.Operand = (sbyte)2;
                    i.OpCode = OpCodes.Ldc_I4_2;
                    break;
                }
            }

            break;
            
        }

        Log("Completed RecipeList Lag Fix.");
    }


    private void RemoveSetAllChildrenDirty(ModuleDefinition module)
    {
        // The method we want may be obfuscated, so we need to be creative to find it. Find all the methods in this class.

        Log("== Remove Dirty Bit ==");

        // Grab the class and all its methods.
        var workStationClass = module.Types.First(d => d.Name == "XUiC_WorkstationWindowGroup");  
        var classMethods = workStationClass.Methods;

        // We want to remove the SetAlLChildrenDirty from most calls, so find the reference in the XUIController class, then the method.
        var XUIControllerClass = module.Types.First(d => d.Name == "XUiController");
        var childDirty = XUIControllerClass.Methods.First(d => d.Name == "SetAllChildrenDirty");
        var isDirty = XUIControllerClass.Fields.First(d => d.Name == "IsDirty");
        Log("Found the SetAllChildrenDirty Method ");


        // For each of these methods, we want to loop around, looking at the instructions for a method call
        // that's inside of them.
        foreach (var method in classMethods)
        {
            Log("Searching in " + method.Name.ToString());
            var body = method.Body.Instructions
                    .Where(instruction => instruction.OpCode == OpCodes.Callvirt)
                    .Select(instruction => (MethodReference)instruction.Operand)
                    .Where(methodReference => methodReference.FullName.Contains("SetAllChildrenDirty"));

            // no instructions found, so just continue the loop.
            if (body.Count() == 0)
                continue;

            // We don't want to remove the dirty bits on the OnOpen or OnClose
            if (method.Name.Contains("OnOpen") || method.Name.Contains("OnClose"))
                continue;

          

            int delNextLines = 0;
            var instructions = method.Body.Instructions;
            var pro = method.Body.GetILProcessor();
            foreach (var i in instructions.Reverse())
            {
                // We want the first opcode for ldc_i4 and change it from 0 to 3
                if (i.OpCode == OpCodes.Callvirt && i.Operand == childDirty)
                {
                    Log("Removing call to " + childDirty.Name.ToString() + " in " + method.Name.ToString());
                    instructions.Remove(i);
                    delNextLines = 1;
                    continue;
                }

    
                // We want to delete the virtCall, plus the "this" attached to it, so enable the flag.
                if (delNextLines > 0)
                {
                    Log("Removing: " + i.OpCode.Name.ToString());
                    instructions.Remove(i);
                    delNextLines--;
                }

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

    private void Log( String strLogMessage )
    {
        if (this.DebugLog == true)
            SDX.Core.Logging.LogInfo( this.GetType().Name.ToString() + ": " + strLogMessage);

    }
}