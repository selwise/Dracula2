using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class AdventureLoopControllerTests
{
    [Test]
    public void NewControllerDefaultsToDraculaForRoomTesting()
    {
        GameObject obj = new GameObject("Adventure Loop Controller Test");

        try
        {
            AdventureLoopController controller = obj.AddComponent<AdventureLoopController>();

            InvokeAwake(controller);

            Assert.IsFalse(controller.startDayWithRenfield);
            Assert.IsFalse(controller.draculaSleepsDuringDay);
            Assert.AreEqual(AdventureCharacter.Dracula, controller.State.ActiveCharacter);
        }
        finally
        {
            Object.DestroyImmediate(obj);
        }
    }

    [Test]
    public void StartDayWithRenfieldRemainsExplicitOptIn()
    {
        GameObject obj = new GameObject("Adventure Loop Controller Test");

        try
        {
            AdventureLoopController controller = obj.AddComponent<AdventureLoopController>();
            controller.startDayWithRenfield = true;

            InvokeAwake(controller);

            Assert.AreEqual(AdventureCharacter.Renfield, controller.State.ActiveCharacter);
        }
        finally
        {
            Object.DestroyImmediate(obj);
        }
    }

    [Test]
    public void GameOptionsCanChooseStartingCharacter()
    {
        GameObject obj = new GameObject("Adventure Loop Controller Test");
        GameObject optionsObject = new GameObject("GameOptions Test");

        try
        {
            GameOptions options = optionsObject.AddComponent<GameOptions>();
            options.startAsRenfield = true;

            AdventureLoopController controller = obj.AddComponent<AdventureLoopController>();
            controller.gameOptions = options;

            InvokeAwake(controller);

            Assert.AreEqual(AdventureCharacter.Renfield, controller.State.ActiveCharacter);
        }
        finally
        {
            Object.DestroyImmediate(optionsObject);
            Object.DestroyImmediate(obj);
        }
    }

    private static void InvokeAwake(AdventureLoopController controller)
    {
        MethodInfo awake = typeof(AdventureLoopController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
        awake.Invoke(controller, null);
    }
}
