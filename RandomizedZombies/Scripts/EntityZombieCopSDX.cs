using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityZombieCopSDX : EntityZombieCop
{
    private float nextCheck = 0;
    byte lightLevel;

    // set to true if you want the zombies to run in the dark.
    bool blRunInDark = false;

    // Update the Approach speed, and add a randomized speed to it
    public override float GetApproachSpeed()
    {
        // Find the default approach speed
        float fDefaultSpeed = base.GetApproachSpeed();

        // Grabs a random multiplier for the speed, with 0.2 being low, and 0.5 being high.
        float fRandomMultiplier = UnityEngine.Random.Range(0.2f, 0.5f);

        // if it's greater than 1, just use that value. 
        // This would make the football and wights run even faster than they do now.
        if (fDefaultSpeed > 1.0f)
            return fDefaultSpeed;

        if (GamePrefs.GetInt(EnumGamePrefs.ZombiesRun) == 1)
            return this.speedApproach * UnityEngine.Random.Range(0.8f, 1.2f);
        else
        {
            // Rnadomize the zombie speeds types If you have the blRunInDark set to true, then it'll randomize it too.
            if (blRunInDark && this.world.IsDark() || lightLevel < EntityZombieSDX.LightThreshold || this.Health < this.GetMaxHealth() * 0.4)
                return this.speedApproachNight * fRandomMultiplier;

            // If it's night time, then use the speedApproachNight value
            if (this.world.IsDark())
                return this.speedApproachNight * fRandomMultiplier;
            else
                return this.speedApproach * fRandomMultiplier;
        }
    }

    // Randomize the Walk types.
    public override int GetWalkType()
    {
        // Grab the current walk type in the baes class
        int WalkType = base.GetWalkType();

        // If the WalkType is 4, then just return, since this is the crawler animation
        if (WalkType == 4)
            return WalkType;

        return EntityZombieSDX.GetRandomWalkType();

    }

    public override void OnUpdateLive()
    {
        base.OnUpdateLive();

        if (nextCheck < Time.time)
        {
            nextCheck = Time.time + EntityZombieSDX.CheckDelay;
            Vector3i v = new Vector3i(this.position);
            if (v.x < 0) v.x -= 1;
            if (v.z < 0) v.z -= 1;
            lightLevel = GameManager.Instance.World.ChunkClusters[0].GetLight(v, Chunk.LIGHT_TYPE.SUN);
        }

    }

}

