using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using SDX.Core;
using SDX.Compiler;
using SDX.Payload;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

public class HalDllFixesPatcher : IPatcherMod
{
    public bool Patch(ModuleDefinition module)

    {
        Console.WriteLine(" == Hal's DLL Updates Running == ");

        CustomEntityClasses(module);

        KillProcessByName("7DaysToDie");
        KillProcessByName("7DaysToDieServer");
        return true;

    }

    public static bool KillProcessByName(string sName)
    {


        try
        {
            Process[] pros = Process.GetProcessesByName(sName);

            Process p = null;


            for (int x = 0; x <= pros.Length - 1; x++)
            {
                p = pros[x];

                p.Kill();

                if (!p.WaitForExit(10000))
                {
                    return false;
                }

            }

            return true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log("Could not kill process: " + ex.ToString());

        }
        return false;

    }

    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        return true;
    }
    private void CustomEntityClasses(ModuleDefinition module)
    {
        var method = module.GetType("EntityFactory").Methods.First(d => d.IsStatic && d.Parameters.Count == 2
                                                                                   && d.Parameters[0].ParameterType.FullName == "UnityEngine.GameObject"
                                                                                   && d.Parameters[1].ParameterType.FullName == "System.String"
                                                                                   && d.ReturnType.Name == "Entity");
 
        MethodReference addComponent = null;
        foreach (var t in module.Types)
        {
            foreach (var meth in t.Methods)
            {
                if (meth.Body == null) continue;

                foreach (var i in meth.Body.Instructions)
                {
                    if (i.Operand == null) continue;

                    if (i.Operand.GetType() == typeof(MethodReference) && ((MethodReference)i.Operand).FullName == "UnityEngine.Component UnityEngine.GameObject::AddComponent(System.Type)")
                    {
                        addComponent = (MethodReference)i.Operand;
                        goto processMethod;
                    }
                }
            }
        }

        processMethod:
        var first = method.Body.Instructions[0];
        var il = method.Body.GetILProcessor();

        TypeReference tEntity = module.Types.First(d => d.Name == "Entity");
        TypeReference tString = null;
        if (!module.TryGetTypeReference("System.String", out tString))
        {
            //return false;
        }
        TypeReference tType = null;
        if (!module.TryGetTypeReference("System.Type", out tType))
        {
            //return false;
        }
        TypeReference tInt = null;
        if (!module.TryGetTypeReference("System.Int32", out tInt))
        {
            //return false;
        }
        TypeReference tBool = null;
        if (!module.TryGetTypeReference("System.Boolean", out tBool))
        {
            //return false;
        }
        TypeReference tUnity = null;
        if (!module.TryGetTypeReference("UnityEngine.Object", out tUnity))
        {
            //return false;
        }

        var concat = module.Import(module.GetTypeReferences().First(t => t.FullName == "System.String").Resolve().Methods.First(d => d.FullName == "System.String System.String::Concat(System.String,System.String)"));
        var getType = module.Import(module.GetTypeReferences().First(t => t.FullName == "System.Type").Resolve().Methods.First(d => d.FullName == "System.Type System.Type::GetType(System.String)"));
        var objectEquals = module.Import(module.GetTypeReferences().First(t => t.FullName == "System.Object").Resolve().Methods.First(d => d.FullName == "System.Boolean System.Object::Equals(System.Object,System.Object)"));

     
        var varEntity = new VariableDefinition("classCheck", tString);
        method.Body.Variables.Add(varEntity);
        varEntity = new VariableDefinition("t", tType);
        method.Body.Variables.Add(varEntity);
        varEntity = new VariableDefinition("t", tBool);
        method.Body.Variables.Add(varEntity);
        varEntity = new VariableDefinition("tUnity", tUnity);
        method.Body.Variables.Add(varEntity);
        varEntity = new VariableDefinition("entity", tEntity);
        method.Body.Variables.Add(varEntity);


        var loadRet = Instruction.Create(OpCodes.Ldloc_S, method.Body.Variables[5]);

        il.InsertBefore(first, il.Create(OpCodes.Nop));
        il.InsertBefore(first, il.Create(OpCodes.Ldarg_1));
        il.InsertBefore(first, il.Create(OpCodes.Ldstr, ", Mods"));
        il.InsertBefore(first, il.Create(OpCodes.Call, concat));
        il.InsertBefore(first, il.Create(OpCodes.Stloc_1));
        il.InsertBefore(first, il.Create(OpCodes.Ldloc_1));
        il.InsertBefore(first, il.Create(OpCodes.Call, getType));
        il.InsertBefore(first, il.Create(OpCodes.Stloc_S, method.Body.Variables[2]));
        il.InsertBefore(first, il.Create(OpCodes.Ldloc_S, method.Body.Variables[2]));
        il.InsertBefore(first, il.Create(OpCodes.Ldnull));
        il.InsertBefore(first, il.Create(OpCodes.Call, objectEquals));
        il.InsertBefore(first, il.Create(OpCodes.Ldc_I4_0));
        il.InsertBefore(first, il.Create(OpCodes.Ceq));
        il.InsertBefore(first, il.Create(OpCodes.Stloc_S, method.Body.Variables[3]));
        il.InsertBefore(first, il.Create(OpCodes.Ldloc_S, method.Body.Variables[3]));
        il.InsertBefore(first, il.Create(OpCodes.Brfalse_S, first));
        il.InsertBefore(first, il.Create(OpCodes.Nop));
        il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(first, il.Create(OpCodes.Ldloc_S, method.Body.Variables[2]));
        il.InsertBefore(first, il.Create(OpCodes.Callvirt, addComponent));
        il.InsertBefore(first, il.Create(OpCodes.Stloc_S, method.Body.Variables[4]));
        il.InsertBefore(first, il.Create(OpCodes.Ldloc_S, method.Body.Variables[4]));
        il.InsertBefore(first, il.Create(OpCodes.Castclass, method.ReturnType));
        il.InsertBefore(first, il.Create(OpCodes.Stloc_S, method.Body.Variables[5]));
        il.InsertBefore(first, il.Create(OpCodes.Br_S, loadRet));
        il.InsertBefore(first, il.Create(OpCodes.Ldnull));
        il.InsertBefore(first, il.Create(OpCodes.Stloc_S, method.Body.Variables[5]));
        il.InsertBefore(first, il.Create(OpCodes.Br_S, loadRet));
        il.InsertBefore(first, loadRet);
        il.InsertBefore(first, il.Create(OpCodes.Ret));

    }
    
    /*private void Hook_TileEntity_Instantiate(ModuleDefinition module)
	{
		PatchHelpers.OnElement("TileEntity::Instantiate", module.GetType("TileEntity").Methods,
			method => (method.IsStatic && method.Name == "Instantiate"),
			method =>
			{
				var engineHook = module.Import(typeof(SDX.Payload.ModEngine).GetMethod("TileEntityInstantiate"));
				Instruction first = method.Body.Instructions[0];

				var il = method.Body.GetILProcessor();

				il.Body.Variables.Add(new VariableDefinition("TempEntity", module.GetType("TileEntity").Resolve()));

				//TempEntity = Runner::TileEntityInstantiate(tileType, (object)chunk)
				il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
				il.InsertBefore(first, il.Create(OpCodes.Ldarg_1));
				il.InsertBefore(first, il.Create(OpCodes.Castclass, module.Import(typeof(System.Object))));
				il.InsertBefore(first, il.Create(OpCodes.Call, engineHook));
				il.InsertBefore(first, il.Create(OpCodes.Castclass, module.GetType("TileEntity").Resolve()));
				il.InsertBefore(first, il.Create(OpCodes.Stloc_2));

				// If TempEntity == null
				il.InsertBefore(first, il.Create(OpCodes.Ldloc_2));
				il.InsertBefore(first, il.Create(OpCodes.Ldnull));
				il.InsertBefore(first, il.Create(OpCodes.Ceq));
				il.InsertBefore(first, il.Create(OpCodes.Brtrue_S, first));

				//     return TempEntity
				il.InsertBefore(first, il.Create(OpCodes.Ldloc_2));
				il.InsertBefore(first, il.Create(OpCodes.Stloc_0));

				var retInst = method.Body.Instructions[method.Body.Instructions.Count - 2];
				il.InsertBefore(first, il.Create(OpCodes.Br, retInst));

				return true;
			});
	}
	
	private void Hook_EModelBase_createModel(ModuleDefinition module)
	{
		PatchHelpers.OnElement("EModelBase.createModel", module.GetType("EModelBase").Methods,
			method => (method.Name == "createModel" && method.IsVirtual),
			method =>
			{
				var OnEModelBaseCreateModelMethod = module.Import(typeof(SDX.Payload.ModEngine).GetMethod("OnEModelBase_CreateModel"));

				var il = method.Body.GetILProcessor();

				Instruction lastInst = method.Body.Instructions[method.Body.Instructions.Count - 1];
				il.InsertBefore(lastInst, il.Create(OpCodes.Ldarg_0));
				il.InsertBefore(lastInst, il.Create(OpCodes.Castclass, module.Import(typeof(System.Object))));
				il.InsertBefore(lastInst, il.Create(OpCodes.Ldloc_0));
				il.InsertBefore(lastInst, il.Create(OpCodes.Ldarg_2));
				il.InsertBefore(lastInst, il.Create(OpCodes.Castclass, module.Import(typeof(System.Object))));
				il.InsertBefore(lastInst, il.Create(OpCodes.Call, OnEModelBaseCreateModelMethod));

				return true;
			});
	}*/
}