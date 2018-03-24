using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SDX.Compiler;

public class StompySpawnPointListPatch : IPatcherMod
{
  public bool Patch(ModuleDefinition module)
  {
    return true;
  }

  public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
  {
    var spawnPointList = gameModule.Types.First(d => d.Name == "SpawnPointList");
    var getRandomSpawnPosition = spawnPointList.Methods.First(d => d.Name == "GetRandomSpawnPosition");
    getRandomSpawnPosition.Body.Instructions.Clear();

    var patchSpawnPointList = modModule.Types.First(d => d.Name == "PatchSpawnPointList");
    var patchGetRandomSpawnPosition = gameModule.Import(patchSpawnPointList.Methods.First(d => d.Name == "PatchGetRandomSpawnPosition"));
    var proc1 = getRandomSpawnPosition.Body.GetILProcessor();

    proc1.Append(Instruction.Create(OpCodes.Ldarg_0));
    proc1.Append(Instruction.Create(OpCodes.Ldarg_1));
    proc1.Append(Instruction.Create(OpCodes.Ldarg_2));
    proc1.Append(Instruction.Create(OpCodes.Ldarg_3));
    proc1.Append(Instruction.Create(OpCodes.Callvirt, patchGetRandomSpawnPosition));
    proc1.Append(Instruction.Create(OpCodes.Ret));

    return true;
  }
}
