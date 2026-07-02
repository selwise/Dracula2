using System.Collections.Generic;
using UnityEngine;

public sealed class AdventureDayReport : MonoBehaviour
{
    public AdventureLoopController loopController;
    public IntruderEncounter[] intruders;
    public Texture2D panelTexture;
    public float panelWidth = 430f;
    public float panelRightOffset = 18f;
    public float panelTopOffset = 18f;

    private readonly List<string> lines = new List<string>();
    private AdventurePhase lastPhase;
    private int lastDayNumber;
    private int castleDamage;
    private int villageSuspicion;
    private int draculaWounds;
    private int demeterProgress;
    private int hungerDebt;

    private void Awake()
    {
        if (loopController == null)
        {
            loopController = GetComponent<AdventureLoopController>();
        }

        RefreshIntrudersIfNeeded();

        if (loopController != null)
        {
            lastPhase = loopController.State.Phase;
            lastDayNumber = loopController.State.DayNumber;
            BuildPanelForPhase(lastPhase);
        }
    }

    private void Update()
    {
        if (loopController == null)
        {
            return;
        }

        RefreshIntrudersIfNeeded();

        if (loopController.State.DayNumber != lastDayNumber)
        {
            lastDayNumber = loopController.State.DayNumber;
            castleDamage = 0;
            villageSuspicion = 0;
            draculaWounds = 0;
            demeterProgress = 0;
            hungerDebt = 0;
            BuildPanelForPhase(loopController.State.Phase);
        }

        if (loopController.State.Phase != lastPhase)
        {
            lastPhase = loopController.State.Phase;
            BuildPanelForPhase(lastPhase);
        }
        else if (lastPhase == AdventurePhase.Day)
        {
            BuildDayPlan();
        }
        else if (lastPhase == AdventurePhase.Dusk || lastPhase == AdventurePhase.Night)
        {
            BuildThreatStatus(lastPhase == AdventurePhase.Dusk ? "DUSK REPORT" : "NIGHT PRESSURE");
        }
    }

    public void ForceRefresh()
    {
        if (loopController == null)
        {
            return;
        }

        lastPhase = loopController.State.Phase;
        lastDayNumber = loopController.State.DayNumber;
        BuildPanelForPhase(lastPhase);
    }

    private void RefreshIntrudersIfNeeded()
    {
        if (intruders != null && intruders.Length > 0)
        {
            return;
        }

        intruders = FindObjectsByType<IntruderEncounter>(FindObjectsInactive.Include);
    }

    private void BuildPanelForPhase(AdventurePhase phase)
    {
        switch (phase)
        {
            case AdventurePhase.Day:
                BuildDayPlan();
                break;
            case AdventurePhase.Dusk:
                BuildThreatStatus("DUSK REPORT");
                break;
            case AdventurePhase.Night:
                BuildThreatStatus("NIGHT PRESSURE");
                break;
            default:
                BuildDawnReport();
                break;
        }
    }

    private void BuildDayPlan()
    {
        lines.Clear();
        lines.Add("RENFIELD'S DAY " + loopController.State.DayNumber);
        if (loopController.State.RenfieldActionsRemaining > 0)
        {
            lines.Add("Choose " + loopController.State.RenfieldActionsRemaining + " preparations.");
        }
        else
        {
            lines.Add("Preparations spent. Press N for dusk.");
        }

        lines.Add("Castle work");
        lines.Add(GetPrepStatus(RenfieldAction.PrepareBlackCandles, "Black Candles"));
        lines.Add(GetPrepStatus(RenfieldAction.ResetChandelier, "Gallery Trap"));
        lines.Add(GetPrepStatus(RenfieldAction.MoveCoffin, "Demeter Crate"));
        lines.Add("Village work");
        lines.Add(GetPrepStatus(RenfieldAction.ScoutVillage, "Threat Rumors"));
        lines.Add(GetPrepStatus(RenfieldAction.LureVictim, "Lure Victim"));
        lines.Add(GetDayLoopHint());
    }

    private string GetPrepStatus(RenfieldAction action, string label)
    {
        return (loopController.State.HasPerformedRenfieldAction(action) ? "DONE  " : "OPEN  ") + label + " - " + GetPrepRole(action);
    }

    private static string GetPrepRole(RenfieldAction action)
    {
        switch (action)
        {
            case RenfieldAction.PrepareBlackCandles:
                return "priest answer";
            case RenfieldAction.ResetChandelier:
                return "peasant answer";
            case RenfieldAction.MoveCoffin:
                return "Act 2 cargo";
            case RenfieldAction.ScoutVillage:
                return "threat labels";
            case RenfieldAction.LureVictim:
                return "Dracula fed";
            default:
                return "fallback";
        }
    }

    private string GetDayLoopHint()
    {
        bool demeter = loopController.State.HasPerformedRenfieldAction(RenfieldAction.MoveCoffin);
        bool fed = loopController.State.HasPerformedRenfieldAction(RenfieldAction.LureVictim);

        if (demeter && fed)
        {
            return "Route: cargo and hunger covered.";
        }

        if (demeter)
        {
            return "Route: cargo ready, hunger open.";
        }

        if (fed)
        {
            return "Route: hunger covered, cargo open.";
        }

        return "Route: threats, cargo, hunger compete.";
    }

    private void BuildThreatStatus(string title)
    {
        lines.Clear();
        lines.Add(title);

        bool scouted = loopController.State.HasPerformedRenfieldAction(RenfieldAction.ScoutVillage);
        if (!scouted)
        {
            lines.Add("Objective: infer threats, then press N.");
            lines.Add("Unscouted: vague map hints.");
        }
        else
        {
            lines.Add("Objective: pick your target order.");
            lines.Add("Scouted: exact intruder labels.");
        }

        if (intruders == null || intruders.Length == 0)
        {
            lines.Add("No intruders registered.");
            return;
        }

        for (int i = 0; i < intruders.Length; i++)
        {
            IntruderEncounter intruder = intruders[i];
            if (intruder == null)
            {
                continue;
            }

            lines.Add(intruder.GetMapHintLine(scouted));
        }

        if (loopController.State.Phase == AdventurePhase.Night)
        {
            lines.Add("J jump threat  M map if lost.");
        }
        else
        {
            lines.Add(GetLogisticsLine());
            lines.Add("J jump threat  N begins night.");
        }
    }

    private string GetLogisticsLine()
    {
        bool demeter = loopController.State.HasPerformedRenfieldAction(RenfieldAction.MoveCoffin);
        bool fed = loopController.State.HasPerformedRenfieldAction(RenfieldAction.LureVictim);

        if (demeter && fed)
        {
            return "Demeter cargo ready. Dracula fed.";
        }

        if (demeter)
        {
            return "Demeter cargo ready. Hunger unsolved.";
        }

        if (fed)
        {
            return "Dracula fed. Demeter cargo neglected.";
        }

        return "Demeter and hunger both neglected.";
    }

    private void BuildDawnReport()
    {
        lines.Clear();
        lines.Add("DAWN REPORT");
        lines.Add("Objective: read results, R to replay.");
        lines.Add("R reset  M map");

        castleDamage = 0;
        villageSuspicion = 0;
        draculaWounds = 0;
        demeterProgress = HasDemeterCrate ? 1 : 0;
        hungerDebt = HasFedDracula ? 0 : 1;

        if (intruders != null)
        {
            for (int i = 0; i < intruders.Length; i++)
            {
                IntruderEncounter intruder = intruders[i];
                if (intruder == null)
                {
                    continue;
                }

                intruder.ForceDawnOutcome();
                lines.Add(intruder.GetDawnReportLine());
                castleDamage += intruder.CastleDamage;
                villageSuspicion += intruder.VillageSuspicion;
                draculaWounds += intruder.DraculaWounds;
            }
        }

        lines.Add("THREATS  Damage +" + castleDamage + "  Suspicion +" + villageSuspicion + "  Wounds +" + draculaWounds);
        lines.Add(GetDemeterOutcomeLine());
        lines.Add(GetHungerOutcomeLine());
        lines.Add(GetDawnMoodLine(castleDamage, villageSuspicion, draculaWounds, HasDemeterCrate, HasFedDracula));
    }

    private string GetDemeterOutcomeLine()
    {
        if (demeterProgress > 0)
        {
            return "DEMETER  Cargo ready. Act 2 route +1.";
        }

        return "DEMETER  Cargo neglected. Act 2 route +0.";
    }

    private string GetHungerOutcomeLine()
    {
        if (hungerDebt <= 0)
        {
            return "HUNGER  Dracula fed before nightfall.";
        }

        return "HUNGER  Dracula enters night unfed.";
    }

    private bool HasDemeterCrate
    {
        get { return loopController.State.HasPerformedRenfieldAction(RenfieldAction.MoveCoffin); }
    }

    private bool HasFedDracula
    {
        get { return loopController.State.HasPerformedRenfieldAction(RenfieldAction.LureVictim); }
    }

    public static string GetDawnMoodLine(int damage, int suspicion, int wounds, bool demeterReady, bool fed)
    {
        int trouble = damage + suspicion + wounds + (demeterReady ? 0 : 1) + (fed ? 0 : 1);
        if (trouble <= 0)
        {
            return "Perfect night. Renfield can risk greedier plans.";
        }

        if (trouble <= 2)
        {
            return demeterReady ? "Survived. The Demeter plan can advance." : "Survived, but the voyage is slipping.";
        }

        return "Messy survival. Day 2 starts under pressure.";
    }

    private void OnGUI()
    {
        if (loopController == null || lines.Count == 0)
        {
            return;
        }

        float uiScale = Mathf.Clamp(Screen.height / 1080f, 0.85f, 1.35f);
        bool dayPanel = loopController.State.Phase == AdventurePhase.Day;
        float width = (dayPanel ? 386f : panelWidth) * uiScale;
        float rowHeight = (dayPanel ? 18f : 19f) * uiScale;
        float height = Mathf.Max(116f * uiScale, 34f * uiScale + lines.Count * rowHeight);
        Rect panel = new Rect(
            Screen.width - width - panelRightOffset * uiScale,
            panelTopOffset * uiScale,
            width,
            height);

        int previousDepth = GUI.depth;
        Color previousColor = GUI.color;
        GUI.depth = -75;

        if (panelTexture != null)
        {
            GUI.color = Color.white;
            GUI.DrawTexture(panel, panelTexture, ScaleMode.StretchToFill, true);
        }
        else
        {
            GUI.color = new Color(0.012f, 0.018f, 0.023f, 0.88f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(0.08f, 0.75f, 0.84f, 1f);
            GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 2f * uiScale), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panel.x, panel.yMax - 2f * uiScale, panel.width, 2f * uiScale), Texture2D.whiteTexture);
        }

        float x = panel.x + 16f * uiScale;
        float y = panel.y + 12f * uiScale;
        float contentWidth = panel.width - 32f * uiScale;
        for (int i = 0; i < lines.Count; i++)
        {
            bool header = i == 0;
            GUI.color = header ? new Color(0.96f, 0.84f, 0.28f, 1f) : new Color(0.82f, 0.91f, 0.95f, 1f);
            GUI.Label(new Rect(x, y, contentWidth, header ? 24f * uiScale : rowHeight), lines[i].ToUpperInvariant(), header ? HeaderStyle(uiScale) : BodyStyle(uiScale));
            y += header ? 26f * uiScale : rowHeight;
        }

        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }

    private static GUIStyle HeaderStyle(float uiScale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = Mathf.RoundToInt(17f * uiScale);
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(0.96f, 0.84f, 0.28f, 1f);
        return style;
    }

    private static GUIStyle BodyStyle(float uiScale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = Mathf.RoundToInt(12f * uiScale);
        style.normal.textColor = new Color(0.82f, 0.91f, 0.95f, 1f);
        style.wordWrap = true;
        return style;
    }
}
