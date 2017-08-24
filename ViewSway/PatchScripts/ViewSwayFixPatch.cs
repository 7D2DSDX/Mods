using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;

public class ViewSwapFix : IPatcherMod
{

    public bool Patch(ModuleDefinition module)
    {
        Console.WriteLine("==Patching View Sway Fix===");
        SetAccessLevels(module);
        return true;
    }

    private void SetAccessLevels(ModuleDefinition module)
    {
        // The EntityPlayerLocal is set to Private, so we can't make a change to it.
        Console.WriteLine("Updating vp_FPWeapon class");
        var gm = module.Types.First(d => d.Name == "vp_FPWeapon");
        UpdateFixedUpdate(module);

    }

    public void UpdateFixedUpdate(ModuleDefinition module)
    {
        var varClass = module.Types.First(d => d.Name == "vp_FPWeapon");
        var varMethod = varClass.Methods.First(d => d.Name == "FixedUpdate");
        var instructions = varMethod.Body.Instructions;

        // List to store the instructions we want to remove.
        List<int> lstRemove = new List<int>();


        foreach (var i in instructions)
        {
            
            // If the IL code matches what we want, and has the default value we want (old inventory size is 32 ), update it with the new value.
            if ((i.OpCode == OpCodes.Ldarg_0)  && (i.Next.OpCode == OpCodes.Callvirt ) )       
            {
                // Search for the sub strings of the items we want, and flag them for removal
                if ((i.Next.Operand.ToString().Contains("UpdateSwaying")))
                {
                    lstRemove.Add(instructions.IndexOf(i));
                    lstRemove.Add(instructions.IndexOf(i.Next));
                }
                if ((i.Next.Operand.ToString().Contains("UpdateBob")))
                {
                    lstRemove.Add(instructions.IndexOf(i));
                    lstRemove.Add(instructions.IndexOf(i.Next));
                }
                if ((i.Next.Operand.ToString().Contains("UpdateSprings")))
                {
                    lstRemove.Add(instructions.IndexOf(i));
                    lstRemove.Add(instructions.IndexOf(i.Next));
                }
             
            }
        }

        lstRemove.Reverse();
        foreach (int i in lstRemove)
        {
            instructions.RemoveAt(i);
        }
    }


    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        HookMethods(gameModule, modModule);
        return true;
    }

    private void HookMethods(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        var WeaponClass = gameModule.Types.First(d => d.Name == "vp_FPWeapon");

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