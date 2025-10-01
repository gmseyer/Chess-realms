using UnityEngine;

public class EternityPiercePlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string player; // Store the player who cast the skill
    private int distance; // 1st, 2nd, or 3rd tile

    public void Setup(Game gameRef, int xPos, int yPos, string playerName, int tileDistance)
    {
        game = gameRef;
        x = xPos;
        y = yPos;
        player = playerName;
        distance = tileDistance;
    }

    public void OnMouseUp()
    {
        // Log which tile was clicked
        if (distance == 1)
        {
            Debug.Log("[Eternity Pierce] 1st tile clicked - will spend 1 SP and stun pieces on this tile");
        }
        else if (distance == 2)
        {
            Debug.Log("[Eternity Pierce] 2nd tile clicked - will spend 2 SP and stun pieces on 1st and 2nd tiles");
        }
        else if (distance == 3)
        {
            Debug.Log("[Eternity Pierce] 3rd tile clicked - will spend 3 SP and stun pieces on 1st, 2nd, and 3rd tiles");
        }

        // Execute the Eternity Pierce attack
        ExecuteEternityPierce();
        
      
        
    }

    private void ExecuteEternityPierce()
    {
        // Calculate SP cost based on distance
        int spCost = distance; // 1 SP for 1st tile, 2 SP for 2nd tile, 3 SP for 3rd tile

        // Check if player has enough SP
        if (SkillManager.Instance.GetPlayerSP(player) < spCost)
        {
            Debug.LogWarning($"[Eternity Pierce] Not enough SP for {player} (cost {spCost}).");
            DestroyAllMovePlates();
            return;
        }

        // Deduct SP from correct player
        SkillManager.Instance.SpendPlayerSP(player, spCost);
        Debug.Log($"[Eternity Pierce] SP deducted from {player}: {spCost}");

        // Get Archbishop's position to determine the direction
        GameObject selectedPiece = UIManager.Instance.selectedPiece;
        if (selectedPiece == null)
        {
            Debug.LogError("[Eternity Pierce] No piece selected!");
            DestroyAllMovePlates();
            return;
        }

        Chessman archbishopCm = selectedPiece.GetComponent<Chessman>();
        if (archbishopCm == null)
        {
            Debug.LogError("[Eternity Pierce] No Chessman component found on selected piece!");
            DestroyAllMovePlates();
            return;
        }

        int archbishopX = archbishopCm.GetXBoard();
        int archbishopY = archbishopCm.GetYBoard();

        // Calculate the direction vector
        int deltaX = x - archbishopX;
        int deltaY = y - archbishopY;
        
        // Normalize the direction (should be -1, 0, or 1 for diagonal)
        int dirX = deltaX == 0 ? 0 : (deltaX > 0 ? 1 : -1);
        int dirY = deltaY == 0 ? 0 : (deltaY > 0 ? 1 : -1);

        // Stun all pieces in the line up to the clicked distance
        StunPiecesInLine(archbishopX, archbishopY, dirX, dirY, distance);

        // ✅ Set cooldown using CooldownManager (following HealingBenediction pattern)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "EternityPierce", CooldownManager.CooldownType.OncePerBattle);
        }
        Debug.Log($"[Eternity Pierce] Cooldown activated for {player} - once per battle.");
        
        // Log skill usage
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(player, "ARCHBISHOP", "ETERNITY PIERCE", spCost);
        }

        // Destroy all moveplates
        DestroyAllMovePlates();
        
        // End the Archbishop's turn
        game.NextTurn();
        
        // Update visual status of all pieces on the board immediately
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        foreach (Chessman piece in allPieces)
        {
            piece.UpdateVisualStatus();
        }
    }

    private void StunPiecesInLine(int startX, int startY, int dirX, int dirY, int maxDistance)
    {
        int currentTurn = game.GetTurnCount();
        int stunDuration = 4; // Stun for 2 turns

        for (int i = 1; i <= maxDistance; i++)
        {
            int targetX = startX + (dirX * i);
            int targetY = startY + (dirY * i);

            // Check if position is on board
            if (!game.PositionOnBoard(targetX, targetY)) break;

            // Get piece at this position
            GameObject piece = game.GetPosition(targetX, targetY);
            if (piece != null)
            {
                Chessman targetCm = piece.GetComponent<Chessman>();
                if (targetCm != null)
                {
                    // Only stun enemy pieces (compare against the player who cast the skill)
                    if (targetCm.GetPlayer() != player)
                    {
                        // Apply stunned status
                        StatusManager statusManager = targetCm.GetComponent<StatusManager>();
                        if (statusManager != null)
                        {
                            statusManager.AddStatus(StatusType.Stunned, currentTurn + stunDuration);
                            Debug.Log($"[Eternity Pierce] {player} stunned enemy {targetCm.name} at ({targetX},{targetY}) for {stunDuration} turns");
                        }
                    }
                }
            }
        }
    }

    private void DestroyAllMovePlates()
    {
        // Destroy all move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
    }
}