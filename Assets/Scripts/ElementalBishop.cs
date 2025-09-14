using UnityEngine;

public class ElementalBishop : MonoBehaviour
{
    public GameObject movePlatePrefab; // Drag your MovePlate prefab in inspector

    // Generic method to spawn a skill tile
    public void CastSkill(string tileName)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        // Destroy old move plates first
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Spawn move plates on empty tiles
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    SpawnMovePlate(game, x, y, tileName);
                }
            }
        }

        Debug.Log($"[ElementalBishop] {tileName} skill activated: move plates spawned.");
    }

    // Spawns a move plate for a given tile
    private void SpawnMovePlate(Game game, int x, int y, string tileName)
    {
        float fx = x * 0.66f - 2.3f;
        float fy = y * 0.66f - 2.3f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove existing MovePlate script if present
        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // Add SkillEndTurnPlate with tileName
        mp.AddComponent<SkillEndTurnPlate>().Setup(game, x, y, tileName);
    }

    // Optional helper methods for buttons
    public void InfernalBrand() => CastSkill("tile_lava");
    public void GlacialPath() => CastSkill("tile_ice");

    public void StoneSentinel() => CastSkill("tile_earth");
    
}
