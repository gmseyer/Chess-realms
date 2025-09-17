using UnityEngine;
using System.Collections.Generic;
using TMPro;


public class ElementalBishop : MonoBehaviour
{



    public GameObject movePlatePrefab;
    public int turns; // start at 1 (or 0 if you prefer)

    public int skillPoints = 3; // ✅ Bishop-specific SP (can refill later)
    private int nextSkillAvailableTurn = 0; // ✅ Cooldown tracker

    private Dictionary<string, int> skillCooldowns = new Dictionary<string, int>();

    // TMP UI references (drag in from inspector)
    public TMP_Text infernalBrandCooldownText;
    public TMP_Text glacialPathCooldownText;
    public TMP_Text stoneSentinelCooldownText;

    private Game game;

    private List<ActiveTile> activeTiles = new List<ActiveTile>();
    public int tileDuration = 5; // ✅ configurable duration for tiles

     // Helper methods
    public void InfernalBrand() => CastSkill("tile_lava");
    public void GlacialPath() => CastSkill("tile_ice");
    public void StoneSentinel() => CastSkill("tile_earth");
    


    private void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }






    // Main skill casting function
    public void CastSkill(string tileName)
    {
        if (game == null)
        {
            Debug.LogError("[ElementalBishop] Game reference is missing!");
            return;
        }

        int currentTurn = game.GetTurnCount(); // assumes Game has a turn counter
        if (skillCooldowns.TryGetValue(tileName, out int availableTurn) && currentTurn < availableTurn)
        {
            Debug.Log($"[ElementalBishop] {tileName} is still on cooldown! Available on turn {availableTurn}.");
            return;
        }

        // ✅ Deduct Skill Point
        // ✅ Deduct Skill Point using SkillManager (not game)
if (!SkillManager.Instance.SpendPlayerSP("white", 1)) // assuming white is the player
{
    Debug.LogWarning("[ElementalBishop] Not enough Skill Points!");
    return;
}


        // ✅ Put skill on cooldown for 5 turns
        skillCooldowns[tileName] = currentTurn + 5;
        UpdateCooldownUI();

        // ✅ Spawn move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (game.GetPosition(x, y) == null)
                    SpawnMovePlate(game, x, y, tileName);
            }
        }

        Debug.Log($"[ElementalBishop] {tileName} skill activated (cooldown until turn {skillCooldowns[tileName]}).");
    }

    private void UpdateCooldownUI()
    {
        int currentTurn = game.GetTurnCount();

        infernalBrandCooldownText.text = GetCooldownText("tile_lava", currentTurn);
        glacialPathCooldownText.text = GetCooldownText("tile_ice", currentTurn);
        stoneSentinelCooldownText.text = GetCooldownText("tile_earth", currentTurn);
    }

    private string GetCooldownText(string skillName, int currentTurn)
    {
        if (!skillCooldowns.TryGetValue(skillName, out int availableTurn))
            return ""; // no cooldown yet

        int remaining = availableTurn - currentTurn;
        return (remaining > 0) ? $"COOLDOWN: {remaining}" : "";
    }


    private void SpawnMovePlate(Game game, int x, int y, string tileName)
    {
        float fx = x * 0.57f - 1.98f;
    float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        mp.AddComponent<SkillEndTurnPlate>().Setup(game, x, y, tileName);
    }

   

    private class ActiveTile
{
    public GameObject tileObject;
    public int expireTurn;
}

public void RegisterTile(GameObject tile)
{
    int expireOn = game.GetTurnCount() + tileDuration;
    activeTiles.Add(new ActiveTile { tileObject = tile, expireTurn = expireOn });
    Debug.Log($"[ElementalBishop] Registered {tile.name}, will expire on turn {expireOn}");
}

public void CheckAndDestroyExpiredTiles()
{
    int currentTurn = game.GetTurnCount();
    for (int i = activeTiles.Count - 1; i >= 0; i--)
    {
        if (currentTurn >= activeTiles[i].expireTurn)
        {
            if (activeTiles[i].tileObject != null)
            {
                Debug.Log($"[ElementalBishop] Destroying expired tile {activeTiles[i].tileObject.name}");
                Destroy(activeTiles[i].tileObject);
            }
            activeTiles.RemoveAt(i);
        }
    }
}



}
