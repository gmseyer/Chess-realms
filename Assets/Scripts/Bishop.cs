using UnityEngine;

public class Bishop : MonoBehaviour
{
    public GameObject movePlatePrefab; // Drag your MovePlate prefab in the inspector

    // Call this from the Bishop Button (UI Button OnClick)
    public void OnBishopButtonClick()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        // ✅ Destroy old move plates first
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // ✅ Loop all tiles and spawn plates on empty ones
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    SpawnTile(game, x, y);
                }
            }
        }
    }

   private void SpawnTile(Game game, int x, int y)
{
    float fx = x * 0.66f + -2.3f;
    float fy = y * 0.66f + -2.3f;

    GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

    MovePlate oldScript = mp.GetComponent<MovePlate>();
    if (oldScript != null) Destroy(oldScript);

    mp.AddComponent<EndTurnPlate>().Setup(game, x, y); // ✅ pass x,y
}





}
