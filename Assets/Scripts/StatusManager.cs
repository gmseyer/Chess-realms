using System.Collections.Generic;
using UnityEngine;

public enum StatusType
{
    Invulnerable,
    Ghost,
    Phase,
    Locked,
    Stunned,
    Ethereal,
    Soulbrand,

    specialTile, //status for special tiles like lava
     SolidBlock //status for tile_earth
}
   
public class StatusManager : MonoBehaviour
{
    private class Status
    {
        public StatusType type;
        public int expiresOnTurn; // turn when this status ends
    }

    private List<Status> activeStatuses = new List<Status>();
    
    // Soulbrand stack tracking
    private int soulbrandStacks = 0;

    public void AddStatus(StatusType type, int expiresOnTurn)
    {
        activeStatuses.Add(new Status { type = type, expiresOnTurn = expiresOnTurn });
        Debug.Log($"{gameObject.name} gained status {type} until turn {expiresOnTurn}");
    }

    public void RemoveStatus(StatusType type)
    {
        Debug.Log($"[StatusManager] RemoveStatus called on {gameObject.name} for status {type}");
        bool hadStatus = activeStatuses.Exists(s => s.type == type);
        activeStatuses.RemoveAll(s => s.type == type);
        
        // Check if ethereal status was removed from a Bishop
        if (hadStatus && type == StatusType.Ethereal && gameObject.name.Contains("bishop") && !gameObject.name.Contains("royal"))
        {
            Debug.Log($"[StatusManager] {gameObject.name} ethereal status removed - destroying Bishop for failing promotion!");
            // Get the game controller
            Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
            if (game != null)
            {
                // Clear the position (using same method as Sacrifice)
                Chessman chessman = gameObject.GetComponent<Chessman>();
                if (chessman != null)
                {
                    game.ClearPosition(chessman.GetXBoard(), chessman.GetYBoard());
                }
                // Destroy the Bishop GameObject (using same method as Sacrifice)
                Destroy(gameObject);
            }
        }
    }

    public bool HasStatus(StatusType type, int currentTurn)
    {
        // Check for ethereal status on Bishop before any processing
        bool hadEthereal = activeStatuses.Exists(s => s.type == StatusType.Ethereal);
        bool etherealExpired = activeStatuses.Exists(s => s.type == StatusType.Ethereal && s.expiresOnTurn < currentTurn);
        
        // Check for expired ethereal status on Bishop before auto-removal
        if (type == StatusType.Ethereal && gameObject.name.Contains("bishop") && !gameObject.name.Contains("royal"))
        {
            Debug.Log($"[StatusManager] HasStatus check on {gameObject.name}: hadEthereal={hadEthereal}, etherealExpired={etherealExpired}, currentTurn={currentTurn}");
            
            if (hadEthereal && etherealExpired)
            {
                Debug.Log($"[StatusManager] {gameObject.name} ethereal status expired - destroying Bishop for failing promotion!");
                // Get the game controller
                Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
                if (game != null)
                {
                    // Clear the position (using same method as Sacrifice)
                    Chessman chessman = gameObject.GetComponent<Chessman>();
                    if (chessman != null)
                    {
                        game.ClearPosition(chessman.GetXBoard(), chessman.GetYBoard());
                    }
                    // Destroy the Bishop GameObject (using same method as Sacrifice)
                    Destroy(gameObject);
                    return false; // Status no longer exists
                }
            }
        }
        
        // Auto-remove expired statuses (after checking for ethereal expiration)
        // Check for ethereal expiration before removing
        if (hadEthereal && etherealExpired && gameObject.name.Contains("bishop") && !gameObject.name.Contains("royal"))
        {
            Debug.Log($"[StatusManager] {gameObject.name} ethereal status expired during auto-removal - destroying Bishop for failing promotion!");
            // Get the game controller
            Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
            if (game != null)
            {
                // Clear the position (using same method as Sacrifice)
                Chessman chessman = gameObject.GetComponent<Chessman>();
                if (chessman != null)
                {
                    game.ClearPosition(chessman.GetXBoard(), chessman.GetYBoard());
                }
                // Destroy the Bishop GameObject (using same method as Sacrifice)
                Destroy(gameObject);
                return false; // Status no longer exists
            }
        }
        
        activeStatuses.RemoveAll(s => s.expiresOnTurn < currentTurn);

        return activeStatuses.Exists(s => s.type == type);
    }

    public void ClearAll()
    {
        activeStatuses.Clear();
        soulbrandStacks = 0;
    }

    // Soulbrand stack management
    public void AddSoulbrandStack()
    {
        soulbrandStacks++;
        Debug.Log($"{gameObject.name} gained 1 soulbrand stack (total: {soulbrandStacks})");
        
        // Check if piece should be destroyed
        if (soulbrandStacks >= 3)
        {
            Debug.Log($"{gameObject.name} destroyed by 3 soulbrand stacks!");
            // The destruction will be handled by the calling code
        }
    }

    public int GetSoulbrandStacks()
    {
        return soulbrandStacks;
    }

    public void ClearSoulbrandStacks()
    {
        soulbrandStacks = 0;
        Debug.Log($"{gameObject.name} soulbrand stacks cleared");
    }



    
}
