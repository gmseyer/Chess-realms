using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public enum SkillType
{
    Fortify, //Rook 1st skill
    HealingBenediction, //Bishop 1st skill 
    LunarLeap, // Knight 1st skill
    CelestialSummon, //Bishop 2nd skill
    RoyalAcolyte, //Pawn promotion skill
    PawnsGambit, //Pawn's Gambit skill
    RussianRoulette //Russian Roulette skill
     
    // Add more skills here
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("Skill Points")] 
    public int whiteSkillPoints = 2; // Changed from 5 to 0
    public int blackSkillPoints = 2; // Changed from 5 to 0

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

[Header("Passive SP Gain System")]
private int lastProcessedTurn = 0; // Track last turn we processed for SP gains

[Header("Pawn's Gambit Tracking")]
public static bool pawnsGambitUsed = false; // Track if Pawn's Gambit has been used this battle



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Force set SP to 0 (overrides Inspector values)
            whiteSkillPoints = 5; // FORCE WORKING
            blackSkillPoints = 5;
            lastProcessedTurn = 0;
            UpdateSPUI(); // initialize text at start
            Debug.Log("[SkillManager] SP system initialized: Both players start with 0 SP");
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
        {
            whiteSkillPoints += amount;
            // Cap SP at 5
            if (whiteSkillPoints > 5)
            {
                whiteSkillPoints = 5;
                Debug.Log($"[SkillManager] {player} SP capped at 5 (was going to be {whiteSkillPoints + amount})");
            }
        }
        else
        {
            blackSkillPoints += amount;
            // Cap SP at 5
            if (blackSkillPoints > 5)
            {
                blackSkillPoints = 5;
                Debug.Log($"[SkillManager] {player} SP capped at 5 (was going to be {blackSkillPoints + amount})");
            }
        }

        Debug.Log($"[SkillManager] {player} gained {amount} SP. Now: {GetPlayerSP(player)}");
        
        // Update text first
        UpdateSPText();
        
        // Trigger SP gain animation (this will handle gem opacity)
        StartCoroutine(AnimateSPGain(player, amount));
    }

    // Animate SP gain with a special effect
    private System.Collections.IEnumerator AnimateSPGain(string player, int amountGained)
    {
        List<Image> gems = (player == "white") ? whiteGems : blackGems;
        int currentSP = GetPlayerSP(player);
        
        // Animate the newly gained gems with a special effect
        for (int i = currentSP - amountGained; i < currentSP && i < gems.Count; i++)
        {
            if (gems[i] != null)
            {
                // Flash effect for gained SP
                StartCoroutine(FlashGem(gems[i]));
            }
        }
        
        // Wait for animation to complete, then update gem opacity
        yield return new WaitForSeconds(0.5f); // Wait for flash animation
        
        // Update gem opacity to final state
        UpdateGemOpacity(player);
    }

    // Flash animation for SP gain
    private System.Collections.IEnumerator FlashGem(Image gem)
    {
        if (gem == null) yield break;
        
        Color originalColor = gem.color;
        Color flashColor = new Color(1f, 1f, 0f, 1f); // Bright yellow flash
        
        // Flash bright yellow
        gem.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        
        // Fade back to original color
        float timer = 0f;
        float duration = 0.3f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            gem.color = Color.Lerp(flashColor, originalColor, timer / duration); 
            yield return null;
        }
        
        gem.color = originalColor;
    }

    // ✅ Cooldown handling 
    public bool IsSkillOnCooldown(string player, SkillType skill)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<Game>();
        if (game == null) return false;
        
        if (cooldowns[player].ContainsKey(skill))
            return cooldowns[player][skill] > game.turns;
        return false;
    }

    public void StartCooldown(string player, SkillType skill, int duration)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<Game>();
        if (game == null) return;
        
        int endTurn = game.turns + duration;
        cooldowns[player][skill] = endTurn;
        Debug.Log($"[SkillManager] {skill} on {player} is now on cooldown until turn {endTurn}");
    }

    // ✅ Updates UI live
    private void UpdateSPUI()
    {
        UpdateSPText();
        UpdateGemOpacity("white");
        UpdateGemOpacity("black");
    }

    private void UpdateSPText()
    {
        if (whiteSPText != null)
            whiteSPText.text = $"White SP: {whiteSkillPoints}";
        if (blackSPText != null)
            blackSPText.text = $"Black SP: {blackSkillPoints}";
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

// Passive SP Gain System
public void CheckPassiveSPGain()
{
    // Find Game component directly instead of using singleton
    Game game = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<Game>();
    if (game == null) 
    {
        Debug.LogWarning("[SkillManager] Game component not found - cannot check SP gains");
        return;
    }
    
    int currentTurn = game.turns;
    
    // Only process if turn has changed
    if (currentTurn <= lastProcessedTurn) 
    {
        Debug.Log($"[SkillManager] Turn {currentTurn} already processed (last: {lastProcessedTurn})");
        return;
    }
    
    Debug.Log($"[SkillManager] Processing SP gains for turn {currentTurn} (last processed: {lastProcessedTurn})");
    lastProcessedTurn = currentTurn;
    
    // Turn 5: Both players gain 4 SP
    if (currentTurn == 5)
    {
        AddPlayerSP("white", 4);
        AddPlayerSP("black", 4);
        Debug.Log($"[SkillManager] Turn {currentTurn}: Both players gained 4 SP!");
    }
    // Turn 15, 25, 35: Both players gain 1 SP
    else if (currentTurn == 15 || currentTurn == 25 || currentTurn == 35)
    {
        AddPlayerSP("white", 1);
        AddPlayerSP("black", 1);
        Debug.Log($"[SkillManager] Turn {currentTurn}: Both players gained 1 SP!");
    }
    else
    {
        Debug.Log($"[SkillManager] Turn {currentTurn}: No SP gains scheduled");
    }
}

// Public method to reset SP system (for game restart)
public void ResetSPSystem()
{
    whiteSkillPoints = 0;
    blackSkillPoints = 0;
    lastProcessedTurn = 0;
    pawnsGambitUsed = false; // Reset Pawn's Gambit usage
    UpdateSPUI();
    Debug.Log("[SkillManager] SP system reset to 0 for both players");
}

// Pawn's Gambit skill execution
public bool ExecutePawnsGambit(string player)
{
    // Check if already used this battle
    if (pawnsGambitUsed)
    {
        Debug.Log("[Pawn's Gambit] Already used this battle - cannot use again!");
        return false;
    }

    // Check SP cost (0 SP)
    if (!SpendPlayerSP(player, 0))
    {
        Debug.LogWarning("[Pawn's Gambit] Not enough SP to use Pawn's Gambit. Need 0 SP.");
        return false;
    }

    // Find all pawns on the board
    Chessman[] allPieces = FindObjectsOfType<Chessman>();
    List<Chessman> whitePawns = new List<Chessman>();
    List<Chessman> blackPawns = new List<Chessman>();

    foreach (Chessman piece in allPieces)
    {
        if (piece != null && piece.name.Contains("pawn") && !piece.name.Contains("royal_pawn") && !piece.name.Contains("wraith_pawn"))
        {
            if (piece.GetPlayer() == "white")
                whitePawns.Add(piece);
            else if (piece.GetPlayer() == "black")
                blackPawns.Add(piece);
        }
    }

    // Check if we have at least one pawn of each color
    if (whitePawns.Count == 0 || blackPawns.Count == 0)
    {
        Debug.LogWarning("[Pawn's Gambit] Need at least one white and one black pawn to use this skill!");
        // No SP to refund since cost is 0
        return false;
    }

    // Randomly select one pawn from each color
    Chessman selectedWhitePawn = whitePawns[Random.Range(0, whitePawns.Count)];
    Chessman selectedBlackPawn = blackPawns[Random.Range(0, blackPawns.Count)];

    // Add bounty status to both pawns (1 SP bounty, expires at end of battle)
    Game game = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<Game>();
    if (game != null)
    {
        int currentTurn = game.turns;
        selectedWhitePawn.statusManager.AddBountyStatus(1, currentTurn + 999); // Expires far in the future
        selectedBlackPawn.statusManager.AddBountyStatus(1, currentTurn + 999); // Expires far in the future
        
        Debug.Log($"[Pawn's Gambit] {player} used Pawn's Gambit! Added bounty to {selectedWhitePawn.name} and {selectedBlackPawn.name}");
        
        // Mark as used
        pawnsGambitUsed = true;
        
        // End turn
        game.NextTurn();
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        return true;
    }

    return false;
}






}
