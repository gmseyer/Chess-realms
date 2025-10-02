using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIBuffManager : MonoBehaviour
{
    [Header("UI Buff Sprites")]
    public Sprite invulnerableIconSprite; // Assign your shield sprite here
    public Sprite bountyIconSprite; // Assign your bounty sprite here
    public Sprite summonedIconSprite; // Assign your summoned sprite here
    public Sprite stunnedIconSprite; // Assign your stunned sprite here
    public Sprite kingMovementIconSprite; // Assign your king movement sprite here
    public Sprite crippledIconSprite; // Assign your crippled sprite here
    public Sprite phoenixResurrectionIconSprite; // Assign your phoenix resurrection sprite here
    public Sprite guardIconSprite; // Assign your guard sprite here
    public Sprite solidarityIconSprite; // Assign your solidarity sprite here
    
    // Static reference to sprites for runtime assignment
    private static Sprite _staticInvulnerableSprite;
    private static Sprite _staticBountySprite;
    private static Sprite _staticSummonedSprite;
    private static Sprite _staticStunnedSprite;
    private static Sprite _staticKingMovementSprite;
    private static Sprite _staticCrippledSprite;
    private static Sprite _staticPhoenixResurrectionSprite;
    private static Sprite _staticGuardSprite;
    private static Sprite _staticSolidaritySprite;
    
    [Header("Icon Settings")]
    public Vector3 iconOffset = new Vector3(1.5f, 1.5f, -1f); // Position relative to piece (top middle)
    public float iconScale = 3.5f; // Size of the icon (same as chess piece)
    public Color iconColor = new Color(1f, 1f, 1f, 0.75f); // Color of the icon with 60% opacity
    
    // Track active buff icons
    private Dictionary<StatusType, GameObject> activeBuffIcons = new Dictionary<StatusType, GameObject>();
    
    private Chessman chessman;
    private StatusManager statusManager;
    
    private void Awake()
    {
        chessman = GetComponent<Chessman>();
        statusManager = GetComponent<StatusManager>();
        
        // If no sprite is assigned, try to use the static one
        if (invulnerableIconSprite == null && _staticInvulnerableSprite != null)
            invulnerableIconSprite = _staticInvulnerableSprite;
        if (bountyIconSprite == null && _staticBountySprite != null)
            bountyIconSprite = _staticBountySprite;
        if (summonedIconSprite == null && _staticSummonedSprite != null)
            summonedIconSprite = _staticSummonedSprite;
        if (stunnedIconSprite == null && _staticStunnedSprite != null)
            stunnedIconSprite = _staticStunnedSprite;
        if (kingMovementIconSprite == null && _staticKingMovementSprite != null)
            kingMovementIconSprite = _staticKingMovementSprite;
        if (crippledIconSprite == null && _staticCrippledSprite != null)
            crippledIconSprite = _staticCrippledSprite;
        if (phoenixResurrectionIconSprite == null && _staticPhoenixResurrectionSprite != null)
            phoenixResurrectionIconSprite = _staticPhoenixResurrectionSprite;
        if (guardIconSprite == null && _staticGuardSprite != null)
            guardIconSprite = _staticGuardSprite;
        if (solidarityIconSprite == null && _staticSolidaritySprite != null)
            solidarityIconSprite = _staticSolidaritySprite;
    }
    
    /// <summary>
    /// Sets the static invulnerable sprite for all UIBuffManagers
    /// </summary>
    public static void SetInvulnerableSprite(Sprite sprite)
    {
        if (sprite == null) return;
        
        _staticInvulnerableSprite = sprite;
        
        // Update all existing UIBuffManagers
        UIBuffManager[] allManagers = FindObjectsOfType<UIBuffManager>();
        foreach (UIBuffManager manager in allManagers)
        {
            if (manager.invulnerableIconSprite == null)
            {
                manager.invulnerableIconSprite = sprite;
            }
        }
    }
    
    /// <summary>
    /// Sets the static bounty sprite for all UIBuffManagers
    /// </summary>
    public static void SetBountySprite(Sprite sprite)
    {
        if (sprite == null) return;
        
        _staticBountySprite = sprite;
        
        // Update all existing UIBuffManagers
        UIBuffManager[] allManagers = FindObjectsOfType<UIBuffManager>();
        foreach (UIBuffManager manager in allManagers)
        {
            if (manager.bountyIconSprite == null)
            {
                manager.bountyIconSprite = sprite;
            }
        }
    }
    
    /// <summary>
    /// Sets the static summoned sprite for all UIBuffManagers
    /// </summary>
    public static void SetSummonedSprite(Sprite sprite)
    {
        if (sprite == null) return;
        _staticSummonedSprite = sprite;
        UIBuffManager[] allManagers = FindObjectsOfType<UIBuffManager>();
        foreach (UIBuffManager manager in allManagers)
        {
            if (manager.summonedIconSprite == null)
                manager.summonedIconSprite = sprite;
        }
    }
    
    /// <summary>
    /// Sets the static stunned sprite for all UIBuffManagers
    /// </summary>
    public static void SetStunnedSprite(Sprite sprite)
    {
        if (sprite == null) return;
        _staticStunnedSprite = sprite;
        UIBuffManager[] allManagers = FindObjectsOfType<UIBuffManager>();
        foreach (UIBuffManager manager in allManagers)
        {
            if (manager.stunnedIconSprite == null)
                manager.stunnedIconSprite = sprite;
        }
    }
    
    /// <summary>
    /// Sets the static king movement sprite for all UIBuffManagers
    /// </summary>
    public static void SetKingMovementSprite(Sprite sprite)
    {
        if (sprite == null) return;
        _staticKingMovementSprite = sprite;
        UIBuffManager[] allManagers = FindObjectsOfType<UIBuffManager>();
        foreach (UIBuffManager manager in allManagers)
        {
            if (manager.kingMovementIconSprite == null)
                manager.kingMovementIconSprite = sprite;
        }
    }
    
    /// <summary>
    /// Sets the static crippled sprite for all UIBuffManagers
    /// </summary>
    public static void SetCrippledSprite(Sprite sprite)
    {
        if (sprite == null) return;
        _staticCrippledSprite = sprite;
        UIBuffManager[] allManagers = FindObjectsOfType<UIBuffManager>();
        foreach (UIBuffManager manager in allManagers)
        {
            if (manager.crippledIconSprite == null)
                manager.crippledIconSprite = sprite;
        }
    }
    
    /// <summary>
    /// Sets the static phoenix resurrection sprite for all UIBuffManagers
    /// </summary>
    public static void SetPhoenixResurrectionSprite(Sprite sprite)
    {
        if (sprite == null) return;
        _staticPhoenixResurrectionSprite = sprite;
        UIBuffManager[] allManagers = FindObjectsOfType<UIBuffManager>();
        foreach (UIBuffManager manager in allManagers)
        {
            if (manager.phoenixResurrectionIconSprite == null)
                manager.phoenixResurrectionIconSprite = sprite;
        }
    }
    
    /// <summary>
    /// Sets the static guard sprite for all UIBuffManagers
    /// </summary>
    public static void SetGuardSprite(Sprite sprite)
    {
        if (sprite == null) return;
        _staticGuardSprite = sprite;
        UIBuffManager[] allManagers = FindObjectsOfType<UIBuffManager>();
        foreach (UIBuffManager manager in allManagers)
        {
            if (manager.guardIconSprite == null)
                manager.guardIconSprite = sprite;
        }
    }
    
    /// <summary>
    /// Sets the static solidarity sprite for all UIBuffManagers
    /// </summary>
    public static void SetSolidaritySprite(Sprite sprite)
    {
        if (sprite == null) return;
        _staticSolidaritySprite = sprite;
        UIBuffManager[] allManagers = FindObjectsOfType<UIBuffManager>();
        foreach (UIBuffManager manager in allManagers)
        {
            if (manager.solidarityIconSprite == null)
                manager.solidarityIconSprite = sprite;
        }
    }
    
    /// <summary>
    /// Updates all buff icons based on current status effects
    /// </summary>
    public void UpdateBuffIcons()
    {
        if (chessman == null || statusManager == null) return;
        
        Game game = chessman.controller?.GetComponent<Game>();
        if (game == null) return;
        
        // Check for invulnerable status
        bool isInvulnerable = statusManager.HasStatus(StatusType.Invulnerable, game.turns);
        UpdateBuffIcon(StatusType.Invulnerable, isInvulnerable);
        
        // Check for bounty status
        bool hasBounty = statusManager.HasBounty(game.turns);
        UpdateBuffIcon(StatusType.Bounty, hasBounty);
        
        // Check for summoned status
        bool isSummoned = statusManager.HasStatus(StatusType.Summoned, game.turns);
        UpdateBuffIcon(StatusType.Summoned, isSummoned);
        
        // Check for stunned status
        bool isStunned = statusManager.HasStatus(StatusType.Stunned, game.turns);
        UpdateBuffIcon(StatusType.Stunned, isStunned);
        
        // Check for king movement status
        bool hasKingMovement = statusManager.HasStatus(StatusType.KingMovement, game.turns);
        UpdateBuffIcon(StatusType.KingMovement, hasKingMovement);
        
        // Check for crippled status
        bool isCrippled = statusManager.HasStatus(StatusType.Crippled, game.turns);
        UpdateBuffIcon(StatusType.Crippled, isCrippled);
        
        // Check for phoenix resurrection status
        bool hasPhoenixResurrection = statusManager.HasStatus(StatusType.PhoenixResurrection, game.turns);
        UpdateBuffIcon(StatusType.PhoenixResurrection, hasPhoenixResurrection);
        
        // Check for guard status
        bool hasGuard = statusManager.HasStatus(StatusType.Guard, game.turns);
        UpdateBuffIcon(StatusType.Guard, hasGuard);
        
        // Check for solidarity status
        bool hasSolidarity = statusManager.HasStatus(StatusType.Solidarity, game.turns);
        UpdateBuffIcon(StatusType.Solidarity, hasSolidarity);
    }
    
    /// <summary>
    /// Updates a specific buff icon based on status
    /// </summary>
    private void UpdateBuffIcon(StatusType statusType, bool hasStatus)
    {
        if (hasStatus)
        {
            // Show buff icon if not already shown
            if (!activeBuffIcons.ContainsKey(statusType))
            {
                ShowBuffIcon(statusType);
            }
        }
        else
        {
            // Hide buff icon if currently shown
            if (activeBuffIcons.ContainsKey(statusType))
            {
                HideBuffIcon(statusType);
            }
        }
    }
    
    /// <summary>
    /// Shows a buff icon for the specified status type
    /// </summary>
    private void ShowBuffIcon(StatusType statusType)
    {
        Sprite iconSprite = GetIconSprite(statusType);
        if (iconSprite == null) return;
        
        // Calculate position based on number of active status effects
        int statusIndex = activeBuffIcons.Count;
        Vector3 iconPosition = GetStatusIconPosition(statusIndex);
        
        // Create the buff icon GameObject
        GameObject buffIcon = new GameObject($"{statusType}Icon");
        buffIcon.transform.SetParent(transform); // Make it a child of the piece
        buffIcon.transform.localPosition = iconPosition; // Use calculated position
        buffIcon.transform.localScale = Vector3.one * iconScale;
        
        // Add SpriteRenderer component
        SpriteRenderer spriteRenderer = buffIcon.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = iconSprite;
        spriteRenderer.color = iconColor;
        spriteRenderer.sortingOrder = 10; // Make sure it appears on top
        
        // Store reference to the icon
        activeBuffIcons[statusType] = buffIcon;
    }
    
    /// <summary>
    /// Hides a buff icon for the specified status type
    /// </summary>
    private void HideBuffIcon(StatusType statusType)
    {
        if (activeBuffIcons.ContainsKey(statusType))
        {
            GameObject icon = activeBuffIcons[statusType];
            if (icon != null)
            {
                Destroy(icon);
            }
            activeBuffIcons.Remove(statusType);
            
            // Reposition remaining icons
            RepositionAllIcons();
        }
    }
    
    /// <summary>
    /// Repositions all active buff icons based on their current order
    /// </summary>
    private void RepositionAllIcons()
    {
        int index = 0;
        foreach (var kvp in activeBuffIcons)
        {
            if (kvp.Value != null)
            {
                Vector3 newPosition = GetStatusIconPosition(index);
                kvp.Value.transform.localPosition = newPosition;
                index++;
            }
        }
    }
    
    /// <summary>
    /// Gets the appropriate sprite for a status type
    /// </summary>
    private Sprite GetIconSprite(StatusType statusType)
    {
        switch (statusType)
        {
            case StatusType.Invulnerable:
                return invulnerableIconSprite;
            case StatusType.Bounty:
                return bountyIconSprite;
            case StatusType.Summoned:
                return summonedIconSprite;
            case StatusType.Stunned:
                return stunnedIconSprite;
            case StatusType.KingMovement:
                return kingMovementIconSprite;
            case StatusType.Crippled:
                return crippledIconSprite;
            case StatusType.PhoenixResurrection:
                return phoenixResurrectionIconSprite;
            case StatusType.Guard:
                return guardIconSprite;
            case StatusType.Solidarity:
                return solidarityIconSprite;
            default:
                return null;
        }
    }
    
    /// <summary>
    /// Calculates the position for a status icon based on how many status effects are active
    /// </summary>
    private Vector3 GetStatusIconPosition(int statusIndex)
    {
        switch (statusIndex)
        {
            case 0: // First status - top middle
                return new Vector3(0f, 1.5f, -1f);
            case 1: // Second status - top left
                return new Vector3(-1.5f, 1.5f, -1f);
            case 2: // Third status - middle right
                return new Vector3(1.5f, 0f, -1f);
            case 3: // Fourth status - middle left
                return new Vector3(-1.5f, 0f, -1f);
            default: // Additional statuses - spread around
                return new Vector3(0f, 1.5f, -1f);
        }
    }
    
    /// <summary>
    /// Clears all buff icons
    /// </summary>
    public void ClearAllBuffIcons()
    {
        foreach (var kvp in activeBuffIcons)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        activeBuffIcons.Clear();
    }
}