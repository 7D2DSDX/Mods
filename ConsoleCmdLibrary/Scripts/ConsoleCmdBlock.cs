using System;
using System.Collections.Generic;

namespace Commands
{
  public class BCBlock : BCCommandAbstract
  {
    public override string[] GetCommands()
    {
      return new[] { "block" };
    }

    public override string GetDescription()
    {
      return "Changes blocks in an area defined by 'loc' command and your location, or via co-ordinates";
    }

    public override void Process()
    {
      var world = GameManager.Instance.World;
      if (world == null) return;

      var localPlayer = world.GetPrimaryPlayer();
      if (localPlayer == null) return;

      var loc = new Vector3i(BCLocation.Loc.x, BCLocation.Loc.y, BCLocation.Loc.z);
      var pos = Vector3FloorToInt(localPlayer.GetPosition());

      string blockname;
      string blockname2 = null;

      switch (Params.Count)
      {
        case 2:
        case 3:
          if (loc.y == int.MinValue)
          {
            SendOutput("Store a location with the loc command or use x y z co-ords");

            return;
          }

          blockname = Params[1];
          if (Params.Count == 3)
          {
            blockname2 = Params[2];
          }
          break;
        case 5:
        case 6:
          //single block
          if (!int.TryParse(Params[1], out loc.x) || !int.TryParse(Params[2], out loc.y) || !int.TryParse(Params[3], out loc.z))
          {
            SendOutput("Error: unable to parse coordinates");

            return;
          }
          pos = loc;
          blockname = Params[4];
          if (Params.Count == 6)
          {
            blockname2 = Params[5];
          }
          break;
        case 8:
        case 9:
          //area
          if (!int.TryParse(Params[1], out loc.x) || !int.TryParse(Params[2], out loc.y) || !int.TryParse(Params[3], out loc.z) || !int.TryParse(Params[4], out pos.x) || !int.TryParse(Params[5], out pos.y) || !int.TryParse(Params[6], out pos.z))
          {
            SendOutput("Error: unable to parse coordinates");

            return;
          }
          blockname = Params[7];
          if (Params.Count == 9)
          {
            blockname2 = Params[8];
          }
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

      //**************** GET BLOCKVALUE
      int blockId;
      var bvNew = int.TryParse(blockname, out blockId) ? Block.GetBlockValue(blockId) : Block.GetBlockValue(blockname);

      switch (Params[0])
      {
        case "scan":
          ScanBlocks(position, size, bvNew, blockname);
          break;
        case "fill":
          FillBlocks(position, size, bvNew, blockname);
          break;
        case "swap":
          SwapBlocks(position, size, bvNew, blockname2);
          break;
        case "repair":
          RepairBlocks(position, size);
          break;
        case "damage":
          DamageBlocks(position, size);
          break;
        case "upgrade":
          UpgradeBlocks(position, size);
          break;
        case "downgrade":
          DowngradeBlocks(position, size);
          break;
        case "paint":
          SetPaint(position, size);
          break;
        case "paintface":
          SetPaintFace(position, size);
          break;
        case "paintstrip":
          RemovePaint(position, size);
          break;
        case "density":
          SetDensity(position, size);
          break;
        case "rotate":
          SetRotation(position, size);
          break;
        default:
          SendOutput(GetHelp());
          break;
      }
    }

    private static void SetDensity(Vector3i p3, Vector3i size)
    {
      sbyte density = 1;
      if (Options.ContainsKey("d"))
      {
        if (sbyte.TryParse(Options["d"], out density))
        {
          SendOutput(string.Format("Using density {0}", density));
        }
      }

      const int clrIdx = 0;
      var counter = 0;
      for (var j = 0; j < size.y; j++)
      {
        for (var i = 0; i < size.x; i++)
        {
          for (var k = 0; k < size.z; k++)
          {
            var p5 = new Vector3i(i + p3.x, j + p3.y, k + p3.z);
            var blockValue = GameManager.Instance.World.GetBlock(clrIdx, p5);
            if (blockValue.Equals(BlockValue.Air) || blockValue.ischild) continue;

            GameManager.Instance.World.ChunkClusters[clrIdx].SetBlock(p5, false, blockValue, true, density, false, false, false);
            counter++;
          }
        }
      }

      SendOutput(string.Format("Setting density on {0} blocks '{1}' @ {2} to {3}", counter, density, p3, p3 + size));
    }

    private static void SetRotation(Vector3i p3, Vector3i size)
    {
      byte rotation = 0;
      if (Options.ContainsKey("rot"))
      {
        if (!byte.TryParse(Options["rot"], out rotation))
        {
          SendOutput(string.Format("Unable to parse rotation '{0}'", Options["rot"]));

          return;
        }
      }

      const int clrIdx = 0;
      var counter = 0;
      for (var j = 0; j < size.y; j++)
      {
        for (var i = 0; i < size.x; i++)
        {
          for (var k = 0; k < size.z; k++)
          {
            var p5 = new Vector3i(i + p3.x, j + p3.y, k + p3.z);
            var blockValue = GameManager.Instance.World.GetBlock(clrIdx, p5);
            if (blockValue.Equals(BlockValue.Air) || blockValue.ischild || !blockValue.Block.shape.IsRotatable) continue;

            blockValue.rotation = rotation;
            GameManager.Instance.World.ChunkClusters[clrIdx].SetBlock(p5, blockValue, false, false);
            counter++;
          }
        }
      }

      SendOutput(string.Format("Setting rotation on '{0}' blocks @ {1} to {2}", counter, p3, p3 + size));
    }

    private static void DowngradeBlocks(Vector3i p3, Vector3i size)
    {
      const int clrIdx = 0;
      var counter = 0;
      for (var j = 0; j < size.y; j++)
      {
        for (var i = 0; i < size.x; i++)
        {
          for (var k = 0; k < size.z; k++)
          {
            var p5 = new Vector3i(i + p3.x, j + p3.y, k + p3.z);
            var blockValue = GameManager.Instance.World.GetBlock(clrIdx, p5);
            var downgradeBlockValue = blockValue.Block.DowngradeBlock;
            if (downgradeBlockValue.Equals(BlockValue.Air) || blockValue.ischild) continue;

            downgradeBlockValue.rotation = blockValue.rotation;
            GameManager.Instance.World.ChunkClusters[clrIdx].SetBlock(p5, downgradeBlockValue, false, false);
            counter++;
          }
        }
      }

      SendOutput(string.Format("Downgrading {0} blocks @ {1} to {2}", counter, p3, p3 + size));
    }

    private static void UpgradeBlocks(Vector3i p3, Vector3i size)
    {
      const int clrIdx = 0;
      var counter = 0;
      for (var j = 0; j < size.y; j++)
      {
        for (var i = 0; i < size.x; i++)
        {
          for (var k = 0; k < size.z; k++)
          {
            var p5 = new Vector3i(i + p3.x, j + p3.y, k + p3.z);
            var blockValue = GameManager.Instance.World.GetBlock(clrIdx, p5);
            var upgradeBlockValue = blockValue.Block.UpgradeBlock;
            if (upgradeBlockValue.Equals(BlockValue.Air) || blockValue.ischild) continue;

            upgradeBlockValue.rotation = blockValue.rotation;
            GameManager.Instance.World.ChunkClusters[clrIdx].SetBlock(p5, upgradeBlockValue, false, false);
            counter++;
          }
        }
      }

      SendOutput(string.Format("Upgrading {0} blocks @ {1} to {2}", counter, p3, p3 + size));
    }

    private static void DamageBlocks(Vector3i p3, Vector3i size)
    {
      var damageMin = 0;
      var damageMax = 0;
      if (Options.ContainsKey("d"))
      {
        if (Options["d"].IndexOf(",", StringComparison.InvariantCulture) > -1)
        {
          var dRange = Options["d"].Split(',');
          if (dRange.Length != 2)
          {
            SendOutput("Unable to parse damage values");

            return;
          }

          if (!int.TryParse(dRange[0], out damageMin))
          {
            SendOutput("Unable to parse damage min value");

            return;
          }

          if (!int.TryParse(dRange[1], out damageMax))
          {
            SendOutput("Unable to parse damage max value");

            return;
          }
        }
        else
        {
          if (!int.TryParse(Options["d"], out damageMin))
          {
            SendOutput("Unable to parse damage value");

            return;
          }
        }
      }

      const int clrIdx = 0;
      var counter = 0;
      for (var j = 0; j < size.y; j++)
      {
        for (var i = 0; i < size.x; i++)
        {
          for (var k = 0; k < size.z; k++)
          {
            var p5 = new Vector3i(i + p3.x, j + p3.y, k + p3.z);
            var blockValue = GameManager.Instance.World.GetBlock(clrIdx, p5);
            if (blockValue.Equals(BlockValue.Air)) continue;

            var max = blockValue.Block.blockMaterial.MaxDamage;
            var damage = (damageMax != 0 ? UnityEngine.Random.Range(damageMin, damageMax) : damageMin) + blockValue.damage;
            if (Options.ContainsKey("nobreak"))
            {
              blockValue.damage = Math.Min(damage, max - 1);
            }
            else if (Options.ContainsKey("overkill"))
            {
              //needs to downgrade if overflow damage, then apply remaining damage until all used or downgraded to air
              var d = damage;
              while (d > 0 || blockValue.type != 0)
              {
                var downgrade = blockValue.Block.DowngradeBlock;
                downgrade.rotation = blockValue.rotation;
                blockValue = downgrade;
                blockValue.damage = d;
                d = d - max;
              }
              blockValue.damage = damageMin;
            }
            else
            {
              //needs to downgrade if damage > max, no overflow damage
              if (damage >= max)
              {
                var downgrade = blockValue.Block.DowngradeBlock;
                downgrade.rotation = blockValue.rotation;
                blockValue = downgrade;
              }
              else
              {
                blockValue.damage = damage;
              }
            }

            GameManager.Instance.World.ChunkClusters[clrIdx].SetBlock(p5, blockValue, false, false);
            counter++;
          }
        }
      }

      SendOutput(string.Format("Damaging {0} blocks @ {1} to {2}", counter, p3, p3 + size));
    }

    private static void RepairBlocks(Vector3i p3, Vector3i size)
    {
      const int clrIdx = 0;
      var counter = 0;
      for (var j = 0; j < size.y; j++)
      {
        for (var i = 0; i < size.x; i++)
        {
          for (var k = 0; k < size.z; k++)
          {
            var p5 = new Vector3i(i + p3.x, j + p3.y, k + p3.z);
            var blockValue = GameManager.Instance.World.GetBlock(clrIdx, p5);
            if (blockValue.Equals(BlockValue.Air)) continue;

            blockValue.damage = 0;
            GameManager.Instance.World.ChunkClusters[clrIdx].SetBlock(p5, blockValue, false, false);
            counter++;
          }
        }
      }

      SendOutput(string.Format("Repairing {0} blocks @ {1} to {2}", counter, p3, p3 + size));
    }

    private static void SetPaintFace(Vector3i p3, Vector3i size)
    {
      byte texture = 0;
      if (Options.ContainsKey("t"))
      {
        if (!byte.TryParse(Options["t"], out texture))
        {
          SendOutput("Unable to parse texture value");

          return;
        }
        if (BlockTextureData.list[texture] == null)
        {
          SendOutput(string.Format("Unknown texture index {0}", texture));

          return;
        }
      }
      uint setFace = 0;
      if (Options.ContainsKey("face"))
      {
        if (!uint.TryParse(Options["face"], out setFace))
        {
          SendOutput("Unable to parse face value");

          return;
        }
      }
      if (setFace > 5)
      {
        SendOutput("Face must be between 0 and 5");

        return;
      }

      const int clrIdx = 0;
      var counter = 0;
      for (var j = 0; j < size.y; j++)
      {
        for (var i = 0; i < size.x; i++)
        {
          for (var k = 0; k < size.z; k++)
          {
            var p5 = new Vector3i(i + p3.x, j + p3.y, k + p3.z);
            var blockValue = GameManager.Instance.World.GetBlock(clrIdx, p5);
            if (blockValue.Equals(BlockValue.Air)) continue;

            GameManager.Instance.World.ChunkClusters[clrIdx].SetBlockFaceTexture(p5, (BlockFace)setFace, texture);
            counter++;
          }
        }
      }
      var textureName = "";
      if (BlockTextureData.GetDataByTextureID(texture) != null)
      {
        textureName = BlockTextureData.GetDataByTextureID(texture).Name;
      }

      SendOutput(string.Format("Painting {0} blocks on face '{1}' with texture '{2}' @ {3} to {4}", counter,
        ((BlockFace) setFace).ToString(), textureName, p3, p3 + size));
    }

    private static void SetPaint(Vector3i p3, Vector3i size)
    {
      var texture = 0;
      if (Options.ContainsKey("t"))
      {
        if (!int.TryParse(Options["t"], out texture))
        {
          SendOutput("Unable to parse texture value");

          return;
        }
        if (BlockTextureData.list[texture] == null)
        {
          SendOutput(string.Format("Unknown texture index {0}", texture));

          return;
        }
      }

      var num = 0L;
      for (var face = 0; face < 6; face++)
      {
        var num2 = face * 8;
        num &= ~(255L << num2);
        num |= (long)(texture & 255) << num2;
      }
      var textureFull = num;

      const int clrIdx = 0;
      var counter = 0;
      for (var j = 0; j < size.y; j++)
      {
        for (var i = 0; i < size.x; i++)
        {
          for (var k = 0; k < size.z; k++)
          {
            var p5 = new Vector3i(i + p3.x, j + p3.y, k + p3.z);
            var blockValue = GameManager.Instance.World.GetBlock(clrIdx, p5);
            if (blockValue.Block.shape.IsTerrain() || blockValue.Equals(BlockValue.Air)) continue;

            GameManager.Instance.World.ChunkClusters[clrIdx].SetTextureFull(p5, textureFull);
            counter++;
          }
        }
      }

      var textureName = "";
      if (BlockTextureData.GetDataByTextureID(texture) != null)
      {
        textureName = BlockTextureData.GetDataByTextureID(texture).Name;
      }

      SendOutput(string.Format("Painting {0} blocks with texture '{1}' @ {2} to {3}", counter,
        textureName, p3, p3 + size));
    }

    private static void RemovePaint(Vector3i p3, Vector3i size)
    {
      const int clrIdx = 0;
      var counter = 0;
      for (var j = 0; j < size.y; j++)
      {
        for (var i = 0; i < size.x; i++)
        {
          for (var k = 0; k < size.z; k++)
          {
            var p5 = new Vector3i(i + p3.x, j + p3.y, k + p3.z);
            var blockValue = GameManager.Instance.World.GetBlock(clrIdx, p5);
            if (blockValue.Block.shape.IsTerrain() || blockValue.Equals(BlockValue.Air)) continue;

            GameManager.Instance.World.ChunkClusters[clrIdx].SetTextureFull(p5, 0L);
            counter++;
          }
        }
      }

      SendOutput(string.Format("Paint removed from {0} blocks @ {1} to {2}", counter, p3, p3 + size));
    }

    private static void SwapBlocks(Vector3i p3, Vector3i size, BlockValue newbv, string blockname)
    {
      int blockId;
      var targetbv = int.TryParse(blockname, out blockId) ? Block.GetBlockValue(blockId) : Block.GetBlockValue(blockname);

      var block1 = Block.list[targetbv.type];
      if (block1 == null)
      {
        SendOutput("Unable to find target block by id or name");

        return;
      }

      var block2 = Block.list[newbv.type];
      if (block2 == null)
      {
        SendOutput("Unable to find replacement block by id or name");

        return;
      }

      const int clrIdx = 0;
      var counter = 0;
      var world = GameManager.Instance.World;
      for (var i = 0; i < size.x; i++)
      {
        for (var j = 0; j < size.y; j++)
        {
          for (var k = 0; k < size.z; k++)
          {
            sbyte density = 1;
            if (Options.ContainsKey("d"))
            {
              if (sbyte.TryParse(Options["d"], out density))
              {
                SendOutput(string.Format("Using density {0}", density));
              }
            }
            else
            {
              if (newbv.Equals(BlockValue.Air))
              {
                density = MarchingCubes.DensityAir;
              }
              else if (newbv.Block.shape.IsTerrain())
              {
                density = MarchingCubes.DensityTerrain;
              }
            }

            var textureFull = 0L;
            if (Options.ContainsKey("t"))
            {
              byte texture;
              if (!byte.TryParse(Options["t"], out texture))
              {
                SendOutput("Unable to parse texture index");

                return;
              }

              if (BlockTextureData.list[texture] == null)
              {
                SendOutput(string.Format("Unknown texture index {0}", texture));

                return;
              }

              var num = 0L;
              for (var face = 0; face < 6; face++)
              {
                var num2 = face * 8;
                num &= ~(255L << num2);
                num |= (long)(texture & 255) << num2;
              }
              textureFull = num;
            }

            var p5 = new Vector3i(i + p3.x, j + p3.y, k + p3.z);
            if (world.GetBlock(p5).Block.GetBlockName() != block1.GetBlockName()) continue;

            //todo
            world.ChunkClusters[clrIdx].SetBlock(p5, true, newbv, false, density, false, false, false);
            world.ChunkClusters[clrIdx].SetTextureFull(p5, textureFull);
            counter++;
          }
        }
      }

      SendOutput(string.Format("Replaced {0} '{1}' blocks with '{2}' @ {3} to {4}", counter, block1.GetBlockName(),
        block2.GetBlockName(), p3, p3 + size));
    }

    private static void FillBlocks(Vector3i p3, Vector3i size, BlockValue bv, string search)
    {
      const int clrIdx = 0;

      if (Block.list[bv.type] == null)
      {
        SendOutput("Unable to find block by id or name");

        return;
      }

      SetBlocks(clrIdx, p3, size, bv, search == "*");

      if (Options.ContainsKey("delmulti"))
      {
        SendOutput(string.Format("Removed multidim blocks @ {0} to {1}", p3, p3 + size));
      }
      else
      {
        SendOutput(string.Format("Inserting block '{0}' @ {1} to {2}", Block.list[bv.type].GetBlockName(), p3,
          p3 + size));
      }
    }

    private static void SetBlocks(int clrIdx, Vector3i p0, Vector3i size, BlockValue bvNew, bool searchAll)
    {
      var world = GameManager.Instance.World;
      var chunkCluster = world.ChunkClusters[clrIdx];

      sbyte density = 1;
      if (Options.ContainsKey("d"))
      {
        if (sbyte.TryParse(Options["d"], out density))
        {
          SendOutput(string.Format("Using density {0}", density));
        }
      }

      var textureFull = 0L;
      if (Options.ContainsKey("t"))
      {
        byte texture;
        if (!byte.TryParse(Options["t"], out texture))
        {
          SendOutput("Unable to parse texture index");

          return;
        }

        if (BlockTextureData.list[texture] == null)
        {
          SendOutput(string.Format("Unknown texture index {0}", texture));

          return;
        }

        var num = 0L;
        for (var face = 0; face < 6; face++)
        {
          var num2 = face * 8;
          num &= ~(255L << num2);
          num |= (long)(texture & 255) << num2;
        }
        textureFull = num;
      }

      for (var j = 0; j < size.y; j++)
      {
        for (var i = 0; i < size.x; i++)
        {
          for (var k = 0; k < size.z; k++)
          {
            var p5 = new Vector3i(p0.x + i, p0.y + j, p0.z + k);
            var bvCurrent = world.GetBlock(p5);

            if (Options.ContainsKey("delmulti") && (!searchAll || bvNew.type != bvCurrent.type)) continue;

            //REMOVE PARENT OF MULTIDIM
            if (bvCurrent.Block.isMultiBlock && bvCurrent.ischild)
            {
              var parentPos = bvCurrent.Block.multiBlockPos.GetParentPos(p5, bvCurrent);
              var parent = chunkCluster.GetBlock(parentPos);
              if (parent.ischild || parent.type != bvCurrent.type) continue;

              chunkCluster.SetBlock(parentPos, BlockValue.Air, false, false);
            }
            if (Options.ContainsKey("delmulti")) continue;

            //REMOVE LCB's
            if (bvCurrent.Block.IndexName == "lpblock")
            {
              GameManager.Instance.persistentPlayers.RemoveLandProtectionBlock(new Vector3i(p5.x, p5.y, p5.z));
            }

            var chunkSync = world.GetChunkFromWorldPos(p5.x, p5.y, p5.z) as Chunk;

            if (chunkSync != null)
            {
              if (bvNew.Equals(BlockValue.Air))
              {
                density = MarchingCubes.DensityAir;

                if (world.GetTerrainHeight(p5.x, p5.z) > p5.y)
                {
                  chunkSync.SetTerrainHeight(p5.x & 15, p5.z & 15, (byte)p5.y);
                }
              }
              else if (bvNew.Block.shape.IsTerrain())
              {
                density = MarchingCubes.DensityTerrain;

                if (world.GetTerrainHeight(p5.x, p5.z) < p5.y)
                {
                  chunkSync.SetTerrainHeight(p5.x & 15, p5.z & 15, (byte)p5.y);
                }
              }
              else
              {
                //SET TEXTURE
                world.ChunkClusters[clrIdx].SetTextureFull(p5, textureFull);
              }
            }

            //SET BLOCK
            world.ChunkClusters[clrIdx].SetBlock(p5, true, bvNew, true, density, false, false, false);
          }
        }
      }
    }

    private static void ScanBlocks(Vector3i p3, Vector3i size, BlockValue bv, string search)
    {
      var block1 = Block.list[bv.type];
      if (block1 == null && search != "*")
      {
        SendOutput("Unable to find block by id or name");

        return;
      }

      var stats = new SortedDictionary<string, int>();
      const int clrIdx = 0;
      for (var j = 0; j < size.y; j++)
      {
        for (var i = 0; i < size.x; i++)
        {
          for (var k = 0; k < size.z; k++)
          {
            var p5 = new Vector3i(i + p3.x, j + p3.y, k + p3.z);
            var blockValue = GameManager.Instance.World.GetBlock(clrIdx, p5);
            //var d = GameManager.Instance.World.GetDensity(_clrIdx, p5);
            //var t = GameManager.Instance.World.GetTexture(i + p3.x, j + p3.y, k + p3.z);
            var name = "";
            if (ItemClass.list[blockValue.type] != null)
            {
              name = ItemClass.list[blockValue.type].Name;
            }
            if (string.IsNullOrEmpty(name))
            {
              name = "air";
            }

            if (search == "*")
            {
              SetStats(name, blockValue, stats);
            }
            else
            {
              if (name != bv.Block.GetBlockName()) continue;

              SetStats(name, blockValue, stats);
            }
          }
        }
      }

      //todo
      SendOutput(stats.ToString());
    }

    private static void SetStats(string name, BlockValue bv, IDictionary<string, int> stats)
    {
      if (stats.ContainsKey(string.Format("{0:D4}:{1}", bv.type, name)))
      {
        stats[string.Format("{0:D4}:{1}", bv.type, name)] += 1;
      }
      else
      {
        stats.Add(string.Format("{0:D4}:{1}", bv.type, name), 1);
      }
    }
  }
}
