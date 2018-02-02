using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SDX.Compiler;

public class ConsoleCmdInjection : IPatcherMod
{
  public bool Patch(ModuleDefinition module)
  {
    return true;
  }

  public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
  {
    var sdtdConsole = gameModule.Types.First(d => d.Name == "SdtdConsole");
    var registerCommands = sdtdConsole.Methods.First(d => d.Name == "RegisterCommands");
    var instructions = registerCommands.Body.Instructions;
    var proc1 = registerCommands.Body.GetILProcessor();

    var consoleCmdLoader = modModule.Types.First(d => d.Name == "ConsoleCmdLoader");
    var addCommands = gameModule.Import(consoleCmdLoader.Methods.First(d => d.Name == "AddCommands"));

    foreach (var i in instructions)
    {
      if (i.OpCode == OpCodes.Ldloc_1)
      {
        proc1.InsertBefore(i, Instruction.Create(OpCodes.Ldloc_1));
        proc1.InsertBefore(i, Instruction.Create(OpCodes.Callvirt, addCommands));

        break;
      }
    }

    return true;
  }
}
