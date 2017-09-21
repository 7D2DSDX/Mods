using System;
using System.Collections.Generic;
using UnityEngine;

// Extending HAL9000's Zombies Run In Dark mod by adding speed variation
public class EntityZombieSDX: EntityZombie
    {
    // Stores when to do the next light check and what the current light level is
    // Determines if they run or not.
    private float nextCheck = 0;
    byte lightLevel;

    // Globa value for the light threshold in which zombies run in the dark
    public static byte LightThreshold = 10;

    // Frequency of check to determine the light level.
    public static float CheckDelay = 1f;

    public static System.Random random = new System.Random();

    // Caching the walk types and approach speed
    private int intWalkType = 0;
    private float flApproachSpeed = 0.0f;
    private float flEyeHeight = 0.0f;
    // set to true if you want the zombies to run in the dark.
    bool blRunInDark = false;

    // Returns a random walk type for the spawned entity
    public static int GetRandomWalkType()
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
        int[] numbers = new int[9] { 1,2,2,3,4,5,6,7,8 };

        // Randomly generates a number between 0 and the maximum number of elements in the numbers.
        int randomNumber = random.Next(0, numbers.Length);

    
        // return the randomly selected walk type
        return numbers[randomNumber];
    }

   
    // Adding a bit of variety in the height of the zombies.
    public override float GetEyeHeight()
    {
        // default eye height is 0.0 for this class, so if it's anything different, just return that value, since we've already set it once.
        if (flEyeHeight > 0.0f)
            return flEyeHeight;

        // Grab the base Eye Heigth;
        flEyeHeight = base.GetEyeHeight();
        // If the walk types are 4 ( crawler) or 8 ( spider ), skip their heigh change
        if (GetWalkType() == 4 || GetWalkType() == 8)
        {
            return flEyeHeight;
        }


        // This is the distributed random heigh multiplier. Add or adjust values as you see fit. By default, it's just a small adjustment.
        float[] numbers = new float[9] { 0.5f,0.6f,0.7f, 0.7f, 0.8f, 0.8f, 0.9f, 0.9f, 1.0f };
        float randomNumber = random.Next(0, numbers.Length);
    
        // We'll cache the random eye height, so there's not contstant adjustments.
        flEyeHeight = (!this.IsCrouching) ? (base.height * randomNumber) : (base.height * 0.5f);
        return flEyeHeight;
    }

    // Update the Approach speed, and add a randomized speed to it
    public override float GetApproachSpeed()
    {
        // default approach speed of this new class is 0, so if we are already above that, just re-use the value.
        if (flApproachSpeed > 0.0f)
            return flApproachSpeed;

        // Find the default approach speed from the base class to give us a reference.
        float fDefaultSpeed = base.GetApproachSpeed();

        // Grabs a random multiplier for the speed
        float fRandomMultiplier = UnityEngine.Random.Range(0.0f, 0.5f);

        // if it's greater than 1, just use the base value in the XML. 
        // This would otherwise make the football and wights run even faster than they do now.
        if (fDefaultSpeed > 1.0f)
            return fDefaultSpeed;

        // If the zombies are set never to run, still apply the multiplier, but don't bother doing calulations based on the night speed.
        if (GamePrefs.GetInt(EnumGamePrefs.ZombiesRun) == 1)
            flApproachSpeed = this.speedApproach + fRandomMultiplier;
        else
        {
            // Rnadomize the zombie speeds types If you have the blRunInDark set to true, then it'll randomize it too.
            if (blRunInDark && this.world.IsDark() || lightLevel < LightThreshold || this.Health < this.GetMaxHealth() * 0.4)
                flApproachSpeed = this.speedApproachNight + fRandomMultiplier;

            // If it's night time, then use the speedApproachNight value
            if (this.world.IsDark())
                flApproachSpeed = this.speedApproachNight + fRandomMultiplier;
            else
                flApproachSpeed = this.speedApproach + fRandomMultiplier;
        }

        return flApproachSpeed;
    }
    // Randomize the Walk types.
    public override int GetWalkType()
    {
        // Grab the current walk type in the baes class
        int WalkType = base.GetWalkType();

        // If the WalkType is 4, then just return, since this is the crawler animation
        if (WalkType == 4)
            return WalkType;

        // If the WalkType is greater than the default, then return the already randomized one
        if (intWalkType > 0)
            return intWalkType;

        // Grab a random walk type, and store it for this instance.
        intWalkType = EntityZombieSDX.GetRandomWalkType();

        // Add another randomess twist for a crouching zombie, by default, it'll be a 1 in 10 chance of crouching.
        // Reducer the number range if you want them to be more abundant. The default 3 for selected crouch is just randomly selected.
        int randomCrouch = random.Next(0, 10);
        if (randomCrouch == 3)
            Crouching = true;

        // Grab a random walk type
        return intWalkType;

    }

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
        }

    }

}

