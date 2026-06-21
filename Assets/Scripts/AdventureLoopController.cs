using UnityEngine;
using UnityEngine.InputSystem;

public sealed class AdventureLoopController : MonoBehaviour
{
    [Header("Actors")]
    public AdventureActor dracula;
    public AdventureActor renfield;
    public bool startDayWithRenfield;
    public bool draculaSleepsDuringDay;

    [Header("Day Support")]
    public AdventureActionStation[] renfieldStations;

    [Header("Camera")]
    public Camera sceneCamera;
    public Vector3 cameraOffset = new Vector3(0.72f, 2.45f, -10f);
    public float cameraFollowSpeed = 7f;

    [Header("UI")]
    public Texture2D panelTexture;
    public float uiScaleMin = 0.85f;
    public float uiScaleMax = 1.4f;

    private readonly AdventureLoopState state = new AdventureLoopState();
    private AdventureActionStation nearestStation;
    private string feedbackText;
    private float feedbackTimer;
    private string contextualPrompt;
    private int contextualPromptFrame = -1;
    private int interactionConsumedFrame = -1;

    public AdventureLoopState State
    {
        get { return state; }
    }

    public AdventureActor ActiveActor
    {
        get { return GetActiveActor(); }
    }

    private void Awake()
    {
        if (sceneCamera == null)
        {
            sceneCamera = Camera.main;
        }

        if (startDayWithRenfield)
        {
            state.SetActiveCharacter(AdventureCharacter.Renfield);
        }

        ApplyControlState();
        RefreshStationState();
        ShowFeedback(GetInitialFeedback());
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.tabKey.wasPressedThisFrame)
            {
                state.SwitchActiveCharacter();
                ApplyControlState();
                ShowFeedback("Control switched to " + GetCharacterName(state.ActiveCharacter) + ".");
            }

            if (keyboard.nKey.wasPressedThisFrame)
            {
                AdvancePhase();
            }

            if (keyboard.eKey.wasPressedThisFrame)
            {
                if (interactionConsumedFrame != Time.frameCount)
                {
                    TryUseNearestStation();
                }
            }
        }

        nearestStation = FindNearestStation();
        RefreshStationState();
        TickFeedback();
        ApplyControlState();
    }

    private void LateUpdate()
    {
        if (sceneCamera == null)
        {
            return;
        }

        AdventureActor activeActor = GetActiveActor();
        if (activeActor == null)
        {
            return;
        }

        Vector3 desiredPosition = activeActor.CameraPosition + cameraOffset;
        desiredPosition.z = cameraOffset.z;
        sceneCamera.transform.position = Vector3.Lerp(
            sceneCamera.transform.position,
            desiredPosition,
            1f - Mathf.Exp(-cameraFollowSpeed * Time.deltaTime));
    }

    private void AdvancePhase()
    {
        state.AdvancePhase();

        if (state.Phase == AdventurePhase.Day)
        {
            state.SetActiveCharacter(AdventureCharacter.Renfield);
        }
        else if (state.Phase == AdventurePhase.Night)
        {
            state.SetActiveCharacter(AdventureCharacter.Dracula);
        }

        ApplyControlState();
        RefreshStationState();
        ShowFeedback(GetPhaseFeedback());
    }

    private void TryUseNearestStation()
    {
        if (state.ActiveCharacter != AdventureCharacter.Renfield)
        {
            ShowFeedback("Renfield must handle daytime preparations.");
            return;
        }

        if (nearestStation == null)
        {
            ShowFeedback("Move Renfield near a task and press E.");
            return;
        }

        if (state.Phase != AdventurePhase.Day)
        {
            ShowFeedback("Renfield preparations happen during the day.");
            return;
        }

        if (state.HasPerformedRenfieldAction(nearestStation.action))
        {
            ShowFeedback(nearestStation.displayName + " is already done today.");
            return;
        }

        if (state.RenfieldActionsRemaining <= 0)
        {
            ShowFeedback("Renfield has no daytime actions left.");
            return;
        }

        if (state.TryPerformRenfieldAction(nearestStation.action))
        {
            ShowFeedback("Renfield: " + nearestStation.displayName + ".");
        }

        RefreshStationState();
    }

    private AdventureActionStation FindNearestStation()
    {
        if (state.ActiveCharacter != AdventureCharacter.Renfield || renfield == null || renfieldStations == null)
        {
            return null;
        }

        AdventureActionStation nearest = null;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < renfieldStations.Length; i++)
        {
            AdventureActionStation station = renfieldStations[i];
            if (station == null || !station.IsInRange(renfield.transform))
            {
                continue;
            }

            float distance = (station.transform.position - renfield.transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = station;
            }
        }

        return nearest;
    }

    private void ApplyControlState()
    {
        ApplyActorControl(dracula);
        ApplyActorControl(renfield);
    }

    private void ApplyActorControl(AdventureActor actor)
    {
        if (actor == null)
        {
            return;
        }

        bool active = actor.character == state.ActiveCharacter;
        bool canMove = active && CanMove(actor);
        actor.ApplyControl(active, canMove);
    }

    private bool CanMove(AdventureActor actor)
    {
        if (actor.character == AdventureCharacter.Dracula && draculaSleepsDuringDay && state.Phase == AdventurePhase.Day)
        {
            return false;
        }

        return true;
    }

    public bool IsActiveActor(AdventureActor actor)
    {
        return actor != null && actor == GetActiveActor();
    }

    public void SnapCameraToActiveActor()
    {
        if (sceneCamera == null)
        {
            return;
        }

        AdventureActor activeActor = GetActiveActor();
        if (activeActor == null)
        {
            return;
        }

        Vector3 desiredPosition = activeActor.CameraPosition + cameraOffset;
        desiredPosition.z = cameraOffset.z;
        sceneCamera.transform.position = desiredPosition;
    }

    public void SetContextualPrompt(string text)
    {
        contextualPrompt = text;
        contextualPromptFrame = Time.frameCount;
    }

    public void ConsumeInteractionThisFrame()
    {
        interactionConsumedFrame = Time.frameCount;
    }

    public void ShowFeedbackMessage(string text)
    {
        ShowFeedback(text);
    }

    private AdventureActor GetActiveActor()
    {
        return state.ActiveCharacter == AdventureCharacter.Dracula ? dracula : renfield;
    }

    private void RefreshStationState()
    {
        if (renfieldStations == null)
        {
            return;
        }

        for (int i = 0; i < renfieldStations.Length; i++)
        {
            AdventureActionStation station = renfieldStations[i];
            if (station != null)
            {
                station.SetCompleted(state.HasPerformedRenfieldAction(station.action));
            }
        }
    }

    private void ShowFeedback(string text)
    {
        feedbackText = text;
        feedbackTimer = 2.8f;
    }

    private void TickFeedback()
    {
        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;
        }
    }

    private string GetPhaseFeedback()
    {
        switch (state.Phase)
        {
            case AdventurePhase.Day:
                return "Day " + state.DayNumber + ": Renfield prepares the castle.";
            case AdventurePhase.Dusk:
                return "Dusk: review preparations before night.";
            case AdventurePhase.Night:
                return "Night: Dracula wakes.";
            default:
                return "Dawn: return to safety before the next day.";
        }
    }

    private string GetInitialFeedback()
    {
        if (state.ActiveCharacter == AdventureCharacter.Renfield)
        {
            return "Day " + state.DayNumber + ": Renfield prepares the castle.";
        }

        return "Day " + state.DayNumber + ": Dracula explores the castle.";
    }

    private static string GetCharacterName(AdventureCharacter character)
    {
        return character == AdventureCharacter.Dracula ? "Dracula" : "Renfield";
    }

    private void OnGUI()
    {
        float uiScale = Mathf.Clamp(Screen.height / 1080f, uiScaleMin, uiScaleMax);
        Rect panel = new Rect(18f * uiScale, 18f * uiScale, 430f * uiScale, 184f * uiScale);

        int previousDepth = GUI.depth;
        Color previousColor = GUI.color;
        GUI.depth = -80;

        if (panelTexture != null)
        {
            GUI.color = Color.white;
            GUI.DrawTexture(panel, panelTexture, ScaleMode.StretchToFill, true);
        }
        else
        {
            GUI.color = new Color(0.025f, 0.018f, 0.022f, 0.86f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
        }

        float x = panel.x + 18f * uiScale;
        float y = panel.y + 14f * uiScale;
        float width = panel.width - 36f * uiScale;
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y, width, 28f * uiScale), "Day " + state.DayNumber + " / " + state.Phase, HeaderStyle(uiScale));

        y += 34f * uiScale;
        AdventureActor activeActor = GetActiveActor();
        string room = activeActor != null ? activeActor.roomName : "Unknown Room";
        GUI.Label(new Rect(x, y, width, 24f * uiScale), "Active: " + GetCharacterName(state.ActiveCharacter) + " / " + room, BodyStyle(uiScale));

        y += 27f * uiScale;
        GUI.Label(new Rect(x, y, width, 24f * uiScale), "Renfield actions: " + state.RenfieldActionsRemaining + " / " + AdventureLoopState.RenfieldActionsPerDay, BodyStyle(uiScale));

        y += 29f * uiScale;
        GUI.Label(new Rect(x, y, width, 24f * uiScale), "Tab switch   E use task   N next phase", HintStyle(uiScale));

        y += 26f * uiScale;
        string prompt = GetPromptText();
        GUI.Label(new Rect(x, y, width, 44f * uiScale), prompt, BodyStyle(uiScale));

        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }

    private string GetPromptText()
    {
        if (feedbackTimer > 0f && !string.IsNullOrEmpty(feedbackText))
        {
            return feedbackText;
        }

        if (state.ActiveCharacter == AdventureCharacter.Dracula && state.Phase == AdventurePhase.Day && draculaSleepsDuringDay)
        {
            return "Dracula sleeps by day. Switch to Renfield for preparations.";
        }

        if (nearestStation != null)
        {
            return "E: " + nearestStation.displayName + " - " + nearestStation.description;
        }

        if (contextualPromptFrame == Time.frameCount && !string.IsNullOrEmpty(contextualPrompt))
        {
            return contextualPrompt;
        }

        return GetPhaseFeedback();
    }

    private static GUIStyle HeaderStyle(float uiScale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = Mathf.RoundToInt(22f * uiScale);
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(0.95f, 0.88f, 0.54f, 1f);
        return style;
    }

    private static GUIStyle BodyStyle(float uiScale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = Mathf.RoundToInt(16f * uiScale);
        style.wordWrap = true;
        style.normal.textColor = new Color(0.92f, 0.88f, 0.82f, 1f);
        return style;
    }

    private static GUIStyle HintStyle(float uiScale)
    {
        GUIStyle style = BodyStyle(uiScale);
        style.fontSize = Mathf.RoundToInt(14f * uiScale);
        style.normal.textColor = new Color(0.74f, 0.76f, 0.82f, 1f);
        return style;
    }
}
