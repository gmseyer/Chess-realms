using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum SkillType
{
    Fortify,
    HealingBenediction,
    // Add more skills here
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("Skill Points")]
    public int whiteSkillPoints = 4;
    public int blackSkillPoints = 4;

    [Header("UI References")]
    public Text whiteSPText;  // Drag UI Text from Inspector
    public Text blackSPText;  // Drag UI Text from Inspector

    [Header("Cooldowns")]
    private Dictionary<string, Dictionary<SkillType, int>> cooldowns =
        new Dictionary<string, Dictionary<SkillType, int>>()
        {
            { "white", new Dictionary<SkillType, int>() },
            { "black", new Dictionary<SkillType, int>() }
        };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            UpdateSPUI(); // initialize text at start
        }
        else Destroy(gameObject);
    }

    public int GetPlayerSP(string player)
    {
        return (player == "white") ? whiteSkillPoints : blackSkillPoints;
    }

    public bool SpendPlayerSP(string player, int amount)
    {
        if (player == "white")
        {
            if (whiteSkillPoints >= amount)
            {
                whiteSkillPoints -= amount;
                Debug.Log($"[SkillManager] {player} spent {amount} SP. Remaining: {whiteSkillPoints}");
                UpdateSPUI();
                return true;
            }
            return false;
        }
        else
        {
            if (blackSkillPoints >= amount)
            {
                blackSkillPoints -= amount;
                Debug.Log($"[SkillManager] {player} spent {amount} SP. Remaining: {blackSkillPoints}");
                UpdateSPUI();
                return true;
            }
            return false;
        }
    }

    public void AddPlayerSP(string player, int amount)
    {
        if (player == "white")
            whiteSkillPoints += amount;
        else
            blackSkillPoints += amount;

        Debug.Log($"[SkillManager] {player} gained {amount} SP. Now: {GetPlayerSP(player)}");
        UpdateSPUI();
    }

    // ✅ Cooldown handling
    public bool IsSkillOnCooldown(string player, SkillType skill)
    {
        if (cooldowns[player].ContainsKey(skill))
            return cooldowns[player][skill] > Game.Instance.turns;
        return false;
    }

    public void StartCooldown(string player, SkillType skill, int duration)
    {
        int endTurn = Game.Instance.turns + duration;
        cooldowns[player][skill] = endTurn;
        Debug.Log($"[SkillManager] {skill} on {player} is now on cooldown until turn {endTurn}");
    }

    // ✅ Updates UI live
    private void UpdateSPUI()
    {
        if (whiteSPText != null)
            whiteSPText.text = $"White SP: {whiteSkillPoints}";
        if (blackSPText != null)
            blackSPText.text = $"Black SP: {blackSkillPoints}";
    }
}
