using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public sealed class SpectrumSlidingDoorHotspot : MonoBehaviour
{
    [Header("Door Panels")]
    public Transform leftDoor;
    public Transform rightDoor;
    public Vector3 leftOpenOffset = new Vector3(-1.3f, 0f, 0f);
    public Vector3 rightOpenOffset = new Vector3(1.3f, 0f, 0f);
    public float openSeconds = 0.75f;

    [Header("Warp")]
    public Transform warpDestination;
    public string destinationRoomName = "Elsewhere";
    public bool updateWalkerBounds;
    public Vector2 destinationMinBounds = new Vector2(-4f, -2.4f);
    public Vector2 destinationMaxBounds = new Vector2(4f, 0.4f);

    [Header("Optional Loop")]
    public AdventureLoopController loopController;

    [Header("Prompt")]
    public Texture2D promptPanelTexture;
    public float promptBottomOffset = 94f;
    public string openPrompt = "OPEN DOOR";
    public string enterPrompt = "ENTER";

    private AdventureActor nearbyActor;
    private Vector3 leftClosedLocalPosition;
    private Vector3 rightClosedLocalPosition;
    private Transform cachedLeftDoor;
    private Transform cachedRightDoor;
    private bool cachedDoorPositions;
    private bool openingStarted;
    private float openAmount;
    private float messageTimer;
    private string messageText;

    public bool IsOpen
    {
        get { return openAmount >= 1f; }
    }

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
        CacheDoorPositions();
        ApplyDoorPositions();
    }

    private void OnValidate()
    {
        openSeconds = Mathf.Max(0.01f, openSeconds);
    }

    private void Update()
    {
        AnimateDoor();
        TickMessage();
        UpdateLoopPrompt();

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || nearbyActor == null || !keyboard.eKey.wasPressedThisFrame)
        {
            return;
        }

        if (loopController != null)
        {
            loopController.ConsumeInteractionThisFrame();
        }

        TryInteract(nearbyActor);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        AdventureActor actor = other.GetComponentInParent<AdventureActor>();
        if (actor == null)
        {
            return;
        }

        nearbyActor = actor;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        AdventureActor actor = other.GetComponentInParent<AdventureActor>();
        if (actor != null && actor == nearbyActor)
        {
            nearbyActor = null;
        }
    }

    public void StartOpening()
    {
        openingStarted = true;
        ShowMessage("The door slides open.");
    }

    public void OpenInstantlyForTests()
    {
        openingStarted = true;
        openAmount = 1f;
        ApplyDoorPositions();
    }

    private void TryInteract(AdventureActor actor)
    {
        if (!openingStarted)
        {
            StartOpening();
            return;
        }

        if (!IsOpen)
        {
            ShowMessage("The door is still opening.");
            return;
        }

        Warp(actor);
    }

    private void AnimateDoor()
    {
        if (!openingStarted || IsOpen)
        {
            return;
        }

        openAmount = Mathf.MoveTowards(openAmount, 1f, Time.deltaTime / openSeconds);
        ApplyDoorPositions();
    }

    private void CacheDoorPositions()
    {
        if (cachedDoorPositions && cachedLeftDoor == leftDoor && cachedRightDoor == rightDoor)
        {
            return;
        }

        if (leftDoor != null)
        {
            leftClosedLocalPosition = leftDoor.localPosition;
        }

        if (rightDoor != null)
        {
            rightClosedLocalPosition = rightDoor.localPosition;
        }

        cachedLeftDoor = leftDoor;
        cachedRightDoor = rightDoor;
        cachedDoorPositions = leftDoor != null || rightDoor != null;
    }

    private void ApplyDoorPositions()
    {
        CacheDoorPositions();

        if (leftDoor != null)
        {
            leftDoor.localPosition = Vector3.Lerp(leftClosedLocalPosition, leftClosedLocalPosition + leftOpenOffset, openAmount);
        }

        if (rightDoor != null)
        {
            rightDoor.localPosition = Vector3.Lerp(rightClosedLocalPosition, rightClosedLocalPosition + rightOpenOffset, openAmount);
        }
    }

    private void Warp(AdventureActor actor)
    {
        if (actor == null || warpDestination == null)
        {
            ShowMessage("No destination wired yet.");
            return;
        }

        Vector3 target = warpDestination.position;
        target.z = actor.transform.position.z;

        Rigidbody2D body = actor.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.position = new Vector2(target.x, target.y);
        }

        actor.transform.position = target;
        actor.roomName = destinationRoomName;

        if (updateWalkerBounds && actor.walker != null)
        {
            actor.walker.minBounds = destinationMinBounds;
            actor.walker.maxBounds = destinationMaxBounds;
        }

        if (loopController != null)
        {
            loopController.SnapCameraToActiveActor();
            loopController.ShowFeedbackMessage("Entered " + destinationRoomName + ".");
        }
        else
        {
            ShowMessage("Entered " + destinationRoomName + ".");
        }
    }

    private void UpdateLoopPrompt()
    {
        if (loopController == null || nearbyActor == null)
        {
            return;
        }

        loopController.SetContextualPrompt(GetPromptText());
    }

    private string GetPromptText()
    {
        if (messageTimer > 0f && !string.IsNullOrEmpty(messageText))
        {
            return messageText;
        }

        return openingStarted && IsOpen ? "E: " + enterPrompt : "E: " + openPrompt;
    }

    private void ShowMessage(string text)
    {
        messageText = text;
        messageTimer = 1.8f;

        if (loopController != null)
        {
            loopController.ShowFeedbackMessage(text);
        }
    }

    private void TickMessage()
    {
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
        }
    }

    private void OnGUI()
    {
        if (loopController != null || nearbyActor == null)
        {
            return;
        }

        string text = GetPromptText();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        float uiScale = Mathf.Clamp(Screen.height / 1080f, 1f, 2f);
        float width = 330f * uiScale;
        float height = 64f * uiScale;
        float promptY = Screen.height - promptBottomOffset * uiScale;
        Rect rect = new Rect((Screen.width - width) * 0.5f, promptY, width, height);
        Color previousColor = GUI.color;
        int previousDepth = GUI.depth;

        GUI.depth = -50;
        GUI.color = Color.white;
        if (promptPanelTexture != null)
        {
            GUI.DrawTexture(rect, promptPanelTexture, ScaleMode.StretchToFill, true);
        }
        else
        {
            GUI.color = new Color(0f, 0f, 0f, 0.78f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
        }

        GUI.color = new Color(0.95f, 0.9f, 0.45f, 1f);
        GUI.Label(new Rect(rect.x + 18f * uiScale, rect.y + 10f * uiScale, rect.width - 36f * uiScale, rect.height - 20f * uiScale), text, GetPromptStyle(uiScale));
        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }

    private static GUIStyle GetPromptStyle(float uiScale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = Mathf.RoundToInt(20f * uiScale);
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(0.95f, 0.9f, 0.45f, 1f);
        return style;
    }
}
