using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class DraculaWalkerAnimationTests
{
    [Test]
    public void StoppingDownMovementUsesIdleDownImmediately()
    {
        GameObject obj = new GameObject("Dracula Walker Test");

        try
        {
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            DraculaWalker walker = obj.AddComponent<DraculaWalker>();
            Sprite idleSprite = CreateSprite("idle");
            Sprite transitionSprite = CreateSprite("transition");

            walker.spriteRenderer = renderer;
            walker.idleDown = new[] { idleSprite };
            walker.walkDown = new[] { transitionSprite };

            SetPrivateField(walker, "wasMoving", true);

            InvokeAnimate(walker, 0.18f);

            Assert.AreSame(idleSprite, renderer.sprite);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(obj);
        }
    }

    [Test]
    public void WalkerDoesNotExposeToIdleTransitionFrames()
    {
        const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        Assert.IsNull(typeof(DraculaWalker).GetField("toIdleDown", FieldFlags));
        Assert.IsNull(typeof(DraculaWalker).GetField("toIdleFrameTime", FieldFlags));
    }

    [Test]
    public void IdleDownAdvancesFramesWhenStandingStill()
    {
        GameObject obj = new GameObject("Dracula Idle Advance Test");

        try
        {
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            DraculaWalker walker = obj.AddComponent<DraculaWalker>();
            Sprite frame0 = CreateSprite("idle0");
            Sprite frame1 = CreateSprite("idle1");

            walker.spriteRenderer = renderer;
            walker.idleDown = new[] { frame0, frame1 };
            walker.idleFrameTime = 0.18f;

            InvokeAnimate(walker, 0.18f);

            Assert.AreSame(frame1, renderer.sprite);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(obj);
        }
    }

    [Test]
    public void LeftMovementUsesExplicitLeftFramesWhenAssigned()
    {
        GameObject obj = new GameObject("Dracula Explicit Left Test");

        try
        {
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            DraculaWalker walker = obj.AddComponent<DraculaWalker>();
            Sprite rightSprite = CreateSprite("right");
            Sprite leftSprite = CreateSprite("left");

            walker.spriteRenderer = renderer;
            walker.walkRight = new[] { rightSprite };
            walker.walkLeft = new[] { leftSprite };

            SetPrivateField(walker, "moveInput", Vector2.left);
            SetPrivateField(walker, "sideSign", -1);
            SetPrivateEnumField(walker, "facing", "Side");

            InvokeAnimate(walker, 0.01f);

            Assert.AreSame(leftSprite, renderer.sprite);
            Assert.IsFalse(renderer.flipX);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(obj);
        }
    }

    [Test]
    public void LeftMovementFallsBackToFlippedSideFrames()
    {
        GameObject obj = new GameObject("Dracula Flipped Left Test");

        try
        {
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            DraculaWalker walker = obj.AddComponent<DraculaWalker>();
            Sprite rightSprite = CreateSprite("right");

            walker.spriteRenderer = renderer;
            walker.walkRight = new[] { rightSprite };

            SetPrivateField(walker, "moveInput", Vector2.left);
            SetPrivateField(walker, "sideSign", -1);
            SetPrivateEnumField(walker, "facing", "Side");

            InvokeAnimate(walker, 0.01f);

            Assert.AreSame(rightSprite, renderer.sprite);
            Assert.IsTrue(renderer.flipX);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(obj);
        }
    }

    private static Sprite CreateSprite(string name)
    {
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.name = name + "_texture";
        texture.SetPixels32(new[]
        {
            new Color32(255, 255, 255, 255),
            new Color32(255, 255, 255, 255),
            new Color32(255, 255, 255, 255),
            new Color32(255, 255, 255, 255)
        });
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
        return Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0f), 2f);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(target, value);
    }

    private static void SetPrivateEnumField(object target, string fieldName, string value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(target, Enum.Parse(field.FieldType, value));
    }

    private static void InvokeAnimate(DraculaWalker walker, float deltaTime)
    {
        MethodInfo method = typeof(DraculaWalker).GetMethod("Animate", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(walker, new object[] { deltaTime });
    }
}
