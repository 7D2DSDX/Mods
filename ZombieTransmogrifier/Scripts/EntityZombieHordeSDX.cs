/*
 *
 *      Test Class, Please Ignore
 *
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using XMLData.Item;
using SDX.Payload;
using UMA;
using System.IO;

// Extending HAL9000's Zombies Run In Dark mod by adding speed variation
public class EntityZombieHordeSDX : EntityZombie
{
    private bool blRandomSpeeds = true;

    // Defualt max and min speed for uninitialized values.
    private float MaxSpeed = 0f;
    private float MinSpeed = 0f;
    private float flApproachSpeed = 0.0f;
    public static System.Random random = new System.Random();

    private Animator anim;

    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
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
        float[] numbers = new float[9] {-0.2f, -0.2f, -0.1f, -0.1f, 0.0f, 0.0f, 0.0f, 0.1f, 0.1f };
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


    // Calls the base class, but also does an update on how much light is on the current entity.
    // This only determines if the zombies run in the dark, if enabled.
    public override void OnUpdateLive()
    {
        base.OnUpdateLive();
        
    }

}
