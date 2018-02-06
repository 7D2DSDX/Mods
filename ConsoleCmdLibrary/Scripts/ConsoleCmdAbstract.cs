using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Commands
{
  public abstract class BCCommandAbstract : ConsoleCmdAbstract
  {
    public static CommandSenderInfo SenderInfo;
    public static List<string> Params = new List<string>();
    public static Dictionary<string, string> Options = new Dictionary<string, string>();

    public override bool IsExecuteOnClient
    {
      get { return true; }
    }

    public override string[] GetCommands()
    {
      return new[] { "" };
    }

    public override string GetDescription()
    {
      return "";
    }

    public override void Execute(List<string> _params, CommandSenderInfo senderInfo)
    {
      if (!senderInfo.IsLocalGame)
      {
        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients");

        return;
      }

      SenderInfo = senderInfo;
      Params = new List<string>();
      Options = new Dictionary<string, string>();
      ParseParams(_params);
      try
      {
        Process();
      }
      catch (Exception e)
      {
        SendOutput("Error while executing command.");
        Log.Out("Error in " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name + ": " + e);
      }
    }

    public virtual void Process(Dictionary<string, string> o, List<string> p)
    {
      Options = o;
      Params = p;
      Process();
    }

    public virtual void Process()
    {
      // function to override in extention commands instead of Execute
      // this allows param parsing and exception handling to be done in this class
    }

    private static void ParseParams(List<string> _params)
    {
      foreach (var param in _params)
      {
        if (param.IndexOf('/', 0) != 0)
        {
          Params.Add(param);
        }
        else
        {
          if (param.IndexOf('=', 1) == -1)
          {
            Options.Add(param.Substring(1).ToLower(), null);
          }
          else
          {
            var p1 = param.Substring(1).Split('=');
            Options.Add(p1[0] == "f" ? "filter" : p1[0], p1[1]);
          }
        }
      }
    }

    public static void SendOutput(string output)
    {
      if (Options.ContainsKey("log"))
      {
        Log.Out(output);

        return;
      }

      if (Options.ContainsKey("chat"))
      {
        if (Options.ContainsKey("color"))
        {
          output = "[" + Options["color"] + "]" + output + "[-]";
        }
        foreach (var text in output.Split('\n'))
        {
          GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, text, "Server", false, string.Empty, false);
        }

        return;
      }

      if (ThreadManager.IsMainThread())
      {
        foreach (var text in output.Split('\n'))
        {
          SingletonMonoBehaviour<SdtdConsole>.Instance.Output(text);
        }

        return;
      }

      if (SenderInfo.RemoteClientInfo != null)
      {
        foreach (var text in output.Split('\n'))
        {
          SenderInfo.RemoteClientInfo.SendPackage(new NetPackageConsoleCmdClient(text, false));
        }

        return;
      }

      if (SenderInfo.NetworkConnection != null && SenderInfo.NetworkConnection is TelnetConnection)
      {
        foreach (var text in output.Split('\n'))
        {
          SenderInfo.NetworkConnection.SendLine(text);
        }

        return;
      }

      Log.Out(output);
    }

    public static Vector3i Vector3FloorToInt(Vector3 v)
    {
      return new Vector3i((int)Math.Floor(v.x), (int)Math.Floor(v.y), (int)Math.Floor(v.z));
    }
  }
}
