using UnityEngine;

public class PolarityMovePlate : MonoBehaviour
{
    private int x, y;

    public void Setup(int tileX, int tileY)
    {
        x = tileX;
        y = tileY;
    }

    private void OnMouseUp()
    {
        Debug.Log($"[PolarityMovePlate] Polarity destination selected at ({x},{y})!");

        // Get the Game component
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[PolarityMovePlate] Could not find Game component!");
            return;
        }

        // Get the selected piece position from EarthboundBishop
        Vector2Int selectedPos = EarthboundBishop.selectedPolarityPiecePosition;
        if (selectedPos.x == -1 || selectedPos.y == -1)
        {
            Debug.LogError("[PolarityMovePlate] No piece selected for Polarity movement!");
            return;
        }

        // Get the piece to move
        GameObject pieceToMove = game.GetPosition(selectedPos.x, selectedPos.y);
        if (pieceToMove == null)
        {
            Debug.LogError($"[PolarityMovePlate] No piece found at selected position ({selectedPos.x},{selectedPos.y})!");
            return;
        }

        // Apply stun effects to adjacent enemies first
        ApplyAdjacentStunEffect(x, y);

        // Move the piece to the destination (following normal MovePlate.cs pattern)
        game.SetPositionEmpty(selectedPos.x, selectedPos.y);

        // Update the piece's coordinates and transform
        Chessman chessman = pieceToMove.GetComponent<Chessman>();
        if (chessman != null)
        {
            chessman.SetXBoard(x);
            chessman.SetYBoard(y);
            chessman.SetCoords();
        }

        game.SetPosition(pieceToMove);

        // Mark piece as moved (for castling tracking if applicable)
        if (chessman != null)
        {
            chessman.SetHasMoved(true);
        }

        // Reset the selected piece position
        EarthboundBishop.selectedPolarityPiecePosition = new Vector2Int(-1, -1);

        Debug.Log($"[PolarityMovePlate] Moved {pieceToMove.name} from ({selectedPos.x},{selectedPos.y}) to ({x},{y})");

        // Destroy all move plates
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

        Debug.Log($"[PolarityMovePlate] Polarity movement completed!");
    }

    /// <summary>
    /// Apply stun effects to adjacent enemies when landing
    /// </summary>
    private void ApplyAdjacentStunEffect(int landingX, int landingY)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[PolarityMovePlate] Could not find Game component for stun effects!");
            return;
        }

        // Get the piece that was moved to determine the current player
        Vector2Int selectedPos = EarthboundBishop.selectedPolarityPiecePosition;
        GameObject movedPiece = game.GetPosition(selectedPos.x, selectedPos.y);
        if (movedPiece == null)
        {
            Debug.LogError("[PolarityMovePlate] Could not find moved piece for player determination!");
            return;
        }

        Chessman movedChessman = movedPiece.GetComponent<Chessman>();
        if (movedChessman == null)
        {
            Debug.LogError("[PolarityMovePlate] Moved piece has no Chessman component!");
            return;
        }

        string currentPlayer = movedChessman.GetPlayer();
        int stunnedEnemies = 0;

        // Check 8 adjacent tiles around the landing position
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip center (landing position)

                int checkX = landingX + dx;
                int checkY = landingY + dy;

                // Check if position is within board bounds
                if (checkX >= 0 && checkX < 8 && checkY >= 0 && checkY < 8)
                {
                    GameObject piece = game.GetPosition(checkX, checkY);
                    if (piece != null)
                    {
                        Chessman pieceChessman = piece.GetComponent<Chessman>();
                        if (pieceChessman != null)
                        {
                            // Check if it's an enemy piece
                            if (pieceChessman.GetPlayer() != currentPlayer)
                            {
                                // Apply Stunned status for 2 turns
                                StatusManager status = pieceChessman.GetComponent<StatusManager>();
                                if (status != null)
                                {
                                    status.AddStatus(StatusType.Stunned, game.turns + 2);
                                    stunnedEnemies++;
                                    Debug.Log($"[PolarityMovePlate] Stunned enemy {pieceChessman.name} at ({checkX},{checkY}) for 2 turns!");
                                }
                            }
                        }
                    }
                }
            }
        }

        Debug.Log($"[PolarityMovePlate] Applied stun effects to {stunnedEnemies} adjacent enemies!");
    }
}
