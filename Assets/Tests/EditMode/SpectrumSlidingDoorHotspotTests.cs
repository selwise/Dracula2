using NUnit.Framework;
using UnityEngine;

public sealed class SpectrumSlidingDoorHotspotTests
{
    [Test]
    public void OpenInstantlyMovesDoorHalvesToConfiguredOffsets()
    {
        GameObject root = new GameObject("Sliding Door Test");

        try
        {
            GameObject left = new GameObject("Left");
            GameObject right = new GameObject("Right");
            left.transform.SetParent(root.transform);
            right.transform.SetParent(root.transform);
            left.transform.localPosition = new Vector3(-0.5f, 0.25f, 0f);
            right.transform.localPosition = new Vector3(0.5f, 0.25f, 0f);

            root.AddComponent<BoxCollider2D>();
            SpectrumSlidingDoorHotspot hotspot = root.AddComponent<SpectrumSlidingDoorHotspot>();
            hotspot.leftDoor = left.transform;
            hotspot.rightDoor = right.transform;
            hotspot.leftOpenOffset = new Vector3(-1.25f, 0f, 0f);
            hotspot.rightOpenOffset = new Vector3(1.25f, 0f, 0f);

            hotspot.OpenInstantlyForTests();

            Assert.IsTrue(hotspot.IsOpen);
            Assert.AreEqual(new Vector3(-1.75f, 0.25f, 0f), left.transform.localPosition);
            Assert.AreEqual(new Vector3(1.75f, 0.25f, 0f), right.transform.localPosition);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }
}
