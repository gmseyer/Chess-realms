using UnityEngine;
using TMPro;
using System.Collections.Generic; 


public class SkillTracker : MonoBehaviour
{
    public static SkillTracker Instance { get; private set; }
    
    [Header("UI References")]
    public TextMeshProUGUI skillHistoryText;  // For full skill history
    public TextMeshProUGUI latestSkillText;   // For latest skill only
    
    private List<string> skillHistory = new List<string>();
    
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
    
    public void LogSkillUsage(string player, string pieceName, string skillName, int cost = 0)
{
    // Get current turn count from Game
    Game game = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<Game>();
    int currentTurn = game != null ? game.turns : 0;
    
    
    string skillLog = $"TURN {currentTurn}: {pieceName} USED {skillName}";
    if (cost > 0)
    {
        skillLog += $" ({cost} SP).";
    }
    
    skillHistory.Add(skillLog);
    UpdateSkillUI(skillLog);

    if (game != null)
    {
        game.AddSkillToHistory();
    }
    
    Debug.Log($"[Skill] {skillLog}");
}
    
    private void UpdateSkillUI(string latestSkill)
    {
        // Update latest skill text
        if (latestSkillText != null)
        {
            latestSkillText.text = latestSkill;
        }
        
        // Update full history text
        if (skillHistoryText != null)
        {
            string historyText = "Skill History:\n";
            foreach (string skill in skillHistory)
            {
                historyText += skill + "\n" + "\n";
            }
            skillHistoryText.text = historyText;
        }
    }
}