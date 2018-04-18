/*
 * Class: EntityZombieSDX
 * Author:  sphereii and HAL9000
 * Category: Entity
 * Description:
 *      This mod is an extension of the base zombie class. It currently enables small features sets, such as different scaling of zombies, subtle differences in speed, etc
 * 
 * Usage:
 *      Add the following class to entities that are meant to use these features.
 *
 *      <property name="Class" value="EntityZombieSDX, Mods" />
 *
 * Features:
 *      This class supports the following features.
 *
 *  Headshots:
 *      This feature enables bonus multipliers to head shots, and reduces damage done to the body.
 *
 *    	  <property name="HeadShots" value="true" />
 *
 * Hand Items:
 *      This class features the ability to give zombies hand items, for ranged weapons and the like, replicating the cop's spit attack.
 *
 *  Random Speeds:
 *      This feature enables the zombies to have subtle speed differences.
 *
 *      This line is added to the base approach speeds, and either slows them down or speeds them up.
 *         float[] numbers = new float[9] {-0.2f, -0.2f, -0.1f, -0.1f, 0.0f, 0.0f, 0.0f, 0.1f, 0.1f };
 *
 * 		  <property name="RandomSpeeds" value="true" />
 *
 *  Random Sizes:
 *      This feature enables the zombies to have slight scaling differences, so they'll appear differently in game.
 *
 *       <property name="RandomSize" value="true" />
 *
 *  Random Walk Types:
 *      This speed randomizes the zombie's walk types, making each spawn have a randomly selected walk type for variety
 *
 *      This is a built-in feature, and does not need any additional XML changes.
 */
using System;
using UnityEngine;
using System.IO;

// Extending HAL9000's Zombies Run In Dark mod by adding speed variation
public class EntityZombieSDX : EntityZombie
{
    // Stores when to do the next light check and what the current light level is
    // Determines if they run or not.
    private float nextCheck = 0;
    byte lightLevel = 10;

    // Globa value for the light threshold in which zombies run in the dark
    public  byte LightThreshold = 5;
    // Frequency of check to determine the light level.
    public  float CheckDelay = 1f;

    public  float WalkTypeDelay = 10f;
    public float NextWalkCheck = 0;

    public  System.Random random = new System.Random();

    // Caching the walk types and approach speed
    private int intWalkType = 0;
    private float flApproachSpeed = 0.0f;
    private bool blHeadShotsMatter = false;
    private bool blRandomSpeeds = false;

    private bool blRandomSize = true;
    // Default value
    private bool blIdleSleep = false;

    // Defualt max and min speed for uninitialized values.
    private float MaxSpeed = 0f;
    private float MinSpeed = 0f;

    private String strCustomUMA = "None";
    // set to true if you want the zombies to run in the dark.
    bool blRunInDark = false;

    // Set the default scale of the zombies
    private float flScale = 1f;
    private Animator anim;

    public override void Init(int _entityClass)
    {

        base.Init(_entityClass);

        EntityClass entityClass = EntityClass.list[_entityClass];

        // If true, reduces body damage, and requires headshots
        if (entityClass.Properties.Values.ContainsKey("HeadShots"))
            bool.TryParse(entityClass.Properties.Values["HeadShots"], out this.blHeadShotsMatter);

        // If true, allows the zombies to move faster or slower
        if (entityClass.Properties.Values.ContainsKey("RandomSpeeds"))
            bool.TryParse(entityClass.Properties.Values["RandomSpeeds"], out this.blRandomSpeeds);

        // If true, puts the zombie to sleep, rather than Idle animation
        if (entityClass.Properties.Values.ContainsKey("IdleSleep"))
            bool.TryParse(entityClass.Properties.Values["IdleSleep"], out this.blIdleSleep);

        if (entityClass.Properties.Values.ContainsKey("RandomSize"))
            bool.TryParse(entityClass.Properties.Values["RandomSize"], out this.blRandomSize);


        // leave a 5% chance of this zombie running in the dark.
        //if (random.Next(100) <= 5)
        //    blRunInDark = true;

     //   GetWalkType();
      //  GetApproachSpeed();

        // Sets the hand value, so we can give our entities ranged weapons.
        this.inventory.SetSlots(new ItemStack[]
        {
               new ItemStack(this.inventory.GetBareHandItemValue(), 1)
        });

        
        // If the idle sleep flag is set, put the zombie to sleep
        if (blIdleSleep)
            SetRandomSleeperPose();

        // If Random sizes set, adjust the scale
        if (this.blRandomSize)
        {
            // This is the distributed random heigh multiplier. Add or adjust values as you see fit. By default, it's just a small adjustment.
            float[] numbers = new float[9] {0.8f, 0.8f, 0.9f, 0.9f, 1.0f, 1.0f, 1.0f, 1.1f, 1.1f};
            int randomIndex = random.Next(0, numbers.Length);
            this.flScale = numbers[randomIndex];

            // scale down the zombies, or upscale them
            this.gameObject.transform.localScale = new Vector3(this.flScale, this.flScale, this.flScale);
        }

    }

    // Overrides the base write to allow the entity to be consistent across the MP servers
    public override void Read(byte version, BinaryReader reader)
    {
        base.Read(version, reader);
        if (reader.BaseStream.Position != reader.BaseStream.Length)
        {
            try
            {
                // We want to read all the special new values here, in the same order in which they are written.
                this.flApproachSpeed = reader.ReadSingle();
                this.intWalkType = reader.ReadInt32();
                this.lastSleeperPose = reader.ReadInt32();
                this.blRunInDark = reader.ReadBoolean();
                this.flScale = reader.ReadSingle();
            }
            catch (Exception ex)
            {
                Debug.Log("Failed to read from stream: " + ex.Message);
            }
        }
    }

    // Overrides the base write to allow the entity to be consistent across the MP servers
    public override void Write(BinaryWriter writer)
    {
        base.Write(writer);
        try
        {
            writer.Write(this.flApproachSpeed);
            writer.Write(this.intWalkType);
            writer.Write(this.lastSleeperPose);
            writer.Write(this.blRunInDark);
            writer.Write(this.flScale);
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to write to stream: " + ex.Message);
        }
    }

    // Randomly assign a sleeper pose for each spawned in zombie, with it weighted to the standing sleeper pose.
    public void SetRandomSleeperPose()
    {
        // We wait for the standing pose, since there's no animation to make them fall down to sleep
        int[] numbers = new int[9] { 0, 1, 2, 3, 4, 5, 5, 5, 5 };
        int randomNumber = random.Next(0, numbers.Length);
        this.lastSleeperPose = numbers[randomNumber];
    }
    // Returns a random walk type for the spawned entity
    public int GetRandomWalkType()
    {

        /**************************
         *  Walk Types - A16.x
         * 1 - female fat, moe
         * 2 - zombieWightFeral, zombieBoe
         * 3 - Arlene
         * 4 - crawler
         * 5 - zombieJoe, Marlene
         * 6 - foot ball player, steve
         * 7 - zombieTemplateMale, business man
         * 8 - spider
         * 9 - zombieBehemoth
         * *****************************/

        // Distribution of Walk Types in an array. Adjust the numbers as you want for distribution. The 9 in the default int[9] indicates how many walk types you've specified.
        int[] numbers = new int[9] { 1, 2, 2, 3, 4, 5, 6, 7, 8 };

        // Randomly generates a number between 0 and the maximum number of elements in the numbers.
        int randomNumber = random.Next(0, numbers.Length);


        // return the randomly selected walk type
        return numbers[randomNumber];
    }


    // Update the Approach speed, and add a randomized speed to it
    public override float GetApproachSpeed()
    {
        // default approach speed of this new class is 0, so if we are already above that, just re-use the value.
        if (flApproachSpeed > 0.0f)
            return flApproachSpeed;

        // Find the default approach speed from the base class to give us a reference.
        float fDefaultSpeed = base.GetApproachSpeed();

        // If random run is disables, return the base speed
        if (!blRandomSpeeds)
            return fDefaultSpeed;

        // if it's greater than 1, just use the base value in the XML. 
        // This would otherwise make the football and wights run even faster than they do now.
        if (fDefaultSpeed > 1.0f)
            return fDefaultSpeed;

        // new way to generate the multiplier to control their speeds
        float[] numbers = new float[9] {-0.2f, -0.2f, -0.1f, -0.1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        int randomIndex = random.Next(0, numbers.Length);

        float fRandomMultiplier = numbers[randomIndex];

        // If the zombies are set never to run, still apply the multiplier, but don't bother doing calulations based on the night speed.
        if (GamePrefs.GetInt(EnumGamePrefs.ZombiesRun) == 1)
        {
            flApproachSpeed = this.speedApproach + fRandomMultiplier;
        }
        else
        {
            // Rnadomize the zombie speeds types If you have the blRunInDark set to true, then it'll randomize it too.
            //if (blRunInDark && this.world.IsDark() || lightLevel < EntityZombieSDX.LightThreshold || this.Health < this.GetMaxHealth() * 0.4)
            //{
            //    flApproachSpeed = this.speedApproachNight + fRandomMultiplier;
            //}
            //else 
            if (this.world.IsDark())
            {
                flApproachSpeed = this.speedApproachNight + fRandomMultiplier;
            }
            else
            {
                flApproachSpeed = this.speedApproach + fRandomMultiplier;
            }
        }

        // If the approach speed is too low, set it to default speed
        if (flApproachSpeed <= 0)
            flApproachSpeed = base.GetApproachSpeed();

        // Cap the top end of the speed to be 1.35 or less, otherwise animations may go wonky.
        return Math.Min(flApproachSpeed, 1.1f);

    }

    // Randomize the Walk types.
    public int GetWalkType()
    {
        // Grab the current walk type in the baes class
        int WalkType = base.GetWalkType();

        // If the WalkType is 4, then just return, since this is the crawler animation
        if (WalkType == 4)
        {
            return WalkType;
        }
        // If the WalkType is greater than the default, then return the already randomized one
        if (intWalkType > 0)
        {
            return intWalkType;
        }
        // Grab a random walk type, and store it for this instance.
        intWalkType = GetRandomWalkType();


        // Grab a random walk type
        return intWalkType;

    }


    //public void OnRandomHitAnimation()
    //{
    //    try
    //    {
    //        int randomNumber = random.Next(0, 4);
    //        Debug.Log("Random Hit Animation");
    //        Animator anim = this.PhysicsTransform.GetComponent<Animator>();
    //        this.anim.SetInteger("HitBodyPart", randomNumber);
    //        // this.anim.SetInteger("MovementState", 0);
    //        Debug.Log("Random hit Animation Done");
    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine(e);
    //        throw;
    //    }
        
    //}

    // Calls the base class, but also does an update on how much light is on the current entity.
    // This only determines if the zombies run in the dark, if enabled.
    public override void OnUpdateLive()
    {
        base.OnUpdateLive();

        
        if (nextCheck < Time.time)
        {
            nextCheck = Time.time + CheckDelay;
            Vector3i v = new Vector3i(this.position);
            if (v.x < 0) v.x -= 1;
            if (v.z < 0) v.z -= 1;
            lightLevel = GameManager.Instance.World.ChunkClusters[0].GetLight(v, Chunk.LIGHT_TYPE.SUN);

          //  OnRandomHitAnimation();

            // If the Idle Sleep flag is set, then we'll do a check to see if the zombie can go to sleep or not.
            if (this.blIdleSleep)
            {
                try
                {
                    // If its not alert, and not already sleeping, put it to sleep.
                    if (!this.IsAlert)
                    {
                        this.ResumeSleeperPose();
                    }

                }
                catch (Exception ex)
                {
                    // No Sleeper code!
                    this.blIdleSleep = false;


                }

            }
        
        }
    }

    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
    {

        if (blHeadShotsMatter)
        {
            EnumBodyPartHit bodyPart = _damageSource.GetEntityDamageBodyPart(this);
            if (bodyPart == EnumBodyPartHit.Head)
            {
                // Apply a damage multiplier for the head shot, and bump the dismember bonus for the head shot
                // This will allow the heads to go explode off, which according to legend, if the only want to truly kill a zombie.
                _damageSource.DamageMultiplier = 1f;
                _damageSource.DismemberBonus = 0.08f;
            }
            // Reducing the damage to the torso will prevent the entity from being killed by torso shots, while also maintaining de-limbing.
            else if (bodyPart == EnumBodyPartHit.Torso)
            {
                _damageSource.DamageMultiplier = 0.1f;
            }

        }

        return base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
    }

    //protected override string GetSoundRandom()
    //{
    //    this.emodel.avatarController.StartTaunt();
    //    return base.GetSoundRandom();
    //}

    /* underground 
     * ------------
     * Use Blight code to make underrground caverns, x, y, z  with random sizes
     * over-ride stone block to allow random growth:
     * Top:  stalactites, cobwebs
     * Bottom: stalactites, moss, water block
     * Random spawn entities, like bats, creatures
     * 
    /**************************** Test Code for reading and loading entity data from an asset bundle
     public void SetCustomTextures()
     {
         if (strCustomUMA != "None ")
         {
             try
             {
                 Debug.Log("Checking Main Texture");

                 Texture2D mainTexture = ResourceWrapper.Load(this.strCustomUMA + "_d", typeof(Texture2D)) as Texture2D;
                 Texture2D Bump = ResourceWrapper.Load(this.strCustomUMA + "_n", typeof(Texture2D)) as Texture2D;
                 Texture2D Spec = ResourceWrapper.Load(this.strCustomUMA + "_s", typeof(Texture2D)) as Texture2D;

                 if (mainTexture == null)
                     Debug.Log("main Text is null");
                 if (Bump == null)
                     Debug.Log("Bump is null ");
                 if (Spec == null)
                     Debug.Log("Light is null ");
                 Debug.Log("Found external texture");

                 Renderer[] componentsInChildren2 = this.GetComponentsInChildren<Renderer>();
                 foreach (var r in componentsInChildren2)
                 {
                     Debug.Log("Found a Renderer: " + r.name.ToString());
                     Debug.Log("MainText was: " + r.material.mainTexture.name.ToString());
                     Debug.Log(r.material.GetTexture("_MainTex").name.ToString());
                     Debug.Log(r.material.GetTexture("_SpecGlossMap").name.ToString());
                     r.material.SetTexture("_MainTex", mainTexture);
                     r.material.SetTexture("_BumpMap", Bump);
                     r.material.SetTexture("_SpecGlossMap", Spec);
                     r.material.mainTexture = mainTexture;
                     Debug.Log("MainText is: " + r.material.mainTexture.name.ToString());
                 }

                 Debug.Log("New Main Texture");
             }
             catch (Exception ex)
             {
                 Debug.Log("Error parsing main texture: " + ex.ToString());
             }
         }
     }
     public void CheckForNull(object source, String strTag)
     {
         if (source == null)
             Debug.Log("This is null: " + strTag);
         else
             Debug.Log("This is NOT null: " + strTag);
     }

     */

}
