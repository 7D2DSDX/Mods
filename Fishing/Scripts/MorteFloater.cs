using System;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Custom class for floating blocks
/// Mortelentus 2016 - v1.0
/// </summary>
public class BlockMorteFloater : Block
{
    private bool disableDebug = true;

    /// <summary>
    /// Stores the date and time the tool tip was last displayed
    /// </summary>
    private DateTime dteNextToolTipDisplayTime;

    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Displays text in the chat text area (top left corner)
    /// </summary>
    /// <param name="str">The string to display in the chat text area</param>
    private void DisplayChatAreaText(string str)
    {
        if (!disableDebug)
        {
            str = "FLOATER: " + str;
            bool debug = false;
            if (this.Properties.Values.ContainsKey("debug"))
            {
                if (bool.TryParse(this.Properties.Values["debug"], out debug) == false) debug = false;
            }
            if (debug)
            {
                // Check if the game instance is not null
                if (GameManager.Instance != null)
                {
                    // Display the string in the chat text area
                    //GameManager.Instance.GameMessageClient(EnumGameMessages.Chat, str, "", false, "", false);
                    EntityAlive entity = GameManager.Instance.World.GetLocalPlayer();
                    GameManager.Instance.GameMessage(EnumGameMessages.Chat, str, entity);
                }
            }
        }
    }

    /// <summary>
    /// Displays tooltip text at the bottom of the screen above the tool belt
    /// </summary>
    /// <param name="str">The string to display as a tool tip</param>
    private void DisplayToolTipText(string str)
    {
        // We can only call this code once every 5 seconds because the CanPlaceBlockAt code
        // is a bit spammy (right clicking to place a block once can result in many calls)

        // Check if we are already displaying as tool tip message
        if (DateTime.Now > dteNextToolTipDisplayTime)
        {
            // Display the string as a tool tip message
            EntityPlayerLocal epLocalPlayer = GameManager.Instance.World.GetLocalPlayer();
            if (epLocalPlayer != null)
            {
                // Display the string as a tool tip message
                GameManager.ShowTooltip(epLocalPlayer, str);
            }

            // Set time we can next display a tool tip message (once every 5 seconds)
            dteNextToolTipDisplayTime = DateTime.Now.AddSeconds(5);
        }
    }

    public override void OnBlockStartsToFall(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
    {
        //DisplayChatAreaText("STARTS TO FALL OVERRIDE");
    }

    public override void OnBlockPlaceBefore(WorldBase _world, ref BlockPlacement.Result _bpResult, EntityAlive _ea, Random _rnd)
    {
        DisplayChatAreaText("Gonna put ON water");
        // save the current block type (water or watermoving)
        base.OnBlockPlaceBefore(_world, ref _bpResult, _ea, _rnd);
    }

    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        // put back original water...        
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
        DisplayChatAreaText("Gonna put back water");
        BlockValue offBlock = Block.GetBlockValue("water");
        // spawns it on top of (if anything is there, it will be destroyed anyway                
        GameManager.Instance.World.SetBlockRPC(_chunk.ClrIdx, _blockPos, offBlock);
    }

    public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue,
        bool _bOmitCollideCheck)
    {
        int type = _world.GetBlock(_clrIdx, _blockPos).type;
        DisplayChatAreaText(string.Format("Block name: {0}, Block Liquid_ {1}", Block.list[type].GetBlockName(),
            Block.list[type].blockMaterial.IsLiquid));
        return Block.list[type].blockMaterial.IsLiquid;
    }
}