using System;
using UnityEngine;
//using Random = System.Random;

/// <summary>
/// Custom class for fishing rod (inherited from ItemClass)
/// </summary>
public class ItemClassFloater : ItemClass
{
    // -----------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------
    // Fishing Rod v1.0
    // --------------
    // by Mortelentus - based on Matite ACW Laser code   
    // April 2016
    // 7 Days to Die Alpha 12.5
    // This Fishing Rod code is inherited from the devs ItemClass code. It allows it to act like
    // a normal item apart from the code this class adds:
    // * bait fishing line by pressing the numberpad 5 key
    // * drop fishing line by pressing the numberpad 6 key
    // * randomly simulate fish hooked
    // * pull fishing like by pressing the numerpad 7 key
    //
    // -----------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Stores a reference to the local player
    /// </summary>
    private EntityPlayerLocal epLocalPlayer;

    /// <summary>
    /// Stores whether the player is aiming water or not (so we can check if the player is targeting water - if he is not he cannot start fishing, or fishing needs to be interupted)
    /// </summary>
    private bool boolAimingWater = false;

    private bool boolPlaced = false;

    /// <summary>
    /// Stores the date and time the tool tip was last displayed
    /// </summary>
    private DateTime dteNextToolTipDisplayTime;

    /// <summary>
    /// Stores the date and time a numberpad key was last pressed
    /// </summary>
    private DateTime dteNextAction;

    private Vector3i vPlace;

   // -----------------------------------------------------------------------------------------------


    /// <summary>
    /// Displays text in the chat text area (top left corner)
    /// </summary>
    /// <param name="str">The string to display in the chat text area</param>
    private void DisplayChatAreaText(string str)
    {
        // Check if the game instance is not null
        if (GameManager.Instance != null)
        {
            // Display the string in the chat text area
            EntityAlive entity = GameManager.Instance.World.GetLocalPlayer();
            GameManager.Instance.GameMessage(EnumGameMessages.Chat, str, entity);
        }
    }


    /// <summary>
    /// Displays tooltip text at the bottom of the screen above the tool belt
    /// </summary>
    /// <param name="str">The string to display as a tool tip</param>
    private void DisplayToolTipText(string str)
    {
        // We can only call this code once every 5 seconds to prevent spamming

        // Check if we are already displaying as tool tip message
        if (DateTime.Now > dteNextToolTipDisplayTime)
        {
            // Display the string as a tool tip message
            GameManager.ShowTooltip(epLocalPlayer, str);

            // Set time we can next display a tool tip message (once every 5 seconds)
            dteNextToolTipDisplayTime = DateTime.Now.AddSeconds(5);
        }
    }


    // -----------------------------------------------------------------------------------------------



    public override void StartHolding(ItemInventoryData _data, Transform _modelTransform)
    {
        base.StartHolding(_data, _modelTransform);
        epLocalPlayer = null;
    }


    /// <summary>
    /// Called when the player stops holding the ACW item
    /// </summary>
    /// <param name="_data">A reference to the items inventory data.</param>
    /// <param name="_modelTransform">A reference to the models transform.</param>
    public override void StopHolding(ItemInventoryData _data, Transform _modelTransform)
    {
       // Call base code
        base.StopHolding(_data, _modelTransform);

       OnActivateItemGameObjectReference component = _modelTransform.GetComponent<OnActivateItemGameObjectReference>();
        if (component != null && component.IsActivated())
        {
            component.ActivateItem(false);
        }
    }

    /// <summary>
    /// Called when holding the fishing rod
    /// </summary>
    /// <param name="_data">A reference to the items inventory data.</param>
    public override void OnHoldingUpdate(ItemInventoryData _data)
    {
        // Debug
        //DisplayChatAreaText("OnHoldingUpdate");

        // Base code - no need to run it for the rod.
        //base.OnHoldingUpdate(_data);

        // Don't run this code if remote entity
        if (_data.holdingEntity.isEntityRemote)
        {
            return;
        }       
        // check if the player is aiming at water
        // Check reference to local player
        if (!epLocalPlayer)
        {
            // Get and store a reference to the local player
            epLocalPlayer = GameManager.Instance.World.GetLocalPlayer();

            // Debug
            //DisplayChatAreaText("Reference to local player stored.");
        }
        try
        {
            if (!CheckWaterInRange())
            {
                if (boolAimingWater)
                {
                    boolAimingWater = false;
                    DisplayToolTipText("No, not a good place to put this...");
                }
                return;
            }
            else
            {
                if (!boolAimingWater)
                {
                    boolAimingWater = true;
                    DisplayToolTipText("Could work...");
                }
            }
        }
        catch (Exception)
        {
            DisplayChatAreaText("Opps, something is wrong with floater");
        }
        // Check if the player is already fishing
        if (boolAimingWater)
        {
            if (Input.GetKey(KeyCode.Keypad1) || Input.GetKey(KeyCode.Mouse0) || Input.GetMouseButton(0))
            {
                //DisplayChatAreaText("Place block here");
                boolPlaced = true;
                // place the "boat" at that position -> must be always the item name + B
                BlockValue offBlock = Block.GetBlockValue(_data.item.GetItemName() + "B");
                Block block = Block.list[offBlock.type];
                // spawns it on top of (if anything is there, it will be destroyed anyway                
                GameManager.Instance.World.SetBlockRPC(vPlace, offBlock);
                _data.holdingEntity.inventory.DecHoldingItem(1);
                //_data.holdingEntity.inventory.SetItem(_data.holdingEntity.inventory.holdingItemIdx, new ItemStack(_itemValue, 1));
                //_data.itemStack.count = 0; // see if it just disapears.
                dteNextAction = DateTime.Now.AddSeconds(1);
            }
        }
    }

    private bool CheckWaterInRange()
    {
        bool resultado = false;
        // check from 1 to 4 (min to max distance), if the block is water... if water is in range, returns true otherwise returns false. 
        // the player CANNOT fish on a toilet or in water in hole
        // so i'll correct the function so that it checks if it is at least 2 blocks deep and has 2 water blocks to each side.
        // because I dont want them fishing on a pond
        for (int i = 1; i <= 4; i++)
        {
            Vector3i vv = Vector3i.FromVector3Rounded(epLocalPlayer.GetLookRay().GetPoint(i));
            BlockValue valor = GameManager.Instance.World.GetBlock(vv);
            Block block = Block.list[valor.type];
            if (block.blockMaterial.IsLiquid &&
                (block.GetBlockName() == "water" || block.GetBlockName() == "waterMoving"))
            {
                //check deepness
                vPlace = vv;
                vv.y++; // needs to check if there's air on top of water
                valor = GameManager.Instance.World.GetBlock(vv);
                block = Block.list[valor.type];
                if (valor.type == 0)
                {
                    return true;
                }
            }
        }
        return resultado;
    }
}
