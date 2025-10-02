using UnityEngine;

/// <summary>
/// Helper class to make it easy to add UI logging after Debug.Log calls
/// Usage: Debug.Log("message"); UILogger.Log("keyCd", "Cooldown not available");
/// </summary>
public static class DebugHelper
{
    /// <summary>
    /// Logs to console AND UI in one call
    /// </summary>
    /// <param name="keyword">UI category keyword</param>
    /// <param name="consoleMessage">Message for console</param>
    /// <param name="uiMessage">Message for UI (optional, uses consoleMessage if not provided)</param>
    public static void LogBoth(string keyword, string consoleMessage, string uiMessage = null)
    {
        // Log to console
        Debug.Log(consoleMessage);
        
        // Log to UI
        UILogger.Log(keyword, uiMessage ?? consoleMessage);
    }

    /// <summary>
    /// Quick methods for common categories
    /// </summary>
    public static void LogCooldown(string message)
    {
        Debug.Log(message);
        UILogger.Log("keyCd", message);
    }

    public static void LogSkillPoint(string message)
    {
        Debug.Log(message);
        UILogger.Log("keySp", message);
    }

    public static void LogTurn(string message)
    {
        Debug.Log(message);
        UILogger.Log("keyTurn", message);
    }

    public static void LogMove(string message)
    {
        Debug.Log(message);
        UILogger.Log("keyMove", message);
    }
}
