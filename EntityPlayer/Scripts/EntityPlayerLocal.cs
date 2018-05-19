/*
 * Class: EntityPlayerSDXLocal
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base PlayerLocal Class. It must be incuded with the EntityPlayerSDX class, as the code explicitly looks for this pair.
 *
 */
using System;
using System.IO;
using UnityEngine;
using XMLData.Item;

class EntityPlayerSDXLocal : EntityPlayerLocal
{
    private bool blOneBlockCrouch = false;
    private bool blSoftHands = false;
    private bool blJumpingDrain = false;
    private bool blAttackReleased = false;

    public float flTotalEncumbrance = 0f;
    public float flMaxEncumbrance = 10000f;
    public bool blUseEncumbrance = true;

    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);

        // Read the OneBlockCrouch setting if it's set, then adjust the crouch modifier accordingly
        EntityClass entityClass = EntityClass.list[_entityClass];
        if (entityClass.Properties.Values.ContainsKey("OneBlockCrouch"))
            bool.TryParse(entityClass.Properties.Values["OneBlockCrouch"], out this.blOneBlockCrouch);

        // Soft hands hurt when you hit things
        if (entityClass.Properties.Values.ContainsKey("SoftHands"))
            bool.TryParse(entityClass.Properties.Values["SoftHands"], out this.blSoftHands);

        // Perform stamina drain
        if (entityClass.Properties.Values.ContainsKey("JumpingDrain"))
            bool.TryParse(entityClass.Properties.Values["JumpingDrain"], out this.blJumpingDrain);

        if (blOneBlockCrouch)
        {
            this.vp_FPController.PhysicsCrouchHeightModifier = 0.5f;
            this.vp_FPController.SyncCharacterController();
        }

        if (entityClass.Properties.Values.ContainsKey("MaxEncumbrance"))
            float.TryParse(entityClass.Properties.Values["MaxEncumbrance"], out this.flMaxEncumbrance);

        if (entityClass.Properties.Values.ContainsKey("UseEncumbrance"))
            bool.TryParse(entityClass.Properties.Values["UseEncumbrance"], out this.blUseEncumbrance);
    }

    // Encumbrance fires whenever the backpack changes, and checks if you are overloaded.
    public void CheckEncumbrance()
    {
        int num = 0;

        // Re-set the encumbrance, since we'll just re-calculate it.
        this.flTotalEncumbrance = 0f;

        ItemStack[] slots = this.bag.GetSlots();
        while (num < slots.Length && num < slots.Length)
        {
            if (slots[num].IsEmpty())
            {
                num++;
                continue;
            }
            // By default, set everything as 0.1 weight (pounds? ounces? magic?  )
            float itemWeight = 0.1f;

            // Check if the item value has an item weight attached to it, and if so, use that number instead.
            if (ItemClass.list[slots[num].itemValue.type].Properties.Values.ContainsKey("ItemWeight"))
                float.TryParse(ItemClass.list[slots[num].itemValue.type].Properties.Values["ItemWeight"], out itemWeight);

            // Calculate the total weight of the stack
            float flTotalWeight = itemWeight * slots[num].count;

            this.flTotalEncumbrance += flTotalWeight;
            num++;
        }

        String strBuff = string.Empty;

        // 1 being at Max loaded, and anything above is extra encumberance.
        float over = this.flTotalEncumbrance / this.flMaxEncumbrance;

        // 25% Over weight
        if (over > 1 && over < 1.25)
        {
            strBuff = "Encumbered";
        }
        // 26 to 50%
        else if (over >= 1.25 && over < 1.5)
        {
            strBuff = "HeavyEncumbered";
        }
        // Over 50%
        else if (over >= 1.5)
        {
            strBuff = "MaxEncumbered";
        }
        else
        {
            // Under 100%
            strBuff = "Unencumbered";
        }

        if (!String.IsNullOrEmpty(strBuff))
        {
            MultiBuffClass multiBuffClass = MultiBuffClass.FindClass(strBuff);
            if (multiBuffClass != null)
                MultiBuffClassAction.Execute(this.entityId, multiBuffClass, this, multiBuffClass.ExpiryBuffChance, false, false, EnumBodyPartHit.None, null);

        }


    }

    public override bool IsAttackValid()
    {

        if (base.IsAttackValid() && this.inventory.holdingItem.Name == "handPlayer" && this.blSoftHands)
        {
            WorldRayHitInfo executeActionTarget = this.inventory.holdingItem.Actions[0].GetExecuteActionTarget(this.inventory.holdingItemData.actionData[0]);
            if (executeActionTarget == null)
            {
                return true;
            }

            // If we hit something in our bare hands, get hurt!
            if (executeActionTarget.bHitValid)
            {
                DamageSource dmg = new DamageSource(EnumDamageSourceType.Melee);
                DamageEntity(dmg, 1, false, 1f);
            }

        }


        return true;
    }


    public override void OnUpdateLive()
    {
        // If they are crouching and not moving, give them a slight stamina boost.
        if (this.IsCrouching && this.MovementState == 0 && !this.inventory.IsHoldingItemActionRunning())
        {
            if (this.StealthState == AIDirector.StealthState.Hidden)
                this.Stamina += 0.2f;
            else
                this.Stamina += 0.1f;
        }

        // If they are jumping, take a stamina hit.
        if (this.Jumping && this.blJumpingDrain)
        {
            Skill skillByName = this.Skills.GetSkillByName("Athletics");
            if (skillByName.Level < 50)
                if (this.Stamina > 2f)
                    this.Stamina -= 2f;
        }

   
        base.OnUpdateLive();
    }


    protected override void Awake()
    {
        base.Awake();
        if ( this.blUseEncumbrance )
            this.bag.OnBackpackItemsChangedInternal += this.CheckEncumbrance;
    }
   
}
