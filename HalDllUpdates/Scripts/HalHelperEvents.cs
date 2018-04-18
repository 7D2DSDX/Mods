using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
public static class HalHelperEvents
{

    private static Dictionary<string, Func<object, bool>> Actions = new Dictionary<string, Func<object, bool>>();

    public static void ClearEvents()
    {
        Actions.Clear();
    }
    public static void RegisterEvent(string Name, Func<object, bool> action)
    {

        if (Actions.ContainsKey(Name))
        {
            Debug.Log("Overwriting HalHelper action " + Name);
            Actions.Remove(Name);
        }
        Actions.Add(Name, action);
    }

    public static bool RunEvent(string Name, object param)
    {

        if (!Actions.ContainsKey(Name))
            return false;

        return Actions[Name](param);
    }

}

