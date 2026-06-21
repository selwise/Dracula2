using System.Collections.Generic;

public enum AdventureCharacter
{
    Dracula,
    Renfield
}

public enum AdventurePhase
{
    Day,
    Dusk,
    Night,
    Dawn
}

public enum RenfieldAction
{
    ResetChandelier,
    RepairGrandHall,
    ScoutVillage,
    PrepareBlackCandles,
    MoveCoffin,
    EraseSigns,
    PrepareArtifact,
    ReleaseVermin
}

public sealed class AdventureLoopState
{
    public const int RenfieldActionsPerDay = 2;

    private readonly HashSet<RenfieldAction> performedRenfieldActions = new HashSet<RenfieldAction>();

    public AdventureCharacter ActiveCharacter { get; private set; } = AdventureCharacter.Dracula;
    public AdventurePhase Phase { get; private set; } = AdventurePhase.Day;
    public int RenfieldActionsRemaining { get; private set; } = RenfieldActionsPerDay;
    public int DayNumber { get; private set; } = 1;

    public void SwitchActiveCharacter()
    {
        ActiveCharacter = ActiveCharacter == AdventureCharacter.Dracula
            ? AdventureCharacter.Renfield
            : AdventureCharacter.Dracula;
    }

    public void SetActiveCharacter(AdventureCharacter character)
    {
        ActiveCharacter = character;
    }

    public void AdvancePhase()
    {
        switch (Phase)
        {
            case AdventurePhase.Day:
                Phase = AdventurePhase.Dusk;
                break;
            case AdventurePhase.Dusk:
                Phase = AdventurePhase.Night;
                break;
            case AdventurePhase.Night:
                Phase = AdventurePhase.Dawn;
                break;
            default:
                Phase = AdventurePhase.Day;
                DayNumber++;
                RenfieldActionsRemaining = RenfieldActionsPerDay;
                performedRenfieldActions.Clear();
                break;
        }
    }

    public bool TryPerformRenfieldAction(RenfieldAction action)
    {
        if (Phase != AdventurePhase.Day || RenfieldActionsRemaining <= 0 || performedRenfieldActions.Contains(action))
        {
            return false;
        }

        performedRenfieldActions.Add(action);
        RenfieldActionsRemaining--;
        return true;
    }

    public bool HasPerformedRenfieldAction(RenfieldAction action)
    {
        return performedRenfieldActions.Contains(action);
    }
}
