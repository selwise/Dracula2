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
        Assert.IsTrue(CastleRoomLayout.VillageRoadMinBounds.x <= CastleRoomLayout.VillageRoadEntryFromCastle.x);
        Assert.IsTrue(CastleRoomLayout.VillageRoadEntryFromCastle.x <= CastleRoomLayout.VillageRoadMaxBounds.x);
        Assert.IsTrue(CastleRoomLayout.CastleProperEastGalleryMinBounds.x <= CastleRoomLayout.CastleEntryFromVillage.x);
        Assert.IsTrue(CastleRoomLayout.CastleEntryFromVillage.x <= CastleRoomLayout.CastleProperEastGalleryMaxBounds.x);
        Assert.IsTrue(CastleRoomLayout.UpperEastGalleryMinBounds.x <= CastleRoomLayout.UpperEastGalleryEntryFromLower.x);
        Assert.IsTrue(CastleRoomLayout.UpperEastGalleryEntryFromLower.x <= CastleRoomLayout.UpperEastGalleryMaxBounds.x);
        Assert.IsTrue(CastleRoomLayout.UpperEastGalleryMinBounds.y <= CastleRoomLayout.UpperEastGalleryEntryFromLower.y);
        Assert.IsTrue(CastleRoomLayout.UpperEastGalleryEntryFromLower.y <= CastleRoomLayout.UpperEastGalleryMaxBounds.y);
        Assert.IsTrue(CastleRoomLayout.CastleProperEastGalleryMinBounds.x <= CastleRoomLayout.EastGalleryEntryFromUpper.x);
        Assert.IsTrue(CastleRoomLayout.EastGalleryEntryFromUpper.x <= CastleRoomLayout.CastleProperEastGalleryMaxBounds.x);
        Assert.IsTrue(CastleRoomLayout.CastleProperEastGalleryMinBounds.y <= CastleRoomLayout.EastGalleryEntryFromUpper.y);
        Assert.IsTrue(CastleRoomLayout.EastGalleryEntryFromUpper.y <= CastleRoomLayout.CastleProperEastGalleryMaxBounds.y);
    }

    [Test]
    public void VillageRoadIsDelimitedOutsideCastleGallery()
    {
        Assert.Less(CastleRoomLayout.VillageRoadMaxBounds.x, CastleRoomLayout.CastleProperEastGalleryMinBounds.x);
        Assert.Greater(CastleRoomLayout.CastleProperEastGalleryMinBounds.x - CastleRoomLayout.VillageRoadMaxBounds.x, 8f);
        Assert.Less(CastleRoomLayout.VillageRoadMaxBounds.x - CastleRoomLayout.VillageRoadMinBounds.x, 14f);
    }

    [Test]
    public void EastGalleryVisualBaysEndAtRenfieldPrivateRoom()
    {
        Assert.Less(CastleRoomLayout.CastleProperEastGalleryMinBounds.x, CastleRoomLayout.CastleProperEastGalleryWestBayDividerX);
        Assert.Less(CastleRoomLayout.CastleProperEastGalleryWestBayDividerX, CastleRoomLayout.CastleProperEastGalleryMiddleBayDividerX);
        Assert.Less(CastleRoomLayout.CastleProperEastGalleryMiddleBayDividerX, CastleRoomLayout.CastleProperEastGalleryRenfieldRoomDividerX);
        Assert.Less(CastleRoomLayout.CastleProperEastGalleryRenfieldRoomDividerX, CastleRoomLayout.CastleProperEastGalleryEndStart.x);
        Assert.Less(CastleRoomLayout.CastleProperEastGalleryEndStart.x, CastleRoomLayout.CastleProperEastGalleryEastEndWallX);
    }

    [Test]
    public void EastGalleryBaysAreBroadEnoughToReadAsRooms()
    {
        Assert.Greater(CastleRoomLayout.CastleProperEastGalleryWestBayDividerX - CastleRoomLayout.CastleProperEastGalleryMinBounds.x, 8f);
        Assert.Greater(CastleRoomLayout.CastleProperEastGalleryMiddleBayDividerX - CastleRoomLayout.CastleProperEastGalleryWestBayDividerX, 8f);
        Assert.Greater(CastleRoomLayout.CastleProperEastGalleryRenfieldRoomDividerX - CastleRoomLayout.CastleProperEastGalleryMiddleBayDividerX, 8f);
        Assert.Greater(CastleRoomLayout.CastleProperEastGalleryEastEndWallX - CastleRoomLayout.CastleProperEastGalleryRenfieldRoomDividerX, 8f);
    }

    [Test]
    public void EastGalleryMovementStopsBeforeTerminalWall()
    {
        Assert.Less(CastleRoomLayout.CastleProperEastGalleryMaxBounds.x, CastleRoomLayout.CastleProperEastGalleryEastEndWallX);
        Assert.Greater(CastleRoomLayout.CastleProperEastGalleryEastEndWallX - CastleRoomLayout.CastleProperEastGalleryMaxBounds.x, 0.25f);
    }

    [Test]
    public void UpperEastGalleryStacksAboveLowerGallery()
    {
        Assert.AreEqual(CastleRoomLayout.CastleProperEastGalleryMinBounds.x, CastleRoomLayout.UpperEastGalleryMinBounds.x);
        Assert.AreEqual(CastleRoomLayout.CastleProperEastGalleryMaxBounds.x, CastleRoomLayout.UpperEastGalleryMaxBounds.x);
        Assert.Greater(CastleRoomLayout.UpperEastGalleryMinBounds.y, CastleRoomLayout.CastleProperEastGalleryMaxBounds.y);
        Assert.AreEqual(CastleRoomLayout.EastGalleryCentralStairTrigger.x, CastleRoomLayout.UpperEastGalleryEntryFromLower.x);
        Assert.AreEqual(CastleRoomLayout.EastGalleryEntryFromUpper.x, CastleRoomLayout.UpperEastGalleryEntryFromLower.x);
        Assert.AreEqual(CastleRoomLayout.UpperEastGalleryRowOffset.y, CastleRoomLayout.UpperEastGalleryEntryFromLower.y - CastleRoomLayout.EastGalleryEntryFromUpper.y);
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
        AssertOutsideTrigger(CastleRoomLayout.VillageRoadEntryFromCastle, new UnityEngine.Vector2(CastleRoomLayout.VillageRoadMaxBounds.x - 0.24f, -2.18f), 0.52f, 1.38f);
        AssertOutsideTrigger(CastleRoomLayout.CastleEntryFromVillage, new UnityEngine.Vector2(CastleRoomLayout.CastleProperEastGalleryMinBounds.x + 0.36f, -2.14f), 0.52f, 1.38f);
    }

    private static void AssertOutsideTrigger(UnityEngine.Vector2 point, UnityEngine.Vector2 triggerCenter, float width, float height)
    {
        bool inside = UnityEngine.Mathf.Abs(point.x - triggerCenter.x) <= width * 0.5f
            && UnityEngine.Mathf.Abs(point.y - triggerCenter.y) <= height * 0.5f;

        Assert.IsFalse(inside, "Destination " + point + " is inside trigger centered at " + triggerCenter + ".");
    }
}
