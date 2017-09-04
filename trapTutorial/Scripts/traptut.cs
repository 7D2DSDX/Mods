using System;
using UnityEngine;

/// <summary>
/// Custom class for simple unpowered traps
/// Mortelentus 2017 - v1.0
/// We inherit Block class. This is the Basic Vanilla block without any special behaviour.
/// </summary>
public class BlockTrapTutorial : Block
{
    private string WJ;
    private string EJ;

	// overriding the initialization function.
    public override void Init()
    {
        base.Init();
		// assign custom sounds to open and close, IF configured in xml
        if (this.Properties.Values.ContainsKey("OpenSound"))
            this.WJ = this.Properties.Values["OpenSound"];
        if (this.Properties.Values.ContainsKey("CloseSound"))
            this.EJ = this.Properties.Values["CloseSound"];
        this.IsRandomlyTick = true;
    }

    public override bool OnEntityCollidedWithBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos,
        BlockValue _blockValue, Entity _entity)
    {
		// when a entity collides with the block, it will trigger the trap action.
		// for this simple example, our traps are not doing damage, so they will ONLY play an animation
		// we'll add damage in a future guide
        OperateTrap(_world, _clrIdx, _blockPos, _blockValue, _entity, true);        
        return true;
    }

    public override void OnBlockValueChanged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue,
        BlockValue _newBlockValue)
    {
        // the animations need to be triggered here so that they are shown to all players
		// what happens is that, everytime an action occurs, we update the blockvale.
		// that causes this function to be triggered for ALL players, and every client can then play the correct animation locally.
        base.OnBlockValueChanged(_world, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
        if (!(this.shape is BlockShapeModelEntity) || _oldBlockValue.type == _newBlockValue.type && (int)_oldBlockValue.meta == (int)_newBlockValue.meta || _newBlockValue.ischild)
            return;
        // trigger animation
        playAnimation(_world, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
    }

	// custom function to play the animation.
    private void playAnimation(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _blockValue)
    {      
		// we get the block entity data so that we can find the object transform and search for an animator.
        BlockEntityData _ebcd = _world.ChunkClusters[_clrIdx].GetBlockEntity(_blockPos);
        Animator[] componentsInChildren;
        if (_ebcd == null || !_ebcd.bHasTransform ||
            (componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>(false)) == null)
            return;
        // once we find an animator, lets use the correct trigger for the action.
        foreach (Animator animator in componentsInChildren)
        {
            if (BlockTrapTutorial.IsTrapFired(_blockValue.meta) && !BlockTrapTutorial.IsTrapFired(_oldBlockValue.meta))
            {
                // if the trap was triggered while ready, we'll play open animation
				// if you remember the animator, we used the trigger openT to run the open animation.
				// if a open sound exists, it will also be played!
                Audio.Manager.BroadcastPlay(_blockPos.ToVector3(), WJ);
                animator.SetTrigger("openT");
            }
            else if (!BlockTrapTutorial.IsTrapFired(_blockValue.meta) && BlockTrapTutorial.IsTrapFired(_oldBlockValue.meta))
            {
                // if the trap is being reseted by a player, we'll play close animation
				// if you remember the animator, we used the trigger closeT to run the open animation.
				// if a close sound exists, it will also be played!
                Audio.Manager.BroadcastPlay(_blockPos.ToVector3(), EJ);
                animator.SetTrigger("closeT");
            }                      
        }
    }

	// custom function to operate the trap. If it is resulting from a collision "fireTrap" will be true, if its player interaction it will be false.
    private void OperateTrap(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _entity,
        bool fireTrap)
    {
        // here's where we do operation actions. This will only run if the originator is alive!
        if (!_entity.IsAlive()) return;
        if (fireTrap)
        {
			// if the operation results from block collision, we will try to "open" the trap.
			// but only if it's not already opened (waiting for reset)
            if (!BlockTrapTutorial.IsTrapFired(_blockValue.meta))
            {
                #region Fire trap;                                                         
                // I will be using the bit 0, and set it to 1 here. This will let everyone know that the trap is open (waiting for reset)
                _blockValue.meta = (byte) (_blockValue.meta | (1 << 0));
				// as soon as we "commit" this information, all clients will be informed and the correct animations will play
                _world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);                				
                #endregion;
            }
        }
        else
        {
			// if the operation results from player interaction (reseting the trap)
            if (BlockTrapTutorial.IsTrapFired(_blockValue.meta))
            {
                #region Reset trap;                                                
                // I will be using the bit 0, and reset it to 0. This will let everyone know that the trap is ready and will fire if collided.
                _blockValue.meta = (byte) (_blockValue.meta & ~(1 << 0));
				// as soon as we "commit" this information, all clients will be informed and the correct animations will play
                _world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
                #endregion;
            }            
        }
    }

    public override bool OnBlockActivated(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {       
		// player interaction with the trap.
        OperateTrap(_world, _clrIdx, _blockPos, _blockValue, _player, false);
        return true;
    }

    public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos,
        BlockValue _blockValue, EntityAlive _player)
    {
        return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
    }

    public override void ForceAnimationState(BlockValue _blockValue, BlockEntityData _ebcd)
    {        
		// this is very important to keep the animator state in sync.
		// basically, everytime this object is loaded into the player sceane, we will check what is the trap state
		// and force the animator to the correct state.
		// Imagine that a trap was already triggered (bit0 = 1): if we didn't do this, a player entering the sceane would see the trap on its iddle state.
		// by forcing the animator state, he will correctly see the trap waiting for reset.
        Animator[] componentsInChildren;
        if (_ebcd == null || !_ebcd.bHasTransform ||
            (componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>(false)) == null)
            return;
        bool flag = BlockTrapTutorial.IsTrapFired(_blockValue.meta);
        foreach (Animator animator in componentsInChildren)
        {
            if (flag)
                animator.CrossFade("open", 0.0f);
            else
                animator.CrossFade("Idle", 0.0f);
        }        
    }

    /// <summary>
    /// Check if trap is already triggered
    /// </summary>
    /// <param name="_metadata"></param>
    /// <returns>True if triggered</returns>
    public static bool IsTrapFired(byte _metadata)
    {
        return ((int) _metadata & 1 << 0) != 0;
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos,
        EntityAlive _entityFocusing)
    {
		// basically changes the text the player will see while pointing at the trap.
		// if the trap is ready, he wont be able to interact.
        if (!BlockTrapTutorial.IsTrapFired(_blockValue.meta))
        {
            return "";
        }
        else
        {
           return "Press <{0}> to reset trap";
        }
    }
}