using UnityEngine;

public class Logger
{
    [System.Diagnostics.Conditional("DEBUG")]
    public static void Log(string message)
    {
        Debug.Log(message);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void LogError(string message)
    {
        Debug.LogError(message);
    }
}