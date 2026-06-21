using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class AdventureActor : MonoBehaviour
{
    public AdventureCharacter character;
    public string roomName = "Unknown Room";
    public DraculaWalker walker;
    public SpriteRenderer spriteRenderer;
    public Color activeTint = Color.white;
    public Color inactiveTint = new Color(0.62f, 0.62f, 0.68f, 1f);
    private float portalLockedUntil;

    public Vector3 CameraPosition
    {
        get { return transform.position; }
    }

    private void Awake()
    {
        CacheReferences();
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    public void ApplyControl(bool isActive, bool movementAllowed)
    {
        CacheReferences();

        if (walker != null)
        {
            walker.inputEnabled = isActive && movementAllowed;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = isActive ? activeTint : inactiveTint;
        }
    }

    public bool CanUsePortal
    {
        get { return Time.time >= portalLockedUntil; }
    }

    public void LockPortals(float duration)
    {
        portalLockedUntil = Mathf.Max(portalLockedUntil, Time.time + Mathf.Max(0f, duration));
    }

    private void CacheReferences()
    {
        if (walker == null)
        {
            walker = GetComponent<DraculaWalker>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
