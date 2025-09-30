using UnityEngine;

public class ChivalricGuardSelectionPlate : MonoBehaviour
{
    private int x, y;
    private Knight knight;

    public void Setup(int tileX, int tileY, Knight knightInstance)
    {
        x = tileX;
        y = tileY;
        knight = knightInstance;
    }

    private void OnMouseUp()
    {
        Debug.Log($"[ChivalricGuardSelectionPlate] Chivalric Guard target selected at ({x},{y})!");

        // Get the piece at this position
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[ChivalricGuardSelectionPlate] Could not find Game component!");
            return;
        }

        GameObject selectedPiece = game.GetPosition(x, y);
        if (selectedPiece == null)
        {
            Debug.LogError("[ChivalricGuardSelectionPlate] No piece found at selected position!");
            return;
        }

        Chessman selectedChessman = selectedPiece.GetComponent<Chessman>();
        if (selectedChessman == null)
        {
            Debug.LogError("[ChivalricGuardSelectionPlate] No Chessman component found!");
            return;
        }

        // Apply Guard status to the selected piece using the correct knight instance
        if (knight != null)
        {
            knight.ApplyGuardStatusToPiece(selectedChessman);
        }
        else
        {
            Debug.LogError("[ChivalricGuardSelectionPlate] No knight reference!");
        }

        // Note: Do NOT clear the ChivalricGuard reference here - it needs to stay until the actual sacrifice happens

        // Destroy all selection plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Hide UI panels
        if (UIManager.Instance != null)
        {
            UIManager.Instance.pawnPanel?.SetActive(false);
            UIManager.Instance.knightPanel?.SetActive(false);
            UIManager.Instance.bishopPanel?.SetActive(false);
            UIManager.Instance.rookPanel?.SetActive(false);
            UIManager.Instance.queenPanel?.SetActive(false);
            UIManager.Instance.kingPanel?.SetActive(false);
            UIManager.Instance.whiteElementalBishopPanel?.SetActive(false);
            UIManager.Instance.whiteArchBishopPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalKnightPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalPawnPanel?.SetActive(false);
            UIManager.Instance.whiteSpectralHeraldPanel?.SetActive(false);
            UIManager.Instance.whiteIceBishopPanel?.SetActive(false);
            UIManager.Instance.whiteEarthBishopPanel?.SetActive(false);
            UIManager.Instance.whiteFireBishopPanel?.SetActive(false);
            UIManager.Instance.whiteChronomagusPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalRookPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalBishopPanel?.SetActive(false);
            UIManager.Instance.blackChronomagusPanel?.SetActive(false);
        }

        // End turn
        game.NextTurn();

        Debug.Log($"[ChivalricGuardSelectionPlate] Chivalric Guard completed!");
    }
}
