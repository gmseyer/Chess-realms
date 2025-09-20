using UnityEngine;
using System.Collections.Generic;

public class EtherealMovePlate : MonoBehaviour
{
    private GameObject bishop;
    private int x, y;
    private List<GameObject> passedEnemies;

    public void Setup(GameObject bishopRef, int tileX, int tileY, List<GameObject> enemies)
    {
        bishop = bishopRef;
        x = tileX;
        y = tileY;
        passedEnemies = enemies ?? new List<GameObject>();
    }

    private void OnMouseUp()
    {
        Debug.Log($"[EtherealMovePlate] Bishop moving to ({x},{y}) - passed through {passedEnemies.Count} enemies");
        
        // Apply soulbrand stacks to all passed enemies
        List<GameObject> destroyedEnemies = new List<GameObject>();
        
        foreach (GameObject enemy in passedEnemies)
        {
            if (enemy != null)
            {
                Chessman enemyCm = enemy.GetComponent<Chessman>();
                if (enemyCm != null && enemyCm.statusManager != null)
                {
                    // Add soulbrand stack
                    enemyCm.statusManager.AddSoulbrandStack();
                    
                    // Check if enemy should be destroyed
                    if (enemyCm.statusManager.GetSoulbrandStacks() >= 3)
                    {
                        destroyedEnemies.Add(enemy);
                        Debug.Log($"[EtherealMovePlate] {enemy.name} destroyed by 3 soulbrand stacks!");
                    }
                }
            }
        }
        
        // Move the Bishop to the new position
        MoveBishopToPosition();
        
        // Handle Royal Bishop promotion if any enemies were destroyed
        if (destroyedEnemies.Count > 0)
        {
            PromoteToRoyalBishop(destroyedEnemies);
        }
        
        // Clean up all move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        
        // End the Bishop's turn
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        game.NextTurn();
    }

    private void MoveBishopToPosition()
    {
        if (bishop == null) return;
        
        Chessman bishopCm = bishop.GetComponent<Chessman>();
        if (bishopCm == null) return;
        
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        
        // Clear the old position
        game.SetPositionEmpty(bishopCm.GetXBoard(), bishopCm.GetYBoard());
        
        // Update the bishop's coordinates
        bishopCm.SetXBoard(x);
        bishopCm.SetYBoard(y);
        bishopCm.SetCoords(); // Update visual position
        
        // Set the bishop at the new position
        game.SetPosition(bishop);
        
        Debug.Log($"[EtherealMovePlate] Bishop moved to ({x},{y})");
    }

    private void PromoteToRoyalBishop(List<GameObject> destroyedEnemies)
    {
        if (destroyedEnemies.Count == 0) return;
        
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        
        // Find the nearest destroyed enemy position for Royal Bishop placement
        Vector2Int royalBishopPosition = FindNearestDestroyedEnemyPosition(destroyedEnemies);
        
        // End ethereal status
        Chessman bishopCm = bishop.GetComponent<Chessman>();
        if (bishopCm != null && bishopCm.statusManager != null)
        {
            bishopCm.statusManager.RemoveStatus(StatusType.Ethereal);
            Debug.Log("[EtherealMovePlate] Ethereal status ended for Royal Bishop promotion");
        }
        
        // Destroy the current Bishop
        game.SetPositionEmpty(bishopCm.GetXBoard(), bishopCm.GetYBoard());
        Destroy(bishop);
        
        // Destroy all enemies with 3 soulbrand stacks
        foreach (GameObject enemy in destroyedEnemies)
        {
            if (enemy != null)
            {
                Chessman enemyCm = enemy.GetComponent<Chessman>();
                if (enemyCm != null)
                {
                    game.SetPositionEmpty(enemyCm.GetXBoard(), enemyCm.GetYBoard());
                    Destroy(enemy);
                }
            }
        }
        
        // Create Royal Bishop at the nearest destroyed enemy position
        GameObject royalBishop = game.Create("white_royal_bishop", royalBishopPosition.x, royalBishopPosition.y);
        if (royalBishop != null)
        {
            // Add 2-turn invulnerability to the Royal Bishop
            Chessman royalBishopCm = royalBishop.GetComponent<Chessman>();
            if (royalBishopCm != null && royalBishopCm.statusManager != null)
            {
                int currentTurn = game.turns;
                int invulnerabilityEndTurn = currentTurn + 2;
                royalBishopCm.statusManager.AddStatus(StatusType.Invulnerable, invulnerabilityEndTurn);
                Debug.Log($"[EtherealMovePlate] Royal Bishop created at ({royalBishopPosition.x},{royalBishopPosition.y}) with 2-turn invulnerability!");
            }
            else
            {
                Debug.Log($"[EtherealMovePlate] Royal Bishop created at ({royalBishopPosition.x},{royalBishopPosition.y})!");
            }
        }
    }

    private Vector2Int FindNearestDestroyedEnemyPosition(List<GameObject> destroyedEnemies)
    {
        if (destroyedEnemies.Count == 0) return new Vector2Int(x, y);
        
        // For now, use the first destroyed enemy's position
        // In a more complex system, you could calculate the nearest one
        GameObject firstEnemy = destroyedEnemies[0];
        if (firstEnemy != null)
        {
            Chessman enemyCm = firstEnemy.GetComponent<Chessman>();
            if (enemyCm != null)
            {
                return new Vector2Int(enemyCm.GetXBoard(), enemyCm.GetYBoard());
            }
        }
        
        // Fallback to current position
        return new Vector2Int(x, y);
    }
}
