using UnityEngine;

public class InvocationSummonPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string newPieceName;
    private string invocationType;

    public void Setup(Game g, int tileX, int tileY, string pieceName, string invocation)
    {
        game = g;
        x = tileX;
        y = tileY;
        newPieceName = pieceName;
        invocationType = invocation;
    }

    private void OnMouseUp()
    {
        if (game == null) return;

        Debug.Log($"[InvocationSummonPlate] Player selected position ({x},{y}) for {invocationType}!");

        // Create the specialized bishop at the selected location
        game.Create(newPieceName, x, y);

        // Clean up all summon plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Hide UI panels
        if (UIManager.Instance != null)
        {
            UIManager.Instance.whiteElementalBishopPanel?.SetActive(false);
            UIManager.Instance.whiteFireBishopPanel?.SetActive(false);
            UIManager.Instance.whiteIceBishopPanel?.SetActive(false);
            UIManager.Instance.whiteEarthBishopPanel?.SetActive(false);
            UIManager.Instance.HideStatusPanel();
            UIManager.Instance.selectedPiece = null;
        }

        Debug.Log($"[InvocationSummonPlate] {invocationType} SUCCESS! {newPieceName} summoned at ({x},{y})!");

        // End turn
        game.NextTurn();
    }
}
