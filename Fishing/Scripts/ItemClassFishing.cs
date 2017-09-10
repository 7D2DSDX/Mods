using System;
using UnityEngine;
//using Random = System.Random;

/// <summary>
/// Custom class for fishing rod (inherited from ItemClass)
/// </summary>
public class ItemClassFishing : ItemClass
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

    /// <summary>
    /// Stores whether the fishing rod is waiting for bait already
    /// </summary>
    private bool boolBaitWait = false;

    /// <summary>
    /// Stores whether the fishing rod is baited or not
    /// </summary>
    private bool boolRodBaited = false;

    /// <summary>
    /// Stores whether the player has started fishing or not
    /// </summary>
    private bool boolFishing = false;

    private LootType intLoot = LootType.nothing;

    /// <summary>
    /// Stores whether there is a fish hooked or not
    /// </summary>
    private bool boolFishHooked = false;

    /// <summary>
    /// Stores the date and time the tool tip was last displayed
    /// </summary>
    private DateTime dteNextToolTipDisplayTime;

    /// <summary>
    /// Stores the date and time a numberpad key was last pressed
    /// </summary>
    private DateTime dteNextAction;

    /// <summary>
    /// Stores the date and time a hook event begun
    /// </summary>
    private DateTime dteEndHook;

    /// <summary>
    /// Stores the date and time a fish event begun
    /// </summary>
    private DateTime dteEndFish;

    /// <summary>
    /// Stores a reference to the animator control
    /// </summary>
    private Animator animator;
    
    /// <summary>
    /// Stores water deepness of the fishing spot
    /// </summary>
    private int deepNess = 0;

    /// <summary>
    /// The range of the probability values (dividing a value in _lootNormal by this would give a probability in the range 0..1).
    /// </summary>
    protected const int MaxProbability = 1000;
    protected const int numLootTypes = 10;

    /// <summary>
    /// The loot types.
    /// </summary>
    private enum LootType
    {
        fish,
        rudFish,
        bigFish,
        trashBag,
        bass,
        salmon,
        head,
        bigSalmon,
        bigBass,
        nothing
    };    
    
    protected int[] _defaultChances = new int[]
    {
        20, 20, 30, 35, 50, 50, 150, 200, 250 // (like in 1 in _defaultChance)
    };

    protected int[] _lootDist = new int[numLootTypes]; // will hold the loot distribution to use in the loot check


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
            //GameManager.Instance.GameMessageClient(EnumGameMessages.Chat, str, "", false, "", false);
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


    // *************** AUXILIAR FUNCTIONS ***********************

    private bool CalculateDistribution()
    {
        bool result = true;
        for (int i = 0; i <= 8; i++)
        {
            _lootDist[i] = 0;
        }
        try
        {
            int[] _chances = (_defaultChances.Clone() as int[]);
            // NOTE TO SELF - i'm manipulating the 1 in x number...
            // so when I increase chance I'm basically reducing x, and when I decrease chance I'm increasing x.
            int lureNumber = 0;
            if (epLocalPlayer.Stats.Buffs.Count > 0)
            {
                foreach (Buff buff in epLocalPlayer.Stats.Buffs)
                {
                    if (buff.Name == "lure1")
                    {
                        lureNumber = 1;
                        break;
                    }
                    else if (buff.Name == "lure2")
                    {
                        lureNumber = 2;
                        break;
                    }
                    else if (buff.Name == "lure3")
                    {
                        lureNumber = 3;
                        break;
                    }
                    else if (buff.Name == "lure4")
                    {
                        lureNumber = 4;
                        break;
                    }
                    else if (buff.Name == "lure5")
                    {
                        lureNumber = 5;
                        break;
                    }
                    else if (buff.Name == "lure6")
                    {
                        lureNumber = 6;
                        break;
                    }
                }
            }

            // **** LURE MODIFIERS *****
            switch (lureNumber)
            {

                case 1:
                    // fish lure - +25% on small fish
                    _chances[LootType.fish.GetHashCode()] = _chances[LootType.fish.GetHashCode()] - (_chances[LootType.fish.GetHashCode()]*25/100); // fish
                    _chances[LootType.rudFish.GetHashCode()] = _chances[LootType.rudFish.GetHashCode()] - (_chances[LootType.rudFish.GetHashCode()]*25/100); // rud fish
                    break;
                case 2:
                    // bass lure - +50% on small fish, +34% on big fish, 10% on bass 
                    _chances[LootType.fish.GetHashCode()] = _chances[LootType.fish.GetHashCode()] - (_chances[LootType.fish.GetHashCode()]*50/100); // fish
                    _chances[LootType.rudFish.GetHashCode()] = _chances[LootType.rudFish.GetHashCode()] - (_chances[LootType.rudFish.GetHashCode()]*50/100); // rud fish
                    _chances[LootType.bigFish.GetHashCode()] = _chances[LootType.bigFish.GetHashCode()] - (_chances[LootType.bigFish.GetHashCode()]*34/100); // big fish
                    _chances[LootType.bass.GetHashCode()] = _chances[LootType.bass.GetHashCode()] - (_chances[LootType.bass.GetHashCode()]*10/100); // bass
                    break;
                case 3:
                    // big bass lure - +25% on small fish, +20% bass. 50% big bass 
                    _chances[LootType.fish.GetHashCode()] = _chances[LootType.fish.GetHashCode()] - (_chances[LootType.fish.GetHashCode()]*25/100); // fish
                    _chances[LootType.rudFish.GetHashCode()] = _chances[LootType.rudFish.GetHashCode()] - (_chances[LootType.rudFish.GetHashCode()]*25/100); // rud fish
                    _chances[LootType.bass.GetHashCode()] = _chances[LootType.bass.GetHashCode()] - (_chances[LootType.bass.GetHashCode()]*20/100); // bass
                    _chances[LootType.bigBass.GetHashCode()] = _chances[LootType.bigBass.GetHashCode()] - (_chances[LootType.bigBass.GetHashCode()]*50/100); // big bass
                    break;
                case 4:
                    // big salmon lure - +25% on small fish, +20% salmon. 50% big salmon 
                    _chances[LootType.fish.GetHashCode()] = _chances[LootType.fish.GetHashCode()] - (_chances[LootType.fish.GetHashCode()]*25/100); // fish
                    _chances[LootType.rudFish.GetHashCode()] = _chances[LootType.rudFish.GetHashCode()] - (_chances[LootType.rudFish.GetHashCode()]*25/100); // rud fish
                    _chances[LootType.salmon.GetHashCode()] = _chances[LootType.salmon.GetHashCode()] - (_chances[LootType.salmon.GetHashCode()]*20/100); // salmon
                    _chances[LootType.bigSalmon.GetHashCode()] = _chances[LootType.bigSalmon.GetHashCode()] - (_chances[LootType.bigSalmon.GetHashCode()]*50/100); // big salmon
                    break;
                case 5:
                    // salmon lure - +50% on small fish, +34% on big fish, 10% on salmon 
                    _chances[LootType.fish.GetHashCode()] = _chances[LootType.fish.GetHashCode()] - (_chances[LootType.fish.GetHashCode()]*50/100); // fish
                    _chances[LootType.rudFish.GetHashCode()] = _chances[LootType.rudFish.GetHashCode()] - (_chances[LootType.rudFish.GetHashCode()]*50/100); // rud fish
                    _chances[LootType.bigFish.GetHashCode()] = _chances[LootType.bigFish.GetHashCode()] - (_chances[LootType.bigFish.GetHashCode()]*34/100); // big fish
                    _chances[LootType.salmon.GetHashCode()] = _chances[LootType.salmon.GetHashCode()] - (_chances[LootType.salmon.GetHashCode()]*10/100); // salmon
                    break;
                case 6:
                    // testing head -> increases head 90%
                    _chances[LootType.head.GetHashCode()] = _chances[LootType.head.GetHashCode()] - (_chances[LootType.head.GetHashCode()] * 90 / 100); // head
                    break;
                default:
                    break;
            }
            // **** DEEPNESS MODIFIERS *****
            // for example - deeper increases chance to get bass and big bass but decreases the chance to get salmons        
            if (deepNess <= 2)
            {
                // SHALLOW WATER
                //decreases big fish, bass and big bass
                _chances[LootType.bigFish.GetHashCode()] = _chances[LootType.bigFish.GetHashCode()] + (_chances[LootType.bigFish.GetHashCode()]*10/100); // big fish -> -10%
                _chances[LootType.salmon.GetHashCode()] = _chances[LootType.salmon.GetHashCode()] + (_chances[LootType.salmon.GetHashCode()]*10/100); // bass -> -10%
                _chances[LootType.bigBass.GetHashCode()] = _chances[LootType.bigBass.GetHashCode()] + (_chances[LootType.bigBass.GetHashCode()]*25/100); // big bass -> -25%
                //increases small fish and also small salmon a little bit
                _chances[LootType.fish.GetHashCode()] = _chances[LootType.fish.GetHashCode()] - (_chances[LootType.fish.GetHashCode()]*5/100); // fish -> +5%
                _chances[LootType.rudFish.GetHashCode()] = _chances[LootType.rudFish.GetHashCode()] - (_chances[LootType.rudFish.GetHashCode()]*5/100); // rud fish -> +5%
                _chances[LootType.salmon.GetHashCode()] = _chances[LootType.salmon.GetHashCode()] - (_chances[LootType.salmon.GetHashCode()]*5/100); // salmon -> +5%

            }
            else if (deepNess > 2 && deepNess <= 6)
            {
                // low WATER
                //increases big fish and bass a bit
                _chances[LootType.bigFish.GetHashCode()] = _chances[LootType.bigFish.GetHashCode()] - (_chances[LootType.bigFish.GetHashCode()]*5/100); // big fish -> +5%
                _chances[LootType.salmon.GetHashCode()] = _chances[LootType.salmon.GetHashCode()] - (_chances[LootType.salmon.GetHashCode()]*3/100); // bass -> +3%
                //increases big salmon a bit
                _chances[LootType.bigSalmon.GetHashCode()] = _chances[LootType.bigSalmon.GetHashCode()] - (_chances[LootType.bigSalmon.GetHashCode()]*5/100); // big salmon -> +5%
                // decreases big bass a bit
                _chances[LootType.bigBass.GetHashCode()] = _chances[LootType.bigBass.GetHashCode()] + (_chances[LootType.bigBass.GetHashCode()]*15/100); // big bass -> -15%

            }
            else if (deepNess > 6 && deepNess < 10)
            {
                // deep water
                // increases big bass a bit
                _chances[LootType.bigBass.GetHashCode()] = _chances[LootType.bigBass.GetHashCode()] - (_chances[LootType.bigBass.GetHashCode()]*15/100); // big bass -> +15%
                //increases big fish and bass a bit
                _chances[LootType.bigFish.GetHashCode()] = _chances[LootType.bigFish.GetHashCode()] - (_chances[LootType.bigFish.GetHashCode()]*5/100); // big fish -> +5%
                _chances[LootType.salmon.GetHashCode()] = _chances[LootType.salmon.GetHashCode()] - (_chances[LootType.salmon.GetHashCode()]*10/100); // bass -> +10%
                //decrease big salmon, salmon and fish a bit
                _chances[LootType.bigSalmon.GetHashCode()] = _chances[LootType.bigSalmon.GetHashCode()] + (_chances[LootType.bigSalmon.GetHashCode()]*15/100); // big salmon -> -15%
                _chances[LootType.salmon.GetHashCode()] = _chances[LootType.salmon.GetHashCode()] + (_chances[LootType.salmon.GetHashCode()]*25/100); // salmon -> -25%
                _chances[LootType.fish.GetHashCode()] = _chances[LootType.fish.GetHashCode()] + (_chances[LootType.fish.GetHashCode()]*15/100); // fish -> -15%
                _chances[LootType.rudFish.GetHashCode()] = _chances[LootType.rudFish.GetHashCode()] + (_chances[LootType.rudFish.GetHashCode()]*15/100); // rud fish -> -15%

            }
            else if (deepNess >= 10)
            {
                // deep water
                // increases big bass a lot
                _chances[LootType.bigSalmon.GetHashCode()] = _chances[LootType.bigSalmon.GetHashCode()] - (_chances[LootType.bigSalmon.GetHashCode()]*35/100); // big bass -> +35%
                //increases big fish and bass a bit
                _chances[LootType.bigFish.GetHashCode()] = _chances[LootType.bigFish.GetHashCode()] - (_chances[LootType.bigFish.GetHashCode()]*10/100); // big fish -> +10%
                _chances[LootType.salmon.GetHashCode()] = _chances[LootType.salmon.GetHashCode()] - (_chances[LootType.salmon.GetHashCode()]*20/100); // bass -> +20%
                //decrease big salmon, salmon and fish a bit
                _chances[6] = _chances[6] + (_chances[6]*15/100); // big salmon -> -15%
                _chances[LootType.salmon.GetHashCode()] = _chances[LootType.salmon.GetHashCode()] + (_chances[LootType.salmon.GetHashCode()]*25/100); // salmon -> -25%
                _chances[LootType.fish.GetHashCode()] = _chances[LootType.fish.GetHashCode()] + (_chances[LootType.fish.GetHashCode()]*15/100); // fish -> -15%
                _chances[LootType.rudFish.GetHashCode()] = _chances[LootType.rudFish.GetHashCode()] + (_chances[LootType.rudFish.GetHashCode()]*15/100); // rud fish -> -15%

            }
            int auxProb = MaxProbability;
            int acc = 0;
            for (int i = 0; i <= 7; i++)
            {
                _lootDist[i] = Convert.ToInt32(Math.Round((1/Convert.ToDouble(_chances[i])*auxProb), 0));
                acc += _lootDist[i];
                _lootDist[i] = acc;
            }
            _lootDist[LootType.nothing.GetHashCode()] = auxProb;
        }
        catch (Exception)
        {
            result = false;
            DisplayChatAreaText("DEBUG FISHING: Impossible to calculate loot distribution");
        }
        return result;
    }

    // -----------------------------------------------------------------------------------------------



    public override void StartHolding(ItemInventoryData _data, Transform _modelTransform)
    {
        base.StartHolding(_data, _modelTransform);
        animator = null;
        epLocalPlayer = null;
        ResetFishing();
    }


    /// <summary>
    /// Called when the player stops holding the ACW item
    /// </summary>
    /// <param name="_data">A reference to the items inventory data.</param>
    /// <param name="_modelTransform">A reference to the models transform.</param>
    public override void StopHolding(ItemInventoryData _data, Transform _modelTransform)
    {
        // Debug
        //DisplayChatAreaText("StopHolding");
        // Reset flags
        ResetFishing();

        // Call base code
        base.StopHolding(_data, _modelTransform);

        // Check if the model transform is null
        if (_modelTransform == null)
        {
            return;
        }
        OnActivateItemGameObjectReference component = _modelTransform.GetComponent<OnActivateItemGameObjectReference>();
        if (component != null && component.IsActivated())
        {
            component.ActivateItem(false);
        }
    }


    /// <summary>
    /// Checks if we have stored a reference to the Animator component... if not it gets one and returns true if all is ok
    /// The animator will control the different fishing rod states
    /// </summary>
    /// <param name="_data">Item inventory data reference</param>
    /// <returns>Returns true if the reference to the Animator component is valid</returns>
    private bool CheckAnimatorReference(ItemInventoryData _data)
    {
        // Check if the reference to the LineRenderer component is null
        if (!animator)
        {
            // Debug
            //DisplayChatAreaText("Warning: animator reference is null... getting reference now.");

            // Get and store a reference to the animator component
            animator = _data.model.GetComponentInChildren<Animator>();

            // Check if we were not able to get a reference to the animator component
            if (!animator)
            {
                // Debug
                //DisplayChatAreaText("Error: animator component not found!");
                return false;
            }
            else
            {
                // Return true as we now have a reference to the animator component
                //DisplayChatAreaText("Found animator");
                return true;
            }
        }
        else
        {
            // Return true as we already have a reference to the LineRenderer component
            return true;
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
                    DisplayToolTipText("Man... I can't fish here...");
                    ResetFishing();
                }
                return;
            }
            else
            {
                if (!boolAimingWater)
                {
                    if (CheckAnimatorReference(_data))
                    {
                        ResetFishing();
                        if (epLocalPlayer)
                        {
                            MultiBuffClassAction multiBuffClassAction = MultiBuffClassAction.NewAction("fishingWater");
                            multiBuffClassAction.Execute(epLocalPlayer.entityId, (EntityAlive) epLocalPlayer, false,
                                EnumBodyPartHit.None, (string) null);
                        }
                    }
                    boolAimingWater = true;
                    DisplayToolTipText("Yea... This looks like a nice spot.");
                }
            }
        }
        catch (Exception)
        {
            DisplayChatAreaText("DEBUG: Oops something went wrong with the fishing mod.");
        }
        // Check if the player is already fishing
        if (true)
        {
            #region Hook event expired;

            if (boolFishHooked && boolAimingWater)
            {
                if (DateTime.Now > dteEndHook)
                {
                    animator.SetTrigger("stopFishing");
                    DisplayToolTipText("Shit... Whatever it was... It's gone!");
                    boolFishHooked = false;
                    dteNextAction = DateTime.Now.AddSeconds(0.5);
                }
            }

            #endregion;

            #region fish event expired;

            if (boolFishing && boolAimingWater)
            {
                if (DateTime.Now > dteEndFish)
                {
                    animator.SetTrigger("stopFishing");
                    if (intLoot != LootType.nothing)
                        DisplayToolTipText("Great... It's gone... Let's be faster next time, shall we?");
                    else DisplayToolTipText("Oh well... Shall we try again?");
                    boolFishing = false;
                    intLoot = LootType.nothing;
                    dteNextAction = DateTime.Now.AddSeconds(0.5);
                }
            }

            #endregion;

            #region Wait for bait;

            if (!boolFishHooked && !boolRodBaited && !boolBaitWait && !boolFishing && boolAimingWater)
            {
                if (DateTime.Now > dteNextAction)
                {
                    if (animator)
                    {
                        animator.SetTrigger("waitBait");
                        boolBaitWait = true;
                        dteNextAction = DateTime.Now.AddSeconds(0.5);
                    }
                }
            }

            #endregion;

            #region Bait rod;

            // bait rod if not already baited, no hook event present, and no loot to get.
            if (Input.GetKey(KeyCode.R) && boolAimingWater && !boolRodBaited && !boolFishHooked && boolBaitWait)
            {
                if (DateTime.Now > dteNextAction)
                {
                    ItemValue earthworm = ItemClass.GetItem("earthworm", false);
                    int numWorms = epLocalPlayer.bag.GetItemCount(earthworm);
                    if (numWorms >= 1)
                    {
                        bool itemGood = true;
                        // cause decay
                        if (_data.itemValue.MaxUseTimes > 0)
                        {
                            ItemValue itemValue = _data.itemValue;
                            itemValue.UseTimes += AttributeBase.GetVal<AttributeDegradationRate>(_data.itemValue, 1);
                            _data.itemValue = itemValue;
                            if (_data.itemValue.MaxUseTimes > 0 &&
                                _data.itemValue.UseTimes >= _data.itemValue.MaxUseTimes ||
                                _data.itemValue.UseTimes == 0 && _data.itemValue.MaxUseTimes == 0)
                            {
                                // cane is broken
                                itemGood = false;
                                DisplayToolTipText("Hmm, i think i've been fishing too much... Need to fix this...");
                            }
                        }
                        if (itemGood)
                        {
                            epLocalPlayer.bag.DecItem(earthworm, 1);
                            if (animator)
                            {
                                animator.SetTrigger("stopFishing");
                                DisplayToolTipText("Alright! Got this thing baited, let's see what I get...");
                                boolRodBaited = true;
                                boolBaitWait = false;
                            }
                            dteNextAction = DateTime.Now.AddSeconds(2);
                        }
                        else dteNextAction = DateTime.Now.AddSeconds(0.5);
                    }
                    else
                    {
                        DisplayToolTipText("You don't have enough earth worms...");
                        dteNextAction = DateTime.Now.AddSeconds(0.5);
                    }
                }
            }

            #endregion;

            #region Hook event;

            // if baited and not fish hooked...
            if (boolRodBaited && !boolFishHooked && boolAimingWater && !boolFishing)
            {
                // randomly does hook warning -> it lasts 2 seconds.
                if (DateTime.Now > dteNextAction)
                {
                    System.Random r = new System.Random();
                    int hookNow = r.Next(1, 101);
                    if (hookNow <= 8)
                    {
                        if (animator)
                        {
                            this.boolFishHooked = true;
                            boolRodBaited = false; // always looses the bait here.
                            // decides what the loot will be here, to determine what time the player will have to react
                            // the rarer the loot the less time he has
                            intLoot = Choose(_data);
                            double timeToReact = numLootTypes - 2;
                            if (intLoot > 0 && intLoot != LootType.nothing)
                            {
                                timeToReact = timeToReact/intLoot.GetHashCode();
                            }
                            animator.SetTrigger("fishHook");
                            dteEndHook = DateTime.Now.AddSeconds(timeToReact);
                            //DisplayToolTipText("You've hooked something");
                            dteNextAction = DateTime.Now.AddSeconds(0.1);
                        }
                    }
                    else
                    {
                        // next possible bite will only be evaluated every one second
                        dteNextAction = DateTime.Now.AddSeconds(1);
                    }
                }
            }

            #endregion;                        

            #region Hook Pull;

            // pulls the rod, decides what is the loot and waits 2 seconds...
            // if the player doesn't react, puff... Loot is gone!
            if (boolFishHooked && boolAimingWater)
            {
                if (Input.GetKey(KeyCode.Keypad1) || Input.GetKey(KeyCode.Mouse0) || Input.GetMouseButton(0))
                {
                    if (DateTime.Now > dteNextAction)
                    {
                        boolFishHooked = false;
                        if (animator)
                        {
                            animator.SetTrigger("hookPull");
                            boolFishing = true;
                            // see what was gotten, and show the proper animation
                            if (intLoot == LootType.nothing) DisplayToolTipText("What the fuck? Nothing?");
                            else if (intLoot == LootType.head)
                            {
                                DisplayToolTipText("What... the... f... AAAAAAHHHHH!");
                                animator.SetTrigger("head");
                            }
                            else if (intLoot == LootType.trashBag)
                            {
                                DisplayToolTipText("YES! Got it! hmm... no? what?");
                                animator.SetTrigger("trash");
                            }
                            else if (intLoot == LootType.bigBass || intLoot == LootType.bigSalmon)
                            {
                                DisplayToolTipText("YES! That's a big one!");
                                animator.SetTrigger("bigFish");
                            }
                            else if (intLoot == LootType.salmon)
                            {
                                DisplayToolTipText("A salmon... Tasty!");
                                animator.SetTrigger("smallFish");
                            }
                            else if (intLoot == LootType.bass)
                            {
                                DisplayToolTipText("A nice looking bass fish!");
                                animator.SetTrigger("smallFish");
                            }
                            else
                            {
                                DisplayToolTipText("Better then nothing. Food is food!");
                                animator.SetTrigger("smallFish");
                            }
                            if (intLoot != LootType.nothing)
                                dteEndFish = DateTime.Now.AddSeconds(10); // 10 seconds to pick the fish up
                            else dteEndFish = DateTime.Now.AddSeconds(0.5);
                        }
                        dteNextAction = DateTime.Now.AddSeconds(0.5);
                    }
                }
            }

            #endregion;

            #region Get loot;

            // get's whatever is in the hook
            if (boolFishing && boolAimingWater)
            {
                if (Input.GetKey(KeyCode.Keypad2) || Input.GetKey(KeyCode.Mouse1) || Input.GetMouseButton(1))
                {
                    if (DateTime.Now > dteNextAction)
                    {
                        boolFishing = false;
                        if (intLoot != LootType.nothing)
                        {
                            //DisplayToolTipText("Got it... Shall we try again?");                            
                            //ItemValue lootItem = ItemClass.GetItem(intLoot.ToString());
                            ItemValue lootItem = ItemClass.GetItem(intLoot.ToString(), false);
                            ItemStack lootStack = new ItemStack(lootItem, 1);
                            epLocalPlayer.bag.AddItem(lootStack);
                        }
                        //else DisplayToolTipText("Oh well... Shall we try again?");
                        ResetFishing();
                        dteNextAction = DateTime.Now.AddSeconds(0.5);
                    }
                }
            }

            #endregion;
        }
    }

    /// <summary>
    /// Choose a random loot type.
    /// </summary>
    private LootType Choose(ItemInventoryData _data)
    {

        // quality modifier - it changes the chance to get anything by reducing the max range
        int auxMaxProb = MaxProbability;
        double probModifier = 1;
        if (_data.itemValue.HasQuality)
        {
            if (_data.itemValue.Quality < 100) probModifier = 1;
            else if (_data.itemValue.Quality >= 100 && _data.itemValue.Quality < 250) probModifier = 0.85;
            else if (_data.itemValue.Quality >= 250 && _data.itemValue.Quality < 550) probModifier = 0.65;
            else probModifier = 0.45;
        }
        auxMaxProb = Convert.ToInt32(Math.Floor(MaxProbability*probModifier));
        System.Random _rnd = new System.Random((int) (DateTime.Now.Ticks & 0x7FFFFFFF));
        LootType lootType = 0; // start at first one
        int randomValue = _rnd.Next(auxMaxProb);
        if (CalculateDistribution())
        {
            //DisplayChatAreaText(randomValue.ToString());
            if (randomValue >= 1000) randomValue = 999;
            while (_lootDist[(int) lootType] <= randomValue)
            {
                lootType++; // next loot type
            }
        }
        else lootType=LootType.nothing;
        return lootType;
    }


    private void ResetFishing()
    {
        boolAimingWater = false;
        boolRodBaited = false;
        boolFishing = false;
        boolBaitWait = false;
        boolFishHooked = false;
        intLoot = LootType.nothing;
        if (animator)
        {
            animator.ResetTrigger("fishHook");
            animator.ResetTrigger("stopFishing");
            animator.ResetTrigger("hookPull");
            animator.ResetTrigger("waitBait");
            animator.ResetTrigger("smallFish");
            animator.ResetTrigger("bigFish");
            animator.ResetTrigger("head");

            animator.CrossFade("Idle", 0.0f);
            // go directly to idle
        }
        if (epLocalPlayer)
        {
            if (epLocalPlayer.Stats.Buffs.Count > 0)
            {
                foreach (Buff buff in epLocalPlayer.Stats.Buffs)
                {
                    if (buff.Name == "fishingWater")
                    {
                        epLocalPlayer.Stats.Debuff("fishingWater");
                        break;
                    }
                }
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
                int deepNessAux = 1;
                for (int j = 1; j <= 10; i++)
                {
                    vv.y = vv.y - 1;
                    valor = GameManager.Instance.World.GetBlock(vv);
                    block = Block.list[valor.type];
                    if (block.blockMaterial.IsLiquid &&
                        (block.GetBlockName() == "water" || block.GetBlockName() == "waterMoving"))
                    {
                        deepNessAux++;
                    }
                    else break;
                }

                deepNess = deepNessAux;
                //DisplayChatAreaText("Deepness = " + deepNess);
                //"waterMovingBucket" and "waterSource8" will not be considered valid blocks for fishing
                return true;
            }
        }
        return resultado;
    }
}
