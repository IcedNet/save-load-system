using UnityEngine;

public class DebugLogger
{
    public static bool debug = true;
    private readonly string _className;

    public DebugLogger(string className)
    {
        _className = "[" + className + "]";
    }

    public void Log(string message)
    {
        if (DebugLogger.debug)
        {
            Debug.Log(_className + message);
        }
    }

    public void LogError(string message)
    {
        if (DebugLogger.debug)
        {
            Debug.LogError(_className + message);
        }
    }

    public void LogError(System.Exception exception, string message)
    {
        if (DebugLogger.debug)
        {
            Debug.LogError(_className + message);
            Debug.LogException(exception);
        }
    }
}
