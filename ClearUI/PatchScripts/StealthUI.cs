using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;

public class RemoveStealthUI : IPatcherMod
{

    // Debug Logging
    private bool DebugLog = true;

    public bool Patch(ModuleDefinition module)
    {
        Log("=== " + this.GetType().Name.ToString() + " ===");
            RemoveStealth(module);
        return true;

    }


    // Generic Helper Method to remove a method, and replace it with a return type.
    private void CleanMethod(MethodDefinition method)
    {
        Log(" - Cleaning " + method.Name.ToString() );

        var pro = method.Body.GetILProcessor();
        pro.Body.Instructions.Clear();
        Log("    - Return Type is Detected as: " + method.ReturnType.Name.ToLower());

        // We want to auto-detect what the return value is for this method, and return something relevant and generic.
        switch ( method.ReturnType.Name.ToLower() )
        {
            case "string":
                var StringEmpty = method.Module.Import(typeof(System.String).GetField("Empty"));
                pro.Body.Instructions.Add(Instruction.Create(OpCodes.Ldsfld, StringEmpty));
                break;
            case "boolean":
                pro.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                break;
            default:
                break;
        }

        pro.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));     
    }

    private void RemoveStealth(ModuleDefinition module)
    {       
        // Removing Cross Hairs from UI
        Log("Clearing CrossHairs");
        var myClass = module.Types.First(d => d.Name == "EntityPlayerLocal");
        var myMethod = myClass.Methods.First(d => d.Name == "guiDrawCrosshair");
        CleanMethod(myMethod);

        // While the UI was adjusted and removed, the dimming effect was still enabled. This removes it.
        myClass = module.Types.First(d => d.Name == "StealthScreenOverlay");
        myMethod = myClass.Methods.First(d => d.Name == "Update");
        CleanMethod(myMethod);

        // Removes the Compass from the main GUI. Inhibits the marking for the bed roll, and other compass updates.
        Log("Clearing Compass Window Update Method");
        myClass = module.Types.First(d => d.Name == "XUiC_CompassWindow");
        myMethod = myClass.Methods.First(d => d.Name == "Update");
        CleanMethod(myMethod);

        // We need to remove the Downgrade and Upgrade overlays. This also takes care of displaying damage on trees / rocks / corpses.
        Log("Disabling Overlay for Upgrade / Downgrade");
        myClass = module.Types.First(d => d.Name == "ItemActionAttack");
        myMethod = myClass.Methods.First(d => d.Name == "canShowOverlay");
        CleanMethod(myMethod);

        Log("Removing OVerlay for Item Action use other");
        myClass = module.Types.First(d => d.Name == "ItemActionUseOther");
        myMethod = myClass.Methods.First(d => d.Name == "canShowOverlay");
        CleanMethod(myMethod);

        Log("Removing Overlay for Item Action Ranged");
        myClass = module.Types.First(d => d.Name == "ItemActionRanged");
        myMethod = myClass.Methods.First(d => d.Name == "canShowOverlay");
        CleanMethod(myMethod);

        Log("Removing Tool Tips for Journal");
        myClass = module.Types.First(d => d.Name == "XUiC_TipWindow");
        myMethod = myClass.Methods.First(d => d.Name == "ShowTip");
        CleanMethod(myMethod);

        // Removing Tool Tips, such as skill perks
        Log("Removing Tool Tips");
        myClass = module.Types.First(d => d.Name == "NGuiWdwInGameHUD");
        myMethod = myClass.Methods.First(d => d.Name == "ShowInfoText");
        CleanMethod(myMethod);

        // Tool Tips has a few over-rides, so let's get them too.
        foreach (var method in myClass.Methods.Where(d => d.Name == "SetTooltipText"))
        {
            CleanMethod(method);
        }

        // Removes the Damage Multiplier and Sneak Damage messages
        myClass = module.Types.First(d => d.Name == "EntityPlayerLocal");
        myMethod = myClass.Methods.First(d => d.Name == "NotifySneakDamage");
        CleanMethod(myMethod);

        myMethod = myClass.Methods.First(d => d.Name == "NotifyDamageMultiplier");
        CleanMethod(myMethod);
        

        // This is an awkward one. We need to to update the PlayverMoveController::Update()
        // but it's a big method, and we only want to hit a few things.
        myClass = module.Types.First(d => d.Name == "PlayerMoveController");
        myMethod = myClass.Methods.First(d => d.Name == "Update");

        // We want to search for the SetLabelText call, so find the reference
        var nguiWindow = module.Types.First(d => d.Name == "NGUIWindowManager");
        var SetLabelMethod = nguiWindow.Methods.First(d => d.Name == "SetLabelText");

        // We want to replace the localized text with String.Empty.
        var StringEmpty = module.Import(typeof(System.String).GetField("Empty"));

        var pro = myMethod.Body.GetILProcessor();
        var instructions = myMethod.Body.Instructions;
        foreach ( var i in instructions )
        {
            // We don't want to change the method call; we want to change the operand right before, so that the parameter is String.Empty, rather than the generated
            // localized text.
            if ( i.OpCode == OpCodes.Callvirt && i.Operand == SetLabelMethod)
            {
                i.Previous.OpCode = OpCodes.Ldsfld;
                i.Previous.Operand = StringEmpty;
            }
        }


        // We need to disable the chat screen. We'll remove the contents of ShowAll, and just add in a call to close the base.
        // This has an effect of opening and closing the chat window quickly, and not noticable.
        myClass = module.Types.First(d => d.Name == "GUIWindowChatInputLine");
        myMethod = myClass.Methods.First(d => d.Name == "Update");
        pro = myMethod.Body.GetILProcessor();

        // We want to change the second brfalse_s to true, so that it thinks the player pressed the cancel key.
        int Counter = 0;
        foreach( var i in pro.Body.Instructions )
        {
            if ( i.OpCode == OpCodes.Brfalse_S )
            {
                Counter++;
                if (Counter == 2)
                {
                    i.OpCode = OpCodes.Brtrue_S;
                    break;
                }
            }
        }

        // Remove the contents on the OnOpen of the GUIWindowChatInputLine too.
        myMethod = myClass.Methods.First(d => d.Name == "OnOpen");
        CleanMethod(myMethod);
       
    }



    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        return true;
    }


    private void Log(String strLogMessage)
    {
        if (this.DebugLog == true)
            SDX.Core.Logging.LogInfo(this.GetType().Name.ToString() + ": " + strLogMessage);

    }
}