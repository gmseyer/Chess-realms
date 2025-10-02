using UnityEngine;
using TMPro;

/// <summary>
/// UI Logger system that displays categorized debug messages on UI panels
/// Usage: UILogger.Log("keyCd", "Cooldown not available");
/// </summary>
public class UILogger : MonoBehaviour
{
    public static UILogger Instance { get; private set; }

    [Header("UI Text Panels")]
    public TMP_Text cooldownPanel;      // For keyCd messages
    public TMP_Text skillPointPanel;    // For keySp messages  
    public TMP_Text turnPanel;          // For keyTurn messages
    public TMP_Text movePanel;          // For keyMove messages

    [Header("Display Settings")]
    public float messageDisplayTime = 3f; // How long each message stays visible
    public bool showTimestamp = true;     // Show time with message

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Main logging function - call this after Debug.Log
    /// </summary>
    /// <param name="keyword">Category keyword (keyCd, keySp, keyTurn, keyMove)</param>
    /// <param name="message">Message to display</param>
    public static void Log(string keyword, string message)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[UILogger] No instance found! Make sure UILogger is in the scene.");
            return;
        }

        Instance.DisplayMessage(keyword, message);
    }

    /// <summary>
    /// Displays a message on the appropriate panel based on keyword
    /// </summary>
    private void DisplayMessage(string keyword, string message)
    {
        TMP_Text targetPanel = GetPanelForKeyword(keyword);
        if (targetPanel == null)
        {
            Debug.LogWarning($"[UILogger] No panel found for keyword: {keyword}");
            return;
        }

        // Format message with timestamp if enabled
        string formattedMessage = FormatMessage(message);
        
        // Display message
        targetPanel.text = formattedMessage;
        
        // Clear message after display time
        StopAllCoroutines();
        StartCoroutine(ClearMessageAfterDelay(targetPanel, messageDisplayTime));
    }

    /// <summary>
    /// Gets the appropriate UI panel for a keyword
    /// </summary>
    private TMP_Text GetPanelForKeyword(string keyword)
    {
        switch (keyword.ToLower())
        {
            case "keycd":
                return cooldownPanel;
            case "keysp":
                return skillPointPanel;
            case "keyturn":
                return turnPanel;
            case "keymove":
                return movePanel;
            default:
                Debug.LogWarning($"[UILogger] Unknown keyword: {keyword}");
                return null;
        }
    }

    /// <summary>
    /// Formats the message with optional timestamp
    /// </summary>
    private string FormatMessage(string message)
    {
        
        return message;
    }

    /// <summary>
    /// Clears the message after a delay
    /// </summary>
    private System.Collections.IEnumerator ClearMessageAfterDelay(TMP_Text panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null)
        {
            panel.text = "";
        }
    }

    /// <summary>
    /// Manually clear all panels
    /// </summary>
    public static void ClearAll()
    {
        if (Instance == null) return;

        if (Instance.cooldownPanel != null) Instance.cooldownPanel.text = "";
        if (Instance.skillPointPanel != null) Instance.skillPointPanel.text = "";
        if (Instance.turnPanel != null) Instance.turnPanel.text = "";
        if (Instance.movePanel != null) Instance.movePanel.text = "";
    }

    /// <summary>
    /// Clear a specific panel by keyword
    /// </summary>
    public static void Clear(string keyword)
    {
        if (Instance == null) return;

        TMP_Text panel = Instance.GetPanelForKeyword(keyword);
        if (panel != null)
        {
            panel.text = "";
        }
    }
}
