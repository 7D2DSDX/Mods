using System;
using UnityEngine;

// experiemental Irrgation Block
// <property name="Class" value="IrrigationBlock, Mods" />
// <property name="EvaporationTime" value="1" />
// <property name="UpdateRate" value="64" />
public class BlockIrrigationBlock : BlockLiquidv2
{
    protected float EvaporationTime = 1f;
    protected float UpdateRate = 60f;

    public override void Init()
    {
        base.Init();
    }

    public override void LateInit()
    {
        base.LateInit();
  
        // Multiplier used for the evaporation.  The Tick Rate is   Evaporation * 20f * 60f. Should be similar to plant's value.
        if (this.Properties.Values.ContainsKey("EvaporationTime"))
        {
            this.EvaporationTime = global::Utils.ParseFloat(this.Properties.Values["EvaporationTime"]);
        }
        else
        {
            this.EvaporationTime = 1f;
        }
        if (this.Properties.Values.ContainsKey("UpdateRate"))
        {
            this.UpdateRate = global::Utils.ParseFloat(this.Properties.Values["UpdateRate"]);
        }
        else
        {
            this.UpdateRate = 60f;
        }
        if (this.EvaporationTime > 0f)
        {
            this.BlockTag = global::BlockTags.GrowablePlant;
            this.IsRandomlyTick = true;
        }
        else
        {
            this.IsRandomlyTick = false;
        }
    }


    public override ulong GetTickRate()
    {
        return (ulong)(this.EvaporationTime * 20f * 60f);
    }

   // Call the PlantUpdate tick to control upgrades, and the base UpdateTick to allow water flow.
    public override bool UpdateTick(global::WorldBase world, int _clrIdx, global::Vector3i _blockPos, global::BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, System.Random _rnd)
    {
        PlantUpdateTick(world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
        return base.UpdateTick(world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
    }

    public bool PlantUpdateTick(global::WorldBase _world, int _clrIdx, global::Vector3i _blockPos, global::BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, System.Random _rnd)
    {
        // The first few ticks, random is null, so don't do the check if its null.
        if ( _rnd == null )
        {
            return false;
        }
         if (!this.IsRandomlyTick && _bRandomTick)
        {
            _world.GetWBT().AddScheduledBlockUpdate(_clrIdx, _blockPos, this.blockID, this.GetTickRate());
            return true;
        }

        global::ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
        if (chunkCluster == null)
        {
            return true;
        }

        if (this.IsRandomlyTick)
        {
            if ((float)_blockValue.meta3and2 < this.UpdateRate - 1f)
            {

                if (_rnd.Next(2) == 0)
                {
                    _blockValue.meta3and2 += 1;
                    _world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
                }


                return true;
            }
            _blockValue.meta3and2 = 0;
        }

        if (this.IsRandomlyTick || _ticksIfLoaded <= this.GetTickRate() )
        {
            DepleteFromBlock(_blockValue, _blockPos);
        }
      
        return true;
    }

   
}
