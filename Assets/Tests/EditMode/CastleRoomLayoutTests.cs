using NUnit.Framework;

public sealed class CastleRoomLayoutTests
{
    [Test]
    public void ServantWingIsOutsideCryptBoundsWithCastleGap()
    {
        Assert.IsFalse(CastleRoomLayout.CryptBounds.Overlaps(CastleRoomLayout.ServantWingBounds));
        Assert.GreaterOrEqual(CastleRoomLayout.ServantWingBounds.xMin - CastleRoomLayout.CryptBounds.xMax, CastleRoomLayout.MinimumRoomGap);
    }

    [Test]
    public void ServantWingIsSeveralRoomsAwayFromCrypt()
    {
        float centerDistance = CastleRoomLayout.ServantWingCenter.x - CastleRoomLayout.CryptCenter.x;
        float approximateRoomWidth = CastleRoomLayout.ServantWingSize.x;

        Assert.GreaterOrEqual(centerDistance, approximateRoomWidth * 5f);
    }

    [Test]
    public void PlaceholderFloorUsesShallowExistingRoomPerspective()
    {
        Assert.Greater(CastleRoomLayout.FloorRecessionSlope, 0.2f);
        Assert.Less(CastleRoomLayout.FloorRecessionSlope, 0.45f);
    }

    [Test]
    public void PortalDestinationsLandInsideTheirTargetRoomBounds()
    {
        Assert.IsTrue(CastleRoomLayout.CryptBounds.Contains(CastleRoomLayout.CryptEntryFromCastleMap));
        Assert.IsTrue(CastleRoomLayout.ServantWingBounds.Contains(CastleRoomLayout.ServantWingEntryFromCastleMap));
        Assert.IsTrue(CastleRoomLayout.CastleMapBounds.Contains(CastleRoomLayout.CastleMapEntryFromCrypt));
        Assert.IsTrue(CastleRoomLayout.CastleMapBounds.Contains(CastleRoomLayout.CastleMapEntryFromServantWing));
    }

    [Test]
    public void ServantWingExitLeadsToCastleMapInsteadOfCrypt()
    {
        Assert.IsTrue(CastleRoomLayout.CastleMapBounds.Contains(CastleRoomLayout.CastleMapEntryFromServantWing));
        Assert.IsFalse(CastleRoomLayout.CryptBounds.Contains(CastleRoomLayout.CastleMapEntryFromServantWing));
    }

    [Test]
    public void CastleMapHasIntermediateRoomsBetweenCryptAndServantWing()
    {
        Assert.Less(CastleRoomLayout.CastleMapCryptGate.x, CastleRoomLayout.CastleMapLowerProcessionalGate.x);
        Assert.Less(CastleRoomLayout.CastleMapLowerProcessionalGate.x, CastleRoomLayout.CastleMapReliquaryGate.x);
        Assert.Less(CastleRoomLayout.CastleMapReliquaryGate.x, CastleRoomLayout.CastleMapServiceCorridorGate.x);
        Assert.Less(CastleRoomLayout.CastleMapServiceCorridorGate.x, CastleRoomLayout.CastleMapServantWingGate.x);
    }

    [Test]
    public void CastleMapGatesMatchCurrentRoomExitEdges()
    {
        Assert.Greater(CastleRoomLayout.CastleMapCryptGate.y, CastleRoomLayout.CastleMapCryptRoomCenter.y);
        Assert.Less(CastleRoomLayout.CastleMapServantWingGate.x, CastleRoomLayout.CastleMapServantWingRoomCenter.x);
        Assert.Greater(CastleRoomLayout.CastleMapServantWingSecondDoorGate.x, CastleRoomLayout.CastleMapServantWingRoomCenter.x);
    }

    [Test]
    public void PortalDestinationsDoNotSpawnInsideTheirReturnTriggers()
    {
        AssertOutsideTrigger(CastleRoomLayout.CastleMapEntryFromCrypt, CastleRoomLayout.CastleMapCryptGate, 0.86f, 0.72f);
        AssertOutsideTrigger(CastleRoomLayout.CastleMapEntryFromServantWing, CastleRoomLayout.CastleMapServantWingGate, 0.86f, 0.72f);
        AssertOutsideTrigger(CastleRoomLayout.CryptEntryFromCastleMap, CastleRoomLayout.CryptToCastleMapPortal, 0.84f, 0.62f);
        AssertOutsideTrigger(CastleRoomLayout.ServantWingEntryFromCastleMap, CastleRoomLayout.ServantWingToCastleMapPortal, 0.78f, 0.74f);
    }

    private static void AssertOutsideTrigger(UnityEngine.Vector2 point, UnityEngine.Vector2 triggerCenter, float width, float height)
    {
        bool inside = UnityEngine.Mathf.Abs(point.x - triggerCenter.x) <= width * 0.5f
            && UnityEngine.Mathf.Abs(point.y - triggerCenter.y) <= height * 0.5f;

        Assert.IsFalse(inside, "Destination " + point + " is inside trigger centered at " + triggerCenter + ".");
    }
}
