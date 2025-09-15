using UnityEngine;

/// <summary>
/// Base class for all piece-specific scripts (Rook, Bishop, Knight, etc.)
/// Use this to share common variables and helper functions.
/// </summary>
public class Pieces : MonoBehaviour
{
    // Example: A flag to track if this piece has already used its skill
    protected bool hasUsedSkill = false;

    // Example: Name of the player that owns this piece (optional, use Chessman.GetPlayer() if you prefer)
    protected string owner;

    /// <summary>
    /// Sets the owner of this piece (optional).
    /// </summary>
    public virtual void SetOwner(string player)
    {
        owner = player;
    }

    /// <summary>
    /// Returns the owner (or null if not set).
    /// </summary>
    public virtual string GetOwner()
    {
        return owner;
    }

    /// <summary>
    /// Resets any per-turn flags (override in child classes if needed).
    /// </summary>
    public virtual void ResetTurnFlags()
    {
        // Example: allow skill use again next turn if needed
        // hasUsedSkill = false;
    }

    /// <summary>
    /// Base skill trigger (optional, child classes can override this).
    /// </summary>
    public virtual void UseSkill()
    {
        Debug.Log($"[Pieces] {gameObject.name} used a generic skill (override me!)");
    }
}
