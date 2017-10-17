using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using SDX.Core;
using SDX.Payload;
using System.Reflection;
public class NightlyBloodMoonChange : IPatcherMod
{
  
    // Debug Logging
    private bool DebugLog = true;
    private float BloodMoonInterval = 1;
    private float OriginalBloodMoon = 7;

    /* 
     * This mod will adjust the Blood Moon from every 7th day to every 21st day
     */
    public bool Patch(ModuleDefinition module)
    {
       
        return true;

    }
    private void HookMethods(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        Log("=== Blood Moon Patcher ===");

        // Update the Skymanager, and adjust the two day 7 values up to the new interval.
        // This doesn't control the blood moon itself, but it does control the environment leading up to it. Commend out
        // this section if you still want to leave the suspense in.
        var myClass = gameModule.Types.First(d => d.Name == "SkyManager");
        var myMethod = myClass.Methods.First(d => d.Name == "BloodMoon");



        var instructions = myMethod.Body.Instructions;
        var pro = myMethod.Body.GetILProcessor();

        var TimeOfDay = myClass.Methods.First(d => d.Name == "TimeOfDay");

        instructions.Clear();
        var MyBloodMoon = modModule.Types.First(d => d.Name == "MyBloodMoon");
        var mySkyBloodMoon = gameModule.Import(MyBloodMoon.Methods.First(d => d.Name == "NightlyBloodMoon"));
     //   instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
     //   instructions.Add(Instruction.Create(OpCodes.Ldarg_1));

        instructions.Add(Instruction.Create(OpCodes.Call, mySkyBloodMoon));
        instructions.Add(Instruction.Create(OpCodes.Ret));


        //int Counter = 0;
        //      foreach (var i in instructions)
        //      {
        //	    // This controls on when the Blood Moon Starts. by default, it starts past the 6th day.
        //          if (( i.OpCode == OpCodes.Ldc_R4) && ((float)i.Operand == 6f) && Counter == 0)
        //          {
        //              i.Operand = 0f;
        //              Counter++;
        //          }

        //	 if (( i.OpCode == OpCodes.Ldc_R4) && ((float)i.Operand == 0.6f))
        //          {
        //              i.Operand = 0f;

        //          }
        //          if ((i.OpCode == OpCodes.Ldc_R4) && ((float)i.Operand == OriginalBloodMoon))
        //          {
        //              i.Operand = BloodMoonInterval;
        //          }
        //      }

        // This controls when the blood moon is triggered. The actual method is obfuscated, so it's difficult to get a handle on
        // rather, we'll go through each method, changing the 7 to match the above interval.
        myClass = gameModule.Types.First(d => d.Name == "AIDirectorBloodMoonComponent");
        foreach (var method in myClass.Methods)
        {
            instructions = method.Body.Instructions;
            pro = method.Body.GetILProcessor();

            // If we change a 7 to the blood moon interval, there's another instruction afterwards that we want to switch from 1 to 0
            bool blSearch = false;
            foreach (var i in instructions.Reverse())
            {
                if ((i.OpCode == OpCodes.Ldc_I4_7))
                {
                    i.OpCode = OpCodes.Ldc_I4;
                    i.Operand = (int)BloodMoonInterval;
                    blSearch = true;
                }

                // This dtermines if the number of days is above 1. Since we want the blood moon to start the first day, we need to change the greater than to greater than equal
                if ((i.OpCode == OpCodes.Ble_S) && (blSearch = true))
                {
                    i.OpCode = OpCodes.Blt_S;
                    blSearch = false;
                }
            }
        }

    
          //  return true;
    }

 

    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        HookMethods(gameModule, modModule);
        return true;
    }

 
    private void Log( String strLogMessage )
    {
        if (this.DebugLog == true)
            SDX.Core.Logging.LogInfo( this.GetType().Name.ToString() + ": " + strLogMessage);

    }
}