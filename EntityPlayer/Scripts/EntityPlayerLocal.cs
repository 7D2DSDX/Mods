/*
 * Class: EntityPlayerSDXLocal
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base PlayerLocal Class. It must be incuded with the EntityPlayerSDX class, as the code explicitly looks for this pair.
 *
 */
using System.Collections.Generic;
using UnityEngine;
using XMLData.Item;

class EntityPlayerSDXLocal : EntityPlayerLocal
{
    private bool blOneBlockCrouch = false;
    private bool blSoftHands = false;
    private bool blJumpingDrain = false;
    private bool blAttackReleased = false;

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
    }

    public override bool IsAttackValid()
    {
        
        if (base.IsAttackValid()  && this.inventory.holdingItem.Name == "handPlayer" && this.blSoftHands)
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
                DamageEntity(dmg, 1, false,1f);
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

    

}
