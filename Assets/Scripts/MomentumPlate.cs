using UnityEngine;

public class MomentumPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private Knight knight;

    public void Setup(Game g, int tileX, int tileY, Knight k)
    {
        game = g;
        x = tileX;
        y = tileY;
        knight = k;
    }

    private void OnMouseUp()
    {
        if (knight == null)
        {
            Debug.LogError("[MomentumPlate] No knight reference!");
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

        Debug.Log($"[MomentumPlate] Teleport clicked at ({x},{y}) for {knight.name}");
        // Let the Knight handle the teleport + cooldown + turn end
        knight.ExecuteMomentumTeleport(x, y);
        // In Rook.cs, in AttemptFortify() method:
if (SkillTracker.Instance != null)
{
    SkillTracker.Instance.LogSkillUsage(knight.GetComponent<Chessman>().GetPlayer(), "KNIGHT", "KNIGHTS MOMENTUM", 0);
}
    }
}
