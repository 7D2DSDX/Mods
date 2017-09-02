using System;
using UnityEngine;

public class BlockNeonSign : BlockPowered
{
	static bool showDebugLog = true;
	
	private static bool IsSpRemotePowerAllowed(Vector3i _blockPos)
	{
		BlockValue blockValue = GameManager.Instance.World.GetBlock(_blockPos);
		Block block = Block.list[blockValue.type];
		bool isSpRemotePowerAllowed = false;
		if (block.Properties.Values.ContainsKey("AllowRemotePower"))
        {
			bool.TryParse(block.Properties.Values["AllowRemotePower"], out isSpRemotePowerAllowed);
        }
		return isSpRemotePowerAllowed;
	}
	
	public string LitObject()
	{
		string litObjectName = @"DefaultLitObjectName";
		if (this.Properties.Values.ContainsKey("LightObject"))
		{
			litObjectName = this.Properties.Values["LightObject"].Replace(@"DefaultLitObjectName", @"DefaultLitObjectName");
		}
		else
		litObjectName = "LitObjects";
		return litObjectName;
	}
	
	public static void DebugMsg(string msg)
	{
		if(showDebugLog)
		{
			Debug.Log(msg);
		}
	}
	
	public override bool OnBlockActivated(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		if(!flag)
		{
			return false;
		}
		this.TakeItemWithTimer(_clrIdx, _blockPos, _blockValue, _player);
		return true;
	}
	
	public override void OnBlockEntityTransformBeforeActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		this.shape.OnBlockEntityTransformBeforeActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		DebugMsg("OnBlockEntityTransformBeforeActivated");

        try
        {
            if (_ebcd != null && _ebcd.bHasTransform)
            {
                GameObject gameObject = _ebcd.transform.gameObject;
                if (LitObject() == null)
                    DebugMsg("LitObject is null!");

           
                GameObject litSignObject = _ebcd.transform.Find(LitObject()).gameObject;
                if (litSignObject == null)
                {
                    DebugMsg("litSignObject is null");
                }
                else
                    DebugMsg("Found litSignObject");
                NeonSignControl neonSignScript = gameObject.GetComponent<NeonSignControl>();
                if (neonSignScript == null)
                {
                    neonSignScript = gameObject.AddComponent<NeonSignControl>();
                }
                neonSignScript.enabled = true;
                neonSignScript.cIdx = _cIdx;
                neonSignScript.blockPos = _blockPos;
                neonSignScript.litSignObject = litSignObject;
                neonSignScript.litSignObject.active = false;
            }
            else
                DebugMsg("ERROR: _ebcd null (OnBlockEntityTransformBeforeActivated)");
        }
        catch (Exception ex)
        {
            DebugMsg("Error Message: " + ex.ToString());
        }
	}
	
	public static bool isBlockPoweredUp(Vector3i _blockPos, int _clrIdx)
	{
		WorldBase world = GameManager.Instance.World;
		if(world.IsRemote())
		{
			//Use HasActivePower power instead since directly powering blocks doesnt work on servers.
			return BlockNeonSign.HasActivePower(world, _clrIdx, _blockPos);
		}
		TileEntityPowered tileEntityPowered = (TileEntityPowered)GameManager.Instance.World.GetTileEntity(_clrIdx, _blockPos);
		if (tileEntityPowered != null)
		{
			if(tileEntityPowered.IsPowered)
			{
				DebugMsg("Block Power Is On");
				return true;
			}
		}
		if(BlockNeonSign.IsSpRemotePowerAllowed(_blockPos))
		{
			DebugMsg("No direct power found, checking for remote power");
			return BlockNeonSign.HasActivePower(world, _clrIdx, _blockPos);
		}
		DebugMsg("Block Power Is Off");
		return false;
	}
	
	static Vector3i[] PowerInputLocations(Vector3i _blockPos)
	{
		int inputSpace = 2;
		Vector3i inputPosA = _blockPos;
		Vector3i inputPosB = _blockPos;
		Vector3i inputPosC = _blockPos;
		Vector3i inputPosD = _blockPos;
		Vector3i inputPosE = _blockPos;
		Vector3i inputPosF = _blockPos;
		
		inputPosA.y = _blockPos.y+inputSpace;
		inputPosB.y = _blockPos.y-inputSpace;
		inputPosC.x = _blockPos.x+inputSpace;
		inputPosD.x = _blockPos.x-inputSpace;
		inputPosE.z = _blockPos.z+inputSpace;
		inputPosF.z = _blockPos.z-inputSpace;
		
		Vector3i[] array = new Vector3i[6];
		array[0] = inputPosA;
		array[1] = inputPosB;
		array[2] = inputPosC;
		array[3] = inputPosD;
		array[4] = inputPosE;
		array[5] = inputPosF;
		return array;
		
	}
	
	//Used for severs, block will be NOT be powered directly. Also used in SP if AllowRemotePower is true in the xml.
	public static bool HasActivePower(WorldBase _world, int _cIdx, Vector3i _blockPos)
	{
		Vector3i[] locations = PowerInputLocations(_blockPos);
		foreach (Vector3i vector in locations)
		{
			BlockValue inputBlockValue = _world.GetBlock(vector);
			Type inputBlockType = Block.list[inputBlockValue.type].GetType();
			if(inputBlockType == typeof(BlockPowered))
			{
				TileEntityPowered tileEntityPowered = (TileEntityPowered)_world.GetTileEntity(_cIdx, vector);
				if (tileEntityPowered != null)
				{
					if(tileEntityPowered.IsPowered)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
	
	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		PowerItem.PowerItemTypes powerItemType = PowerItem.PowerItemTypes.Consumer;
		powerItemType = PowerItem.PowerItemTypes.ConsumerToggle;
		return new TileEntityPoweredBlock(chunk)
		{
			PowerItemType = powerItemType
		};
	}
	
	private BlockActivationCommand[] RK = new BlockActivationCommand[]
	{
		new BlockActivationCommand("take", "hand", false)
	};
	
	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!this.RK[0].enabled)
		{
			return string.Empty;
		}
		Block block = Block.list[_blockValue.type];
		string blockName = block.GetBlockName();
		return string.Format(Localization.Get("pickupPrompt", string.Empty), Localization.Get(blockName, string.Empty));
	}
}