using UnityEngine;

/// <summary>
/// Helper script to assign invulnerable sprite to all chess pieces
/// Attach this to any GameObject and press the assigned key to assign sprites
/// </summary>
public class UIBuffSpriteAssigner : MonoBehaviour
{
    [Header("Sprite Assignment")]
    public Sprite invulnerableSprite; // Drag your shield sprite here
    public Sprite bountySprite; // Drag your bounty sprite here
    public Sprite summonedSprite; // Drag your summoned sprite here
    public Sprite stunnedSprite; // Drag your stunned sprite here
    public Sprite kingMovementSprite; // Drag your king movement sprite here
    public Sprite crippledSprite; // Drag your crippled sprite here
    public Sprite phoenixResurrectionSprite; // Drag your phoenix resurrection sprite here
    public Sprite guardSprite; // Drag your guard sprite here
    public Sprite solidaritySprite; // Drag your solidarity sprite here
    
    [Header("Controls")]
    public KeyCode assignKey = KeyCode.S; // Press S to assign sprites
    
    private void Update()
    {
        if (Input.GetKeyDown(assignKey))
        {
            AssignSpritesToAllPieces();
        }
    }
    
    /// <summary>
    /// Assigns sprites to all chess pieces
    /// </summary>
    public void AssignSpritesToAllPieces()
    {
        if (invulnerableSprite == null && bountySprite == null && summonedSprite == null && 
            stunnedSprite == null && kingMovementSprite == null && crippledSprite == null && 
            phoenixResurrectionSprite == null && guardSprite == null && solidaritySprite == null)
        {
            Debug.LogError("[UIBuffSpriteAssigner] No sprites assigned! Please drag sprites to the fields.");
            return;
        }
        
        // Use the static methods to assign sprites
        if (invulnerableSprite != null)
            UIBuffManager.SetInvulnerableSprite(invulnerableSprite);
        
        if (bountySprite != null)
            UIBuffManager.SetBountySprite(bountySprite);
        
        if (summonedSprite != null)
            UIBuffManager.SetSummonedSprite(summonedSprite);
        
        if (stunnedSprite != null)
            UIBuffManager.SetStunnedSprite(stunnedSprite);
        
        if (kingMovementSprite != null)
            UIBuffManager.SetKingMovementSprite(kingMovementSprite);
        
        if (crippledSprite != null)
            UIBuffManager.SetCrippledSprite(crippledSprite);
        
        if (phoenixResurrectionSprite != null)
            UIBuffManager.SetPhoenixResurrectionSprite(phoenixResurrectionSprite);
        
        if (guardSprite != null)
            UIBuffManager.SetGuardSprite(guardSprite);
        
        if (solidaritySprite != null)
            UIBuffManager.SetSolidaritySprite(solidaritySprite);
        
        Debug.Log($"[UIBuffSpriteAssigner] Successfully assigned sprites to all pieces!");
    }
    
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 500, 120), 
            "UIBuffSpriteAssigner:\n" +
            "1. Drag sprites to all status effect fields\n" +
            "2. Press S to assign to all pieces\n" +
            "3. Test status effects to see icons\n" +
            "Status effects: Invulnerable, Bounty, Summoned, Stunned,\n" +
            "KingMovement, Crippled, PhoenixResurrection, Guard, Solidarity");
    }
}
