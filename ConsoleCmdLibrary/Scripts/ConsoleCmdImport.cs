using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Commands
{
  public class BCImport : BCCommandAbstract
  {
    public override string[] GetCommands()
    {
      return new[] { "import" };
    }

    public override string GetDescription()
    {
      return "Imports a prefab to your location, or at co-ordinates";
    }

    public override void Process()
    {
      var world = GameManager.Instance.World;
      if (world == null) return;

      var localPlayer = world.GetPrimaryPlayer();
      if (localPlayer == null) return;

      var pos = Vector3FloorToInt(localPlayer.GetPosition());

      string prefabname;
      Vector3i position;
      var rotation = 0;

      switch (Params.Count)
      {
        case 1:
        case 2:
          prefabname = Params[0];
          if (Params.Count > 1)
          {
            if (!int.TryParse(Params[1], out rotation))
            {
              SendOutput("Error: unable to parse rotation");

              return;
            }
          }
          position = pos;

          break;
        case 4:
        case 5:
          var loc = new Vector3i();
          if (!int.TryParse(Params[1], out loc.x) || !int.TryParse(Params[2], out loc.y) || !int.TryParse(Params[3], out loc.z))
          {
            SendOutput("Error: unable to parse coordinates");

            return;
          }
          if (Params.Count > 4)
          {
            if (!int.TryParse(Params[4], out rotation))
            {
              SendOutput("Error: unable to parse rotation");

              return;
            }
          }
          position = loc;
          prefabname = Params[0];

          break;
        default:
          SendOutput("Error: Incorrect command format.");
          SendOutput(GetHelp());

          return;
      }

      ImportPrefab(prefabname, position, rotation, world);
    }

    private static void ImportPrefab(string filename, Vector3i position, int rotation, World world)
    {
      var prefab = new Prefab();
      if (!prefab.Load(filename))
      {
        SendOutput(string.Format("Unable to load prefab {0}", filename));

        return;
      }

      prefab.RotateY(false, rotation % 4);

      if (Options.ContainsKey("air"))
      {
        prefab.bCopyAirBlocks = true;
      }
      if (Options.ContainsKey("noair"))
      {
        prefab.bCopyAirBlocks = false;
      }

      BlockTranslations(prefab, position);

      Log.Out(string.Format("Spawning prefab {0} @ {1}, size={2}", prefab.filename, position, prefab.size));
      SendOutput(string.Format("Spawning prefab {0} @ {1}, size={2}", prefab.filename, position, prefab.size));

      if (Options.ContainsKey("sblock") || Options.ContainsKey("editmode"))
      {
        SendOutput("* with Sleeper Blocks option set");
      }
      else
      {
        SendOutput("* with Sleeper Spawning option set");
      }

      if (Options.ContainsKey("tfill") || Options.ContainsKey("editmode"))
      {
        SendOutput("* with Terrain Filler option set");
      }

      if (Options.ContainsKey("notrans") || Options.ContainsKey("editmode"))
      {
        SendOutput("* with No Placeholder Translations option set");
      }

      CopyIntoLocal(prefab, world, position);
    }

    private static void BlockTranslations(Prefab prefab, Vector3i pos)
    {
      // ENTITIES
      var entities = new List<int>();
      prefab.CopyEntitiesIntoWorld(GameManager.Instance.World, pos, entities, !(Options.ContainsKey("noent") || Options.ContainsKey("editmode")));

      //BLOCK TRANSLATIONS
      if (Options.ContainsKey("notrans") || Options.ContainsKey("editmode")) return;

      var map = LootContainer.lootPlaceholderMap;
      for (var px = 0; px < prefab.size.x; px++)
      {
        for (var py = 0; py < prefab.size.y; py++)
        {
          for (var pz = 0; pz < prefab.size.z; pz++)
          {
            var bv = prefab.GetBlock(px, py, pz);
            // LOOT PLACEHOLDERS
            if (bv.type == 0) continue;

            var bvr = new BlockValue(map.Replace(bv, new Random(Guid.NewGuid().GetHashCode())).rawData);
            if (bv.type == bvr.type) continue;

            prefab.SetBlock(px, py, pz, bvr);
          }
        }
      }
    }

    private static void AddSleeperSpawns(Prefab prefab, int idx, Vector3i dest, SleeperVolume volume, Vector3i volStart, Vector3i volMax)
    {
      var startX = Mathf.Max(volStart.x, 0);
      var startY = Mathf.Max(volStart.y, 0);
      var startZ = Mathf.Max(volStart.z, 0);
      var sizeX = Mathf.Min(prefab.size.x, volMax.x);
      var sizeY = Mathf.Min(prefab.size.y, volMax.y);
      var sizeZ = Mathf.Min(prefab.size.z, volMax.z);
      for (var x = startX; x < sizeX; ++x)
      {
        var destX = x + dest.x;
        for (var z = startZ; z < sizeZ; ++z)
        {
          var destZ = z + dest.z;
          for (var y = startY; y < sizeY; ++y)
          {
            var blockValue = prefab.GetBlock(x, y, z);
            var block = Block.list[blockValue.type];
            if (!block.IsSleeperBlock) continue;

            var flag = false;
            var point = new Vector3i(x, y, z);
            for (var index = 0; index < prefab.SleeperVolumesStart.Count; ++index)
            {
              if (index == idx || !prefab.SleeperVolumeUsed[index] || !prefab.SleeperIsLootVolume[index] ||
                !prefab.CheckSleeperVolumesContainPoint(index, point)) continue;

              flag = true;
              break;
            }
            if (!flag)
              volume.AddSpawnPoint(destX, y + dest.y, destZ, (BlockSleeper)block, blockValue);
          }
        }
      }
    }

    private static void ProcessSleepers(Prefab prefab, World world, Vector3i dest)
    {
      for (var index = 0; index < prefab.SleeperVolumesStart.Count; ++index)
      {
        if (!prefab.SleeperVolumeUsed[index]) continue;

        var volStart = prefab.SleeperVolumesStart[index];
        var volSize = prefab.SleeperVolumesSize[index];
        var volMax = volStart + volSize;
        var volStartDest = volStart + dest;
        var volMaxDest = volMax + dest;

        var chunkXz1 = World.toChunkXZ(volStartDest.x);
        var chunkXz2 = World.toChunkXZ(volMaxDest.x);
        var chunkXz3 = World.toChunkXZ(volStartDest.z);
        var chunkXz4 = World.toChunkXZ(volMaxDest.z);
        var volume = SleeperVolume.Create(prefab.SleeperVolumesGroup[index], volStartDest, volMaxDest, dest, prefab.SleeperVolumeGameStageAdjust[index]);
        var volKey = world.AddSleeperVolume(volume);
        AddSleeperSpawns(prefab, index, dest, volume, volStart, volMax);
        for (var chunkX = chunkXz1; chunkX <= chunkXz2; ++chunkX)
        {
          for (var chunkZ = chunkXz3; chunkZ <= chunkXz4; ++chunkZ)
          {
            var chunkSync = (Chunk)world.GetChunkSync(chunkX, 0, chunkZ);
            if (chunkSync != null)
            {
              chunkSync.GetSleeperVolumes().Add(volKey);
            }
          }
        }
      }
    }

    private static void CopyIntoLocal(Prefab prefab, World world, Vector3i dest)
    {
      if (!Options.ContainsKey("sblocks"))
      {
        ProcessSleepers(prefab, world, dest);
      }

      var chunkSync = world.ChunkCache.GetChunkSync(World.toChunkXZ(dest.x), World.toChunkXZ(dest.z));

      if (!Options.ContainsKey("refresh"))
      {
        for (var x = 0; x < prefab.size.x; ++x)
        {
          for (var z = 0; z < prefab.size.z; ++z)
          {
            var chunkX = World.toChunkXZ(x + dest.x);
            var chunkZ = World.toChunkXZ(z + dest.z);
            var blockX = World.toBlockXZ(x + dest.x);
            var blockZ = World.toBlockXZ(z + dest.z);
            if (chunkSync == null || chunkSync.X != chunkX || chunkSync.Z != chunkZ)
            {
              chunkSync = world.ChunkCache.GetChunkSync(chunkX, chunkZ);
            }

            if (chunkSync == null) continue;

            for (var y = 0; y < prefab.size.y; ++y)
            {
              var blockY = World.toBlockY(y + dest.y);
              var chunkBlock = chunkSync.GetBlock(blockX, blockY, blockZ);

              //REMOVE PARENT OF MULTIDIM
              if (!chunkBlock.Block.isMultiBlock || !chunkBlock.ischild) continue;

              var parentPos = chunkBlock.Block.multiBlockPos.GetParentPos(new Vector3i(dest.x + x, dest.y + y, dest.z + z), chunkBlock);
              var parent = world.ChunkClusters[0].GetBlock(parentPos);
              if (parent.ischild || parent.type != chunkBlock.type) continue;

              world.SetBlockRPC(parentPos, BlockValue.Air);
              //world.ChunkClusters[0].SetBlock(parentPos, BlockValue.Air, false, false);
            }
          }
        }
      }

      for (var x = 0; x < prefab.size.x; ++x)
      {
        for (var z = 0; z < prefab.size.z; ++z)
        {
          var chunkX = World.toChunkXZ(x + dest.x);
          var chunkZ = World.toChunkXZ(z + dest.z);
          var blockX = World.toBlockXZ(x + dest.x);
          var blockZ = World.toBlockXZ(z + dest.z);
          if (chunkSync == null || chunkSync.X != chunkX || chunkSync.Z != chunkZ)
          {
            chunkSync = world.ChunkCache.GetChunkSync(chunkX, chunkZ);
          }

          if (chunkSync == null) continue;

          var terrainHeight = (int)chunkSync.GetTerrainHeight(blockX, blockZ);

          for (var y = 0; y < prefab.size.y; ++y)
          {
            var prefabBlock = prefab.GetBlock(x, y, z);

            //SLEEPER BLOCKS
            if (!(Options.ContainsKey("sblocks") || Options.ContainsKey("editmode")) && Block.list[prefabBlock.type].IsSleeperBlock)
            {
              prefabBlock = BlockValue.Air;
            }

            //COPY AIR
            if (!prefab.bCopyAirBlocks && prefabBlock.type == 0) continue;

            var blockY = World.toBlockY(y + dest.y);
            var chunkBlock = chunkSync.GetBlock(blockX, blockY, blockZ);

            //REMOVE LCB's
            if (chunkBlock.Block.IndexName == "lpblock")
            {
              GameManager.Instance.persistentPlayers.RemoveLandProtectionBlock(new Vector3i(x, y, z));
            }

            //TERRAIN FILLER
            if (!(Options.ContainsKey("tfill") || Options.ContainsKey("editmode")) && Constants.cTerrainFillerBlockValue.type != 0 && prefabBlock.type == Constants.cTerrainFillerBlockValue.type)
            {
              if (chunkBlock.type == 0 || Block.list[chunkBlock.type] == null || !Block.list[chunkBlock.type].shape.IsTerrain()) continue;

              prefabBlock = chunkBlock;
            }

            //DENSITY
            var density = prefab.GetDensity(x, y, z);
            if (density == 0)
            {
              density = !Block.list[prefabBlock.type].shape.IsTerrain() ? MarchingCubes.DensityAir : MarchingCubes.DensityTerrain;
            }
            chunkSync.SetDensity(blockX, blockY, blockZ, density);

            if (chunkBlock.ischild) continue;

            //DECORATIONS
            if (prefab.bAllowTopSoilDecorations)
            {
              chunkSync.SetDecoAllowedAt(blockX, blockZ, EnumDecoAllowed.NoBigOnlySmall);
            }
            else
            {
              if (blockY >= terrainHeight + 1)
              {
                chunkSync.SetDecoAllowedAt(blockX, blockZ, EnumDecoAllowed.NoBigNoSmall);
              }
              else if (blockY == terrainHeight)
              {
                chunkSync.SetDecoAllowedAt(blockX, blockZ, EnumDecoAllowed.Nothing);
              }
            }

            //SECURE DOORS/CHESTS

            //TRADER SPAWNS

            //SET OWNER

            //***** SET BLOCK *****
            chunkSync.SetTextureFull(blockX, blockY, blockZ, prefab.GetTexture(x, y, z));
            //chunkSync.SetBlock(world, blockX, blockY, blockZ, prefabBlock);
            world.SetBlockRPC(new Vector3i(dest.x + x, dest.y + y, dest.z + z), prefabBlock);

            //SET HEIGHT
            if (Block.list[prefabBlock.type].shape.IsTerrain() && chunkSync.GetTerrainHeight(blockX, blockZ) < blockY)
            {
              chunkSync.SetTerrainHeight(blockX, blockZ, (byte)blockY);
            }
          }
          //todo: should this be here? is it needed?
          chunkSync.SetDecoAllowedAt(blockX, blockZ, EnumDecoAllowed.NoBigOnlySmall);
        }
      }
    }
  }
}
