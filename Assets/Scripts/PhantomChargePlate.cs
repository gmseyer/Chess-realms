using UnityEngine;

public class PhantomChargePlate : MonoBehaviour
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
            Debug.LogError("[PhantomChargePlate] No knight reference!");
            return;
        }

        Debug.Log($"[PhantomChargePlate] Tile clicked at ({x},{y})");
        knight.ExecutePhantomCharge(x, y);
    }
}
