using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class IntruderEncounterTests
{
    private GameObject root;
    private AdventureLoopController loopController;
    private AdventureActor draculaActor;
    private AdventureActor renfieldActor;
    private IntruderEncounter intruder;

    private static readonly FieldInfo IntruderActiveField = typeof(IntruderEncounter).GetField("active", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo IntruderResolvedField = typeof(IntruderEncounter).GetField("resolved", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo IntruderBreachedField = typeof(IntruderEncounter).GetField("breached", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo IntruderRouteIndexField = typeof(IntruderEncounter).GetField("routeIndex", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo LoopControllerAwake = typeof(AdventureLoopController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MethodInfo IntruderAwake = typeof(IntruderEncounter).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Test Root");
        loopController = root.AddComponent<AdventureLoopController>();
        loopController.startDayWithRenfield = false;
        loopController.draculaSleepsDuringDay = false;
        loopController.sceneCamera = null;

        GameObject draculaObj = new GameObject("Test Dracula");
        draculaObj.AddComponent<SpriteRenderer>();
        draculaObj.AddComponent<Rigidbody2D>();
        DraculaWalker draculaWalker = draculaObj.AddComponent<DraculaWalker>();
        draculaActor = draculaObj.AddComponent<AdventureActor>();
        draculaActor.character = AdventureCharacter.Dracula;
        draculaActor.roomName = "Crypt";
        draculaActor.walker = draculaWalker;

        GameObject renfieldObj = new GameObject("Test Renfield");
        renfieldObj.AddComponent<SpriteRenderer>();
        renfieldObj.AddComponent<Rigidbody2D>();
        DraculaWalker renfieldWalker = renfieldObj.AddComponent<DraculaWalker>();
        renfieldActor = renfieldObj.AddComponent<AdventureActor>();
        renfieldActor.character = AdventureCharacter.Renfield;
        renfieldActor.roomName = "Crypt";
        renfieldActor.walker = renfieldWalker;

        loopController.dracula = draculaActor;
        loopController.renfield = renfieldActor;
        LoopControllerAwake.Invoke(loopController, null);

        GameObject intruderObj = new GameObject("Test Intruder");
        intruderObj.AddComponent<SpriteRenderer>();
        AdventureInteractionTarget target = intruderObj.AddComponent<AdventureInteractionTarget>();
        intruder = intruderObj.AddComponent<IntruderEncounter>();
        intruder.loopController = loopController;
        intruder.interactionTarget = target;
        intruder.spriteRenderer = intruderObj.GetComponent<SpriteRenderer>();
        intruder.routePositions = new Vector3[] { new Vector3(2f, 0f, 0f) };
        intruder.routeRoomNames = new string[] { "Crypt" };
        intruder.routeLocationNames = new string[] { "Crypt Door" };
        intruder.announcesWave = false;
        IntruderAwake.Invoke(intruder, null);

        ActivateIntruder();
    }

    [TearDown]
    public void TearDown()
    {
        if (root != null)
            Object.DestroyImmediate(root);
        if (draculaActor != null && draculaActor.gameObject != null)
            Object.DestroyImmediate(draculaActor.gameObject);
        if (renfieldActor != null && renfieldActor.gameObject != null)
            Object.DestroyImmediate(renfieldActor.gameObject);
        if (intruder != null && intruder.gameObject != null)
            Object.DestroyImmediate(intruder.gameObject);
    }

    private void ActivateIntruder()
    {
        IntruderActiveField.SetValue(intruder, true);
        IntruderResolvedField.SetValue(intruder, false);
        IntruderBreachedField.SetValue(intruder, false);
        IntruderRouteIndexField.SetValue(intruder, 0);
    }

    private void ApplyPrep(RenfieldAction action)
    {
        loopController.State.TryPerformRenfieldAction(action);
    }

    private void AdvanceTo(AdventurePhase phase)
    {
        int guard = 0;
        while (loopController.State.Phase != phase && guard < 4)
        {
            loopController.State.AdvancePhase();
            guard++;
        }

        Assert.AreEqual(phase, loopController.State.Phase);
    }

    private string InteractAs(AdventureActor actor)
    {
        return intruder.Interact(actor, null);
    }

    private static IEnumerable<TestCaseData> ResolutionMatrixCases()
    {
        yield return new TestCaseData(RenfieldAction.ScoutVillage, IntruderKind.AngryPeasant, AdventureCharacter.Renfield, true, 0, 0, 0)
            .SetName("Renfield_Scout_Peasant_Clean");
        yield return new TestCaseData(RenfieldAction.ReleaseVermin, IntruderKind.AngryPeasant, AdventureCharacter.Renfield, true, 0, 1, 0)
            .SetName("Renfield_Vermin_Peasant_Messy");
        yield return new TestCaseData(RenfieldAction.EraseSigns, IntruderKind.Priest, AdventureCharacter.Renfield, true, 0, 0, 0)
            .SetName("Renfield_Erase_Priest_Clean");
        yield return new TestCaseData(RenfieldAction.PrepareBlackCandles, IntruderKind.Priest, AdventureCharacter.Renfield, true, 0, 0, 0)
            .SetName("Renfield_Candles_Priest_Clean");
        yield return new TestCaseData(RenfieldAction.ResetChandelier, IntruderKind.AngryPeasant, AdventureCharacter.Dracula, true, 0, 0, 0)
            .SetName("Dracula_Trap_Peasant_Clean");
        yield return new TestCaseData(RenfieldAction.RepairGrandHall, IntruderKind.AngryPeasant, AdventureCharacter.Dracula, true, 0, 0, 0)
            .SetName("Dracula_Hall_Peasant_Clean");
        yield return new TestCaseData(RenfieldAction.PrepareBlackCandles, IntruderKind.AngryPeasant, AdventureCharacter.Dracula, true, 0, 0, 0)
            .SetName("Dracula_Candles_Peasant_Clean");
        yield return new TestCaseData(RenfieldAction.PrepareBlackCandles, IntruderKind.Priest, AdventureCharacter.Dracula, true, 0, 0, 0)
            .SetName("Dracula_Candles_Priest_Clean");
        yield return new TestCaseData(RenfieldAction.EraseSigns, IntruderKind.Priest, AdventureCharacter.Dracula, true, 0, 0, 0)
            .SetName("Dracula_Erase_Priest_Clean");
        yield return new TestCaseData(RenfieldAction.PrepareArtifact, IntruderKind.Priest, AdventureCharacter.Dracula, true, 0, 0, 0)
            .SetName("Dracula_Artifact_Priest_Clean");
    }

    [TestCaseSource(nameof(ResolutionMatrixCases))]
    public void Resolution_ResolvesCleanly(RenfieldAction prep, IntruderKind kind, AdventureCharacter actor, bool shouldResolve, int expectedDamage, int expectedSuspicion, int expectedWounds)
    {
        intruder.kind = kind;
        ApplyPrep(prep);
        AdvanceTo(actor == AdventureCharacter.Dracula ? AdventurePhase.Night : AdventurePhase.Dusk);

        AdventureActor actingActor = actor == AdventureCharacter.Dracula ? draculaActor : renfieldActor;
        string message = InteractAs(actingActor);

        Assert.AreEqual(shouldResolve, intruder.IsResolved, "Resolution mismatch. Message: " + message);
        Assert.AreEqual(expectedDamage, intruder.CastleDamage, "CastleDamage mismatch");
        Assert.AreEqual(expectedSuspicion, intruder.VillageSuspicion, "VillageSuspicion mismatch");
        Assert.AreEqual(expectedWounds, intruder.DraculaWounds, "DraculaWounds mismatch");
    }

    [Test]
    public void Dracula_Unprepared_Peasant_IsMessy()
    {
        intruder.kind = IntruderKind.AngryPeasant;
        AdvanceTo(AdventurePhase.Night);

        string message = InteractAs(draculaActor);

        Assert.IsTrue(intruder.IsResolved, "Expected resolution. Message: " + message);
        Assert.AreEqual(1, intruder.CastleDamage, "Expected castle damage from messy resolution");
        Assert.AreEqual(1, intruder.VillageSuspicion, "Expected village suspicion from messy resolution");
    }

    [Test]
    public void Dracula_Unprepared_Breached_Priest_WoundsDracula()
    {
        intruder.kind = IntruderKind.Priest;
        IntruderBreachedField.SetValue(intruder, true);
        AdvanceTo(AdventurePhase.Night);

        string message = InteractAs(draculaActor);

        Assert.IsTrue(intruder.IsResolved, "Expected resolution. Message: " + message);
        Assert.AreEqual(0, intruder.CastleDamage, "Expected no castle damage");
        Assert.AreEqual(1, intruder.VillageSuspicion, "Expected village suspicion");
        Assert.AreEqual(1, intruder.DraculaWounds, "Expected Dracula wound");
    }

    [Test]
    public void Dracula_Unprepared_Unbreached_Priest_Stalls()
    {
        intruder.kind = IntruderKind.Priest;
        IntruderBreachedField.SetValue(intruder, false);
        AdvanceTo(AdventurePhase.Night);

        string message = InteractAs(draculaActor);

        Assert.IsFalse(intruder.IsResolved, "Expected stall, not resolution. Message: " + message);
        StringAssert.Contains("ward", message.ToLowerInvariant());
    }

    [Test]
    public void Renfield_Unprepared_Peasant_Stalls()
    {
        intruder.kind = IntruderKind.AngryPeasant;
        AdvanceTo(AdventurePhase.Dusk);

        string message = InteractAs(renfieldActor);

        Assert.IsFalse(intruder.IsResolved, "Expected stall, not resolution. Message: " + message);
        StringAssert.Contains("stall", message.ToLowerInvariant());
    }

    [Test]
    public void Renfield_Unprepared_Priest_Stalls()
    {
        intruder.kind = IntruderKind.Priest;
        AdvanceTo(AdventurePhase.Dusk);

        string message = InteractAs(renfieldActor);

        Assert.IsFalse(intruder.IsResolved, "Expected stall, not resolution. Message: " + message);
        StringAssert.Contains("talking", message.ToLowerInvariant());
    }

    [Test]
    public void Resolved_Intruder_Cannot_Be_Interacted_Again()
    {
        intruder.kind = IntruderKind.AngryPeasant;
        ApplyPrep(RenfieldAction.ResetChandelier);
        AdvanceTo(AdventurePhase.Night);

        InteractAs(draculaActor);

        Assert.IsTrue(intruder.IsResolved);
        string secondMessage = InteractAs(draculaActor);
        StringAssert.Contains("not in this part", secondMessage.ToLowerInvariant());
    }

    [Test]
    public void Intruder_Inactive_When_Not_Resolved_At_Dawn_Peasant_Escapes()
    {
        intruder.kind = IntruderKind.AngryPeasant;
        IntruderActiveField.SetValue(intruder, true);
        IntruderResolvedField.SetValue(intruder, false);
        AdvanceTo(AdventurePhase.Dawn);

        string report = intruder.GetDawnReportLine();

        Assert.IsTrue(intruder.IsResolved);
        StringAssert.Contains("loose", report.ToLowerInvariant());
        Assert.AreEqual(1, intruder.CastleDamage);
        Assert.AreEqual(1, intruder.VillageSuspicion);
    }

    [Test]
    public void Intruder_Inactive_When_Not_Resolved_At_Dawn_Priest_Escapes()
    {
        intruder.kind = IntruderKind.Priest;
        IntruderActiveField.SetValue(intruder, true);
        IntruderResolvedField.SetValue(intruder, false);
        AdvanceTo(AdventurePhase.Dawn);

        string report = intruder.GetDawnReportLine();

        Assert.IsTrue(intruder.IsResolved);
        StringAssert.Contains("loose", report.ToLowerInvariant());
        Assert.AreEqual(1, intruder.CastleDamage);
        Assert.AreEqual(2, intruder.VillageSuspicion);
    }

    [Test]
    public void Intruder_Never_Active_At_Dawn_Is_Quiet()
    {
        intruder.kind = IntruderKind.AngryPeasant;
        IntruderActiveField.SetValue(intruder, false);
        IntruderResolvedField.SetValue(intruder, false);
        AdvanceTo(AdventurePhase.Dawn);

        string report = intruder.GetDawnReportLine();

        Assert.IsFalse(intruder.IsResolved);
        StringAssert.Contains("quiet", report.ToLowerInvariant());
        Assert.AreEqual(0, intruder.CastleDamage);
    }

    [Test]
    public void Renfield_Cannot_Interact_During_Day()
    {
        loopController.State.ResetDayOne(AdventureCharacter.Renfield);
        ActivateIntruder();
        intruder.kind = IntruderKind.AngryPeasant;

        string message = InteractAs(renfieldActor);

        Assert.IsFalse(intruder.IsResolved);
        StringAssert.Contains("before the intruders enter", message.ToLowerInvariant());
    }

    [Test]
    public void Dracula_Cannot_Interact_Before_Night()
    {
        loopController.State.ResetDayOne(AdventureCharacter.Dracula);
        loopController.State.AdvancePhase();
        ActivateIntruder();
        intruder.kind = IntruderKind.AngryPeasant;

        string message = InteractAs(draculaActor);

        Assert.IsFalse(intruder.IsResolved);
        StringAssert.Contains("before nightfall", message.ToLowerInvariant());
    }

    [Test]
    public void Actor_In_Wrong_Room_Cannot_Interact()
    {
        intruder.kind = IntruderKind.AngryPeasant;
        draculaActor.roomName = "Servant Wing";
        AdvanceTo(AdventurePhase.Night);

        string message = InteractAs(draculaActor);

        Assert.IsFalse(intruder.IsResolved);
        StringAssert.Contains("elsewhere", message.ToLowerInvariant());
    }

    [Test]
    public void StatusLine_Reflects_Current_State()
    {
        intruder.kind = IntruderKind.AngryPeasant;
        intruder.intruderName = "Angry Peasant";

        IntruderActiveField.SetValue(intruder, false);
        IntruderResolvedField.SetValue(intruder, false);
        StringAssert.Contains("not inside", intruder.GetStatusLine().ToLowerInvariant());

        ActivateIntruder();
        StringAssert.Contains("crypt door", intruder.GetStatusLine().ToLowerInvariant());

        ApplyPrep(RenfieldAction.ResetChandelier);
        AdvanceTo(AdventurePhase.Night);
        InteractAs(draculaActor);
        StringAssert.Contains("handled", intruder.GetStatusLine().ToLowerInvariant());
    }

    [Test]
    public void MapHintLine_Respects_Scout_Status()
    {
        intruder.kind = IntruderKind.AngryPeasant;
        intruder.intruderName = "Angry Peasant";
        ActivateIntruder();

        bool scouted = false;
        string vague = intruder.GetMapHintLine(scouted);
        StringAssert.Contains("noise", vague.ToLowerInvariant());

        ApplyPrep(RenfieldAction.ScoutVillage);
        string exact = intruder.GetMapHintLine(true);
        StringAssert.Contains("angry peasant", exact.ToLowerInvariant());
    }
}
