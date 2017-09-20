using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityZombieCrawlSDX : EntityZombieCrawl
{
    private float nextCheck = 0;
    byte lightLevel;

    // set to true if you want the zombies to run in the dark.
    bool blRunInDark = false;

    // Update the Approach speed, and add a randomized speed to it
    public override float GetApproachSpeed()
    {

        if (GamePrefs.GetInt(EnumGamePrefs.ZombiesRun) == 1)
        {
            return this.speedApproach * UnityEngine.Random.Range(0.8f, 1.2f); ;
        }
        else
        {
            // Rnadomize the zombie speeds types If you have the blRunInDark set to true, then it'll randomize it too.
            if (blRunInDark && this.world.IsDark() || lightLevel < EntityZombieSDX.LightThreshold || this.Health < this.GetMaxHealth() * 0.4)
            {
                return this.speedApproachNight * UnityEngine.Random.Range(0.8f, 1.2f);
            }
            if (this.world.IsDark())  // Make them faster for a base run at night.
                return this.speedApproachNight * UnityEngine.Random.Range(0.8f, 1.2f);
            else
                return this.speedApproach * UnityEngine.Random.Range(0.2f, 1.4f);
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

        // Randomize the Walk Type, between 1 and 8 
        WalkType = UnityEngine.Random.Range(1, 8);

        // If the Walk Type is randomized to 4, then just return the 2nd walk type.
        if (WalkType == 4)
            return 2;

        return WalkType;
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

