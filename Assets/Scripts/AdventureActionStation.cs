using UnityEngine;

public sealed class AdventureActionStation : MonoBehaviour
{
    public RenfieldAction action;
    public string displayName = "Renfield Task";
    [TextArea]
    public string description = "Prepare the castle before nightfall.";
    public float interactRadius = 0.9f;
    public SpriteRenderer spriteRenderer;
    public Color availableTint = new Color(0.86f, 0.76f, 0.38f, 1f);
    public Color completedTint = new Color(0.38f, 0.43f, 0.48f, 1f);

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

    public void SetCompleted(bool completed)
    {
        CacheReferences();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = completed ? completedTint : availableTint;
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
        Gizmos.color = availableTint;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
