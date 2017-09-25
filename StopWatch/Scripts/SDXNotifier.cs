using System;
public class SDXNotifier
{
    public static Action<string> Enter;
    public static Action<string> Exit;
    public static Action<string> JumpOut;
    public static Action<string> JumpBack;

    public static System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

    public static void NotifyEnter(string methodName)
    {
        Console.WriteLine("Entering: " + methodName);
        watch.Start();
        if (Enter != null)
        {
            Enter(methodName);
        }
    }

    public static void NotifyExit(string methodName)
    {
        Console.WriteLine("Exiting: " + methodName);
        watch.Stop();
        TimeSpan ts = watch.Elapsed;

        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
        Console.WriteLine("Total Elapsed Time in " + methodName + ": " + elapsedTime);
        watch.Reset();
        if (Exit != null)
        {
            Exit(methodName);
        }
    }

    public static void NotifyJumpOut(string methodName)
    {
        Console.WriteLine("  -> Calling " + methodName );
        if (JumpOut != null)
        {
            JumpOut(methodName);

        }
    }

    public static void NotifyJumpBack(string methodName)
    {
        Console.WriteLine("  <- Returning From " + methodName);
        if (JumpBack != null)
        {
            JumpBack(methodName);
        }
    }
}