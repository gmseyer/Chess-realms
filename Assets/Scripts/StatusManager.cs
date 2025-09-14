using System.Collections.Generic;
using UnityEngine;

public enum StatusType
{
    Invulnerable,
    Ghost,
    Phase,
    Locked,

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

    public void AddStatus(StatusType type, int expiresOnTurn)
    {
        activeStatuses.Add(new Status { type = type, expiresOnTurn = expiresOnTurn });
        Debug.Log($"{gameObject.name} gained status {type} until turn {expiresOnTurn}");
    }

    public void RemoveStatus(StatusType type)
    {
        activeStatuses.RemoveAll(s => s.type == type);
    }

    public bool HasStatus(StatusType type, int currentTurn)
    {
        // Auto-remove expired statuses
        activeStatuses.RemoveAll(s => s.expiresOnTurn < currentTurn);

        return activeStatuses.Exists(s => s.type == type);
    }

    public void ClearAll()
    {
        activeStatuses.Clear();
    }



    
}
