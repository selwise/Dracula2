using UnityEngine;

public static class CastleRoomLayout
{
    public const float Bg2BackgroundPpu = 124.2f;
    public const float CastleMapPpu = 100f;
    public const float MinimumRoomGap = 20f;
    public const float FloorRecessionSlope = 0.32f;

    public static readonly Vector2 CryptCenter = new Vector2(1.15f, 0.75f);
    public static readonly Vector2 CryptSize = new Vector2(2105f / Bg2BackgroundPpu, 1014f / Bg2BackgroundPpu);
    public static readonly Rect CryptBounds = Rect.MinMaxRect(
        CryptCenter.x - CryptSize.x * 0.5f,
        CryptCenter.y - CryptSize.y * 0.5f,
        CryptCenter.x + CryptSize.x * 0.5f,
        CryptCenter.y + CryptSize.y * 0.5f);

    public static readonly Vector2 ServantWingCenter = new Vector2(42f, -0.35f);
    public static readonly Vector2 ServantWingSize = new Vector2(960f / Bg2BackgroundPpu, 520f / Bg2BackgroundPpu);
    public static readonly Rect ServantWingBounds = Rect.MinMaxRect(
        ServantWingCenter.x - ServantWingSize.x * 0.5f,
        ServantWingCenter.y - ServantWingSize.y * 0.5f,
        ServantWingCenter.x + ServantWingSize.x * 0.5f,
        ServantWingCenter.y + ServantWingSize.y * 0.5f);

    public static readonly Vector2 CryptMinBounds = new Vector2(-8.5f, -3.4f);
    public static readonly Vector2 CryptMaxBounds = new Vector2(9.5f, 1.4f);
    public static readonly Vector2 ServantWingMinBounds = new Vector2(ServantWingBounds.xMin + 0.55f, ServantWingBounds.yMin + 0.24f);
    public static readonly Vector2 ServantWingMaxBounds = new Vector2(ServantWingBounds.xMax - 0.55f, ServantWingBounds.yMax - 0.36f);

    public const string CastleProperEastGalleryRoomName = "Castle Proper - East Gallery";
    public static readonly Vector2 CastleProperEastGalleryMinBounds = new Vector2(-17.25f, -3.05f);
    public static readonly Vector2 CastleProperEastGalleryMaxBounds = new Vector2(7.15f, 0.82f);
    public static readonly Vector2 CastleProperEastGalleryCameraMin = new Vector2(-9.35f, -0.2f);
    public static readonly Vector2 CastleProperEastGalleryCameraMax = new Vector2(-0.15f, -0.2f);
    public static readonly Vector2 CastleProperEastGalleryStart = new Vector2(1.25f, -1.78f);

    public static readonly Vector2 CastleMapCenter = new Vector2(21.5f, -12f);
    public static readonly Vector2 CastleMapSize = new Vector2(1600f / CastleMapPpu, 900f / CastleMapPpu);
    public static readonly Rect CastleMapBounds = Rect.MinMaxRect(
        CastleMapCenter.x - CastleMapSize.x * 0.5f,
        CastleMapCenter.y - CastleMapSize.y * 0.5f,
        CastleMapCenter.x + CastleMapSize.x * 0.5f,
        CastleMapCenter.y + CastleMapSize.y * 0.5f);
    public static readonly Vector2 CastleMapMinBounds = new Vector2(CastleMapBounds.xMin + 0.6f, CastleMapBounds.yMin + 0.55f);
    public static readonly Vector2 CastleMapMaxBounds = new Vector2(CastleMapBounds.xMax - 0.6f, CastleMapBounds.yMax - 0.55f);

    public static readonly Vector2 CentralDemonDoorArtCenter = Bg2PixelCenterToWorld(638f, 85f, 912f, 585f);
    public static readonly Vector2 CentralDemonDoorThreshold = new Vector2(-1.08f, 0.58f);
    public static readonly Vector2 CryptToCastleMapPortal = new Vector2(-1.08f, 0.64f);
    public static readonly Vector2 CryptEntryFromCastleMap = new Vector2(-1.08f, -0.24f);

    public static readonly Vector2 ServantWingToCastleMapPortal = new Vector2(ServantWingBounds.xMin + 0.58f, ServantWingCenter.y - 1.2f);
    public static readonly Vector2 ServantWingEntryFromCastleMap = new Vector2(ServantWingBounds.xMin + 1.58f, ServantWingCenter.y - 1.2f);

    public static readonly Vector2 CastleMapCryptRoomCenter = CastleMapCenter + new Vector2(-6.15f, -1.35f);
    public static readonly Vector2 CastleMapLowerProcessionalRoomCenter = CastleMapCenter + new Vector2(-3.5f, 1.15f);
    public static readonly Vector2 CastleMapReliquaryRoomCenter = CastleMapCenter + new Vector2(-0.4f, 1.3f);
    public static readonly Vector2 CastleMapBrokenStairRoomCenter = CastleMapCenter + new Vector2(1.55f, -1.0f);
    public static readonly Vector2 CastleMapServiceCorridorRoomCenter = CastleMapCenter + new Vector2(4.35f, -0.85f);
    public static readonly Vector2 CastleMapServantWingRoomCenter = CastleMapCenter + new Vector2(5.75f, 1.45f);

    public static readonly Vector2 CastleMapCryptGate = CastleMapCryptRoomCenter + new Vector2(0.12f, 0.84f);
    public static readonly Vector2 CastleMapLowerProcessionalGate = CastleMapLowerProcessionalRoomCenter + new Vector2(-1.02f, -0.28f);
    public static readonly Vector2 CastleMapReliquaryGate = CastleMapReliquaryRoomCenter + new Vector2(-1.0f, -0.06f);
    public static readonly Vector2 CastleMapBrokenStairGate = CastleMapBrokenStairRoomCenter + new Vector2(-0.72f, 0.08f);
    public static readonly Vector2 CastleMapServiceCorridorGate = CastleMapServiceCorridorRoomCenter + new Vector2(-1.08f, -0.02f);
    public static readonly Vector2 CastleMapServantWingGate = CastleMapServantWingRoomCenter + new Vector2(-1.38f, 0f);
    public static readonly Vector2 CastleMapServantWingSecondDoorGate = CastleMapServantWingRoomCenter + new Vector2(1.16f, -0.16f);
    public static readonly Vector2 CastleMapEntryFromCrypt = CastleMapCryptGate + new Vector2(0.74f, 0.42f);
    public static readonly Vector2 CastleMapEntryFromServantWing = CastleMapServantWingGate + new Vector2(-0.88f, -0.08f);

    public static Vector2 Bg2PixelCenterToWorld(float xMin, float yMin, float xMax, float yMax)
    {
        const float width = 2105f;
        const float height = 1014f;
        float x = CryptCenter.x + (((xMin + xMax) * 0.5f) - width * 0.5f) / Bg2BackgroundPpu;
        float y = CryptCenter.y + (height * 0.5f - ((yMin + yMax) * 0.5f)) / Bg2BackgroundPpu;
        return new Vector2(x, y);
    }
}
