using UnityEngine;

public class MomentumPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private Knight knight;
    private bool isForPromotion = false;

    public void Setup(Game g, int tileX, int tileY, Knight k, bool promotion = false)
    {
        game = g;
        x = tileX;
        y = tileY;
        knight = k; 
        isForPromotion = promotion;
    }

    public void OnMouseUp()
    {
        if (knight == null)
        {
            Debug.LogError("[MomentumPlate] No knight reference! Knight may have been destroyed for promotion.");
            // Clean up this momentum plate since the knight is gone
            Destroy(gameObject);
            return;
        }

        // Double-check the tile is still empty
        if (game.GetPosition(x, y) != null)
        {
            Debug.LogWarning($"[MomentumPlate] Target ({x},{y}) occupied. Aborting.");
            // cleanup so UI doesn't get stuck
            foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
                Destroy(plate);
            return;
        }

        Debug.Log($"[MomentumPlate] Teleport clicked at ({x},{y}) for {knight.name} (promotion: {isForPromotion})");
        // Let the Knight handle the teleport + cooldown + turn end
        knight.ExecuteMomentumTeleport(x, y, startCooldown: !isForPromotion);
    }
}
