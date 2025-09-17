using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public enum SkillType
{
    Fortify, //Rook 1st skill
    HealingBenediction, //Bishop 1st skill 
    LunarLeap, // Knight 1st skill
    CelestialSummon //Bishop 2nd skill
     
    // Add more skills here
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("Skill Points")]
    public int whiteSkillPoints = 5;
    public int blackSkillPoints = 5;

    [Header("UI References")]
   public TextMeshProUGUI whiteSPText;  // Drag UI Text from Inspector
   public TextMeshProUGUI blackSPText;  // Drag UI Text from Inspector

    [Header("Cooldowns")]
    private Dictionary<string, Dictionary<SkillType, int>> cooldowns =
        new Dictionary<string, Dictionary<SkillType, int>>()
        {
            { "white", new Dictionary<SkillType, int>() },
            { "black", new Dictionary<SkillType, int>() }
        };

    [Header("Skill Point Gems")]
    public List<Image> whiteGems; // assign 5 gems in Inspector
    public List<Image> blackGems;


    [Header("Not Enough SP Panel")]
public GameObject notEnoughSPPanel; // assign in Inspector
public float spPanelDuration = 1f;  // duration to show panel
public float spPanelFadeDuration = 0.3f; // fade in/out duration



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
        else
        {
            ShowNotEnoughSPPanel();
            return false;
        }
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
        else
        {
            ShowNotEnoughSPPanel();
            return false;
        }
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

    // Update gems for visual feedback
    UpdateGemOpacity("white");
    UpdateGemOpacity("black");
}



    private void UpdateGemOpacity(string player)
{
    int sp = GetPlayerSP(player);
    List<Image> gems = (player == "white") ? whiteGems : blackGems;

    for (int i = 0; i < gems.Count; i++)
    {
        if (gems[i] != null)
        {
            float targetAlpha = (i < sp) ? 1f : 0.3f;

            // Only start fade if alpha is different
            if (!Mathf.Approximately(gems[i].color.a, targetAlpha))
            {
                StartCoroutine(FadeGem(gems[i], targetAlpha, 0.3f));
            }
        }
    }
}

private System.Collections.IEnumerator FadeGem(Image gem, float targetAlpha, float duration)
{
    Color c = gem.color;
    float startAlpha = c.a;
    float timer = 0f;

    while (timer < duration)
    {
        timer += Time.deltaTime;
        c.a = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
        gem.color = c;
        yield return null;
    }

    c.a = targetAlpha;
    gem.color = c;
}


private void ShowNotEnoughSPPanel()
{
    if (notEnoughSPPanel != null)
    {
        notEnoughSPPanel.SetActive(true);

        // Ensure the panel has CanvasGroup for alpha fading
        CanvasGroup cg = notEnoughSPPanel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = notEnoughSPPanel.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        StopCoroutine("FadeSPPanelCoroutine");
        StartCoroutine(FadeSPPanelCoroutine(cg));
    }
}

private System.Collections.IEnumerator FadeSPPanelCoroutine(CanvasGroup cg)
{
    // Fade in
    float timer = 0f;
    while (timer < spPanelFadeDuration)
    {
        timer += Time.deltaTime;
        cg.alpha = Mathf.Lerp(0f, 1f, timer / spPanelFadeDuration);
        yield return null;
    }
    cg.alpha = 1f;

    // Wait for duration
    yield return new WaitForSeconds(spPanelDuration);

    // Fade out
    timer = 0f;
    while (timer < spPanelFadeDuration)
    {
        timer += Time.deltaTime;
        cg.alpha = Mathf.Lerp(1f, 0f, timer / spPanelFadeDuration);
        yield return null;
    }
    cg.alpha = 0f;
    notEnoughSPPanel.SetActive(false);
}









}
