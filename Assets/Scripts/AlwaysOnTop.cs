using UnityEngine;

/// <summary>
/// Simple script that makes a GameObject and all its children render on top of everything else
/// Place this on an empty GameObject and add your UI panels as children
/// </summary>
public class AlwaysOnTop : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How far in front should this object be rendered? (higher = more in front)")]
    public float zOffset = 10f;
    
    [Tooltip("Should this affect all children?")]
    public bool affectChildren = true;
    
    [Tooltip("Should this update every frame? (disable if positions don't change)")]
    public bool updateEveryFrame = false;

    private void Start()
    {
        SetAlwaysOnTop();
    }

    private void Update()
    {
        if (updateEveryFrame)
        {
            SetAlwaysOnTop();
        }
    }

    /// <summary>
    /// Forces this object and its children to render on top
    /// </summary>
    public void SetAlwaysOnTop()
    {
        // Set this object's position
        Vector3 pos = transform.position;
        pos.z = -zOffset; // Negative z means closer to camera
        transform.position = pos;

        // Set all children's positions if enabled
        if (affectChildren)
        {
            SetChildrenZPosition(transform, -zOffset);
        }
    }

    /// <summary>
    /// Recursively sets z-position for all children
    /// </summary>
    private void SetChildrenZPosition(Transform parent, float zValue)
    {
        foreach (Transform child in parent)
        {
            Vector3 childPos = child.position;
            childPos.z = zValue;
            child.position = childPos;
            
            // Recursively set children of children
            SetChildrenZPosition(child, zValue);
        }
    }

    /// <summary>
    /// Call this method to manually update positions (useful for UI buttons)
    /// </summary>
    public void ForceUpdate()
    {
        SetAlwaysOnTop();
    }

    /// <summary>
    /// Reset to original z-position (useful for debugging)
    /// </summary>
    public void ResetZPosition()
    {
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;

        if (affectChildren)
        {
            SetChildrenZPosition(transform, 0f);
        }
    }
}
