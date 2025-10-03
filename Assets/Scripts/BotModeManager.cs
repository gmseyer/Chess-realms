using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages bot mode vs two-player mode
/// Controls input blocking and bot turn handling
/// </summary>
public class BotModeManager : MonoBehaviour
{
    [Header("Mode Settings")]
    public bool botButton = false;
    public bool twoPlayerButton = true; // Default to two-player mode
    
    [Header("UI Buttons")]
    public Button botModeButton; 
    public Button twoPlayerModeButton;
    
    [Header("Debug")]
    public bool debugMode = true;
    
    private ChessBot chessBot;
    
    private void Start()
    {
        // Set up button listeners
        if (botModeButton != null)
            botModeButton.onClick.AddListener(EnableBotMode);
            
        if (twoPlayerModeButton != null)
            twoPlayerModeButton.onClick.AddListener(EnableTwoPlayerMode);
            
        // Initialize default state
        UpdateButtonStates();
        
        // Get chess bot reference
        chessBot = FindObjectOfType<ChessBot>();
        if (chessBot == null)
        {
            Debug.LogError("[BotModeManager] ChessBot not found! Please add ChessBot to the scene.");
        }
    }
    
    /// <summary>
    /// Enable bot mode - black pieces controlled by bot
    /// </summary>
    public void EnableBotMode()
    {
        botButton = true;
        twoPlayerButton = false;
        UpdateButtonStates();
        
        if (debugMode)
            Debug.Log("[BotMode] Bot mode enabled - black pieces will be controlled by bot");
    }
    
    /// <summary>
    /// Enable two-player mode - both players controlled by human
    /// </summary>
    public void EnableTwoPlayerMode()
    {
        botButton = false;
        twoPlayerButton = true;
        UpdateButtonStates();
        
        if (debugMode)
            Debug.Log("[BotMode] Two-player mode enabled - both players controlled by human");
    }
    
    /// <summary>
    /// Update button visual states
    /// </summary>
    private void UpdateButtonStates()
    {
        if (botModeButton != null)
        {
            // Visual feedback - you can customize this
            var colors = botModeButton.colors;
            colors.normalColor = botButton ? Color.green : Color.white;
            botModeButton.colors = colors;
        }
        
        if (twoPlayerModeButton != null)
        {
            // Visual feedback - you can customize this
            var colors = twoPlayerModeButton.colors;
            colors.normalColor = twoPlayerButton ? Color.green : Color.white;
            twoPlayerModeButton.colors = colors;
        }
    }
    
    /// <summary>
    /// Check if bot mode is active
    /// </summary>
    public bool IsBotModeActive()
    {
        return botButton;
    }
    
    /// <summary>
    /// Check if two-player mode is active
    /// </summary>
    public bool IsTwoPlayerModeActive()
    {
        return twoPlayerButton;
    }
    
    /// <summary>
    /// Check if a player should be controlled by bot
    /// </summary>
    public bool ShouldPlayerBeControlledByBot(string player)
    {
        return IsBotModeActive() && player == "black";
    }
    
    /// <summary>
    /// Handle bot turn - called when it's black's turn in bot mode
    /// </summary>
    public void HandleBotTurn()
    {
        if (chessBot != null && IsBotModeActive())
        {
            chessBot.MakeBotMove();
        }
    }
}