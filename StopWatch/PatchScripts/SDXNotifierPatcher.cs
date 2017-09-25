using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

public class SDXStopWatchPatcher : IPatcherMod
{
    // Debug Logging
    private bool DebugLog = true;
    public bool Patch(ModuleDefinition module)
    {
       // HookMethods(module);
        return true;

    }
    private void HookMethods(ModuleDefinition module, ModuleDefinition modModule)
    {
       
        // Enter in a List of classes you want to instrument monitoring on
        List<String> WatchedClasses = new List<String>();
        WatchedClasses.Add("XUiC_RecipeStack");
      

        // Loop around each class selected, then loop around each method to instrument it with ConsoleWriteLines
        foreach (String strWatchedClass in WatchedClasses)
        {
            Log("Searching for " + strWatchedClass);
            var WatchedClass = module.Types.First(d => d.Name == strWatchedClass);
            foreach (var method in WatchedClass.Methods)
            {
                Log("Weaving " + method.Name.ToString());
                WeaveMethod(module, modModule, method);
            }
        }
    }


  
    private static void WeaveMethod(ModuleDefinition module, ModuleDefinition modModule, MethodDefinition method)
    {
        Log("Looking for Notifier Class");
        var SDXNotifier = modModule.Types.First(d => d.Name == "SDXNotifier");
        var enterMethod = module.Import(SDXNotifier.Methods.First(d => d.Name == "NotifyEnter"));
        var exitMethod = module.Import(SDXNotifier.Methods.First(d => d.Name == "NotifyExit"));
        var jumpFromMethod = module.Import(SDXNotifier.Methods.First(d => d.Name == "NotifyJumpOut"));
        var jumpBackMethod = module.Import(SDXNotifier.Methods.First(d => d.Name == "NotifyJumpBack"));

       

        Log("Looking for enterMethod");
        var enterReference = module.Import(enterMethod);
        Log("Looking for exitMethod");
        var exitReference = module.Import(exitMethod);
        Log("Looking for jumpFromMethod");
        var jumpFromReference = module.Import(jumpFromMethod);
        Log("Looking for jumpBackMethod");
        var jumpBackReference = module.Import(jumpBackMethod);
        string name = method.DeclaringType.FullName + "." + method.Name;

        WeaveEnter(method, enterReference, name); 
      //  WeaveJump(method, jumpFromReference, jumpBackReference, name);
        WeaveExit(method, exitReference, name);
    }
    private static void WeaveExit(MethodDefinition method, MethodReference exitReference, string name)
    {
        ILProcessor ilProcessor = method.Body.GetILProcessor();

        List<Instruction> returnInstructions = method.Body.Instructions.Where(instruction => instruction.OpCode == OpCodes.Ret).ToList();
        foreach (var returnInstruction in returnInstructions)
        {
            Instruction loadNameInstruction = ilProcessor.Create(OpCodes.Ldstr, name);
            Instruction callExitReference = ilProcessor.Create(OpCodes.Call, exitReference);

            ilProcessor.InsertBefore(returnInstruction, loadNameInstruction);
            ilProcessor.InsertAfter(loadNameInstruction, callExitReference);
        }
    }
    private static void WeaveJump(MethodDefinition method, MethodReference jumpFromReference, MethodReference jumpBackReference, string name)
    {
        ILProcessor ilProcessor = method.Body.GetILProcessor();

        List<Instruction> callInstructions = method.Body.Instructions.Where(instruction => instruction.OpCode == OpCodes.Call).ToList();
        foreach (var callInstruction in callInstructions)
        {
            Instruction loadNameForFromInstruction = ilProcessor.Create(OpCodes.Ldstr, name);
            Instruction callJumpFromInstruction = ilProcessor.Create(OpCodes.Call, jumpFromReference);

            ilProcessor.InsertBefore(callInstruction, loadNameForFromInstruction);
            ilProcessor.InsertAfter(loadNameForFromInstruction, callJumpFromInstruction);

            Instruction loadNameForBackInstruction = ilProcessor.Create(OpCodes.Ldstr, name);
            Instruction callJumpBackInstruction = ilProcessor.Create(OpCodes.Call, jumpBackReference);

            ilProcessor.InsertAfter(callInstruction, loadNameForBackInstruction);
            ilProcessor.InsertAfter(loadNameForBackInstruction, callJumpBackInstruction);
        }
    }
    private static void WeaveEnter(MethodDefinition method, MethodReference methodReference, string name)
    {
        var ilProcessor = method.Body.GetILProcessor();

        Instruction loadNameInstruction = ilProcessor.Create(OpCodes.Ldstr, name);
        Instruction callEnterInstruction = ilProcessor.Create(OpCodes.Call, methodReference);

        ilProcessor.InsertBefore(method.Body.Instructions.First(), loadNameInstruction);
        ilProcessor.InsertAfter(loadNameInstruction, callEnterInstruction);
    }

    private static void Log(String strLogMessage)
    {
      
            SDX.Core.Logging.LogInfo(": " + strLogMessage);

    }

    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        HookMethods(gameModule, modModule);
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
