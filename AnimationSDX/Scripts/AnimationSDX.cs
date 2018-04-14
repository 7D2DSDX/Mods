/*
 * Class: AnimationSDX
 * Author:  sphereii, HAL9000, Mortelentus
 * Category: Legacy Animator
 * Description:
 *
 *      This class provides legacy-style animations to be applied to custom entities. RootMotion is NOT supported, and must be disabled.
 *
 * Usage:
 *      Add the following class to entities that are meant to use these features.
 *  
 *         <property name="AvatarController" value="AnimationSDX, Mods" />
 *
 *
 * Features:
 *      This class is meant to be used in following the Animation Tutorial: https://7d2dsdx.github.io/Tutorials/Howtosetuptheanimatedcustomentit.html
 *
 *      Because external entities have different movement speeds than baked assets, this class creates a new entity class via the Configs/AnimationSDX which
 *      is meant to be inherited.
 *
 *      These values are meant as a starting value, and seem to work well for most entities:
 *
 *           <property name="WanderSpeed" value="0.8" />
 *           <property name="ApproachSpeed" value="0.8" />
 *           <property name="NightWanderSpeed" value="0.8" />
 *           <property name="NightApproachSpeed" value="1.1" />
 *           <property name="HasRagdoll" value="false" />
 */

using System;
using UnityEngine;

// new controller class, which allows us to assign body parts
// it's also the first step for full animator control
public class AnimationSDX : AvatarZombieController
{
    // Default animation strings.
    private string AnimationIdle = "";
    private string AnimationSecondIdle = "";
    private string AnimationMainAttack = "";
    private string AnimationSecondAttack = "";
    private string AnimationPain = "";
    private string AnimationJump = "";
    private string AnimationDeath = "";
    private string AnimationRun = "";
    private string AnimationWalk = "";
    private string AnimationSpecialAttack = "";
    private string RightHandName = "RightHand";

    private EntityAlive entityAlive;
    private bool IsVisible;
    private bool HasDied;
    private bool isAlwaysWalk;
    public Transform ModelTransform;
    public Transform GraphicsTransform;
    private float lastPlayerX;
    private float lastPlayerZ;
    private float lastDistance;
    private float DoesntSeemToDoAnything;

    protected new int specialAttackTicks;
    protected new float timeSpecialAttackPlaying;
    protected new float idleTime;
    public Animator anim;

    // Support for letting entity shoot weapons
    private string RightHand = "RightHand";
    private Transform rightHandItemTransform;
    protected Animator rightHandAnimator;

    #region Tags Zombie;
    protected bool isCrippled;
    protected bool isCrawler;
    protected bool suppressPainLayer;
    protected float crawlerTime;
    protected bool headDismembered;
    protected bool leftUpperArmDismembered;
    protected bool leftLowerArmDismembered;
    protected bool rightUpperArmDismembered;
    protected bool rightLowerArmDismembered;
    protected bool leftUpperLegDismembered;
    protected bool leftLowerLegDismembered;
    protected bool rightUpperLegDismembered;
    protected bool rightLowerLegDismembered;
    protected Transform neck;
    protected Transform leftUpperArm;
    protected Transform leftLowerArm;
    protected Transform rightLowerArm;
    protected Transform rightUpperArm;
    protected Transform leftUpperLeg;
    protected Transform leftLowerLeg;
    protected Transform rightLowerLeg;
    protected Transform rightUpperLeg;
    protected Transform neckGore;
    protected Transform leftUpperArmGore;
    protected Transform leftLowerArmGore;
    protected Transform rightUpperArmGore;
    protected Transform rightLowerArmGore;
    protected Transform leftUpperLegGore;
    protected Transform leftLowerLegGore;
    protected Transform rightLowerLegGore;
    protected Transform rightUpperLegGore;
    #endregion;


    private bool blDisplayLog = false;
    private void Log(String strLog)
    {
        if (blDisplayLog)
        {
            Debug.Log(strLog);
        }
    }
    public AnimationSDX()
    {
        this.entityAlive = this.transform.gameObject.GetComponent<EntityAlive>();
        EntityClass entityClass = EntityClass.list[this.entityAlive.entityClass];

  

        if (entityClass.Properties.Values.ContainsKey("AnimationMainAttack"))
            this.AnimationMainAttack = entityClass.Properties.Values["AnimationMainAttack"];

        if (entityClass.Properties.Values.ContainsKey("AnimationSecondAttack"))
            this.AnimationSecondAttack = entityClass.Properties.Values["AnimationSecondAttack"];

        if (entityClass.Properties.Values.ContainsKey("AnimationIdle"))
            this.AnimationIdle = entityClass.Properties.Values["AnimationIdle"];

        if (entityClass.Properties.Values.ContainsKey("AnimationSecondIdle"))
            this.AnimationSecondIdle = entityClass.Properties.Values["AnimationSecondIdle"];

        if (entityClass.Properties.Values.ContainsKey("AnimationPain"))
            this.AnimationPain = entityClass.Properties.Values["AnimationPain"];

        if (entityClass.Properties.Values.ContainsKey("AnimationDeath"))
            this.AnimationDeath = entityClass.Properties.Values["AnimationDeath"];

        if (entityClass.Properties.Values.ContainsKey("AnimationRun"))
            this.AnimationRun = entityClass.Properties.Values["AnimationRun"];

        if (entityClass.Properties.Values.ContainsKey("AnimationWalk"))
            this.AnimationWalk = entityClass.Properties.Values["AnimationWalk"];

        if (entityClass.Properties.Values.ContainsKey("AnimationJump"))
            this.AnimationJump = entityClass.Properties.Values["AnimationJump"];

        if (entityClass.Properties.Values.ContainsKey("AnimationSpecialAttack"))
            this.AnimationSpecialAttack = entityClass.Properties.Values["AnimationSpecialAttack"];

        if (entityClass.Properties.Values.ContainsKey("RightHandJointName"))
            this.RightHandName = entityClass.Properties.Values["RightHandJointName"];

        this.IsVisible = true;
    }

    protected override void Awake()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        try
        {

            this.GraphicsTransform = this.transform.Find("Graphics");

            if (this.GraphicsTransform == null)
            {
                Log(" !! Graphics Transform null!");
                return;
            }
            if ( this.GraphicsTransform.Find("Model") == null )
            {
                Log("!! No Model found in GraphicsTransoform");
                return;
            }
            this.ModelTransform = this.GraphicsTransform.Find("Model").GetChild(0);
            if (this.ModelTransform == null)
            {
                Log(" !! Model Transform is null!");
                return;
            }
            //this bit is important for SDXers! It adds the component that links each collider with the Entity class so hits can be registered.
            AddTransformRefs(this.ModelTransform);

            //if you're using A14 or haven't set specific tags for the collision in Unity un-comment this and it will set them all to being body contacts
            //using this method means things like head shot multiplers won't work but it will enable basic collision
            AddTagRecursively(this.ModelTransform, "E_BP_Body");
            Log("Searching for Idle");

            if (this.ModelTransform.GetComponent<Animation>() == null)
            {
                Log("No Animation for this model!");
                return;

            }
            if ((this.ModelTransform.GetComponent<Animation>()[this.AnimationIdle]) == null)
                return;

         
            this.ModelTransform.GetComponent<Animation>().Play(this.AnimationIdle);
            Log("Playing Animation");

            // Find the right hand joint, if it's set.
            EntityClass entityClass = EntityClass.list[this.entityAlive.entityClass];
            if (entityClass.Properties.Values.ContainsKey("RightHandJointName"))
            {
                this.RightHand = entityClass.Properties.Values["RightHandJointName"];
                this.rightHandItemTransform = FindTransform(this.GraphicsTransform, this.GraphicsTransform, RightHand);
                if (this.rightHandItemTransform)
                    Log("Right Hand Item Transform: " + this.rightHandItemTransform.name.ToString());
                else
                    Log("Right Hand Item Transform: Could not find Transofmr: " + RightHand);
            }

        

        }
        catch (Exception ex)
        {
            Log("Exception thrown in Awake() " + ex.ToString());
        }
    }

    private void AddTransformRefs(Transform t)
    {
        //   Log("Checking " + t.name + " tag " + t.tag);
        if (t.GetComponent<Collider>() != null && t.GetComponent<RootTransformRefEntity>() == null)
        //	if (t.GetComponent<Collider>() != null )
        {
            RootTransformRefEntity root = t.gameObject.AddComponent<RootTransformRefEntity>();
            root.RootTransform = this.transform;
            Log("Added root ref on " + t.name + " tag " + t.tag);
        }
        foreach (Transform tran in t)
        {
            AddTransformRefs(tran);
        }
    }

    void AddTagRecursively(Transform trans, string tag)
    {
        // If the objects are untagged, then tag them, otherwise ignore setting the tag.
        if (trans.gameObject.tag.Contains("Untagged"))
        {
            // Check to see if the part contains "head", and let it be a headshot tag
            // otherwise, fall back to default body
            if (trans.name.ToLower().Contains("head"))
                trans.gameObject.tag = "E_BP_Head";
            else
                trans.gameObject.tag = tag;
        }


        //Log("Transoform Tag: " + trans.name + " : " + trans.tag);
        foreach (Transform t in trans)
            AddTagRecursively(t, tag);
    }

    public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
    {
        //Debug.Log("MODEL NAME: " + _modelName);
        Transform transform = this.ModelTransform;
        if (transform == null)
        {
            if (_bFPV)
            {
                transform = this.ModelTransform.Find(_modelName);
                if ( transform == null )
                {
                    Log("SwitchModelAndView: Error finding transform!");
                    return;
                }
            }
            // no main transform, not continueing
            return;
        }
        //if (transform!=null) Debug.Log("TRANSFORM = " + transform.name);
        this.GraphicsTransform = transform;
        this.meshTransform = transform;               
        this.modelName = _modelName;
        this.bMale = _bMale;
        this.bFPV = _bFPV;
        this.assignBodyParts();
        try
        {
            this.anim = this.GraphicsTransform.GetComponent<Animator>();
            if ( this.anim == null )
            {
                Log("SwitchModelAndView: Animator not found!");
                this.anim = this.GraphicsTransform.gameObject.AddComponent<Animator>();
                Log("Added Animator");
               // return;
            }
        }
        catch (Exception)
        {
            Log("No Animator, and could not attach");
            //Debug.Log("NO ANIMATOR");
        }
        try
        {
            if (this.entityAlive.RootMotion)
            {
                Log("Checking if Root MOtion is enabled");
                AvatarRootMotion avatarRootMotion = this.ModelTransform.GetComponent<AvatarRootMotion>();
                if (avatarRootMotion == null)
                {
                    avatarRootMotion = this.ModelTransform.gameObject.AddComponent<AvatarRootMotion>();
                }
                avatarRootMotion.Init(this, this.anim);
            }
        }
        catch (Exception)
        {
            Log("Root Motion is not available.");
        }
        if (this.rightHandItemTransform != null  )
        {
            Debug.Log("Setting Right Hand Item Transform");
            this.rightHandItemTransform.parent = this.rightHand;
            Vector3 position = AnimationGunjointOffsetData.AnimationGunjointOffset[this.entityAlive.inventory.holdingItem.HoldType.Value].position;
            Vector3 rotation = AnimationGunjointOffsetData.AnimationGunjointOffset[this.entityAlive.inventory.holdingItem.HoldType.Value].rotation;
            this.rightHandItemTransform.localPosition = position;
            this.rightHandItemTransform.localEulerAngles = rotation;
            SetInRightHand(this.rightHandItemTransform);
        }
        if (this.anim != null)
        {
            this.anim.SetBool("IsMale", _bMale);
            this.anim.SetInteger("WalkType", this.entityAlive.GetWalkType());
            this.anim.SetBool("IsDead", this.entityAlive.IsDead());
            this.anim.SetBool("IsFPV", this.bFPV);
            this.anim.SetBool("IsAlive", this.entityAlive.IsAlive());
        }

        Log("Done with SwichModelAndView");
    }

    public void SetAlwaysWalk(bool _b)
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        this.isAlwaysWalk = _b;
    }

    public Texture2D SetSkinTexture(Texture2D _newTexture, bool _bMakeACopy)
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return null;
    }

    public override void SetInRightHand(Transform _transform)
    {
        if (this.rightHandItemTransform == null)
            return;

        Log("Setting Right Hand: " + rightHandItemTransform.name.ToString());
        
        this.idleTime = 0f;
        if (rightHandItemTransform != null)
        {
            rightHandItemTransform.parent = this.rightHand;
        }

        Log("Setting Right Hand Transform");
        //this.rightHandItemTransform = _transform;
        this.rightHandAnimator = ((!(rightHandItemTransform != null)) ? null : rightHandItemTransform.GetComponent<Animator>());
        if (this.rightHandItemTransform != null)
        {
            Utils.SetLayerRecursively(this.rightHandItemTransform.gameObject, 0);
        }

        Log("Done with SetInRightHand");
    }

    public override void SetDrunk(float _numBeers)
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public override void SetMinibikeAnimation(string _anim, float _amount)
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public override void SetMinibikeAnimation(string _anim, bool _isPlaying)
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public new bool IsAnimationAttackPlaying()
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        if (this.ModelTransform.GetComponent<Animation>()[this.AnimationMainAttack] != null && this.ModelTransform.GetComponent<Animation>()[this.AnimationMainAttack].enabled)
            return true;
        if (this.ModelTransform.GetComponent<Animation>()[this.AnimationSecondAttack] != null)
            return this.ModelTransform.GetComponent<Animation>()[this.AnimationSecondAttack].enabled;
        return false;
    }

    public override void StartAnimationAttack()
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        if (this.ModelTransform.GetComponent<Animation>()[this.AnimationMainAttack] != null && this.ModelTransform.GetComponent<Animation>()[this.AnimationSecondAttack] != null)
        {
            if (UnityEngine.Random.value > 0.5)
                this.ModelTransform.GetComponent<Animation>().Play(this.AnimationMainAttack);
            else
                this.ModelTransform.GetComponent<Animation>().Play(this.AnimationSecondAttack);
        }
        else
        {
            if ((this.ModelTransform.GetComponent<Animation>()[this.AnimationMainAttack] == null))
                return;
            this.ModelTransform.GetComponent<Animation>().Play(this.AnimationMainAttack);
        }
    }

    public override bool IsAnimationSpecialAttackPlaying()
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return timeSpecialAttackPlaying > 0f;
    }

    public override void StartAnimationSpecialAttack(bool _b)
    {
        if (_b)
        {
            if (this.ModelTransform.GetComponent<Animation>()[this.AnimationSpecialAttack] != null)
            {
                Log("Playing Special Attack!");
                this.ModelTransform.GetComponent<Animation>().Play(this.AnimationSpecialAttack);
                this.idleTime = 0f;
                this.specialAttackTicks = 3;
                this.timeSpecialAttackPlaying = 0.8f;
            }
        }
    }

    public void StartAnimationSpecialAttack()
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public override bool IsAnimationSpecialAttack2Playing()
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public override void StartAnimationSpecialAttack2()
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public override bool IsAnimationRagingPlaying()
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public override void StartAnimationRaging()
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public new bool IsAnimationHarvestingPlaying()
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public new void StartAnimationHarvesting()
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void SetAnimationClimbing(bool _b)
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public override Transform GetFirstPersonCameraTransform()
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return null;
    }


    // Reutrns the righthand transform
    public override Transform GetRightHandTransform()
    {
        return this.rightHandItemTransform;
    }

    protected override void assignBodyParts()
    {
        if ( this.GraphicsTransform == null )
        {
            Log("assignBodyParts: GraphicsTransform is null!");
            return;
        }

   
        //Debug.Log("FIND Head");
        this.head = FindTransform(this.GraphicsTransform, this.GraphicsTransform, "Head");
        //Debug.Log("FIND NECK");
        this.neck = FindTransform(this.GraphicsTransform, this.GraphicsTransform, "Neck");
        //this.entityLiving.GetRightHandTransformName()
        //Debug.Log("LOOKING FOR " + rightHandStr);
        this.rightHand = FindTransform(this.GraphicsTransform, this.GraphicsTransform, RightHandName);
        //if (this.rightHand != null) Debug.Log("Right HAND = " + this.rightHand.name);
        //Debug.Log("FIND LEGS");
        this.leftUpperLeg = FindTransform(this.GraphicsTransform, this.GraphicsTransform, "LeftUpLeg");
        this.leftLowerLeg = FindTransform(this.GraphicsTransform, this.GraphicsTransform, "LeftLeg");
        this.rightUpperLeg = FindTransform(this.GraphicsTransform, this.GraphicsTransform, "RightUpLeg");
        this.rightLowerLeg = FindTransform(this.GraphicsTransform, this.GraphicsTransform, "RightLeg");
        //Debug.Log("FIND ARMS");
        this.leftUpperArm = FindTransform(this.GraphicsTransform, this.GraphicsTransform, "LeftArm");
        this.leftLowerArm = FindTransform(this.GraphicsTransform, this.GraphicsTransform, "LeftForeArm");
        this.rightUpperArm = FindTransform(this.GraphicsTransform, this.GraphicsTransform, "RightArm");
        this.rightLowerArm = FindTransform(this.GraphicsTransform, this.GraphicsTransform, "RightForeArm");
        try
        {
            //Debug.Log("FIND GOORES");
            this.neckGore = GameUtils.FindTagInChilds(this.GraphicsTransform, "L_HeadGore");
            this.leftUpperArmGore = GameUtils.FindTagInChilds(this.GraphicsTransform, "L_LeftUpperArmGore");
            this.leftLowerArmGore = GameUtils.FindTagInChilds(this.GraphicsTransform, "L_LeftLowerArmGore");
            this.rightUpperArmGore = GameUtils.FindTagInChilds(this.GraphicsTransform, "L_RightUpperArmGore");
            this.rightLowerArmGore = GameUtils.FindTagInChilds(this.GraphicsTransform, "L_RightLowerArmGore");
            this.leftUpperLegGore = GameUtils.FindTagInChilds(this.GraphicsTransform, "L_LeftUpperLegGore");
            this.leftLowerLegGore = GameUtils.FindTagInChilds(this.GraphicsTransform, "L_LeftLowerLegGore");
            this.rightUpperLegGore = GameUtils.FindTagInChilds(this.GraphicsTransform, "L_RightUpperLegGore");
            this.rightLowerLegGore = GameUtils.FindTagInChilds(this.GraphicsTransform, "L_RightLowerLegGore");
        }
        catch (Exception)
        {
            Debug.Log("NO GORE FOR YOU, NO SIREE");
        }
    }

    private Transform FindTransform(Transform root, Transform t, string objectName)
    {
        if (t.name.Contains(objectName))
        {
            return t;
        }
        //msg += string.Format("transform={0}, tag={1}", t.name, t.tag) + Environment.NewLine;
        foreach (Transform tran in t)
        {
            //msg += string.Format("CHILDREN OF {0}: ", t.name);
            Transform result = FindTransform(root, tran, objectName);
            if (result != null) return result;
        }
        return null;
    }

    public new bool IsAnimationUsePlaying()
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public new void StartAnimationUse()
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public new void SetRagdollEnabled(bool _b)
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public override void StartAnimationFiring()
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public new void StartAnimationReloading()
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float random)
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name + " hit: "+ _bodyPart.ToString());
        if (this.HasDied || !(bool)(this.ModelTransform.GetComponent<Animation>()[this.AnimationPain]))
            return;
        this.ModelTransform.GetComponent<Animation>().Play(this.AnimationPain);
    }

    public override bool IsAnimationHitRunning()
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        return false;
    }

    public override void StartAnimationJumping()
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (!(this.ModelTransform.GetComponent<Animation>()[this.AnimationJump] != null))
            return;
        this.ModelTransform.GetComponent<Animation>().Play(this.AnimationJump);
    }

    public new void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
    {
        //    Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public new void SetAlive()
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public override void SetLookPosition(Vector3 _pos)
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public override void SetVisible(bool _b)
    {
        //  Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (_b == this.IsVisible && _b == this.GraphicsTransform.gameObject.activeSelf)
            return;
        this.IsVisible = _b;
        this.GraphicsTransform.gameObject.SetActive(_b);

    }

    public new void SetWalkingSpeed(float _f)
    {
        //  Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public new void SetHeadAngles(float _nick, float _yaw)
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public new void SetArmsAngles(float _rightArmAngle, float _leftArmAngle)
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public new void SetAiming(bool _bEnable)
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public new void SetCrouching(bool _bEnable)
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetFPV(bool _fpv)
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    // Helper class to check if the animation is valid or not.
    public bool HasValidAnimation(String strAnimation)
    {
        if (this.ModelTransform.GetComponent<Animation>()[strAnimation] != null)
        {
            //      Log(strAnimation + " is Valid");
            return true;
        }

        //    Log(strAnimation + " is NOT Valid");
        return false;
    }

    public bool HasValidAnimationAndEnabled(String strAnimation)
    {
        if (HasValidAnimation(strAnimation) && this.ModelTransform.GetComponent<Animation>()[strAnimation].enabled)
            return true;

        return false;
    }

    public void PlayAnimation(String strAnimation)
    {
        Log("Tesitng Animation: " + strAnimation);
        if (HasValidAnimation(strAnimation))
        {
            // We only want to play the animation when its not already enabled.
            if (!this.ModelTransform.GetComponent<Animation>()[strAnimation].enabled)
            {
                Log("Playing Animation");
                this.ModelTransform.GetComponent<Animation>().Play(strAnimation);
            }
            else
                Log("Animation is disabled");

        }
    }
    protected override void Update()
    {
        if (!this.IsVisible || this.entityAlive == null )
            return;

        if (this.entityAlive.IsDead() && !this.HasDied)
        {
            //     Log("Update: Entity is Dead");
            this.HasDied = true;
            this.ModelTransform.GetComponent<Animation>().Stop();
            PlayAnimation(this.AnimationDeath);
            return;
        }

        // if the entity is dead, don't process any more updates. Otherwise, the zombie stands up again after death.
        if (this.entityAlive.IsDead())
            return;


        // If an animation is already playing, we don't need to process it any more until its done.
        if (HasValidAnimationAndEnabled(this.AnimationMainAttack))
            return;
        if (HasValidAnimationAndEnabled(this.AnimationSecondAttack))
            return;
        if (HasValidAnimationAndEnabled(this.AnimationDeath))
            return;
        if (HasValidAnimationAndEnabled(this.AnimationPain))
            return;
        if (this.entityAlive != null)
        {
            if (this.timeSpecialAttackPlaying > 0f)
            {
                // it's playing special attack
                this.timeSpecialAttackPlaying -= Time.deltaTime;
                return;
            }
        }


        float playerDistanceX = 0.0f;
        float playerDistanceZ = 0.0f;
        float encroached = this.lastDistance;


        // Calculates how far away the entity is
        playerDistanceX = Mathf.Abs(this.entityAlive.position.x - this.entityAlive.lastTickPos[0].x) * 6f;
        playerDistanceZ = Mathf.Abs(this.entityAlive.position.z - this.entityAlive.lastTickPos[0].z) * 6f;

        if (!this.entityAlive.isEntityRemote)
        {
            if (Mathf.Abs(playerDistanceX - this.lastPlayerX) > 0.00999999977648258 || Mathf.Abs(playerDistanceZ - this.lastPlayerZ) > 0.00999999977648258)
            {
                encroached = Mathf.Sqrt(playerDistanceX * playerDistanceX + playerDistanceZ * playerDistanceZ);
                this.lastPlayerX = playerDistanceX;
                this.lastPlayerZ = playerDistanceZ;
                this.lastDistance = encroached;
            }
        }
        else if (playerDistanceX <= this.lastPlayerX && playerDistanceZ <= this.lastPlayerZ)
        {
            this.lastPlayerX *= 0.9f;
            this.lastPlayerZ *= 0.9f;
            this.lastDistance *= 0.9f;
        }
        else
        {
            encroached = Mathf.Sqrt((playerDistanceX * playerDistanceX + playerDistanceZ * playerDistanceZ));
            this.lastPlayerX = playerDistanceX;
            this.lastPlayerZ = playerDistanceZ;
            this.lastDistance = encroached;
        }

        if ((this.isAlwaysWalk || encroached > 0.150000005960464))
        {

            if (encroached > 1.0)
            {
                // Since the encroached is above 1, we want the zombie to run if need be, to get to the player faster.
                PlayAnimation(this.AnimationRun);
                this.ModelTransform.GetComponent<Animation>()[this.AnimationRun].speed = encroached;
            }
            else
            {
                PlayAnimation(this.AnimationWalk);
                this.ModelTransform.GetComponent<Animation>()[this.AnimationWalk].speed = encroached * 2f;
            }

            // oh Hal... and Hal Code.....
            if (this.DoesntSeemToDoAnything > 0.0)
                return;
            this.DoesntSeemToDoAnything = 0.3f;
        }

        // if the entity is idle, check to see if it has a second idle animation
        else if (HasValidAnimation(this.AnimationIdle) && HasValidAnimation(this.AnimationSecondIdle))
        {
            // Give it a 50/50 chance of playing either idle animation, if they are available.
            if (UnityEngine.Random.value > 0.5)
                PlayAnimation(this.AnimationIdle);
            else
                PlayAnimation(this.AnimationSecondIdle);
        }
        else
        {
            PlayAnimation(this.AnimationIdle);
        }
    }

    protected override void LateUpdate()
    {
        //base.LateUpdate();
    }

    public new Transform GetActiveModelRoot()
    {
        return this.ModelTransform;
    }

    public override void RemoveLimb(EnumBodyPartHit _bodyPart, bool restoreState)
    {
        //  Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public override void CrippleLimb(EnumBodyPartHit _bodyPart, bool restoreState)
    {
    }

    public override void TurnIntoCrawler(bool restoreState)
    {
        //  Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public override void BeginStun(EnumEntityStunType stun, EnumBodyPartHit _bodyPart, bool _criticalHit, float random)
    {
        //  Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public override void EndStun()
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public override void StartEating()
    {
        //   Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public override void StopEating()
    {
        //  Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public new void PlayPlayerFPRevive()
    {
        //  Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public new void SetArchetypeStance(Archetype.StanceTypes stance)
    {

    }

    // sphereii code
    public override bool IsAnimationElectrocutedPlaying()
    {
        return false;
    }

    public override void StartAnimationElectrocuted()
    {
    }

    public new void TriggerSleeperPose(int pose)
    {
        if (this.anim != null)
        {
            this.anim.SetInteger("SleeperPose", pose);
            this.anim.SetTrigger("SleeperTrigger");
        }
    }

    public new void NotifyAnimatorMove(UnityEngine.Animator test)
    {

    }
}