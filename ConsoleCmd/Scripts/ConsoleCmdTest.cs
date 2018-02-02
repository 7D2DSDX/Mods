using System.Collections.Generic;
using UnityEngine;

public class ConsoleCmdTest : ConsoleCmdAbstract
{
  public override bool IsExecuteOnClient
  {
    get { return true; }
  }

  public override string[] GetCommands()
  {
    return new []
    {
      "test"
    };
  }

  public override string GetDescription()
  {
    return "This is a test";
  }

  public override string GetHelp()
  {
    return "Help? We don't give help!";
  }

  public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
  {
    Debug.Log("WARNING WARNING: This is not a drill.... oh wait, yes it is.");

    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("WARNING WARNING: This is not a drill.... oh wait, yes it is.");
  }
}
