using System;
using UnityEngine;

public enum IntruderKind
{
    AngryPeasant,
    Priest
}

public enum IntruderOutcomeKind
{
    None,
    Clean,
    Messy,
    WoundedDracula,
    Escaped
}

public sealed class IntruderEncounter : AdventureInteractionReceiver
{
    public AdventureLoopController loopController;
    public AdventureInteractionTarget interactionTarget;
    public SpriteRenderer spriteRenderer;
    public IntruderKind kind;
    public string intruderName = "Intruder";
    public string[] routeRoomNames;
    public string[] routeLocationNames;
    public Vector3[] routePositions;
    public float stepSeconds = 12f;
    public float blackCandleDelaySeconds = 6f;
    public float preparedRoomDelaySeconds = 5f;
    public float renfieldStallSeconds = 5f;
    public float idlePaceDistance = 0.06f;
    public float idlePaceSpeed = 2.4f;
    public bool announcesWave;

    private AdventurePhase lastPhase;
    private int lastDayNumber;
    private int routeIndex;
    private float stepTimer;
    private bool active;
    private bool resolved;
    private bool breached;
    private Vector3 basePosition;
    private IntruderOutcomeKind outcomeKind;
    private string outcomeSummary;
    private int castleDamage;
    private int villageSuspicion;
    private int draculaWounds;

    public string ReportName
    {
        get { return intruderName; }
    }

    public bool IsResolved
    {
        get { return resolved; }
    }

    public bool IsActive
    {
        get { return active; }
    }

    public string CurrentRoomLabel
    {
        get { return CurrentRoomName; }
    }

    public Vector3 CurrentWorldPosition
    {
        get { return basePosition; }
    }

    public int CastleDamage
    {
        get { return castleDamage; }
    }

    public int VillageSuspicion
    {
        get { return villageSuspicion; }
    }

    public int DraculaWounds
    {
        get { return draculaWounds; }
    }

    private void Awake()
    {
        if (interactionTarget == null)
        {
            interactionTarget = GetComponent<AdventureInteractionTarget>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (loopController == null)
        {
            loopController = FindAnyObjectByType<AdventureLoopController>();
        }

        if (loopController != null)
        {
            lastPhase = loopController.State.Phase;
            lastDayNumber = loopController.State.DayNumber;
        }

        ResetForDay();
    }

    private void Update()
    {
        if (loopController == null)
        {
            return;
        }

        if (loopController.State.DayNumber != lastDayNumber)
        {
            lastDayNumber = loopController.State.DayNumber;
            ResetForDay();
        }

        if (loopController.State.Phase != lastPhase)
        {
            lastPhase = loopController.State.Phase;
            HandlePhaseChanged(lastPhase);
        }

        if (active && !resolved && loopController.State.Phase == AdventurePhase.Night)
        {
            AdvanceRoute(Time.deltaTime);
        }

        UpdateVisibilityAndPrompt();
        PaceWhenVisible();
    }

    public override string Interact(AdventureActor actor, AdventureInventory inventory)
    {
        if (!active || resolved)
        {
            return intruderName + " is not in this part of the castle.";
        }

        if (actor == null)
        {
            return "No one is close enough to confront " + intruderName + ".";
        }

        if (!StringEquals(actor.roomName, CurrentRoomName))
        {
            return intruderName + " is elsewhere in the castle.";
        }

        if (actor.character == AdventureCharacter.Renfield)
        {
            return InteractAsRenfield();
        }

        return InteractAsDracula();
    }

    private string InteractAsRenfield()
    {
        if (loopController.State.Phase == AdventurePhase.Day)
        {
            return "Renfield can only prepare before the intruders enter the castle.";
        }

        if (kind == IntruderKind.AngryPeasant)
        {
            if (HasPrep(RenfieldAction.ScoutVillage))
            {
                return Resolve(
                    "Renfield uses the village gossip he learned by day. The angry peasant follows a false trail.",
                    IntruderOutcomeKind.Clean,
                    "Peasant misled by Renfield.");
            }

            if (HasPrep(RenfieldAction.ReleaseVermin))
            {
                return Resolve(
                    "Rats boil through the stones. The angry peasant runs before he reaches the inner rooms.",
                    IntruderOutcomeKind.Messy,
                    "Peasant routed by vermin.",
                    0,
                    1,
                    0);
            }

            DelayIntruder(renfieldStallSeconds);
            return "Renfield stalls the angry peasant, but this needs Dracula or a prepared trick to end cleanly.";
        }

        if (HasPrep(RenfieldAction.EraseSigns))
        {
            return Resolve(
                "Renfield erased the chalk and garlic signs. The priest loses the trail and withdraws.",
                IntruderOutcomeKind.Clean,
                "Priest lost the trail.");
        }

        if (HasPrep(RenfieldAction.PrepareBlackCandles))
        {
            return Resolve(
                "Renfield lights the black candles. The priest's ward gutters out before Dracula arrives.",
                IntruderOutcomeKind.Clean,
                "Priest's ward smothered early.");
        }

        DelayIntruder(renfieldStallSeconds);
        return "Renfield keeps the priest talking, but holy wards make this dangerous without preparation.";
    }

    private string InteractAsDracula()
    {
        if (loopController.State.Phase != AdventurePhase.Night)
        {
            return "Dracula should not face " + intruderName + " before nightfall.";
        }

        if (kind == IntruderKind.AngryPeasant)
        {
            if (HasPrep(RenfieldAction.ResetChandelier))
            {
                return Resolve(
                    "Renfield's chandelier trap drops. Dracula barely has to lift a finger.",
                    IntruderOutcomeKind.Clean,
                    "Peasant caught by chandelier trap.");
            }

            if (HasPrep(RenfieldAction.RepairGrandHall))
            {
                return Resolve(
                    "The repaired hall locks the angry peasant in a dead end. Dracula finishes the scare.",
                    IntruderOutcomeKind.Clean,
                    "Peasant trapped in repaired hall.");
            }

            if (HasPrep(RenfieldAction.PrepareBlackCandles))
            {
                return Resolve(
                    "Black candle smoke swallows the corridor. The angry peasant flees from Dracula's silhouette.",
                    IntruderOutcomeKind.Clean,
                    "Peasant fled into black candle smoke.");
            }

            return Resolve(
                "Dracula terrifies the angry peasant into flight, but the noise and struggle cost the castle.",
                IntruderOutcomeKind.Messy,
                "Peasant driven off noisily.",
                1,
                1,
                0);
        }

        if (HasPrep(RenfieldAction.PrepareBlackCandles))
        {
            return Resolve(
                "The black candles smother the priest's ward. Dracula drives him out into the night.",
                IntruderOutcomeKind.Clean,
                "Priest banished by black candles.");
        }

        if (HasPrep(RenfieldAction.EraseSigns))
        {
            return Resolve(
                "The priest finds no proof to bless. Dracula sends him running with an empty rite.",
                IntruderOutcomeKind.Clean,
                "Priest found no proof.");
        }

        if (HasPrep(RenfieldAction.PrepareArtifact))
        {
            return Resolve(
                "Renfield's prepared relic catches the holy glare. Dracula can finally break the priest's nerve.",
                IntruderOutcomeKind.Clean,
                "Priest broken by prepared artifact.");
        }

        if (breached)
        {
            return Resolve(
                "Dracula forces the priest back at the crypt door, but the holy wound will matter later.",
                IntruderOutcomeKind.WoundedDracula,
                "Priest repelled at crypt door.",
                0,
                1,
                1);
        }

        DelayIntruder(renfieldStallSeconds * 0.7f);
        return "The priest raises a ward. Dracula recoils; Renfield needs black candles, erased signs, or an artifact.";
    }

    private void HandlePhaseChanged(AdventurePhase phase)
    {
        if (phase == AdventurePhase.Day)
        {
            ResetForDay();
            return;
        }

        if (phase == AdventurePhase.Dusk)
        {
            ActivateIntruder();
            if (announcesWave)
            {
                if (HasPrep(RenfieldAction.ScoutVillage))
                {
                    loopController.ShowFeedbackMessage("Renfield identifies two intruders: an angry peasant and a priest.");
                }
                else
                {
                    loopController.ShowFeedbackMessage("Two intruders enter the castle before nightfall.");
                }
            }
            return;
        }

        if (phase == AdventurePhase.Night)
        {
            ActivateIntruder();
            if (announcesWave)
            {
                loopController.ShowFeedbackMessage("Night falls. Dracula must handle the intruders room by room.");
            }
            return;
        }

        if (phase == AdventurePhase.Dawn && active && !resolved)
        {
            ForceDawnOutcome();
            if (announcesWave)
            {
                loopController.ShowFeedbackMessage("Dawn finds damage where intruders slipped through.");
            }
        }
    }

    private void ActivateIntruder()
    {
        if (resolved || routePositions == null || routePositions.Length == 0)
        {
            return;
        }

        active = true;
        routeIndex = Mathf.Clamp(routeIndex, 0, routePositions.Length - 1);
        SetBasePosition(routePositions[routeIndex]);
    }

    private void ResetForDay()
    {
        active = false;
        resolved = false;
        breached = false;
        routeIndex = 0;
        stepTimer = 0f;
        outcomeKind = IntruderOutcomeKind.None;
        outcomeSummary = "";
        castleDamage = 0;
        villageSuspicion = 0;
        draculaWounds = 0;

        if (routePositions != null && routePositions.Length > 0)
        {
            SetBasePosition(routePositions[0]);
        }

        SetVisible(false);
    }

    private void AdvanceRoute(float deltaTime)
    {
        if (routePositions == null || routePositions.Length <= 1 || routeIndex >= routePositions.Length - 1)
        {
            if (!breached)
            {
                breached = true;
                loopController.ShowFeedbackMessage(intruderName + " reaches " + CurrentLocationName + ".");
            }
            return;
        }

        stepTimer += deltaTime;
        if (stepTimer < stepSeconds + GetPreparedDelay())
        {
            return;
        }

        stepTimer = 0f;
        routeIndex++;
        SetBasePosition(routePositions[routeIndex]);

        if (routeIndex >= routePositions.Length - 1)
        {
            loopController.ShowFeedbackMessage(intruderName + " reaches " + CurrentLocationName + ".");
        }
        else if (HasPrep(RenfieldAction.ScoutVillage))
        {
            loopController.ShowFeedbackMessage("Scouting tracks " + intruderName + " near " + CurrentLocationName + ".");
        }
    }

    private float GetPreparedDelay()
    {
        float delay = 0f;
        if (HasPrep(RenfieldAction.PrepareBlackCandles))
        {
            delay += blackCandleDelaySeconds;
        }

        if (kind == IntruderKind.AngryPeasant && HasPrep(RenfieldAction.RepairGrandHall))
        {
            delay += preparedRoomDelaySeconds;
        }

        if (kind == IntruderKind.Priest && HasPrep(RenfieldAction.EraseSigns))
        {
            delay += preparedRoomDelaySeconds;
        }

        return delay;
    }

    public void ForceDawnOutcome()
    {
        if (resolved)
        {
            SetVisible(false);
            return;
        }

        if (!active)
        {
            outcomeKind = IntruderOutcomeKind.None;
            outcomeSummary = intruderName + " never entered the castle.";
            SetVisible(false);
            return;
        }

        if (kind == IntruderKind.Priest)
        {
            Resolve(
                "Dawn exposes the priest's work.",
                IntruderOutcomeKind.Escaped,
                "Priest left holy signs behind.",
                1,
                2,
                0);
            return;
        }

        Resolve(
            "Dawn finds broken doors and muddy bootprints.",
            IntruderOutcomeKind.Escaped,
            "Peasant slipped through unresolved.",
            1,
            1,
            0);
    }

    public string GetStatusLine()
    {
        if (resolved)
        {
            return intruderName + ": handled";
        }

        if (!active)
        {
            return intruderName + ": not inside";
        }

        return intruderName + ": " + CurrentLocationName;
    }

    public string GetMapHintLine(bool scouted)
    {
        if (resolved)
        {
            return scouted ? intruderName + ": handled" : VagueSignalName + ": quiet";
        }

        if (!active)
        {
            return scouted ? intruderName + ": not inside" : VagueSignalName + ": none";
        }

        if (scouted)
        {
            return intruderName + ": " + CurrentLocationName;
        }

        return VagueSignalName + ": " + VagueLocationName;
    }

    public string GetDawnReportLine()
    {
        if (string.IsNullOrEmpty(outcomeSummary))
        {
            ForceDawnOutcome();
        }

        switch (outcomeKind)
        {
            case IntruderOutcomeKind.Clean:
                return "CLEAN  " + outcomeSummary;
            case IntruderOutcomeKind.Messy:
                return "MESSY  " + outcomeSummary;
            case IntruderOutcomeKind.WoundedDracula:
                return "WOUND  " + outcomeSummary;
            case IntruderOutcomeKind.Escaped:
                return "LOOSE  " + outcomeSummary;
            default:
                return "QUIET  " + outcomeSummary;
        }
    }

    private string Resolve(
        string message,
        IntruderOutcomeKind newOutcome,
        string summary,
        int damage = 0,
        int suspicion = 0,
        int wounds = 0)
    {
        resolved = true;
        active = false;
        outcomeKind = newOutcome;
        outcomeSummary = summary;
        castleDamage = damage;
        villageSuspicion = suspicion;
        draculaWounds = wounds;
        SetVisible(false);
        return message;
    }

    private void DelayIntruder(float seconds)
    {
        stepTimer = Mathf.Min(stepTimer, -Mathf.Max(1f, seconds));
    }

    private bool HasPrep(RenfieldAction action)
    {
        return loopController != null && loopController.State.HasPerformedRenfieldAction(action);
    }

    private void UpdateVisibilityAndPrompt()
    {
        bool visible = active && !resolved && IsVisibleToActiveActor();
        SetVisible(visible);

        if (interactionTarget == null)
        {
            return;
        }

        AdventureActor actor = loopController != null ? loopController.ActiveActor : null;
        interactionTarget.displayName = intruderName;
        interactionTarget.description = "Spotted at " + CurrentLocationName + ".";

        if (actor != null && actor.character == AdventureCharacter.Renfield)
        {
            interactionTarget.verb = kind == IntruderKind.Priest ? "STALL" : "MISLEAD";
        }
        else if (kind == IntruderKind.Priest)
        {
            interactionTarget.verb = HasPriestAnswer() || breached ? "BANISH" : "CONFRONT";
        }
        else
        {
            interactionTarget.verb = HasPrep(RenfieldAction.ResetChandelier) ? "SPRING" : "TERRORIZE";
        }
    }

    private bool HasPriestAnswer()
    {
        return HasPrep(RenfieldAction.PrepareBlackCandles)
            || HasPrep(RenfieldAction.EraseSigns)
            || HasPrep(RenfieldAction.PrepareArtifact);
    }

    private bool IsVisibleToActiveActor()
    {
        if (loopController == null || loopController.ActiveActor == null)
        {
            return false;
        }

        return StringEquals(loopController.ActiveActor.roomName, CurrentRoomName);
    }

    private void SetVisible(bool visible)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
        }

        if (interactionTarget != null)
        {
            interactionTarget.enabled = visible;
        }
    }

    private void SetBasePosition(Vector3 position)
    {
        basePosition = position;
        transform.position = basePosition;
        UpdateSortOrder();
    }

    private void PaceWhenVisible()
    {
        if (spriteRenderer == null || !spriteRenderer.enabled)
        {
            return;
        }

        float pace = Mathf.Sin(Time.time * idlePaceSpeed) * idlePaceDistance;
        transform.position = basePosition + new Vector3(pace, 0f, 0f);
        UpdateSortOrder();
    }

    private void UpdateSortOrder()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        int order = 300 - Mathf.RoundToInt(transform.position.y * 28f);
        spriteRenderer.sortingOrder = Mathf.Clamp(order, 220, 380);
    }

    private string CurrentRoomName
    {
        get
        {
            if (routeRoomNames == null || routeRoomNames.Length == 0)
            {
                return "Castle Map";
            }

            return routeRoomNames[Mathf.Clamp(routeIndex, 0, routeRoomNames.Length - 1)];
        }
    }

    private string CurrentLocationName
    {
        get
        {
            if (routeLocationNames == null || routeLocationNames.Length == 0)
            {
                return CurrentRoomName;
            }

            return routeLocationNames[Mathf.Clamp(routeIndex, 0, routeLocationNames.Length - 1)];
        }
    }

    private string VagueSignalName
    {
        get { return kind == IntruderKind.Priest ? "CHANTING" : "NOISE"; }
    }

    private string VagueLocationName
    {
        get
        {
            if (kind == IntruderKind.Priest)
            {
                return breached ? "near crypt" : "old gallery";
            }

            return breached ? "servant wing" : "outer halls";
        }
    }

    private static bool StringEquals(string a, string b)
    {
        return string.Equals(a, b, StringComparison.Ordinal);
    }

    public void ResetForPlaytest()
    {
        if (loopController != null)
        {
            lastPhase = loopController.State.Phase;
            lastDayNumber = loopController.State.DayNumber;
        }

        ResetForDay();
    }
}
