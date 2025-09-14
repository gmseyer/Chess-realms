using UnityEngine;

public class ElementalBishop : MonoBehaviour
{
    public GameObject movePlatePrefab;

    public int skillPoints = 3; // ✅ Bishop-specific SP (can refill later)
    private int nextSkillAvailableTurn = 0; // ✅ Cooldown tracker

    // Main skill casting function
    public void CastSkill(string tileName)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>(); // ✅ FIXED

        // --- ✅ Step 1: Check SP ---
        if (skillPoints <= 0)
        {
            Debug.LogWarning($"[ElementalBishop] Not enough SP to cast {tileName}!");
            return;
        }

        // --- ✅ Step 2: Check Cooldown ---
        if (game.turns < nextSkillAvailableTurn)
        {
            int remaining = nextSkillAvailableTurn - game.turns;
            Debug.LogWarning($"[ElementalBishop] Skill on cooldown for {remaining} more turns!");
            return;
        }

        // ✅ Deduct SP
        skillPoints--;
        Debug.Log($"[ElementalBishop] Spent 1 SP. Remaining SP: {skillPoints}");

        // ✅ Set cooldown
        nextSkillAvailableTurn = game.turns + 5;
        Debug.Log($"[ElementalBishop] Skill on cooldown until turn {nextSkillAvailableTurn}");

        // ✅ Destroy old move plates first
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // ✅ Spawn skill move plates
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

    private void SpawnMovePlate(Game game, int x, int y, string tileName)
    {
        float fx = x * 0.66f - 2.3f;
        float fy = y * 0.66f - 2.3f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        mp.AddComponent<SkillEndTurnPlate>().Setup(game, x, y, tileName);
    }

    // Helper methods
    public void InfernalBrand() => CastSkill("tile_lava");
    public void GlacialPath() => CastSkill("tile_ice");
    public void StoneSentinel() => CastSkill("tile_earth");
}
