using System;
using System.Collections.Generic;
using System.Reflection;

public class ConsoleCmdLoader
{
  public static void AddCommands(List<Assembly> loadedAssemblies)
  {
    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
      if (!assembly.GetName().Name.Equals("Mods")) continue;

      loadedAssemblies.Add(assembly);

      break;
    }
  }
}
