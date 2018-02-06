using System;

namespace Commands
{
  public class BCExport : BCCommandAbstract
  {
    public override string[] GetCommands()
    {
      return new[] { "export" };
    }

    public override string GetDescription()
    {
      return "Exports to prefab an area defined by 'loc' command and your location, or via co-ordinates";
    }

    public override void Process()
    {
      var world = GameManager.Instance.World;
      if (world == null) return;

      var localPlayer = world.GetPrimaryPlayer();
      if (localPlayer == null) return;

      var loc = new Vector3i(BCLocation.Loc.x, BCLocation.Loc.y, BCLocation.Loc.z);
      var pos = Vector3FloorToInt(localPlayer.GetPosition());

      string prefabname;

      switch (Params.Count)
      {
        case 1:
          if (loc.y == int.MinValue)
          {
            SendOutput("Store a location with the loc command or use x y z co-ords");

            return;
          }

          prefabname = Params[0];
          break;
        case 7:
          if (!int.TryParse(Params[1], out loc.x) || !int.TryParse(Params[2], out loc.y) || !int.TryParse(Params[3], out loc.z) || !int.TryParse(Params[4], out pos.x) || !int.TryParse(Params[5], out pos.y) || !int.TryParse(Params[6], out pos.z))
          {
            SendOutput("Error: unable to parse coordinates");

            return;
          }
          prefabname = Params[0];
          break;
        default:
          SendOutput("Error: Incorrect command format.");
          SendOutput(GetHelp());

          return;
      }

      var size = new Vector3i(Math.Abs(loc.x - pos.x) + 1, Math.Abs(loc.y - pos.y) + 1, Math.Abs(loc.z - pos.z) + 1);

      var position = new Vector3i(
        loc.x < pos.x ? loc.x : pos.x,
        loc.y < pos.y ? loc.y : pos.y,
        loc.z < pos.z ? loc.z : pos.z
      );

      ExportPrefab(prefabname, position, size, world);
    }

    private static void ExportPrefab(string filename, Vector3i position, Vector3i size, World world)
    {
      var prefab = new Prefab();
      prefab.CopyFromWorld(world, position, position + size);

      prefab.filename = filename;
      prefab.bCopyAirBlocks = true;
      prefab.addAllChildBlocks();

      SendOutput(prefab.Save(prefab.filename)
        ? string.Format("Prefab '{0}' exported @ {1} to {2}, size={3}", prefab.filename, position, position + size, size)
        : string.Format("Error: Prefab '{0}' failed to save.", prefab.filename));
    }
  }
}
