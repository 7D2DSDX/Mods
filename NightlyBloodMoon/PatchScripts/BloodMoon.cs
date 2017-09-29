using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;
using SDX.Core;
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
        Log("=== Blood Moon Patcher ===");
       
        // Update the Skymanager, and adjust the two day 7 values up to the new interval.
        // This doesn't control the blood moon itself, but it does control the environment leading up to it. Commend out
        // this section if you still want to leave the suspense in.
        var myClass = module.Types.First(d => d.Name == "SkyManager");
        var myMethod = myClass.Methods.First(d => d.Name == "BloodMoon");
        var instructions = myMethod.Body.Instructions;
        var pro = myMethod.Body.GetILProcessor();
		
		int Counter = 0;
        foreach (var i in instructions.Reverse())
        {
			    // This controls on when the Blood Moon Starts. by default, it starts past the 6th day.
            if (( i.OpCode == OpCodes.Ldc_R4) && ((float)i.Operand == 6f) && Counter == 0)
            {
                i.Operand = 1f;
                Counter++;
            }
            if ((i.OpCode == OpCodes.Ldc_R4) && ((float)i.Operand == OriginalBloodMoon))
            {
                i.Operand = BloodMoonInterval;
            }
        }

        // This controls when the blood moon is triggered. The actual method is obfuscated, so it's difficult to get a handle on
        // rather, we'll go through each method, changing the 7 to match the above interval.
        myClass = module.Types.First(d => d.Name == "AIDirectorBloodMoonComponent");
        foreach( var method in myClass.Methods )
        {
            instructions = method.Body.Instructions;
            pro = method.Body.GetILProcessor();
            foreach (var i in instructions.Reverse())
            {
                if ((i.OpCode == OpCodes.Ldc_I4_7) )
                {
                    i.OpCode = OpCodes.Ldc_I4;
                    i.Operand = (int)BloodMoonInterval;
                }
            }
        }


            return true;
    }

 

    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        return true;
    }

 
    private void Log( String strLogMessage )
    {
        if (this.DebugLog == true)
            SDX.Core.Logging.LogInfo( this.GetType().Name.ToString() + ": " + strLogMessage);

    }
}