using UnityEngine;

public sealed class ScholomanceMirrorStation : MonoBehaviour
{
    public string displayName = "Scholomance Mirror";
    [TextArea]
    public string description = "Consult Dracula's old school for forbidden counsel.";
    public float interactRadius = 1.15f;
    public SpriteRenderer spriteRenderer;
    public Color dormantTint = new Color(0.25f, 0.48f, 0.62f, 0.68f);
    public Color consultedTint = new Color(0.62f, 0.78f, 0.88f, 0.92f);

    private void Awake()
    {
        CacheReferences();
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    public bool IsInRange(Transform actor)
    {
        if (actor == null)
        {
            return false;
        }

        Vector2 delta = actor.position - transform.position;
        return delta.sqrMagnitude <= interactRadius * interactRadius;
    }

    public void SetConsulted(bool consulted)
    {
        CacheReferences();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = consulted ? consultedTint : dormantTint;
        }
    }

    private void CacheReferences()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = consultedTint;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
