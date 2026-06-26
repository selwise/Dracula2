using UnityEngine;
using UnityEngine.InputSystem;

public sealed class AdventureLoopController : MonoBehaviour
{
    [Header("Actors")]
    public AdventureActor dracula;
    public AdventureActor renfield;
    public bool startDayWithRenfield;
    public bool draculaSleepsDuringDay;
    public bool allowEmergencyDayWake = true;
    public float emergencyWakeSeconds = 9f;
    public int emergencyWakeUsesPerDay = 1;
    public float emergencyDraculaMoveSpeedMultiplier = 0.55f;

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
    private float emergencyWakeTimer;
    private int emergencyWakeUsesRemaining;
    private float draculaBaseMoveSpeed;
    private bool hasDraculaBaseMoveSpeed;
    private Vector3 draculaStartPosition;
    private Vector3 renfieldStartPosition;
    private string draculaStartRoomName;
    private string renfieldStartRoomName;
    private Vector2 draculaStartMinBounds;
    private Vector2 draculaStartMaxBounds;
    private Vector2 renfieldStartMinBounds;
    private Vector2 renfieldStartMaxBounds;
    private bool hasInitialActorState;
    private string phaseCardTitle;
    private string phaseCardSubtitle;
    private float phaseCardTimer;
    private int playtestIntruderJumpIndex = -1;
    private const float PhaseCardDuration = 1.85f;

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

        CacheInitialActorState();

        if (startDayWithRenfield)
        {
            state.SetActiveCharacter(AdventureCharacter.Renfield);
        }

        emergencyWakeUsesRemaining = Mathf.Max(0, emergencyWakeUsesPerDay);
        CacheDraculaBaseMoveSpeed();
        ApplyControlState();
        RefreshStationState();
        ShowFeedback(GetInitialFeedback());
        ShowPhaseCard("DAY 1", GetObjectiveText());
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.tabKey.wasPressedThisFrame)
            {
                TrySwitchActiveCharacter();
            }

            if (keyboard.qKey.wasPressedThisFrame)
            {
                TryEmergencyWakeDracula();
            }

            if (keyboard.nKey.wasPressedThisFrame)
            {
                AdvancePhase();
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                ResetPlaytestDay();
            }

            if (keyboard.mKey.wasPressedThisFrame)
            {
                JumpActiveActorToCastleMap();
            }

            if (keyboard.jKey.wasPressedThisFrame)
            {
                JumpActiveActorToNextIntruder();
            }

            TryUsePlaytestPrepShortcut(keyboard);

            if (keyboard.eKey.wasPressedThisFrame)
            {
                if (interactionConsumedFrame != Time.frameCount
                    && (state.ActiveCharacter == AdventureCharacter.Renfield || nearestStation != null))
                {
                    TryUseNearestStation();
                }
            }
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            if (gamepad.selectButton.wasPressedThisFrame)
            {
                TrySwitchActiveCharacter();
            }

            if (gamepad.buttonNorth.wasPressedThisFrame)
            {
                TryEmergencyWakeDracula();
            }

            if (gamepad.startButton.wasPressedThisFrame)
            {
                AdvancePhase();
            }

            if (gamepad.leftShoulder.wasPressedThisFrame)
            {
                JumpActiveActorToCastleMap();
            }

            if (gamepad.rightShoulder.wasPressedThisFrame)
            {
                JumpActiveActorToNextIntruder();
            }
        }

        nearestStation = FindNearestStation();
        TickEmergencyWake();
        RefreshStationState();
        TickPhaseCard();
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
            emergencyWakeTimer = 0f;
            emergencyWakeUsesRemaining = Mathf.Max(0, emergencyWakeUsesPerDay);
        }
        else if (state.Phase == AdventurePhase.Night)
        {
            state.SetActiveCharacter(AdventureCharacter.Dracula);
            emergencyWakeTimer = 0f;
        }

        ApplyControlState();
        RefreshStationState();
        ShowFeedback(GetPhaseFeedback());
        ShowPhaseCard(GetPhaseCardTitle(), GetObjectiveText());
    }

    private void ResetPlaytestDay()
    {
        state.ResetDayOne(startDayWithRenfield ? AdventureCharacter.Renfield : AdventureCharacter.Dracula);
        emergencyWakeTimer = 0f;
        emergencyWakeUsesRemaining = Mathf.Max(0, emergencyWakeUsesPerDay);
        playtestIntruderJumpIndex = -1;

        RestoreInitialActorState();
        ResetIntruders();

        ApplyControlState();
        RefreshStationState();
        SnapCameraToActiveActor();
        RefreshDayReport();
        ShowFeedback("Playtest reset: Day 1 begins again.");
        ShowPhaseCard("DAY 1 RESET", "Choose 2 Renfield preparations.");
    }

    private void JumpActiveActorToCastleMap()
    {
        AdventureActor actor = GetActiveActor();
        if (actor == null)
        {
            return;
        }

        Vector2 entry = actor.character == AdventureCharacter.Renfield
            ? CastleRoomLayout.CastleMapEntryFromServantWing
            : CastleRoomLayout.CastleMapEntryFromCrypt;
        MoveActorTo(
            actor,
            new Vector3(entry.x, entry.y, actor.transform.position.z),
            "Castle Map",
            CastleRoomLayout.CastleMapMinBounds,
            CastleRoomLayout.CastleMapMaxBounds);
        actor.LockPortals(0.35f);
        SnapCameraToActiveActor();
        ShowFeedback(GetCharacterName(actor.character) + " jumped to castle map.");
    }

    private void JumpActiveActorToNextIntruder()
    {
        AdventureActor actor = GetActiveActor();
        if (actor == null)
        {
            return;
        }

        IntruderEncounter[] intruders = FindObjectsByType<IntruderEncounter>(FindObjectsInactive.Include);
        if (intruders == null || intruders.Length == 0)
        {
            ShowFeedback("No intruders registered yet.");
            return;
        }

        if (playtestIntruderJumpIndex >= intruders.Length)
        {
            playtestIntruderJumpIndex = -1;
        }

        for (int i = 0; i < intruders.Length; i++)
        {
            playtestIntruderJumpIndex = (playtestIntruderJumpIndex + 1) % intruders.Length;
            IntruderEncounter intruder = intruders[playtestIntruderJumpIndex];
            if (intruder == null || intruder.IsResolved || !intruder.IsActive)
            {
                continue;
            }

            JumpActiveActorToIntruder(actor, intruder);
            return;
        }

        ShowFeedback("No active intruder yet. Press N toward dusk or night.");
    }

    private void JumpActiveActorToIntruder(AdventureActor actor, IntruderEncounter intruder)
    {
        string roomName = intruder.CurrentRoomLabel;
        Vector2 minBounds;
        Vector2 maxBounds;
        GetRoomBounds(roomName, out minBounds, out maxBounds);

        Vector3 position = intruder.CurrentWorldPosition;
        float sideOffset = actor.character == AdventureCharacter.Dracula ? -0.65f : 0.65f;
        position += new Vector3(sideOffset, -0.22f, 0f);
        position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
        position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
        position.z = actor.transform.position.z;

        MoveActorTo(actor, position, roomName, minBounds, maxBounds);
        actor.LockPortals(0.35f);
        SnapCameraToActiveActor();
        ShowFeedback(GetCharacterName(actor.character) + " jumped to " + intruder.ReportName + ".");
    }

    private static void GetRoomBounds(string roomName, out Vector2 minBounds, out Vector2 maxBounds)
    {
        if (roomName == "Crypt")
        {
            minBounds = CastleRoomLayout.CryptMinBounds;
            maxBounds = CastleRoomLayout.CryptMaxBounds;
            return;
        }

        if (roomName == "Servant Wing")
        {
            minBounds = CastleRoomLayout.ServantWingMinBounds;
            maxBounds = CastleRoomLayout.ServantWingMaxBounds;
            return;
        }

        minBounds = CastleRoomLayout.CastleMapMinBounds;
        maxBounds = CastleRoomLayout.CastleMapMaxBounds;
    }

    private void TrySwitchActiveCharacter()
    {
        if (state.Phase == AdventurePhase.Day
            && draculaSleepsDuringDay
            && state.ActiveCharacter == AdventureCharacter.Renfield)
        {
            ShowFeedback(GetEmergencyWakeHint());
            return;
        }

        state.SwitchActiveCharacter();
        ApplyControlState();
        ShowFeedback("Control switched to " + GetCharacterName(state.ActiveCharacter) + ".");
    }

    private void TryEmergencyWakeDracula()
    {
        if (!allowEmergencyDayWake || !draculaSleepsDuringDay || state.Phase != AdventurePhase.Day)
        {
            return;
        }

        if (emergencyWakeTimer > 0f)
        {
            ShowFeedback("Dracula is already awake, but daylight weakens him.");
            return;
        }

        if (emergencyWakeUsesRemaining <= 0)
        {
            ShowFeedback("Dracula cannot be forced from the coffin again today.");
            return;
        }

        emergencyWakeUsesRemaining--;
        emergencyWakeTimer = Mathf.Max(1f, emergencyWakeSeconds);
        state.SetActiveCharacter(AdventureCharacter.Dracula);
        ApplyControlState();
        SnapCameraToActiveActor();
        ShowFeedback("Emergency wake: Dracula rises weakly in daylight.");
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
        RefreshDayReport();
    }

    private void TryUsePlaytestPrepShortcut(Keyboard keyboard)
    {
        if (keyboard == null || state.Phase != AdventurePhase.Day)
        {
            return;
        }

        if (WasKeyPressed(keyboard, Key.Digit1, Key.Numpad1))
        {
            TryPerformPlaytestPrep(RenfieldAction.ScoutVillage);
        }
        else if (WasKeyPressed(keyboard, Key.Digit2, Key.Numpad2))
        {
            TryPerformPlaytestPrep(RenfieldAction.PrepareBlackCandles);
        }
        else if (WasKeyPressed(keyboard, Key.Digit3, Key.Numpad3))
        {
            TryPerformPlaytestPrep(RenfieldAction.ResetChandelier);
        }
        else if (WasKeyPressed(keyboard, Key.Digit4, Key.Numpad4))
        {
            TryPerformPlaytestPrep(RenfieldAction.RepairGrandHall);
        }
        else if (WasKeyPressed(keyboard, Key.Digit5, Key.Numpad5))
        {
            TryPerformPlaytestPrep(RenfieldAction.EraseSigns);
        }
        else if (WasKeyPressed(keyboard, Key.Digit6, Key.Numpad6))
        {
            TryPerformPlaytestPrep(RenfieldAction.PrepareArtifact);
        }
        else if (WasKeyPressed(keyboard, Key.Digit7, Key.Numpad7))
        {
            TryPerformPlaytestPrep(RenfieldAction.ReleaseVermin);
        }
        else if (WasKeyPressed(keyboard, Key.Digit8, Key.Numpad8))
        {
            TryPerformPlaytestPrep(RenfieldAction.MoveCoffin);
        }
    }

    private void TryPerformPlaytestPrep(RenfieldAction action)
    {
        if (state.HasPerformedRenfieldAction(action))
        {
            ShowFeedback(GetRenfieldActionName(action) + " is already prepared.");
            return;
        }

        if (state.RenfieldActionsRemaining <= 0)
        {
            ShowFeedback("No Renfield prep left. Press R to replay.");
            return;
        }

        if (state.TryPerformRenfieldAction(action))
        {
            RefreshStationState();
            RefreshDayReport();
            ShowFeedback("Quick prep: " + GetRenfieldActionName(action) + ".");
            ShowPhaseCard("PREP SET", GetRenfieldActionName(action));
        }
    }

    private static bool WasKeyPressed(Keyboard keyboard, Key digitKey, Key numpadKey)
    {
        return keyboard[digitKey].wasPressedThisFrame || keyboard[numpadKey].wasPressedThisFrame;
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

        if (actor.character == AdventureCharacter.Dracula && actor.walker != null)
        {
            CacheDraculaBaseMoveSpeed();
            if (hasDraculaBaseMoveSpeed)
            {
                actor.walker.moveSpeed = IsEmergencyDayWakeActive
                    ? draculaBaseMoveSpeed * Mathf.Clamp(emergencyDraculaMoveSpeedMultiplier, 0.1f, 1f)
                    : draculaBaseMoveSpeed;
            }
        }
    }

    private bool CanMove(AdventureActor actor)
    {
        if (actor.character == AdventureCharacter.Dracula && draculaSleepsDuringDay && state.Phase == AdventurePhase.Day)
        {
            return emergencyWakeTimer > 0f;
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

    public bool IsEmergencyDayWakeActive
    {
        get { return state.Phase == AdventurePhase.Day && emergencyWakeTimer > 0f; }
    }

    private AdventureActor GetActiveActor()
    {
        return state.ActiveCharacter == AdventureCharacter.Dracula ? dracula : renfield;
    }

    private void TickEmergencyWake()
    {
        if (emergencyWakeTimer <= 0f)
        {
            return;
        }

        emergencyWakeTimer -= Time.deltaTime;
        if (emergencyWakeTimer > 0f)
        {
            return;
        }

        emergencyWakeTimer = 0f;
        if (state.Phase == AdventurePhase.Day)
        {
            state.SetActiveCharacter(AdventureCharacter.Renfield);
            ApplyControlState();
            SnapCameraToActiveActor();
            ShowFeedback("Daylight overcomes Dracula. Renfield must finish the preparations.");
        }
    }

    private void CacheDraculaBaseMoveSpeed()
    {
        if (hasDraculaBaseMoveSpeed || dracula == null || dracula.walker == null)
        {
            return;
        }

        draculaBaseMoveSpeed = dracula.walker.moveSpeed;
        hasDraculaBaseMoveSpeed = true;
    }

    private void CacheInitialActorState()
    {
        if (hasInitialActorState)
        {
            return;
        }

        if (dracula != null)
        {
            draculaStartPosition = dracula.transform.position;
            draculaStartRoomName = dracula.roomName;
            if (dracula.walker != null)
            {
                draculaStartMinBounds = dracula.walker.minBounds;
                draculaStartMaxBounds = dracula.walker.maxBounds;
            }
        }

        if (renfield != null)
        {
            renfieldStartPosition = renfield.transform.position;
            renfieldStartRoomName = renfield.roomName;
            if (renfield.walker != null)
            {
                renfieldStartMinBounds = renfield.walker.minBounds;
                renfieldStartMaxBounds = renfield.walker.maxBounds;
            }
        }

        hasInitialActorState = true;
    }

    private void RestoreInitialActorState()
    {
        if (!hasInitialActorState)
        {
            CacheInitialActorState();
        }

        if (dracula != null)
        {
            MoveActorTo(dracula, draculaStartPosition, draculaStartRoomName, draculaStartMinBounds, draculaStartMaxBounds);
        }

        if (renfield != null)
        {
            MoveActorTo(renfield, renfieldStartPosition, renfieldStartRoomName, renfieldStartMinBounds, renfieldStartMaxBounds);
        }
    }

    private static void MoveActorTo(AdventureActor actor, Vector3 position, string roomName, Vector2 minBounds, Vector2 maxBounds)
    {
        Rigidbody2D body = actor.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.position = new Vector2(position.x, position.y);
        }

        actor.transform.position = position;
        actor.roomName = roomName;
        if (actor.walker != null)
        {
            actor.walker.minBounds = minBounds;
            actor.walker.maxBounds = maxBounds;
        }
    }

    private static void ResetIntruders()
    {
        IntruderEncounter[] intruders = FindObjectsByType<IntruderEncounter>(FindObjectsInactive.Include);
        for (int i = 0; i < intruders.Length; i++)
        {
            intruders[i].ResetForPlaytest();
        }
    }

    private static void RefreshDayReport()
    {
        AdventureDayReport report = FindAnyObjectByType<AdventureDayReport>();
        if (report != null)
        {
            report.ForceRefresh();
        }
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

    private void ShowPhaseCard(string title, string subtitle)
    {
        phaseCardTitle = title;
        phaseCardSubtitle = subtitle;
        phaseCardTimer = PhaseCardDuration;
    }

    private void TickPhaseCard()
    {
        if (phaseCardTimer > 0f)
        {
            phaseCardTimer -= Time.deltaTime;
        }
    }

    private string GetPhaseFeedback()
    {
        switch (state.Phase)
        {
            case AdventurePhase.Day:
                return "Day " + state.DayNumber + ": Renfield prepares the castle.";
            case AdventurePhase.Dusk:
                return "Dusk: read the intruder report, then press N for night.";
            case AdventurePhase.Night:
                return "Night: Dracula wakes. Find and handle intruders.";
            default:
                return "Dawn: read the report. R resets Day 1.";
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
        GUI.Label(new Rect(x, y, width, 24f * uiScale), "E/A use   N phase   J threat   R reset   M map", HintStyle(uiScale));

        y += 26f * uiScale;
        string prompt = GetPromptText();
        GUI.Label(new Rect(x, y, width, 44f * uiScale), prompt, BodyStyle(uiScale));

        GUI.color = previousColor;
        GUI.depth = previousDepth;

        DrawPhaseCard(uiScale);
    }

    private string GetPromptText()
    {
        if (feedbackTimer > 0f && !string.IsNullOrEmpty(feedbackText))
        {
            return feedbackText;
        }

        if (state.ActiveCharacter == AdventureCharacter.Dracula && state.Phase == AdventurePhase.Day && draculaSleepsDuringDay)
        {
            return IsEmergencyDayWakeActive ? "Daylight weakens Dracula. Time is short." : GetEmergencyWakeHint();
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

    private string GetPhaseCardTitle()
    {
        switch (state.Phase)
        {
            case AdventurePhase.Day:
                return "DAY " + state.DayNumber;
            case AdventurePhase.Dusk:
                return "DUSK";
            case AdventurePhase.Night:
                return "NIGHT";
            default:
                return "DAWN REPORT";
        }
    }

    private string GetObjectiveText()
    {
        switch (state.Phase)
        {
            case AdventurePhase.Day:
                if (state.RenfieldActionsRemaining > 0)
                {
                    return "Choose " + state.RenfieldActionsRemaining + " Renfield preparations.";
                }

                return "Preparations spent. Press N for dusk.";
            case AdventurePhase.Dusk:
                return "Review intruders. Press N when ready for night.";
            case AdventurePhase.Night:
                return "Use J for threats, or M for map.";
            default:
                return "Read outcomes. Press R to replay Day 1.";
        }
    }

    private static string GetRenfieldActionName(RenfieldAction action)
    {
        switch (action)
        {
            case RenfieldAction.ResetChandelier:
                return "Chandelier Trap";
            case RenfieldAction.RepairGrandHall:
                return "Repair Hall";
            case RenfieldAction.ScoutVillage:
                return "Scout Village";
            case RenfieldAction.PrepareBlackCandles:
                return "Black Candles";
            case RenfieldAction.MoveCoffin:
                return "Move Coffin";
            case RenfieldAction.EraseSigns:
                return "Erase Signs";
            case RenfieldAction.PrepareArtifact:
                return "Prepare Artifact";
            default:
                return "Release Vermin";
        }
    }

    private void DrawPhaseCard(float uiScale)
    {
        if (phaseCardTimer <= 0f || string.IsNullOrEmpty(phaseCardTitle))
        {
            return;
        }

        float alpha = Mathf.Clamp01(phaseCardTimer / 0.28f);
        Rect rect = new Rect(
            (Screen.width - 520f * uiScale) * 0.5f,
            (Screen.height - 150f * uiScale) * 0.5f,
            520f * uiScale,
            150f * uiScale);

        int previousDepth = GUI.depth;
        Color previousColor = GUI.color;
        GUI.depth = -120;
        GUI.color = new Color(0.012f, 0.018f, 0.023f, 0.86f * alpha);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = new Color(0.08f, 0.75f, 0.84f, alpha);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3f * uiScale), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - 3f * uiScale, rect.width, 3f * uiScale), Texture2D.whiteTexture);

        GUI.color = new Color(0.96f, 0.84f, 0.28f, alpha);
        GUI.Label(new Rect(rect.x + 18f * uiScale, rect.y + 24f * uiScale, rect.width - 36f * uiScale, 54f * uiScale), phaseCardTitle.ToUpperInvariant(), PhaseTitleStyle(uiScale));
        GUI.color = new Color(0.82f, 0.91f, 0.95f, alpha);
        GUI.Label(new Rect(rect.x + 24f * uiScale, rect.y + 88f * uiScale, rect.width - 48f * uiScale, 40f * uiScale), phaseCardSubtitle.ToUpperInvariant(), PhaseSubtitleStyle(uiScale));

        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }

    private string GetEmergencyWakeHint()
    {
        if (!allowEmergencyDayWake || emergencyWakeUsesRemaining <= 0)
        {
            return "Dracula sleeps by day. Renfield must defend the castle.";
        }

        return "Dracula sleeps by day. Q/Y can wake him briefly, weakened.";
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

    private static GUIStyle PhaseTitleStyle(float uiScale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = Mathf.RoundToInt(34f * uiScale);
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(0.96f, 0.84f, 0.28f, 1f);
        return style;
    }

    private static GUIStyle PhaseSubtitleStyle(float uiScale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = Mathf.RoundToInt(15f * uiScale);
        style.wordWrap = true;
        style.normal.textColor = new Color(0.82f, 0.91f, 0.95f, 1f);
        return style;
    }
}
