using UnityEngine;

public enum AdventureInteractionAction
{
    Inspect,
    Pickup
}

public sealed class AdventureInteractionTarget : MonoBehaviour
{
    public AdventureInteractionAction action = AdventureInteractionAction.Inspect;
    public bool anyCharacter = true;
    public AdventureCharacter requiredCharacter = AdventureCharacter.Dracula;
    public string displayName = "Object";
    public string verb = "LOOK";
    [TextArea]
    public string description = "There is nothing unusual here.";
    public float interactRadius = 0.85f;
    public AdventureInteractionReceiver receiver;

    [Header("Pickup")]
    public string itemId = "item";
    public string itemName = "Item";
    public Sprite itemIcon;
    public bool hideWhenPickedUp = true;

    private bool consumed;

    public bool CanInteract(AdventureActor actor)
    {
        if (!isActiveAndEnabled || consumed || actor == null)
        {
            return false;
        }

        if (!anyCharacter && actor.character != requiredCharacter)
        {
            return false;
        }

        Vector2 delta = actor.transform.position - transform.position;
        return delta.sqrMagnitude <= interactRadius * interactRadius;
    }

    public string PromptText
    {
        get { return verb + " " + displayName; }
    }

    public string Interact(AdventureActor actor, AdventureInventory inventory)
    {
        if (receiver == null)
        {
            receiver = GetComponent<AdventureInteractionReceiver>();
        }

        if (receiver != null)
        {
            return receiver.Interact(actor, inventory);
        }

        if (action == AdventureInteractionAction.Pickup)
        {
            if (inventory == null)
            {
                return "No inventory is available.";
            }

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            Sprite icon = itemIcon != null ? itemIcon : spriteRenderer != null ? spriteRenderer.sprite : null;
            inventory.AddItem(itemId, itemName, icon);
            consumed = true;

            if (hideWhenPickedUp)
            {
                gameObject.SetActive(false);
            }

            return "Picked up " + itemName + ".";
        }

        return description;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.96f, 0.78f, 0.22f, 1f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
