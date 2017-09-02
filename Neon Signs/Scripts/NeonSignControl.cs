using System;
using UnityEngine;

public class NeonSignControl : MonoBehaviour 
{
	public int cIdx;
	public Vector3i blockPos;
	public GameObject litSignObject;
	private bool isSignActive;
	private int flashSpeed;
	private DateTime nextStateChangeTime;
	private bool flash;
	private bool flicker;
	
	void Awake()
	{
		BlockValue blockValue = GameManager.Instance.World.GetBlock(blockPos);
		Block block = Block.list[blockValue.type];
		if (block.Properties.Values.ContainsKey("Flash"))
        {
			bool.TryParse(block.Properties.Values["Flash"], out flash);
			if (block.Properties.Values.ContainsKey("FlashSpeed"))
			{
				int.TryParse(block.Properties.Values["FlashSpeed"], out flashSpeed);
			}
			else
			flashSpeed = 1;
        }
		else
		{
			flash = false;
		}
		if(flashSpeed == 0 && flash)
		{
			flicker = true;
		}
		else
		flicker = false;
		GetColorSlots(block);
	}
	
	void Update()
	{
		if(BlockNeonSign.isBlockPoweredUp(blockPos, cIdx))
		{
			isSignActive = true;
			
		}
		else
		{
			isSignActive = false;
			nextStateChangeTime = default(DateTime);
		}
		if(isSignActive)
		{
			if(litSignObject != null && flicker)
			{
				litSignObject.active = !litSignObject.active;
				return;
			}
			if(flash)
			{
				//Flashing
				if(nextStateChangeTime == default(DateTime))
				{
					nextStateChangeTime = DateTime.Now;
				}
				if (DateTime.Now > nextStateChangeTime)
				{
					if(litSignObject != null && litSignObject.active)
					{
						litSignObject.active = false;
					}
					else
					{
						litSignObject.active = true;
					}
					nextStateChangeTime = DateTime.Now.AddSeconds(flashSpeed);
				}
			}
			else
			{
				//No Flashing
				if(litSignObject != null && !litSignObject.active)
				{
					litSignObject.active = true;
				}
			}
		}
		else
		{
			if(litSignObject != null && litSignObject.active)
			{
				litSignObject.active = false;
			}
		}
	}
	
	private void GetColorSlots(Block block)
	{
		Transform[] colorSlots = litSignObject.GetComponentsInChildren<Transform>();
		foreach (Transform slot in colorSlots)
		{
			GameObject slotObject = slot.gameObject;
			if(slotObject.name.Contains("color"))
			{
				if (block.Properties.Values.ContainsKey(slotObject.name))
				{
					Vector3 colorVector = Utils.ParseVector3(block.Properties.Values[slotObject.name]);
					SetColor(slotObject, colorVector);
				}
			}
		}
	}
	
	private void SetColor(GameObject _slotObject, Vector3 _colorVector)
	{
		Color color = new Color(_colorVector.x, _colorVector.y, _colorVector.z);
		Transform[] children = _slotObject.GetComponentsInChildren<Transform>();
		foreach (Transform child in children)
		{
			GameObject gameObject = child.gameObject;
			Renderer rend = gameObject.GetComponent<Renderer>();
			if(rend != null)
			{
				rend.material.EnableKeyword("_EMISSION");
				rend.material.SetColor("_Emission", color);
			}
		}
	}
}