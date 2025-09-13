using UnityEngine;

using UnityEngine;

public class PanoAnchor : MonoBehaviour
{
    public string anchorId;
    public Cubemap cubemap;

    [Range(0,360)] public float yaw;          // per-image yaw we found
    [Header("Optional (matches your shader)")]
    public float stretchX = 1f, stretchY = 1f, stretchZ = 1f;
}

