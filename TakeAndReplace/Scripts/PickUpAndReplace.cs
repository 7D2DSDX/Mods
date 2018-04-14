using System;
using UnityEngine;

public class BlockTakeAndReplace : Block
{
    // By default, all blocks using this class will have a take delay of 15 seconds, unless over-ridden by the XML.
    private float fTakeDelay = 15f;

    public override void Init()
    {
        base.Init();

        if (this.Properties.Values.ContainsKey("TakeDelay"))
        {
            this.fTakeDelay = Utils.ParseFloat(this.Properties.Values["TakeDelay"]);
        }
    }

    // Override the on Block activated, so we can pop up our timer
    public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx,
        Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        this.TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
        return true;
    }

    // Take logic to replace it with the Downgrade block, matching rotations.
    private void TakeTarget(object obj)
    {
        World world = GameManager.Instance.World;
        object[] array = (object[]) obj;
        int clrIdx = (int) array[0];
        BlockValue _blockValue = (BlockValue) array[1];
        Vector3i vector3i = (Vector3i) array[2];
        BlockValue block = world.GetBlock(vector3i);
        EntityPlayerLocal entityPlayerLocal = array[3] as EntityPlayerLocal;

        // Find the block value for the pick up value, and add it to the inventory
        BlockValue pickUpBlock = Block.GetBlockValue(this.PickedUpItemValue);
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
        ItemStack itemStack = new ItemStack(pickUpBlock.ToItemValue(), 1);
        if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack, true))
        {
            uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
        }

        entityPlayerLocal.PlayOneShot("Sounds/DestroyBlock/wooddestroy1");
        // Replace it with the downgrade block
        BlockValue blockValue2 = this.DowngradeBlock;
        blockValue2.rotation = _blockValue.rotation;
        blockValue2.meta = _blockValue.meta;
        world.SetBlockRPC(clrIdx, vector3i, blockValue2);
    }


    // Displays the UI for the timer, calling TakeTarget when its done.
    public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
        playerUI.windowManager.Open("timer", true, false, true);
        XUiC_Timer xuiC_Timer = (XUiC_Timer) playerUI.xui.GetChildByType<XUiC_Timer>();
        TimerEventData timerEventData = new TimerEventData();
        timerEventData.Data = new object[]
        {
            _cIdx,
            _blockValue,
            _blockPos,
            _player
        };
        timerEventData.Event += this.TakeTarget;

        float newTakeTime = this.fTakeDelay;

        // If the entity is holding a crow bar or hammer, then reduce the take time.
        if (_player.inventory.holdingItem.Name == "CrowBar" || _player.inventory.holdingItem.Name == "clawHammer")
        {
            // Make sure the item can still be used
            if (_player.inventory.holdingItemItemValue.MaxUseTimes > 0)
            {
                // Bump the Use time by one.
                global::ItemValue itemValue = _player.inventory.holdingItemItemValue;
                itemValue.UseTimes += global::AttributeBase.GetVal<global::AttributeDegradationRate>(itemValue, 1);
                _player.inventory.holdingItemData.itemValue = itemValue;

                // Automatically reduce the take delay by half if you have a crow bar or claw hammer.
                newTakeTime = (this.fTakeDelay / 2);

                Skill tmpSkill = (_player as EntityPlayerLocal).Skills.GetSkillByName("Breaking And Entering");

                // Don't divde by 0, but take it as a full timer, since it's level 0.
                if (tmpSkill.Level == 0)
                    newTakeTime = newTakeTime / 1f;
                else
                    newTakeTime = newTakeTime / tmpSkill.Level;
            }
        }

        xuiC_Timer.SetTimer(newTakeTime, timerEventData);
    }

    public override string GetActivationText(global::WorldBase _world, global::BlockValue _blockValue, int _clrIdx, global::Vector3i _blockPos, global::EntityAlive _entityFocusing)
    {
        return "Press <E> to remove the wood from this block.";
    }
}
