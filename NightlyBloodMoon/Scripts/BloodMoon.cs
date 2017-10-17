using System;

public class MyBloodMoon : SkyManager
{
  
    public static bool NightlyBloodMoon()
    {
        return SkyManager.TimeOfDay() >= 18f || SkyManager.TimeOfDay() <= 6f;
    }
  
}