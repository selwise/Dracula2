using NUnit.Framework;

public sealed class AdventureLoopStateTests
{
    [Test]
    public void SwitchActiveCharacterTogglesBetweenDraculaAndRenfield()
    {
        AdventureLoopState state = new AdventureLoopState();

        Assert.AreEqual(AdventureCharacter.Dracula, state.ActiveCharacter);

        state.SwitchActiveCharacter();

        Assert.AreEqual(AdventureCharacter.Renfield, state.ActiveCharacter);

        state.SwitchActiveCharacter();

        Assert.AreEqual(AdventureCharacter.Dracula, state.ActiveCharacter);
    }

    [Test]
    public void SetActiveCharacterSelectsSpecificCharacter()
    {
        AdventureLoopState state = new AdventureLoopState();

        state.SetActiveCharacter(AdventureCharacter.Renfield);
        Assert.AreEqual(AdventureCharacter.Renfield, state.ActiveCharacter);

        state.SetActiveCharacter(AdventureCharacter.Dracula);
        Assert.AreEqual(AdventureCharacter.Dracula, state.ActiveCharacter);
    }

    [Test]
    public void RenfieldCanSpendExactlyTwoDayActions()
    {
        AdventureLoopState state = new AdventureLoopState();

        Assert.IsTrue(state.TryPerformRenfieldAction(RenfieldAction.ScoutVillage));
        Assert.IsTrue(state.TryPerformRenfieldAction(RenfieldAction.PrepareBlackCandles));
        Assert.IsFalse(state.TryPerformRenfieldAction(RenfieldAction.EraseSigns));
        Assert.AreEqual(0, state.RenfieldActionsRemaining);
    }

    [Test]
    public void RenfieldCannotRepeatTheSameDayAction()
    {
        AdventureLoopState state = new AdventureLoopState();

        Assert.IsTrue(state.TryPerformRenfieldAction(RenfieldAction.ScoutVillage));
        Assert.IsFalse(state.TryPerformRenfieldAction(RenfieldAction.ScoutVillage));
        Assert.AreEqual(1, state.RenfieldActionsRemaining);
    }

    [Test]
    public void RenfieldActionsAreRejectedOutsideDay()
    {
        AdventureLoopState state = new AdventureLoopState();

        state.AdvancePhase();
        state.AdvancePhase();

        Assert.AreEqual(AdventurePhase.Night, state.Phase);
        Assert.IsFalse(state.TryPerformRenfieldAction(RenfieldAction.ResetChandelier));
        Assert.AreEqual(2, state.RenfieldActionsRemaining);
    }

    [Test]
    public void AdvancingFromDawnStartsANewDayAndRestoresRenfieldActions()
    {
        AdventureLoopState state = new AdventureLoopState();

        Assert.IsTrue(state.TryPerformRenfieldAction(RenfieldAction.ScoutVillage));
        Assert.IsTrue(state.TryPerformRenfieldAction(RenfieldAction.PrepareBlackCandles));

        state.AdvancePhase();
        state.AdvancePhase();
        state.AdvancePhase();
        state.AdvancePhase();

        Assert.AreEqual(AdventurePhase.Day, state.Phase);
        Assert.AreEqual(2, state.RenfieldActionsRemaining);
        Assert.AreEqual(2, state.DayNumber);
    }
}
