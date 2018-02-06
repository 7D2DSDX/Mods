using System;
using System.Linq;

namespace Commands
{
  public class BCLocation : BCCommandAbstract
  {
    public static Vector3i Loc = new Vector3i(0,int.MinValue,0);

    public override string[] GetCommands()
    {
      return new[] { "loc" };
    }

    public override string GetDescription()
    {
      return "Reports your location and stores it for use with other commands";
    }

    public override void Process()
    {
      var world = GameManager.Instance.World;
      if (world == null) return;

      var localPlayer = world.GetPrimaryPlayer();
      if (localPlayer == null) return;
      //var localPlayerX = world.GetLocalPlayers().First();

      switch (Params.Count)
      {
        case 0:
          Loc = Vector3FloorToInt(localPlayer.GetPosition());
          int height = world.GetHeight(Loc.x, Loc.z);
          SendOutput("Location: " + Loc.x + " " + Loc.y + " " + Loc.z);
          SendOutput("ChunkXZ: " + (Loc.x >> 4) + "," + (Loc.z >> 4) + " + " + "ChunkBlockXZ: " + (Loc.x & 15) + "," + (Loc.z & 15));
          SendOutput("RegionXZ: " + (Loc.x >> 9) + "," + (Loc.z >> 9) + " + " + "RegionBlockXZ: " + (Loc.x & 511) + "," + (Loc.z & 511));
          SendOutput("Distance To Ground Height: " + (Loc.y - height - 1));
          break;
        case 3:
          var loc = new Vector3i();
          if (!int.TryParse(Params[0], out loc.x) || !int.TryParse(Params[1], out loc.y) || !int.TryParse(Params[2], out loc.z))
          {
            SendOutput("Unable to parse co-ords");

            return;
          }
          Loc = loc;
          break;
        default:
          SendOutput(GetHelp());
          break;
      }
    }
  }
}
