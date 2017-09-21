using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityZombieCrawlSDX : EntityZombieCrawl
{
    private float nextCheck = 0;
    byte lightLevel;

    // Caching the walk types and approach speed
    private int intWalkType = 0;
    private float flApproachSpeed = 0.0f;

    // set to true if you want the zombies to run in the dark.
    bool blRunInDark = false;

    // Update the Approach speed, and add a randomized speed to it
    public override float GetApproachSpeed()
    {
        // default approach speed is 0, so if we are already above that, just re-use the value.
        if (this.flApproachSpeed > 0.0f)
            return this.flApproachSpeed;

        // Find the default approach speed
        float fDefaultSpeed = base.GetApproachSpeed();

        // Grabs a random multiplier for the speed
        float fRandomMultiplier = UnityEngine.Random.Range(0.0f, 0.5f);

        // if it's greater than 1, just use that value. 
        // This would make the football and wights run even faster than they do now.
        if (fDefaultSpeed > 1.0f)
            return fDefaultSpeed;

        if (GamePrefs.GetInt(EnumGamePrefs.ZombiesRun) == 1)
            flApproachSpeed = this.speedApproach + fRandomMultiplier;
        else
        {
            // Rnadomize the zombie speeds types If you have the blRunInDark set to true, then it'll randomize it too.
            if (blRunInDark && this.world.IsDark() || lightLevel < EntityZombieSDX.LightThreshold || this.Health < this.GetMaxHealth() * 0.4)
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
        if (this.intWalkType > 0)
            return this.intWalkType;

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

