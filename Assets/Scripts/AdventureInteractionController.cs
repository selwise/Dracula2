using UnityEngine;
using UnityEngine.InputSystem;

public sealed class AdventureInteractionController : MonoBehaviour
{
    public AdventureLoopController loopController;
    public AdventureInventory inventory;
    public AdventureInteractionTarget[] targets;
    public Texture2D panelTexture;
    public float promptBottomOffset = 126f;
    public float inventoryBottomOffset = 24f;
    public int inventorySlotCount = 6;

    private AdventureInteractionTarget nearestTarget;
    private string toastText;
    private float toastTimer;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<AdventureInventory>();
        }

        if (loopController == null)
        {
            loopController = GetComponent<AdventureLoopController>();
        }

        RefreshTargetsIfNeeded();
    }

    private void Update()
    {
        RefreshTargetsIfNeeded();
        nearestTarget = FindNearestTarget();

        if (nearestTarget != null && loopController != null)
        {
            loopController.SetContextualPrompt(GetInteractLabel() + ": " + nearestTarget.PromptText);
        }

        if (inventory != null && WasPreviousItemPressed())
        {
            inventory.SelectPrevious();
        }

        if (inventory != null && WasNextItemPressed())
        {
            inventory.SelectNext();
        }

        if (WasUseItemPressed())
        {
            UseSelectedItem();
        }

        if (nearestTarget != null && WasInteractPressed())
        {
            if (loopController != null)
            {
                loopController.ConsumeInteractionThisFrame();
            }

            string message = nearestTarget.Interact(loopController != null ? loopController.ActiveActor : null, inventory);
            ShowToast(message);
        }

        if (toastTimer > 0f)
        {
            toastTimer -= Time.deltaTime;
        }
    }

    private void RefreshTargetsIfNeeded()
    {
        if (targets != null && targets.Length > 0)
        {
            return;
        }

        targets = FindObjectsByType<AdventureInteractionTarget>(FindObjectsInactive.Exclude);
    }

    private AdventureInteractionTarget FindNearestTarget()
    {
        AdventureActor actor = loopController != null ? loopController.ActiveActor : null;
        if (actor == null || targets == null)
        {
            return null;
        }

        AdventureInteractionTarget nearest = null;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < targets.Length; i++)
        {
            AdventureInteractionTarget target = targets[i];
            if (target == null || !target.CanInteract(actor))
            {
                continue;
            }

            float distance = (target.transform.position - actor.transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = target;
            }
        }

        return nearest;
    }

    private void UseSelectedItem()
    {
        if (inventory == null || inventory.SelectedItem == null)
        {
            ShowToast("Inventory is empty.");
            return;
        }

        ShowToast("No use for " + inventory.SelectedItem.displayName + " here.");
    }

    private void ShowToast(string message)
    {
        toastText = message;
        toastTimer = 2.2f;

        if (loopController != null)
        {
            loopController.ShowFeedbackMessage(message);
        }
    }

    private static bool WasInteractPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            return true;
        }

        Gamepad gamepad = Gamepad.current;
        return gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;
    }

    private static bool WasUseItemPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.uKey.wasPressedThisFrame)
        {
            return true;
        }

        Gamepad gamepad = Gamepad.current;
        return gamepad != null && gamepad.buttonWest.wasPressedThisFrame;
    }

    private static bool WasPreviousItemPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.zKey.wasPressedThisFrame)
        {
            return true;
        }

        Gamepad gamepad = Gamepad.current;
        return gamepad != null && (gamepad.leftShoulder.wasPressedThisFrame || gamepad.dpad.left.wasPressedThisFrame);
    }

    private static bool WasNextItemPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.xKey.wasPressedThisFrame)
        {
            return true;
        }

        Gamepad gamepad = Gamepad.current;
        return gamepad != null && (gamepad.rightShoulder.wasPressedThisFrame || gamepad.dpad.right.wasPressedThisFrame);
    }

    private static string GetInteractLabel()
    {
        return Gamepad.current != null ? "A" : "E";
    }

    private static string GetUseLabel()
    {
        return Gamepad.current != null ? "X" : "U";
    }

    private static string GetCycleLabel()
    {
        return Gamepad.current != null ? "LB/RB" : "Z/X";
    }

    private void OnGUI()
    {
        Color previousColor = GUI.color;
        float uiScale = Mathf.Clamp(Screen.height / 1080f, 0.9f, 1.7f);

        DrawPrompt(uiScale);
        DrawInventory(uiScale);
        GUI.color = previousColor;
    }

    private void DrawPrompt(float uiScale)
    {
        string prompt = null;
        if (toastTimer > 0f && !string.IsNullOrEmpty(toastText))
        {
            prompt = toastText;
        }
        else if (nearestTarget != null)
        {
            prompt = GetInteractLabel() + "  " + nearestTarget.PromptText;
        }

        if (string.IsNullOrEmpty(prompt))
        {
            return;
        }

        Rect rect = new Rect(
            (Screen.width - 430f * uiScale) * 0.5f,
            Screen.height - promptBottomOffset * uiScale,
            430f * uiScale,
            62f * uiScale);

        DrawPanel(rect, uiScale);
        GUI.color = new Color(0.96f, 0.88f, 0.32f, 1f);
        GUI.Label(InnerRect(rect, 18f * uiScale), prompt.ToUpperInvariant(), PromptStyle(uiScale));
    }

    private void DrawInventory(float uiScale)
    {
        if (inventory == null || inventory.Count == 0)
        {
            return;
        }

        int visibleSlots = Mathf.Max(1, inventorySlotCount);
        float slot = 44f * uiScale;
        float gap = 6f * uiScale;
        float width = visibleSlots * slot + (visibleSlots - 1) * gap + 26f * uiScale;
        float height = 82f * uiScale;
        Rect panel = new Rect(18f * uiScale, Screen.height - inventoryBottomOffset * uiScale - height, width, height);

        DrawPanel(panel, uiScale);

        for (int i = 0; i < visibleSlots; i++)
        {
            Rect slotRect = new Rect(panel.x + 13f * uiScale + i * (slot + gap), panel.y + 12f * uiScale, slot, slot);
            DrawSlot(slotRect, i, uiScale);
        }

        AdventureInventoryItem selected = inventory.SelectedItem;
        string itemName = selected != null ? selected.displayName : "Empty";
        GUI.color = new Color(0.82f, 0.91f, 0.95f, 1f);
        GUI.Label(
            new Rect(panel.x + 14f * uiScale, panel.y + 58f * uiScale, panel.width - 28f * uiScale, 20f * uiScale),
            (GetCycleLabel() + " ITEM   " + GetUseLabel() + " USE   " + itemName).ToUpperInvariant(),
            InventoryHintStyle(uiScale));
    }

    private void DrawSlot(Rect rect, int index, float uiScale)
    {
        bool selected = index == inventory.selectedIndex;
        GUI.color = selected ? new Color(0.98f, 0.81f, 0.25f, 1f) : new Color(0.13f, 0.23f, 0.28f, 1f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        Rect inner = InnerRect(rect, 3f * uiScale);
        GUI.color = new Color(0.025f, 0.03f, 0.04f, 0.96f);
        GUI.DrawTexture(inner, Texture2D.whiteTexture);

        if (index >= inventory.items.Count || inventory.items[index] == null)
        {
            return;
        }

        AdventureInventoryItem item = inventory.items[index];
        if (item.icon != null)
        {
            GUI.color = Color.white;
            GUI.DrawTextureWithTexCoords(InnerRect(inner, 4f * uiScale), item.icon.texture, SpriteTexCoords(item.icon), true);
        }
        else
        {
            GUI.color = new Color(0.96f, 0.88f, 0.32f, 1f);
            GUI.Label(inner, item.displayName.Substring(0, Mathf.Min(1, item.displayName.Length)), PromptStyle(uiScale));
        }

        if (item.count > 1)
        {
            GUI.color = new Color(0.96f, 0.88f, 0.32f, 1f);
            GUI.Label(new Rect(rect.x, rect.y + rect.height - 18f * uiScale, rect.width - 3f * uiScale, 16f * uiScale), item.count.ToString(), CountStyle(uiScale));
        }
    }

    private void DrawPanel(Rect rect, float uiScale)
    {
        if (panelTexture != null)
        {
            GUI.color = Color.white;
            GUI.DrawTexture(rect, panelTexture, ScaleMode.StretchToFill, true);
            return;
        }

        GUI.color = new Color(0.015f, 0.02f, 0.03f, 0.88f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = new Color(0.08f, 0.75f, 0.84f, 1f);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f * uiScale), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - 2f * uiScale, rect.width, 2f * uiScale), Texture2D.whiteTexture);
    }

    private static Rect InnerRect(Rect rect, float inset)
    {
        return new Rect(rect.x + inset, rect.y + inset, rect.width - inset * 2f, rect.height - inset * 2f);
    }

    private static Rect SpriteTexCoords(Sprite sprite)
    {
        Rect rect = sprite.textureRect;
        Texture texture = sprite.texture;
        return new Rect(rect.x / texture.width, rect.y / texture.height, rect.width / texture.width, rect.height / texture.height);
    }

    private static GUIStyle PromptStyle(float uiScale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = Mathf.RoundToInt(18f * uiScale);
        style.fontStyle = FontStyle.Bold;
        style.wordWrap = true;
        style.normal.textColor = new Color(0.96f, 0.88f, 0.32f, 1f);
        return style;
    }

    private static GUIStyle InventoryHintStyle(float uiScale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleLeft;
        style.fontSize = Mathf.RoundToInt(11f * uiScale);
        style.normal.textColor = new Color(0.82f, 0.91f, 0.95f, 1f);
        return style;
    }

    private static GUIStyle CountStyle(float uiScale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.LowerRight;
        style.fontSize = Mathf.RoundToInt(12f * uiScale);
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(0.96f, 0.88f, 0.32f, 1f);
        return style;
    }
}
