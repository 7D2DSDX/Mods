// Decompiled with JetBrains decompiler
// Type: GameObjectAnimalAnimation
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7B66E2A5-B854-4FE4-983E-8D0DC9B247E0
// Assembly location: C:\Games\Steam\steamapps\common\7 Days To Die SDX\7DaysToDie_Data\Managed\Assembly-CSharp.dll
// Compiler-generated code is shown

using System;
using UnityEngine;

public class GameObjectAnimalAnimationSDX : MonoBehaviour, IAvatarController
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

    private string strModelName = "";

    private EntityAlive entityAlive;
    private bool IsVisible;
    private bool HasDied;
    private bool isAlwaysWalk;
    private Transform ModelTransform;
    private Transform GraphicsTransform;
    private float lastPlayerX;
    private float lastPlayerZ;
    private float lastDistance;
    private float DoesntSeemToDoAnything;

    private bool blDisplayLog = true;
    private void Log( String strLog )
    {
        if ( blDisplayLog)
        {
            Debug.Log(strLog);
        }
    }
    public GameObjectAnimalAnimationSDX()
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

        this.IsVisible = true;
    }

    void Awake()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        try
        {
           
           this.GraphicsTransform = this.transform.Find("Graphics");
			
			if ( this.GraphicsTransform == null )
				Log(" !! Graphics Transform null!" );
            
            this.ModelTransform = this.GraphicsTransform.Find("Model").GetChild(0);
			if ( this.ModelTransform == null )
				Log(" !! Model Transform is null!" );

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
            if ((this.ModelTransform.GetComponent<Animation>()[ this.AnimationIdle]) == null)
                return;


            this.ModelTransform.GetComponent<Animation>().Play(this.AnimationIdle);
            Log("Playing Animation");
        }
        catch( Exception  ex)
        {
            Log("Exception thrown in Awake() " + ex.ToString());
        }
    }


    private void AddTransformRefs(Transform t)
    {
      //  Log("Checking " + t.name + " tag " + t.tag);
        if (t.GetComponent<Collider>() != null && t.GetComponent<RootTransformRefEntity>() == null)
        {
            RootTransformRefEntity root = t.gameObject.AddComponent<RootTransformRefEntity>();
            root.RootTransform = this.transform;
         //   Log("Added root ref on " + t.name + " tag " + t.tag); 
        }
        foreach (Transform tran in t)
        {
            AddTransformRefs(tran);
        }
    }

    void AddTagRecursively(Transform trans, string tag)
    {
        trans.gameObject.tag = tag;
        foreach (Transform t in trans)
            AddTagRecursively(t, tag);
    }

    public void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void SetAlwaysWalk(bool _b)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        this.isAlwaysWalk = _b;
    }

    public Texture2D SetSkinTexture(Texture2D _newTexture, bool _bMakeACopy)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return null;
    }

    public Texture2D GetTexture()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return null;
    }

    public void SetInRightHand(Transform _transform)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void SetDrunk(float _numBeers)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public virtual void SetMinibikeAnimation(string _anim, float _amount)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public virtual void SetMinibikeAnimation(string _anim, bool _isPlaying)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public bool IsAnimationAttackPlaying()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        if (this.ModelTransform.GetComponent<Animation>()[this.AnimationMainAttack] != null && this.ModelTransform.GetComponent<Animation>()[this.AnimationMainAttack].enabled)
            return true;
        if (this.ModelTransform.GetComponent<Animation>()[this.AnimationSecondAttack] != null)
            return this.ModelTransform.GetComponent<Animation>()[this.AnimationSecondAttack].enabled;
        return false;
    }

    public void StartAnimationAttack()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

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

    public bool IsAnimationSpecialAttackPlaying()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public void StartAnimationSpecialAttack(bool _b)
    {
        if (_b)
        {
            if (this.ModelTransform.GetComponent<Animation>()[this.AnimationSpecialAttack] != null)
                this.ModelTransform.GetComponent<Animation>().Play(this.AnimationSpecialAttack);
        }
    }

    public void StartAnimationSpecialAttack()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public virtual bool IsAnimationSpecialAttack2Playing()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public virtual void StartAnimationSpecialAttack2()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public bool IsAnimationRagingPlaying()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public void StartAnimationRaging()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public bool IsAnimationHarvestingPlaying()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public void StartAnimationHarvesting()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void SetAnimationClimbing(bool _b)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public Transform GetFirstPersonCameraTransform()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return null;
    }

    public Transform GetRightHandTransform()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return null;
    }

    public bool IsAnimationUsePlaying()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public void StartAnimationUse()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void SetRagdollEnabled(bool _b)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void StartAnimationFiring()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void StartAnimationReloading()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float random)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name + " hit: "+ _bodyPart.ToString());
        if (this.HasDied || !(bool)(this.ModelTransform.GetComponent<Animation>()[this.AnimationPain]))
            return;
        this.ModelTransform.GetComponent<Animation>().Play(this.AnimationPain);
    }

    public bool IsAnimationHitRunning()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        return false;
    }

    public void StartAnimationJumping()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (!(this.ModelTransform.GetComponent<Animation>()[this.AnimationJump] != null))
            return;
        this.ModelTransform.GetComponent<Animation>().Play(this.AnimationJump);
    }

    public void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetAlive()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetLookPosition(Vector3 _pos)
    {
       //// Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetVisible(bool _b)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (_b == this.IsVisible && _b == this.GraphicsTransform.gameObject.activeSelf)
            return;
        this.IsVisible = _b;
        this.GraphicsTransform.gameObject.SetActive(_b);

    }

    public void SetWalkingSpeed(float _f)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetHeadAngles(float _nick, float _yaw)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetArmsAngles(float _rightArmAngle, float _leftArmAngle)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetAiming(bool _bEnable)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetCrouching(bool _bEnable)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetFPV(bool _fpv)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    // Helper class to check if the animation is valid or not.
    public bool HasValidAnimation( String strAnimation )
    {
        if (this.ModelTransform.GetComponent<Animation>()[strAnimation] != null)
        {
            Log(strAnimation + " is Valid");
            return true;
        }

        Log(strAnimation + " is NOT Valid");
        return false;
    }

    public bool HasValidAnimationAndEnabled( String strAnimation )
    {
        if (HasValidAnimation(strAnimation) && this.ModelTransform.GetComponent<Animation>()[strAnimation].enabled)
            return true;

        return false;
    }

    public void PlayAnimation( String strAnimation )
    {
        if ( HasValidAnimation( strAnimation ))
        {
            // We only want to play the animation when its not already enabled.
            if ( !this.ModelTransform.GetComponent<Animation>()[ strAnimation].enabled)
                this.ModelTransform.GetComponent<Animation>().Play(strAnimation);
        }
    }
    protected void Update()
    {
        if (!this.IsVisible)
            return;

        if (this.entityAlive != null && this.entityAlive.IsDead() && !this.HasDied)
        {
            Log("Update: Entity is Dead");
            this.HasDied = true;
            this.ModelTransform.GetComponent<Animation>().Stop();
            PlayAnimation(this.AnimationDeath);
            return;
        }

        // if the entity is dead, don't process any more updates. Otherwise, the zombie stands up again after death.
        if (this.entityAlive.IsDead())
            return;


        Log("Update: Checking if any animations are playing");
        // If an animation is already playing, we don't need to process it any more until its done.
        if (HasValidAnimationAndEnabled(this.AnimationMainAttack))
            return;
        if (HasValidAnimationAndEnabled(this.AnimationSecondAttack)) 
            return;
        if (HasValidAnimationAndEnabled(this.AnimationDeath))
            return;
        if (HasValidAnimationAndEnabled(this.AnimationPain))
            return;

        float playerDistanceX = 0.0f;
        float playerDistanceZ = 0.0f;
        float encroached = this.lastDistance;

        if (this.entityAlive != null)
        {

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
        }

        if (this.entityAlive != null && (this.isAlwaysWalk || encroached > 0.150000005960464))
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
        else if ( HasValidAnimation( this.AnimationIdle ) && HasValidAnimation( this.AnimationSecondIdle) )
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

    public Transform GetActiveModelRoot()
    {
        return this.ModelTransform;
    }

    public void RemoveLimb(EnumBodyPartHit _bodyPart, bool restoreState)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void CrippleLimb(EnumBodyPartHit _bodyPart, bool restoreState)
    {
    }

    public void TurnIntoCrawler(bool restoreState)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void BeginStun(EnumEntityStunType stun, EnumBodyPartHit _bodyPart, bool _criticalHit, float random)
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void EndStun()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void StartEating()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void StopEating()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void PlayPlayerFPRevive()
    {
       // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void SetArchetypeStance(Archetype.StanceTypes stance)
    {

    }

    // sphereii code
    public bool IsAnimationElectrocutedPlaying()
    {
        return false;
    }

    public void StartAnimationElectrocuted()
    {
    }

    public void TriggerSleeperPose(int pose)
    {

    }

    public void NotifyAnimatorMove(UnityEngine.Animator test)
    {

    }
}
