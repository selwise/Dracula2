using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CastleHorizontalRowPrototypeBuilder
{
    private const string ScenePath = "Assets/Scenes/CastleProperEastGalleryPrototype.unity";
    private const string PrototypeFolder = "Assets/Art/Castle/Prototype";
    private const string PixelPath = PrototypeFolder + "/castle_blockout_pixel.png";
    private const string ObliqueDoorwaySpritePath = PrototypeFolder + "/castle_gallery_oblique_doorwall_user_exact.png";
    private const string CastleWallTilePath = "Assets/Art/Castle/Tiles/wall_castle_gray_block_tile.png";
    private const string DeeperGalleryWallTilePath = "Assets/Art/Castle/Tiles/wall_ornate_green_bronze_panel_tile.png";
    private const string RenfieldSpritePath = "Assets/Art/Characters/Renfield/Renfield.png";
    private const string DraculaSheetPath = "Assets/Art/Characters/Dracula/Sheets/dracula_spectrum_walk_down_sheet.png";
    private const string GrimReaperStatuePath = "Assets/Art/Castle/Props/GrimReaperStatue.png";
    private const string FountainPath = "Assets/Art/Castle/Props/Fountain.png";
    private const string GalleryWindowPath = "Assets/Art/Castle/Props/Window.png";
    private const string GalleryWindowNightPanePath = "Assets/Art/Castle/Props/WindowNightPane.png";
    private const string GalleryWindowRainOverlayPath = "Assets/Art/Castle/Props/WindowRainOverlay.png";
    private const string FlameAnimationPath = "Assets/Art/Effects/Flame-anim.png";
    private const string DorianToccataPath = "Assets/Audio/Music/DorianToccata.mp3";
    private const string RainLoopPath = "Assets/Audio/SFX/Rain.mp3";
    private const string ThunderOnePath = "Assets/Audio/SFX/Thunder1.mp3";
    private const string ThunderTwoPath = "Assets/Audio/SFX/Thunder2.mp3";
    private const string ThunderThreePath = "Assets/Audio/SFX/Thunder3.mp3";
    private const string WallSconcePrefabPath = "Assets/Prefabs/Castle/WallSconceCandle.prefab";
    private const string SpriteLitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";
    private const string SpriteUnlitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
    private const float CastleWallTilePpu = 240f;
    private const float DeeperGalleryWallTilePpu = 260f;
    private const float ObliqueDoorwayPpu = 264f;
    private const float RenfieldPpu = 220f;
    private const int SpriteAlignmentCenter = 0;
    private const int SpriteAlignmentCustom = 9;
    private const float GalleryVisualWestX = -17.8f;
    private const float GalleryVisualEastPadding = 0.9f;

    private static readonly Vector3 CameraOffset = new Vector3(-3.72f, 1.32f, -10f);
    private static readonly CastleObliqueWallPlacementRule ObliqueDoorwayPlacementRule =
        CastleObliqueWallPlacementRule.CastleProperEastGalleryLeftReturn();

    [MenuItem("Dracula/Build Castle East Gallery Prototype")]
    public static void BuildCastleEastGalleryPrototype()
    {
        Sprite pixel = EnsureBlockoutPixel();
        Sprite castleWallTile = LoadCastleWallTile();
        Sprite deeperGalleryWallTile = LoadDeeperGalleryWallTile();
        Sprite obliqueDoorwaySprite = LoadObliqueDoorwaySprite();
        Sprite renfieldSprite = LoadRenfieldSprite();
        Sprite[] draculaFrames = LoadDraculaFrames();
        Sprite grimReaperStatue = LoadSingleSprite(GrimReaperStatuePath, "Grim Reaper statue");
        Sprite fountain = LoadSingleSprite(FountainPath, "fountain");
        Sprite galleryWindow = LoadSingleSprite(GalleryWindowPath, "gallery window");
        Sprite galleryWindowNightPane = LoadSingleSprite(GalleryWindowNightPanePath, "gallery window night pane");
        Sprite[] galleryWindowRainOverlayFrames = LoadSpriteRepresentations(GalleryWindowRainOverlayPath, "gallery window rain overlay");
        Sprite[] reaperTorchFlameFrames = LoadSpriteRepresentations(FlameAnimationPath, "Grim Reaper torch flame");
        CastleWallSconcePrefabBuilder.EnsurePrefabExists();
        Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteLitMaterialPath);
        Material unlitMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteUnlitMaterialPath);
        AudioClip dorianToccata = AssetDatabase.LoadAssetAtPath<AudioClip>(DorianToccataPath);
        AudioClip rainLoop = AssetDatabase.LoadAssetAtPath<AudioClip>(RainLoopPath);
        AudioClip thunderOne = AssetDatabase.LoadAssetAtPath<AudioClip>(ThunderOnePath);
        AudioClip thunderTwo = AssetDatabase.LoadAssetAtPath<AudioClip>(ThunderTwoPath);
        AudioClip thunderThree = AssetDatabase.LoadAssetAtPath<AudioClip>(ThunderThreePath);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "CastleProperEastGalleryPrototype";

        GameObject root = new GameObject("Castle Proper East Gallery Prototype");
        GameObject row = new GameObject("Continuous East Gallery Hallway");
        row.transform.SetParent(root.transform);

        GameObject cameraObject = CreateCamera(root.transform);
        CreateReferenceLight(root.transform);
        CreateGlobal2DLight(root.transform);

        Transform background = CreateChild(row.transform, "Painted Corridor Blockout");
        Transform props = CreateChild(row.transform, "Segment Detail Props");

        BuildCorridorBase(pixel, castleWallTile, "Hallway Castle Gray Block Wall Tile", background, litMaterial);
        BuildSegmentBreak(obliqueDoorwaySprite, background, litMaterial);
        BuildGalleryRoomDivisions(pixel, obliqueDoorwaySprite, background, props, litMaterial, false);
        BuildWallSconceRun(props);
        BuildEastEndStop(pixel, background, props, litMaterial);
        BuildCentralDepthArchway("Lower Gallery Rear Wall Archway", pixel, props, litMaterial, false);
        BuildLowerGallerySetDressing(
            pixel,
            grimReaperStatue,
            reaperTorchFlameFrames,
            fountain,
            galleryWindow,
            galleryWindowNightPane,
            galleryWindowRainOverlayFrames,
            background,
            props,
            litMaterial,
            unlitMaterial);

        GameObject upperRow = new GameObject("Deeper East Gallery Hallway - Prototype Duplicate");
        upperRow.transform.SetParent(root.transform);
        Transform upperBackground = CreateChild(upperRow.transform, "Deeper Painted Corridor Blockout");
        Transform upperProps = CreateChild(upperRow.transform, "Deeper Segment Detail Props");
        BuildCorridorBase(pixel, deeperGalleryWallTile, "Deeper Gallery Blue Gold Wall Tile", upperBackground, litMaterial);
        BuildSegmentBreak(obliqueDoorwaySprite, upperBackground, litMaterial);
        BuildGalleryRoomDivisions(pixel, obliqueDoorwaySprite, upperBackground, upperProps, litMaterial, true);
        BuildWallSconceRun(upperProps);
        BuildEastEndStop(pixel, upperBackground, upperProps, litMaterial);
        BuildCentralDepthArchway("Deeper Gallery South Return Archway", pixel, upperProps, litMaterial, true);
        BuildDeeperGalleryIdentityOverlay(pixel, upperBackground, upperProps, litMaterial);

        GameObject renfield = CreateRenfield(renfieldSprite, litMaterial, unlitMaterial, root.transform);
        GameObject dracula = CreateDracula(draculaFrames, litMaterial, unlitMaterial, root.transform);
        AdventureLoopController loopController = CreateGameOptions(
            renfield.GetComponent<AdventureActor>(),
            dracula.GetComponent<AdventureActor>(),
            cameraObject.GetComponent<Camera>(),
            root.transform);
        AdventureActionStation[] stations = BuildFirstDayGameplay(pixel, row.transform, upperRow.transform, props, litMaterial, loopController);
        loopController.renfieldStations = stations;
        IntruderEncounter[] intruders = BuildDayOneThreats(pixel, row.transform, loopController);
        CreateAdventureSystems(loopController, root.transform, intruders);
        CreateSceneAudio(loopController, root.transform, dorianToccata, rainLoop, thunderOne, thunderTwo, thunderThree);
        upperRow.transform.position = new Vector3(CastleRoomLayout.UpperEastGalleryRowOffset.x, CastleRoomLayout.UpperEastGalleryRowOffset.y, 0f);
        SnapCameraToActor(cameraObject.transform, renfield.GetComponent<AdventureActor>());
        ConfigureLayoutInspector(root.transform);

        Selection.activeGameObject = root;
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("Built castle horizontal row prototype at " + ScenePath + ".");
    }

    private static void BuildCorridorBase(Sprite pixel, Sprite wallTile, string wallTileName, Transform parent, Material litMaterial)
    {
        float visualEastX = CastleRoomLayout.CastleProperEastGalleryEastEndWallX + GalleryVisualEastPadding;
        float visualWidth = visualEastX - GalleryVisualWestX;
        float visualCenterX = (GalleryVisualWestX + visualEastX) * 0.5f;

        CreateRect("Hallway Deep Background", pixel, new Vector3(visualCenterX, 0.02f, 0f), new Vector2(visualWidth, 7.95f), new Color(0.06f, 0.11f, 0.11f, 1f), -50, parent, null);
        CreateRect("Hallway Upper Warm Wall", pixel, new Vector3(visualCenterX, 2.04f, 0f), new Vector2(visualWidth, 2.42f), new Color(0.38f, 0.27f, 0.16f, 1f), -40, parent, litMaterial);
        CreateRect("Hallway Top Shadow Cap", pixel, new Vector3(visualCenterX, 3.22f, 0f), new Vector2(visualWidth, 0.32f), new Color(0.2f, 0.16f, 0.11f, 1f), -38, parent, litMaterial);
        CreateRect("Hallway Upper Cornice", pixel, new Vector3(visualCenterX, 2.68f, 0f), new Vector2(visualWidth, 0.24f), new Color(0.48f, 0.34f, 0.18f, 1f), -37, parent, litMaterial);

        CreateRect("Hallway Middle Brown Field", pixel, new Vector3(visualCenterX, 0.88f, 0f), new Vector2(visualWidth, 1.82f), new Color(0.31f, 0.22f, 0.14f, 1f), -35, parent, litMaterial);
        CreateRect("Hallway Warm Horizontal Band", pixel, new Vector3(visualCenterX, 0.28f, 0f), new Vector2(visualWidth, 0.26f), new Color(0.62f, 0.39f, 0.18f, 1f), -34, parent, litMaterial);
        CreateRect("Hallway Lower Green Wall", pixel, new Vector3(visualCenterX, -0.66f, 0f), new Vector2(visualWidth, 1.14f), new Color(0.17f, 0.23f, 0.2f, 1f), -33, parent, litMaterial);
        CreateRect("Hallway Lower Dark Rail", pixel, new Vector3(visualCenterX, -1.18f, 0f), new Vector2(visualWidth, 0.16f), new Color(0.07f, 0.06f, 0.06f, 1f), -32, parent, litMaterial);
        CreateRect("Hallway Red Rail Hairline", pixel, new Vector3(visualCenterX, -1.05f, 0f), new Vector2(visualWidth, 0.035f), new Color(0.26f, 0.1f, 0.07f, 1f), -31, parent, litMaterial);

        CreateTiledSprite(wallTileName, wallTile, new Vector3(visualCenterX, 0.88f, 0f), new Vector2(visualWidth, 4.12f), Color.white, -27, parent, litMaterial);

        CreateRect("Hallway Floor Base", pixel, new Vector3(visualCenterX, -2.78f, 0f), new Vector2(visualWidth, 3.12f), new Color(0.25f, 0.2f, 0.13f, 1f), -26, parent, litMaterial);
        CreateRect("Hallway Floor Back Shadow", pixel, new Vector3(visualCenterX, -1.55f, 0f), new Vector2(visualWidth, 0.32f), new Color(0.16f, 0.18f, 0.14f, 1f), -25, parent, litMaterial);
        CreateRect("Hallway Front Floor Lip", pixel, new Vector3(visualCenterX, -4.17f, 0f), new Vector2(visualWidth, 0.38f), new Color(0.17f, 0.14f, 0.09f, 1f), -24, parent, litMaterial);
        CreateRect("Hallway Near Black Groove", pixel, new Vector3(visualCenterX, -3.9f, 0f), new Vector2(visualWidth, 0.07f), new Color(0.055f, 0.048f, 0.038f, 1f), -23, parent, null);

        int wallBlockCount = Mathf.CeilToInt(visualWidth / 1.92f) + 1;
        for (int i = 0; i < wallBlockCount; i++)
        {
            float x = GalleryVisualWestX + 0.92f + i * 1.92f;
            CreateRect("Back Wall Vertical Block " + i, pixel, new Vector3(x, 1.06f, 0f), new Vector2(0.04f, 3.55f), new Color(0.16f, 0.13f, 0.1f, 0.45f), -28, parent, null);
        }

        int plankCount = Mathf.CeilToInt(visualWidth / 1.12f) + 2;
        for (int i = 0; i < plankCount; i++)
        {
            float x = GalleryVisualWestX + 0.62f + i * 1.12f;
            float y = -3.62f + (i % 4) * 0.44f;
            CreateRect("Floor Plank Break " + i, pixel, new Vector3(x, y, 0f), new Vector2(0.82f, 0.035f), new Color(0.11f, 0.095f, 0.07f, 0.72f), -22, parent, null);
        }
    }

    private static void BuildSegmentBreak(Sprite obliqueDoorwaySprite, Transform background, Material litMaterial)
    {
        BuildObliqueGalleryReturn(obliqueDoorwaySprite, background, litMaterial);
    }

    private static void BuildObliqueGalleryReturn(Sprite obliqueDoorwaySprite, Transform background, Material litMaterial)
    {
        CreateObliqueDoorwall(
            "Open Arch Oblique Doorway Candidate",
            obliqueDoorwaySprite,
            ObliqueDoorwayPlacementRule,
            Color.white,
            22,
            background,
            litMaterial);
    }

    private static void BuildGalleryRoomDivisions(
        Sprite pixel,
        Sprite obliqueDoorwaySprite,
        Transform background,
        Transform props,
        Material litMaterial,
        bool hauntedMirrorRoom)
    {
        CreateWindowwallDivider(
            "Gallery West Windowwall Divider",
            CastleRoomLayout.CastleProperEastGalleryWestBayDividerX,
            pixel,
            background,
            litMaterial);
        CreateWindowwallDivider(
            "Gallery Middle Windowwall Divider",
            CastleRoomLayout.CastleProperEastGalleryMiddleBayDividerX,
            pixel,
            background,
            litMaterial);

        if (hauntedMirrorRoom)
        {
            CreateHauntedMirrorRoomBay(pixel, background, props, litMaterial);
            CreateRightRoomDoorwall(
                "Haunted Mirror Room Arch Doorwall",
                "Haunted Mirror Room Threshold Shadow",
                obliqueDoorwaySprite,
                pixel,
                background,
                props,
                litMaterial,
                new Color(0.64f, 0.7f, 0.92f, 1f));
            return;
        }

        CreateRenfieldPrivateRoomBay(pixel, background, props, litMaterial);
        CreateRightRoomDoorwall(
            "Renfield Private Room Arch Doorwall",
            "Renfield Room Threshold Shadow",
            obliqueDoorwaySprite,
            pixel,
            background,
            props,
            litMaterial,
            new Color(0.9f, 0.92f, 0.84f, 1f));
    }

    private static void CreateRightRoomDoorwall(
        string doorwallName,
        string thresholdName,
        Sprite obliqueDoorwaySprite,
        Sprite pixel,
        Transform background,
        Transform props,
        Material litMaterial,
        Color color)
    {
        CastleObliqueWallPlacementRule roomArchRule = ObliqueDoorwayPlacementRule.Clone();
        roomArchRule.ruleName = doorwallName;
        roomArchRule.backWallCenterX = CastleRoomLayout.CastleProperEastGalleryRenfieldRoomDividerX;
        roomArchRule.backWallOffsetX = 0f;
        roomArchRule.perspectiveScale = new Vector3(1.22f, 0.94f, 1f);
        roomArchRule.bottomOverscan = 0.13f;
        CreateObliqueDoorwall(
            doorwallName,
            obliqueDoorwaySprite,
            roomArchRule,
            color,
            27,
            background,
            litMaterial);

        CreateRect(
            thresholdName,
            pixel,
            new Vector3(CastleRoomLayout.CastleProperEastGalleryRenfieldRoomDividerX + 0.12f, -2.26f, 0f),
            new Vector2(0.34f, 1.38f),
            new Color(0.025f, 0.02f, 0.018f, 0.68f),
            47,
            props,
            null);
    }

    private static GameObject CreateObliqueDoorwall(
        string name,
        Sprite obliqueDoorwaySprite,
        CastleObliqueWallPlacementRule rule,
        Color color,
        int sortingOrder,
        Transform parent,
        Material litMaterial)
    {
        GameObject doorway = CreateSprite(
            name,
            obliqueDoorwaySprite,
            rule.CalculatePosition(obliqueDoorwaySprite),
            rule.perspectiveScale,
            color,
            sortingOrder,
            parent,
            litMaterial);

        rule.ApplyTo(doorway.transform, obliqueDoorwaySprite);
        CastleObliqueWallPlacement placement = doorway.AddComponent<CastleObliqueWallPlacement>();
        placement.Configure(doorway.GetComponent<SpriteRenderer>(), rule);
        return doorway;
    }

    private static Vector3 CalculateObliqueDoorwayPosition(Sprite obliqueDoorwaySprite)
    {
        return ObliqueDoorwayPlacementRule.CalculatePosition(obliqueDoorwaySprite);
    }

    private static void CreateWindowwallDivider(
        string name,
        float x,
        Sprite pixel,
        Transform parent,
        Material litMaterial)
    {
        CreateRect(name + " Stone Pier", pixel, new Vector3(x, 0.44f, 0f), new Vector2(0.22f, 4.45f), new Color(0.11f, 0.13f, 0.12f, 0.9f), -19, parent, litMaterial);
        CreateRect(name + " Left Shadow", pixel, new Vector3(x - 0.16f, 0.38f, 0f), new Vector2(0.08f, 4.2f), new Color(0.018f, 0.02f, 0.018f, 0.82f), -18, parent, null);
        CreateRect(name + " Right Highlight", pixel, new Vector3(x + 0.15f, 0.46f, 0f), new Vector2(0.045f, 4.1f), new Color(0.54f, 0.47f, 0.32f, 0.36f), -17, parent, null);
        CreateRect(name + " High Glass", pixel, new Vector3(x - 0.82f, 1.26f, 0f), new Vector2(1.1f, 1.72f), new Color(0.02f, 0.07f, 0.12f, 0.52f), -16, parent, null);
        CreateRect(name + " Glass Mullion", pixel, new Vector3(x - 0.82f, 1.26f, 0f), new Vector2(0.06f, 1.76f), new Color(0.035f, 0.04f, 0.038f, 0.85f), -15, parent, null);
        CreateRect(name + " Threshold Strip", pixel, new Vector3(x, -2.12f, 0f), new Vector2(0.36f, 1.06f), new Color(0.06f, 0.05f, 0.038f, 0.76f), 45, parent, null);
    }

    private static void CreateRenfieldPrivateRoomBay(Sprite pixel, Transform background, Transform props, Material litMaterial)
    {
        float roomCenterX = (CastleRoomLayout.CastleProperEastGalleryRenfieldRoomDividerX + CastleRoomLayout.CastleProperEastGalleryEastEndWallX) * 0.5f;
        float roomWidth = CastleRoomLayout.CastleProperEastGalleryEastEndWallX - CastleRoomLayout.CastleProperEastGalleryRenfieldRoomDividerX;

        CreateRect("Renfield Private Room Wall Wash", pixel, new Vector3(roomCenterX, 0.9f, 0f), new Vector2(roomWidth, 3.55f), new Color(0.13f, 0.09f, 0.105f, 0.56f), -21, background, litMaterial);
        CreateRect("Renfield Private Room Back Window Night", pixel, new Vector3(roomCenterX + 0.62f, 1.0f, 0f), new Vector2(1.34f, 2.06f), new Color(0.015f, 0.04f, 0.085f, 0.64f), -14, background, null);
        CreateRect("Renfield Private Room Window Left Mullion", pixel, new Vector3(roomCenterX + 0.25f, 1.0f, 0f), new Vector2(0.06f, 2.08f), new Color(0.035f, 0.038f, 0.035f, 0.88f), -13, background, null);
        CreateRect("Renfield Private Room Window Right Mullion", pixel, new Vector3(roomCenterX + 0.99f, 1.0f, 0f), new Vector2(0.06f, 2.08f), new Color(0.035f, 0.038f, 0.035f, 0.88f), -13, background, null);
        CreateRect("Renfield Private Room Ledger Table", pixel, new Vector3(roomCenterX - 0.62f, -2.18f, 0f), new Vector2(1.18f, 0.24f), new Color(0.13f, 0.08f, 0.052f, 1f), 103, props, litMaterial);
        CreateRect("Renfield Private Room Ledger Page", pixel, new Vector3(roomCenterX - 0.42f, -2.01f, 0f), new Vector2(0.42f, 0.08f), new Color(0.78f, 0.68f, 0.42f, 0.95f), 104, props, null);
        CreateRect("Renfield Private Room Floor Rug", pixel, new Vector3(roomCenterX + 0.3f, -2.86f, 0f), new Vector2(2.0f, 0.34f), new Color(0.12f, 0.025f, 0.035f, 0.78f), 48, props, null);
    }

    private static void CreateHauntedMirrorRoomBay(Sprite pixel, Transform background, Transform props, Material litMaterial)
    {
        float roomCenterX = (CastleRoomLayout.CastleProperEastGalleryRenfieldRoomDividerX + CastleRoomLayout.CastleProperEastGalleryEastEndWallX) * 0.5f;
        float roomWidth = CastleRoomLayout.CastleProperEastGalleryEastEndWallX - CastleRoomLayout.CastleProperEastGalleryRenfieldRoomDividerX;
        float mirrorX = roomCenterX + 0.34f;

        CreateRect("Haunted Mirror Room Wall Wash", pixel, new Vector3(roomCenterX, 0.9f, 0f), new Vector2(roomWidth, 3.55f), new Color(0.055f, 0.09f, 0.135f, 0.68f), -21, background, litMaterial);
        CreateRect("Haunted Mirror Room Cold Back Panel", pixel, new Vector3(mirrorX, 1.02f, 0f), new Vector2(2.32f, 2.42f), new Color(0.035f, 0.055f, 0.09f, 0.76f), -14, background, null);
        CreateRect("Haunted Mirror Room Side Drape", pixel, new Vector3(roomCenterX + 2.08f, 0.62f, 0f), new Vector2(0.36f, 2.96f), new Color(0.08f, 0.025f, 0.04f, 0.82f), -13, background, litMaterial);

        CreateRect("Magic Mirror Back Shadow", pixel, new Vector3(mirrorX + 0.08f, 0.48f, 0f), new Vector2(1.72f, 2.48f), new Color(0.002f, 0.003f, 0.006f, 0.74f), 91, props, null);
        CreateRect("Magic Mirror Gold Outer Frame", pixel, new Vector3(mirrorX, 0.58f, 0f), new Vector2(1.44f, 2.32f), new Color(0.6f, 0.45f, 0.18f, 1f), 92, props, litMaterial);
        CreateRect("Magic Mirror Dark Frame Cutout", pixel, new Vector3(mirrorX, 0.58f, 0f), new Vector2(1.08f, 1.92f), new Color(0.025f, 0.018f, 0.024f, 1f), 93, props, null);
        CreateRect("Magic Mirror Glass", pixel, new Vector3(mirrorX, 0.62f, 0f), new Vector2(0.82f, 1.52f), new Color(0.25f, 0.48f, 0.62f, 0.68f), 94, props, null);
        CreateRect("Magic Mirror Black Reflection", pixel, new Vector3(mirrorX - 0.1f, 0.5f, 0f), new Vector2(0.42f, 1.18f), new Color(0.004f, 0.006f, 0.012f, 0.72f), 95, props, null);
        CreateRect("Magic Mirror Pale Glint", pixel, new Vector3(mirrorX + 0.26f, 1.18f, 0f), new Vector2(0.08f, 0.72f), new Color(0.68f, 0.9f, 0.95f, 0.72f), 96, props, null);

        CreateRect("Haunted Mirror Room Covered Shape", pixel, new Vector3(roomCenterX - 1.72f, -2.18f, 0f), new Vector2(1.24f, 0.72f), new Color(0.1f, 0.08f, 0.075f, 1f), 103, props, litMaterial);
        CreateRect("Haunted Mirror Room Dust Cloth", pixel, new Vector3(roomCenterX - 1.62f, -1.88f, 0f), new Vector2(1.08f, 0.22f), new Color(0.48f, 0.47f, 0.42f, 0.72f), 104, props, null);
        CreateRect("Haunted Mirror Room Candle Placeholder A", pixel, new Vector3(roomCenterX + 1.82f, -1.86f, 0f), new Vector2(0.16f, 0.58f), new Color(0.74f, 0.64f, 0.42f, 1f), 104, props, litMaterial);
        CreateRect("Haunted Mirror Room Candle Flame A", pixel, new Vector3(roomCenterX + 1.82f, -1.46f, 0f), new Vector2(0.12f, 0.18f), new Color(1f, 0.54f, 0.18f, 0.88f), 105, props, null);
        CreateRect("Haunted Mirror Room Floor Rune Placeholder", pixel, new Vector3(roomCenterX + 0.22f, -2.84f, 0f), new Vector2(2.2f, 0.28f), new Color(0.045f, 0.18f, 0.2f, 0.62f), 48, props, null);
    }

    private static void BuildWallSconceRun(Transform props)
    {
        const float sconceY = 1.12f;
        const float scale = 0.94f;
        TryPlaceWallSconce(new Vector3(-12.2f, sconceY, 0f), props, scale);
        TryPlaceWallSconce(new Vector3(-6.25f, sconceY, 0f), props, scale);
        TryPlaceWallSconce(new Vector3(-0.3f, sconceY, 0f), props, scale);
        TryPlaceWallSconce(new Vector3(3.55f, sconceY, 0f), props, scale);
        TryPlaceWallSconce(new Vector3(8.85f, sconceY, 0f), props, scale);
        TryPlaceWallSconce(new Vector3(14.45f, sconceY, 0f), props, scale);
    }

    private static void BuildEastEndStop(Sprite pixel, Transform background, Transform props, Material litMaterial)
    {
        float endX = CastleRoomLayout.CastleProperEastGalleryEastEndWallX;
        CreateRect("East End Dark Wall Face", pixel, new Vector3(endX, 0.02f, 0f), new Vector2(1.02f, 6.8f), new Color(0.025f, 0.11f, 0.11f, 1f), 24, background, litMaterial);
        CreateRect("East End Inner Brown Return", pixel, new Vector3(endX - 0.68f, 0.48f, 0f), new Vector2(0.44f, 4.9f), new Color(0.36f, 0.24f, 0.14f, 1f), 23, background, litMaterial);
        CreateRect("East End Black Side Shadow", pixel, new Vector3(endX - 0.43f, -0.1f, 0f), new Vector2(0.16f, 4.35f), new Color(0.02f, 0.024f, 0.024f, 1f), 25, background, null);
        CreateRect("East End Terminal Black Mass", pixel, new Vector3(endX + 0.64f, -0.1f, 0f), new Vector2(0.72f, 6.9f), new Color(0.004f, 0.006f, 0.006f, 1f), 28, background, null);
        CreateRect("East End Top Return Block", pixel, new Vector3(endX - 0.62f, 2.45f, 0f), new Vector2(0.88f, 0.9f), new Color(0.3f, 0.22f, 0.15f, 1f), 26, background, litMaterial);
        CreateRect("East End Lower Return Block", pixel, new Vector3(endX - 0.64f, -1.05f, 0f), new Vector2(0.82f, 0.5f), new Color(0.1f, 0.12f, 0.11f, 1f), 26, background, litMaterial);
        CreateRect("East End Floor Stop Shadow", pixel, new Vector3(endX - 0.96f, -2.38f, 0f), new Vector2(1.32f, 0.28f), new Color(0.09f, 0.08f, 0.055f, 1f), 46, background, null);
        CreateRect("East End No Passage Floor Block", pixel, new Vector3(endX + 0.25f, -2.64f, 0f), new Vector2(0.48f, 1.42f), new Color(0.018f, 0.015f, 0.012f, 0.82f), 49, props, null);
        CreateRect("Renfield Ground Shadow", pixel, new Vector3(CastleRoomLayout.CastleProperEastGalleryEndStart.x + 0.08f, -2.86f, 0f), new Vector2(0.95f, 0.13f), new Color(0.02f, 0.018f, 0.014f, 0.65f), 55, props, null);
    }

    private static void BuildCentralDepthArchway(string name, Sprite pixel, Transform props, Material litMaterial, bool southReturn)
    {
        Vector2 archway = CastleRoomLayout.EastGalleryRearArchwayTrigger;
        if (southReturn)
        {
            CreateRect(name + " South Black Square", pixel, new Vector3(archway.x, archway.y - 0.08f, 0f), new Vector2(1.16f, 0.92f), new Color(0.002f, 0.002f, 0.003f, 0.96f), 54, props, null);
            CreateRect(name + " South Inner Void", pixel, new Vector3(archway.x, archway.y - 0.03f, 0f), new Vector2(0.78f, 0.58f), new Color(0f, 0f, 0f, 1f), 55, props, null);
            CreateRect(name + " South Rim Lip", pixel, new Vector3(archway.x, archway.y + 0.42f, 0f), new Vector2(1.24f, 0.09f), new Color(0.2f, 0.22f, 0.24f, 0.82f), 56, props, litMaterial);
            CreateRect(name + " South Floor Pull", pixel, new Vector3(archway.x, archway.y - 0.58f, 0f), new Vector2(1.34f, 0.14f), new Color(0.015f, 0.014f, 0.013f, 0.82f), 53, props, null);
            return;
        }

        CreateRect(name + " Back Wall Mouth", pixel, new Vector3(archway.x, -0.78f, 0f), new Vector2(1.62f, 1.68f), new Color(0.006f, 0.008f, 0.012f, 0.94f), 52, props, null);
        CreateRect(name + " Top Lintel", pixel, new Vector3(archway.x, 0.13f, 0f), new Vector2(1.92f, 0.16f), new Color(0.28f, 0.23f, 0.16f, 0.86f), 53, props, litMaterial);
        CreateRect(name + " Left Jamb", pixel, new Vector3(archway.x - 0.9f, -0.72f, 0f), new Vector2(0.16f, 1.74f), new Color(0.28f, 0.23f, 0.16f, 0.86f), 53, props, litMaterial);
        CreateRect(name + " Right Jamb", pixel, new Vector3(archway.x + 0.9f, -0.72f, 0f), new Vector2(0.16f, 1.74f), new Color(0.28f, 0.23f, 0.16f, 0.86f), 53, props, litMaterial);
        CreateRect(name + " Inner Back Wall Shadow", pixel, new Vector3(archway.x, -0.24f, 0f), new Vector2(1.1f, 0.56f), new Color(0.012f, 0.012f, 0.02f, 0.62f), 54, props, null);
        CreateRect(name + " Threshold Shadow", pixel, new Vector3(archway.x, archway.y - 0.08f, 0f), new Vector2(1.48f, 0.24f), new Color(0.012f, 0.011f, 0.01f, 0.78f), 54, props, null);
    }

    private static void BuildDeeperGalleryIdentityOverlay(Sprite pixel, Transform background, Transform props, Material litMaterial)
    {
        float visualEastX = CastleRoomLayout.CastleProperEastGalleryEastEndWallX + GalleryVisualEastPadding;
        float visualWidth = visualEastX - GalleryVisualWestX;
        float visualCenterX = (GalleryVisualWestX + visualEastX) * 0.5f;

        CreateRect("Deeper Gallery Moon Wash", pixel, new Vector3(visualCenterX, 0.34f, 0f), new Vector2(visualWidth, 6.9f), new Color(0.08f, 0.17f, 0.26f, 0.24f), -12, background, null);
        CreateRect("Deeper Gallery Cold Floor Glaze", pixel, new Vector3(visualCenterX, -2.86f, 0f), new Vector2(visualWidth, 2.42f), new Color(0.08f, 0.16f, 0.2f, 0.22f), 50, props, null);
        CreateRect("Deeper Gallery South Return Sightline", pixel, new Vector3(CastleRoomLayout.EastGalleryRearArchwayTrigger.x, -1.46f, 0f), new Vector2(1.58f, 0.08f), new Color(0.18f, 0.2f, 0.22f, 0.78f), 67, props, litMaterial);
    }

    private static void BuildLowerGallerySetDressing(
        Sprite pixel,
        Sprite grimReaperStatue,
        Sprite[] reaperTorchFlameFrames,
        Sprite fountain,
        Sprite galleryWindow,
        Sprite galleryWindowNightPane,
        Sprite[] galleryWindowRainOverlayFrames,
        Transform background,
        Transform props,
        Material litMaterial,
        Material unlitMaterial)
    {
        GameObject grimReaper = CreateEmpty("GrimReaper", new Vector3(-0.93097746f, -0.67f, 0f), Vector3.one, props);
        GameObject torchFlame = CreateLocalSprite("Grim Reaper Torch Flame", FirstFrameOrNull(reaperTorchFlameFrames), grimReaper.transform, new Vector3(0.7f, 0.79f, 0.64f), Vector3.one * 0.652f, Quaternion.identity, Color.white, 28, unlitMaterial);
        SpriteFrameAnimator torchAnimator = torchFlame.AddComponent<SpriteFrameAnimator>();
        torchAnimator.playOnEnable = true;
        torchAnimator.loop = true;
        torchAnimator.animateInEditMode = true;
        torchAnimator.Configure(torchFlame.GetComponent<SpriteRenderer>(), reaperTorchFlameFrames, 12f);
        CreateLocalSprite("GrimReaperStatue", grimReaperStatue, grimReaper.transform, new Vector3(1f, 1f, -0.43f), Vector3.one, Quaternion.identity, new Color(0.45403165f, 0.5241622f, 0.5377358f, 1f), 29, litMaterial);
        CreateLocalRect("Grim Reaper Back Shadow A", pixel, grimReaper.transform, new Vector3(-0.16f, 1.05f, 0f), new Vector3(0.71982425f, 2.9380257f, 1f), Quaternion.identity, new Color(0f, 0f, 0f, 0.6509804f), 24, null);
        CreateLocalRect("Grim Reaper Back Shadow B", pixel, grimReaper.transform, new Vector3(0.6f, 1.79f, 0f), new Vector3(1.3035566f, 2.0345743f, 1f), Quaternion.Euler(0f, 0f, -153.67f), new Color(0f, 0f, 0f, 0.2784314f), 24, null);
        CreateLocalRect("Grim Reaper Back Shadow C", pixel, grimReaper.transform, new Vector3(0.84f, 1.51f, 0f), new Vector3(2.3625f, 3.007969f, 1f), Quaternion.Euler(0f, 0f, -6.212f), new Color(0f, 0f, 0f, 0.62352943f), 24, null);

        GameObject window = CreateEmpty("Window", new Vector3(9.8049f, 2.75f, 0f), Vector3.one * 7.9593263f, background);
        CreateLocalSprite("WindowFrame", galleryWindow, window.transform, new Vector3(0.32f, 0f, 0f), Vector3.one, Quaternion.identity, new Color(0.3276522f, 0.3490566f, 0.34538046f, 1f), 58, litMaterial);
        CreateLocalSprite("WindowNightPane", galleryWindowNightPane, window.transform, new Vector3(0.32f, 0f, 0.01f), Vector3.one, Quaternion.identity, Color.white, 56, unlitMaterial);
        GameObject rainOverlay = CreateLocalSprite("WindowRainOverlay", FirstFrameOrNull(galleryWindowRainOverlayFrames), window.transform, new Vector3(0.294f, -0.039f, 0.008f), new Vector3(1.0319662f, 1f, 1f), Quaternion.Euler(0f, 0f, 17.191f), new Color(1f, 1f, 1f, 0.95f), 57, unlitMaterial);
        SpriteFrameAnimator rainAnimator = rainOverlay.AddComponent<SpriteFrameAnimator>();
        rainAnimator.playOnEnable = true;
        rainAnimator.loop = true;
        rainAnimator.animateInEditMode = true;
        rainAnimator.Configure(rainOverlay.GetComponent<SpriteRenderer>(), galleryWindowRainOverlayFrames, 10f);

        CreateSprite(
            "Fountain",
            fountain,
            new Vector3(-14.55f, -2.45f, 0f),
            Vector3.one * 0.42f,
            new Color(0.6784888f, 0.745283f, 0.74132234f, 1f),
            96,
            props,
            litMaterial);
    }

    private static AdventureActionStation[] BuildFirstDayGameplay(
        Sprite pixel,
        Transform row,
        Transform upperRow,
        Transform props,
        Material litMaterial,
        AdventureLoopController loopController)
    {
        Transform gameplay = CreateChild(row, "Day 1 Gameplay Test");
        Transform upperGameplay = CreateChild(upperRow, "Deeper Row Travel Test");
        Transform village = CreateChild(row, "Delimited Village Road");
        BuildVillageRoad(pixel, village, litMaterial);

        Transform castleDestination = CreateMarker("Castle Entry From Village", CastleRoomLayout.CastleEntryFromVillage, gameplay);
        Transform villageDestination = CreateMarker("Village Entry From Castle", CastleRoomLayout.VillageRoadEntryFromCastle, gameplay);
        Transform lowerGalleryDestination = CreateMarker("Lower Gallery Entry From South Return", CastleRoomLayout.EastGalleryEntryFromUpper, gameplay);
        Transform upperGalleryDestination = CreateMarker("Deeper Gallery Entry From Rear Archway", CastleRoomLayout.EastGalleryRearArchwayTrigger, upperGameplay);

        CreatePortal(
            "Gate To Village Road",
            new Vector3(CastleRoomLayout.CastleProperEastGalleryMinBounds.x + 0.36f, -2.14f, 0f),
            new Vector2(0.52f, 1.38f),
            villageDestination,
            CastleRoomLayout.VillageRoadRoomName,
            CastleRoomLayout.VillageRoadMinBounds,
            CastleRoomLayout.VillageRoadMaxBounds,
            loopController,
            gameplay);

        CreatePortal(
            "Gate Back To Castle",
            new Vector3(CastleRoomLayout.VillageRoadMaxBounds.x - 0.24f, -2.18f, 0f),
            new Vector2(0.52f, 1.38f),
            castleDestination,
            CastleRoomLayout.CastleProperEastGalleryRoomName,
            CastleRoomLayout.CastleProperEastGalleryMinBounds,
            CastleRoomLayout.CastleProperEastGalleryMaxBounds,
            loopController,
            gameplay);

        CreatePortal(
            "Rear Archway To Deeper Gallery",
            new Vector3(CastleRoomLayout.EastGalleryRearArchwayTrigger.x, CastleRoomLayout.EastGalleryRearArchwayTrigger.y, 0f),
            new Vector2(1.08f, 0.86f),
            upperGalleryDestination,
            CastleRoomLayout.UpperEastGalleryRoomName,
            CastleRoomLayout.UpperEastGalleryMinBounds,
            CastleRoomLayout.UpperEastGalleryMaxBounds,
            loopController,
            gameplay);

        CreatePortal(
            "South Return Archway To East Gallery",
            new Vector3(CastleRoomLayout.EastGalleryRearArchwayTrigger.x, CastleRoomLayout.EastGalleryRearArchwayTrigger.y, 0f),
            new Vector2(1.08f, 0.86f),
            lowerGalleryDestination,
            CastleRoomLayout.CastleProperEastGalleryRoomName,
            CastleRoomLayout.CastleProperEastGalleryMinBounds,
            CastleRoomLayout.CastleProperEastGalleryMaxBounds,
            loopController,
            upperGameplay);

        loopController.scholomanceMirrorStation = CreateScholomanceMirrorStation(pixel, upperGameplay);

        List<AdventureActionStation> stations = new List<AdventureActionStation>();
        stations.Add(CreateActionStation(
            "Prep Station - Black Candles",
            RenfieldAction.PrepareBlackCandles,
            "Black Candles",
            "Prepare ward-smothering candles against priests.",
            new Vector3(3.45f, -2.18f, 0f),
            new Vector2(0.36f, 0.62f),
            props,
            pixel,
            new Color(0.14f, 0.08f, 0.16f, 1f),
            102));
        stations.Add(CreateActionStation(
            "Prep Station - Chandelier Trap",
            RenfieldAction.ResetChandelier,
            "Gallery Trap",
            "Rig a trap for villagers who force the castle doors.",
            new Vector3(-0.55f, -2.08f, 0f),
            new Vector2(0.72f, 0.2f),
            props,
            pixel,
            new Color(0.62f, 0.48f, 0.2f, 1f),
            101));
        stations.Add(CreateActionStation(
            "Prep Station - Demeter Crate",
            RenfieldAction.MoveCoffin,
            "Demeter Crate",
            "Ready the travel coffin and false cargo papers.",
            new Vector3(16.65f, -2.23f, 0f),
            new Vector2(0.92f, 0.26f),
            props,
            pixel,
            new Color(0.18f, 0.12f, 0.09f, 1f),
            101));
        stations.Add(CreateActionStation(
            "Prep Station - Scout Village",
            RenfieldAction.ScoutVillage,
            "Threat Rumors",
            "Listen at the inn road for villagers and priests.",
            new Vector3(CastleRoomLayout.VillageRoadCenter.x - 1.85f, -2.24f, 0f),
            new Vector2(0.62f, 0.86f),
            village,
            pixel,
            new Color(0.34f, 0.19f, 0.09f, 1f),
            80));
        stations.Add(CreateActionStation(
            "Prep Station - Lure Victim",
            RenfieldAction.LureVictim,
            "Lure Victim",
            "Mark an isolated villager for Dracula's hunger.",
            new Vector3(CastleRoomLayout.VillageRoadCenter.x + 1.55f, -2.25f, 0f),
            new Vector2(0.58f, 0.82f),
            village,
            pixel,
            new Color(0.18f, 0.055f, 0.055f, 1f),
            81));

        return stations.ToArray();
    }

    private static ScholomanceMirrorStation CreateScholomanceMirrorStation(Sprite pixel, Transform parent)
    {
        float roomCenterX = (CastleRoomLayout.CastleProperEastGalleryRenfieldRoomDividerX + CastleRoomLayout.CastleProperEastGalleryEastEndWallX) * 0.5f;
        float mirrorX = roomCenterX + 0.34f;
        GameObject stationObject = CreateRect(
            "Scholomance Mirror Station",
            pixel,
            new Vector3(mirrorX, 0.62f, 0f),
            new Vector2(0.82f, 1.52f),
            new Color(0.28f, 0.68f, 0.78f, 0.18f),
            97,
            parent,
            null);
        ScholomanceMirrorStation station = stationObject.AddComponent<ScholomanceMirrorStation>();
        station.displayName = "Scholomance Mirror";
        station.description = "Dracula communes with Scholomance for route counsel.";
        station.interactRadius = 1.2f;
        station.spriteRenderer = stationObject.GetComponent<SpriteRenderer>();
        station.dormantTint = new Color(0.28f, 0.68f, 0.78f, 0.18f);
        station.consultedTint = new Color(0.64f, 0.92f, 0.96f, 0.36f);
        return station;
    }

    private static void BuildVillageRoad(Sprite pixel, Transform parent, Material litMaterial)
    {
        Vector2 center = CastleRoomLayout.VillageRoadCenter;
        CreateRect("Village Night Sky", pixel, new Vector3(center.x, -0.2f, 0f), new Vector2(12.8f, 7.1f), new Color(0.025f, 0.035f, 0.06f, 1f), -60, parent, null);
        CreateRect("Village Far Hill", pixel, new Vector3(center.x - 0.4f, -0.88f, 0f), new Vector2(12.8f, 1.4f), new Color(0.035f, 0.065f, 0.06f, 1f), -55, parent, litMaterial);
        CreateRect("Village Road Mud", pixel, new Vector3(center.x, -2.78f, 0f), new Vector2(12.8f, 2.15f), new Color(0.13f, 0.1f, 0.07f, 1f), -42, parent, litMaterial);
        CreateRect("Village Road Back Edge", pixel, new Vector3(center.x, -1.77f, 0f), new Vector2(12.8f, 0.18f), new Color(0.04f, 0.035f, 0.028f, 1f), -39, parent, null);
        CreateRect("Village Left Black Tree Wall", pixel, new Vector3(CastleRoomLayout.VillageRoadMinBounds.x - 0.7f, -1.08f, 0f), new Vector2(1.25f, 6.6f), new Color(0.004f, 0.007f, 0.008f, 1f), -30, parent, null);
        CreateRect("Village Castle Gate Wall", pixel, new Vector3(CastleRoomLayout.VillageRoadMaxBounds.x + 0.56f, -1.08f, 0f), new Vector2(1.35f, 6.6f), new Color(0.015f, 0.018f, 0.02f, 1f), -30, parent, null);
        CreateRect("Village Inn Blockout", pixel, new Vector3(center.x - 2.05f, -1.2f, 0f), new Vector2(2.2f, 1.65f), new Color(0.19f, 0.105f, 0.055f, 1f), -28, parent, litMaterial);
        CreateRect("Village Inn Roof", pixel, new Vector3(center.x - 2.05f, -0.34f, 0f), new Vector2(2.55f, 0.38f), new Color(0.055f, 0.045f, 0.038f, 1f), -25, parent, null);
        CreateRect("Village Inn Door", pixel, new Vector3(center.x - 1.8f, -1.64f, 0f), new Vector2(0.48f, 0.86f), new Color(0.035f, 0.024f, 0.02f, 1f), -24, parent, null);
        CreateRect("Village Church Silhouette", pixel, new Vector3(center.x + 1.88f, -1.22f, 0f), new Vector2(1.7f, 2.05f), new Color(0.035f, 0.04f, 0.05f, 1f), -29, parent, litMaterial);
        CreateRect("Village Church Steeple", pixel, new Vector3(center.x + 1.88f, 0.18f, 0f), new Vector2(0.58f, 1.45f), new Color(0.025f, 0.03f, 0.04f, 1f), -27, parent, litMaterial);
        CreateRect("Village Church Cross", pixel, new Vector3(center.x + 1.88f, 1.05f, 0f), new Vector2(0.36f, 0.06f), new Color(0.16f, 0.15f, 0.11f, 1f), -23, parent, null);
        CreateRect("Village Church Cross Stem", pixel, new Vector3(center.x + 1.88f, 0.94f, 0f), new Vector2(0.06f, 0.38f), new Color(0.16f, 0.15f, 0.11f, 1f), -23, parent, null);

        for (int i = 0; i < 7; i++)
        {
            float x = CastleRoomLayout.VillageRoadMinBounds.x + 1.2f + i * 1.62f;
            CreateRect("Village Fence Post " + i, pixel, new Vector3(x, -1.62f, 0f), new Vector2(0.08f, 0.58f), new Color(0.07f, 0.052f, 0.037f, 1f), -20, parent, null);
        }
    }

    private static Transform CreateMarker(string name, Vector2 position, Transform parent)
    {
        GameObject marker = new GameObject(name);
        marker.transform.SetParent(parent);
        marker.transform.position = new Vector3(position.x, position.y, 0f);
        return marker.transform;
    }

    private static void CreatePortal(
        string name,
        Vector3 position,
        Vector2 size,
        Transform destination,
        string destinationRoomName,
        Vector2 destinationMinBounds,
        Vector2 destinationMaxBounds,
        AdventureLoopController loopController,
        Transform parent)
    {
        GameObject portalObject = new GameObject(name);
        portalObject.transform.SetParent(parent);
        portalObject.transform.position = position;
        BoxCollider2D collider = portalObject.AddComponent<BoxCollider2D>();
        collider.size = size;

        CastleRoomPortal portal = portalObject.AddComponent<CastleRoomPortal>();
        portal.loopController = loopController;
        portal.destination = destination;
        portal.destinationRoomName = destinationRoomName;
        portal.destinationMinBounds = destinationMinBounds;
        portal.destinationMaxBounds = destinationMaxBounds;
        portal.snapCamera = true;
    }

    private static AdventureActionStation CreateActionStation(
        string name,
        RenfieldAction action,
        string displayName,
        string description,
        Vector3 position,
        Vector2 size,
        Transform parent,
        Sprite pixel,
        Color color,
        int sortingOrder)
    {
        GameObject stationObject = CreateRect(name, pixel, position, size, color, sortingOrder, parent, null);
        AdventureActionStation station = stationObject.AddComponent<AdventureActionStation>();
        station.action = action;
        station.displayName = displayName;
        station.description = description;
        station.interactRadius = 1.05f;
        station.spriteRenderer = stationObject.GetComponent<SpriteRenderer>();
        station.availableTint = color;
        station.completedTint = new Color(0.22f, 0.25f, 0.28f, 1f);
        return station;
    }

    private static GameObject CreateCamera(Transform parent)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.transform.SetParent(parent);
        cameraObject.transform.position = new Vector3(0f, -0.2f, -10f);
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 3.75f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        cameraObject.AddComponent<AudioListener>();
        return cameraObject;
    }

    private static void CreateReferenceLight(Transform parent)
    {
        GameObject lightObject = new GameObject("Directional Light - Reference Only");
        lightObject.transform.SetParent(parent);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0f;
        light.enabled = false;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void CreateGlobal2DLight(Transform parent)
    {
        Type lightType = FindLight2DType();
        if (lightType == null)
        {
            Debug.LogWarning("Light2D type was not found. Castle prototype will render without URP 2D global light.");
            return;
        }

        GameObject lightObject = new GameObject("Global Low Ambient 2D Light");
        lightObject.transform.SetParent(parent);
        Component light = lightObject.AddComponent(lightType);
        SetEnumProperty(light, lightType, "lightType", "Global");
        SetFloatProperty(light, lightType, "intensity", 0.4f);
        SetColorProperty(light, lightType, "color", new Color(0.82f, 0.72f, 0.58f, 1f));
    }

    private static GameObject CreateRenfield(Sprite sprite, Material litMaterial, Material unlitMaterial, Transform parent)
    {
        Sprite[] frame = sprite != null ? new[] { sprite } : new Sprite[0];
        GameObject player = new GameObject("Renfield Placeholder");
        player.transform.SetParent(parent);
        player.transform.position = new Vector3(CastleRoomLayout.CastleProperEastGalleryEndStart.x, CastleRoomLayout.CastleProperEastGalleryEndStart.y, 0f);
        player.transform.localScale = Vector3.one;

        SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 280;
        renderer.sortingLayerName = "Default";
        if (litMaterial != null)
        {
            renderer.sharedMaterial = litMaterial;
        }

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        GameObject footCollider = new GameObject("Ground Contact Collider");
        footCollider.transform.SetParent(player.transform, false);
        footCollider.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        BoxCollider2D collider = footCollider.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.42f, 0.16f);

        DraculaWalker walker = player.AddComponent<DraculaWalker>();
        walker.spriteRenderer = renderer;
        walker.body = body;
        walker.walkDown = frame;
        walker.walkUp = frame;
        walker.walkRight = frame;
        walker.walkLeft = frame;
        walker.idleDown = frame;
        walker.idleUp = frame;
        walker.idleSide = frame;
        walker.idleLeft = frame;
        walker.moveSpeed = 2.25f;
        walker.frameTime = 0.12f;
        walker.sideFrameTime = 0.12f;
        walker.idleFrameTime = 0.24f;
        walker.baseSortingOrder = 278;
        walker.ySortMultiplier = 30f;
        walker.minSortingOrder = 180;
        walker.maxSortingOrder = 345;
        walker.minBounds = CastleRoomLayout.CastleProperEastGalleryMinBounds;
        walker.maxBounds = CastleRoomLayout.CastleProperEastGalleryMaxBounds;

        AdventureActor actor = player.AddComponent<AdventureActor>();
        actor.character = AdventureCharacter.Renfield;
        actor.roomName = CastleRoomLayout.CastleProperEastGalleryRoomName;
        actor.walker = walker;
        actor.spriteRenderer = renderer;
        actor.inactiveTint = new Color(0.38f, 0.38f, 0.42f, 1f);

        CharacterReadabilityOverlay readability = player.AddComponent<CharacterReadabilityOverlay>();
        readability.sourceRenderer = renderer;
        readability.overlayMaterial = unlitMaterial;
        readability.alpha = 0.08f;
        readability.tint = new Color(0.82f, 0.86f, 0.92f, 1f);

        return player;
    }

    private static GameObject CreateDracula(Sprite[] frames, Material litMaterial, Material unlitMaterial, Transform parent)
    {
        Sprite sprite = frames != null && frames.Length > 0 ? frames[0] : null;
        GameObject player = new GameObject("Dracula Playtest Placeholder");
        player.transform.SetParent(parent);
        player.transform.position = new Vector3(CastleRoomLayout.CastleProperEastGalleryEndStart.x + 0.82f, -2.0f, 0f);
        player.transform.localScale = Vector3.one * 0.58f;

        SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 282;
        renderer.sortingLayerName = "Default";
        if (litMaterial != null)
        {
            renderer.sharedMaterial = litMaterial;
        }

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        GameObject footCollider = new GameObject("Ground Contact Collider");
        footCollider.transform.SetParent(player.transform, false);
        footCollider.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        BoxCollider2D collider = footCollider.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.54f, 0.18f);

        DraculaWalker walker = player.AddComponent<DraculaWalker>();
        walker.spriteRenderer = renderer;
        walker.body = body;
        walker.walkDown = frames;
        walker.walkUp = frames;
        walker.walkRight = frames;
        walker.walkLeft = frames;
        walker.idleDown = frames;
        walker.idleUp = frames;
        walker.idleSide = frames;
        walker.idleLeft = frames;
        walker.moveSpeed = 2.05f;
        walker.frameTime = 0.13f;
        walker.sideFrameTime = 0.13f;
        walker.idleFrameTime = 0.25f;
        walker.baseSortingOrder = 280;
        walker.ySortMultiplier = 30f;
        walker.minSortingOrder = 180;
        walker.maxSortingOrder = 350;
        walker.minBounds = CastleRoomLayout.CastleProperEastGalleryMinBounds;
        walker.maxBounds = CastleRoomLayout.CastleProperEastGalleryMaxBounds;

        AdventureActor actor = player.AddComponent<AdventureActor>();
        actor.character = AdventureCharacter.Dracula;
        actor.roomName = CastleRoomLayout.CastleProperEastGalleryRoomName;
        actor.walker = walker;
        actor.spriteRenderer = renderer;
        actor.inactiveTint = new Color(0.24f, 0.24f, 0.3f, 0.72f);

        CharacterReadabilityOverlay readability = player.AddComponent<CharacterReadabilityOverlay>();
        readability.sourceRenderer = renderer;
        readability.overlayMaterial = unlitMaterial;
        readability.alpha = 0.1f;
        readability.tint = new Color(0.84f, 0.86f, 0.94f, 1f);

        return player;
    }

    private static AdventureLoopController CreateGameOptions(AdventureActor renfield, AdventureActor dracula, Camera sceneCamera, Transform parent)
    {
        GameObject optionsObject = new GameObject("GameOptions");
        optionsObject.transform.SetParent(parent);

        GameOptions options = optionsObject.AddComponent<GameOptions>();
        options.startAsRenfield = true;

        AdventureLoopController loopController = optionsObject.AddComponent<AdventureLoopController>();
        loopController.gameOptions = options;
        loopController.startDayWithRenfield = true;
        loopController.dracula = dracula;
        loopController.renfield = renfield;
        loopController.sceneCamera = sceneCamera;
        loopController.cameraOffset = CameraOffset;
        loopController.cameraFollowSpeed = 13f;
        loopController.draculaSleepsDuringDay = true;
        loopController.allowEmergencyDayWake = true;
        loopController.emergencyWakeSeconds = 7f;
        loopController.emergencyWakeUsesPerDay = 1;
        loopController.emergencyDraculaMoveSpeedMultiplier = 0.52f;
        loopController.renfieldStations = new AdventureActionStation[0];

        if (renfield != null)
        {
            renfield.ApplyControl(true, true);
        }

        return loopController;
    }

    private static void CreateAdventureSystems(AdventureLoopController loopController, Transform parent, IntruderEncounter[] intruders)
    {
        GameObject systems = new GameObject("Day 1 Runtime Systems");
        systems.transform.SetParent(parent);

        AdventureInventory inventory = systems.AddComponent<AdventureInventory>();

        AdventureInteractionController interaction = systems.AddComponent<AdventureInteractionController>();
        interaction.loopController = loopController;
        interaction.inventory = inventory;

        AdventureDayReport report = systems.AddComponent<AdventureDayReport>();
        report.loopController = loopController;
        report.intruders = intruders;
    }

    private static void CreateSceneAudio(
        AdventureLoopController loopController,
        Transform parent,
        AudioClip music,
        AudioClip rainLoop,
        AudioClip thunderOne,
        AudioClip thunderTwo,
        AudioClip thunderThree)
    {
        GameObject audioObject = new GameObject("Castle Prototype Audio");
        audioObject.transform.SetParent(parent);

        MusicPlayer musicPlayer = audioObject.AddComponent<MusicPlayer>();
        musicPlayer.loopController = loopController;
        musicPlayer.useActiveActorRoom = false;
        musicPlayer.contextRefreshSeconds = 0.25f;

        if (music != null)
        {
            musicPlayer.musicTracks.Add(new MusicPlayer.MusicTrack
            {
                label = "Dorian Toccata",
                enabled = true,
                clip = music,
                volume = 0.42f,
                loop = true,
                playWhenMatched = true,
                priority = 0
            });
        }

        if (rainLoop != null)
        {
            musicPlayer.loopingAmbience.Add(new MusicPlayer.LoopingAmbience
            {
                label = "Window Rain",
                enabled = true,
                clip = rainLoop,
                volume = 0.18f,
                stopWhenUnmatched = false
            });
        }

        MusicPlayer.RandomOneShotGroup thunder = new MusicPlayer.RandomOneShotGroup
        {
            label = "Distant Thunder",
            enabled = true,
            volume = 0.68f,
            pauseRange = new Vector2(9f, 24f),
            avoidImmediateRepeat = true
        };

        AddRandomClip(thunder, thunderOne, 0.85f);
        AddRandomClip(thunder, thunderTwo, 0.75f);
        AddRandomClip(thunder, thunderThree, 0.9f);
        if (thunder.HasPlayableClip)
        {
            musicPlayer.randomOneShotGroups.Add(thunder);
        }
    }

    private static void AddRandomClip(MusicPlayer.RandomOneShotGroup group, AudioClip clip, float volume)
    {
        if (group == null || clip == null)
        {
            return;
        }

        group.clips.Add(new MusicPlayer.RandomClipSlot
        {
            clip = clip,
            volume = volume
        });
    }

    private static IntruderEncounter[] BuildDayOneThreats(Sprite pixel, Transform row, AdventureLoopController loopController)
    {
        Transform threats = CreateChild(row, "Day 1 Night Threats");
        List<IntruderEncounter> intruders = new List<IntruderEncounter>();

        intruders.Add(CreateIntruder(
            "Angry Peasant Threat",
            IntruderKind.AngryPeasant,
            "Angry Peasant",
            new Color(0.45f, 0.25f, 0.14f, 1f),
            new Vector3[]
            {
                new Vector3(CastleRoomLayout.VillageRoadCenter.x - 0.6f, -2.18f, 0f),
                new Vector3(CastleRoomLayout.CastleProperEastGalleryMinBounds.x + 1.45f, -2.08f, 0f),
                new Vector3(-0.55f, -2.08f, 0f)
            },
            new string[]
            {
                CastleRoomLayout.VillageRoadRoomName,
                CastleRoomLayout.CastleProperEastGalleryRoomName,
                CastleRoomLayout.CastleProperEastGalleryRoomName
            },
            new string[] { "Inn Road", "Castle Gate", "Gallery Trap" },
            10f,
            true,
            pixel,
            threats,
            loopController));

        intruders.Add(CreateIntruder(
            "Priest Threat",
            IntruderKind.Priest,
            "Village Priest",
            new Color(0.72f, 0.68f, 0.52f, 1f),
            new Vector3[]
            {
                new Vector3(CastleRoomLayout.VillageRoadCenter.x + 1.85f, -2.16f, 0f),
                new Vector3(CastleRoomLayout.CastleProperEastGalleryMinBounds.x + 1.1f, -2.06f, 0f),
                new Vector3(3.45f, -2.06f, 0f)
            },
            new string[]
            {
                CastleRoomLayout.VillageRoadRoomName,
                CastleRoomLayout.CastleProperEastGalleryRoomName,
                CastleRoomLayout.CastleProperEastGalleryRoomName
            },
            new string[] { "Church Road", "Castle Gate", "Black Candle Niche" },
            13f,
            false,
            pixel,
            threats,
            loopController));

        return intruders.ToArray();
    }

    private static IntruderEncounter CreateIntruder(
        string objectName,
        IntruderKind kind,
        string intruderName,
        Color color,
        Vector3[] routePositions,
        string[] routeRooms,
        string[] routeLocations,
        float stepSeconds,
        bool announcesWave,
        Sprite pixel,
        Transform parent,
        AdventureLoopController loopController)
    {
        GameObject intruderObject = CreateRect(objectName, pixel, routePositions[0], new Vector2(0.42f, 0.92f), color, 330, parent, null);
        AdventureInteractionTarget target = intruderObject.AddComponent<AdventureInteractionTarget>();
        target.anyCharacter = true;
        target.displayName = intruderName;
        target.verb = "CONFRONT";
        target.description = "A night threat is moving through the route.";
        target.interactRadius = 0.92f;

        IntruderEncounter intruder = intruderObject.AddComponent<IntruderEncounter>();
        intruder.loopController = loopController;
        intruder.interactionTarget = target;
        intruder.spriteRenderer = intruderObject.GetComponent<SpriteRenderer>();
        intruder.kind = kind;
        intruder.intruderName = intruderName;
        intruder.routePositions = routePositions;
        intruder.routeRoomNames = routeRooms;
        intruder.routeLocationNames = routeLocations;
        intruder.stepSeconds = stepSeconds;
        intruder.announcesWave = announcesWave;
        target.receiver = intruder;
        return intruder;
    }

    private static void SnapCameraToActor(Transform cameraTransform, AdventureActor actor)
    {
        if (cameraTransform == null || actor == null)
        {
            return;
        }

        cameraTransform.position = actor.CameraPosition + CameraOffset;
    }

    private static Sprite EnsureBlockoutPixel()
    {
        Directory.CreateDirectory(PrototypeFolder);
        if (!File.Exists(PixelPath))
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            File.WriteAllBytes(PixelPath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(PixelPath);
        }

        ConfigureSpriteImporter(PixelPath, new Vector2(0.5f, 0.5f), 1f);
        return AssetDatabase.LoadAssetAtPath<Sprite>(PixelPath);
    }

    private static Sprite LoadRenfieldSprite()
    {
        if (!File.Exists(RenfieldSpritePath))
        {
            throw new FileNotFoundException("Missing required Renfield sprite.", RenfieldSpritePath);
        }

        ConfigureSpriteImporter(RenfieldSpritePath, new Vector2(0.5f, 0f), RenfieldPpu);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RenfieldSpritePath);
        if (sprite == null)
        {
            throw new FileNotFoundException("Unity failed to import the Renfield sprite.", RenfieldSpritePath);
        }

        return sprite;
    }

    private static Sprite[] LoadDraculaFrames()
    {
        if (!File.Exists(DraculaSheetPath))
        {
            throw new FileNotFoundException("Missing required Dracula sprite sheet.", DraculaSheetPath);
        }

        AssetDatabase.ImportAsset(DraculaSheetPath);
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(DraculaSheetPath);
        List<Sprite> sprites = new List<Sprite>();
        for (int i = 0; i < assets.Length; i++)
        {
            Sprite sprite = assets[i] as Sprite;
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }

        sprites.Sort(CompareSpriteFrameNames);
        if (sprites.Count == 0)
        {
            throw new FileNotFoundException("Unity failed to load Dracula sheet sprites.", DraculaSheetPath);
        }

        return sprites.ToArray();
    }

    private static int CompareSpriteFrameNames(Sprite a, Sprite b)
    {
        int aNumber = ExtractTrailingNumber(a.name);
        int bNumber = ExtractTrailingNumber(b.name);
        if (aNumber >= 0 && bNumber >= 0 && aNumber != bNumber)
        {
            return aNumber.CompareTo(bNumber);
        }

        return string.CompareOrdinal(a.name, b.name);
    }

    private static int ExtractTrailingNumber(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return -1;
        }

        int end = text.Length - 1;
        while (end >= 0 && !char.IsDigit(text[end]))
        {
            end--;
        }

        if (end < 0)
        {
            return -1;
        }

        int start = end;
        while (start >= 0 && char.IsDigit(text[start]))
        {
            start--;
        }

        int value;
        return int.TryParse(text.Substring(start + 1, end - start), out value) ? value : -1;
    }

    private static Sprite LoadSingleSprite(string path, string label)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Missing required " + label + " sprite.", path);
        }

        AssetDatabase.ImportAsset(path);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
        {
            return sprite;
        }

        Sprite[] frames = LoadSpriteRepresentations(path, label);
        if (frames.Length > 0)
        {
            return frames[0];
        }

        throw new FileNotFoundException("Unity failed to load the " + label + " sprite.", path);
    }

    private static Sprite LoadFirstSpriteFrame(string path, string label)
    {
        Sprite[] frames = LoadSpriteRepresentations(path, label);
        if (frames.Length > 0)
        {
            return frames[0];
        }

        return LoadSingleSprite(path, label);
    }

    private static Sprite FirstFrameOrNull(Sprite[] frames)
    {
        return frames != null && frames.Length > 0 ? frames[0] : null;
    }

    private static Sprite[] LoadSpriteRepresentations(string path, string label)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Missing required " + label + " sprite.", path);
        }

        AssetDatabase.ImportAsset(path);
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        List<Sprite> sprites = new List<Sprite>();
        for (int i = 0; i < assets.Length; i++)
        {
            Sprite sprite = assets[i] as Sprite;
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }

        sprites.Sort(CompareSpriteFrameNames);
        return sprites.ToArray();
    }

    private static Sprite LoadCastleWallTile()
    {
        if (!File.Exists(CastleWallTilePath))
        {
            throw new FileNotFoundException("Missing required castle wall tile.", CastleWallTilePath);
        }

        ConfigureSpriteImporter(CastleWallTilePath, new Vector2(0.5f, 0.5f), CastleWallTilePpu, TextureWrapMode.Repeat, false);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CastleWallTilePath);
        if (sprite == null)
        {
            throw new FileNotFoundException("Unity failed to import the castle wall tile.", CastleWallTilePath);
        }

        return sprite;
    }

    private static Sprite LoadDeeperGalleryWallTile()
    {
        if (!File.Exists(DeeperGalleryWallTilePath))
        {
            throw new FileNotFoundException("Missing required deeper gallery wall tile.", DeeperGalleryWallTilePath);
        }

        ConfigureSpriteImporter(DeeperGalleryWallTilePath, new Vector2(0.5f, 0.5f), DeeperGalleryWallTilePpu, TextureWrapMode.Repeat, false);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DeeperGalleryWallTilePath);
        if (sprite == null)
        {
            throw new FileNotFoundException("Unity failed to import the deeper gallery wall tile.", DeeperGalleryWallTilePath);
        }

        return sprite;
    }

    private static Sprite LoadObliqueDoorwaySprite()
    {
        if (!File.Exists(ObliqueDoorwaySpritePath))
        {
            throw new FileNotFoundException("Missing required oblique doorway prototype sprite.", ObliqueDoorwaySpritePath);
        }

        ConfigureSpriteImporter(ObliqueDoorwaySpritePath, new Vector2(0.5f, 0.5f), ObliqueDoorwayPpu);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ObliqueDoorwaySpritePath);
        if (sprite == null)
        {
            throw new FileNotFoundException("Unity failed to import the oblique doorway prototype sprite.", ObliqueDoorwaySpritePath);
        }

        return sprite;
    }

    private static Transform CreateChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent);
        return child.transform;
    }

    private static GameObject CreateEmpty(string name, Vector3 position, Vector3 scale, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        return obj;
    }

    private static GameObject CreateRect(string name, Sprite sprite, Vector3 position, Vector2 size, Color color, int sortingOrder, Transform parent, Material material)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder = sortingOrder;
        if (material != null)
        {
            renderer.sharedMaterial = material;
        }

        return obj;
    }

    private static GameObject CreateLocalRect(string name, Sprite sprite, Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Color color, int sortingOrder, Material material)
    {
        GameObject obj = CreateLocalSprite(name, sprite, parent, localPosition, localScale, localRotation, color, sortingOrder, material);
        return obj;
    }

    private static GameObject CreateTiledSprite(string name, Sprite sprite, Vector3 position, Vector2 size, Color color, int sortingOrder, Transform parent, Material material)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one;

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = size;
        renderer.color = color;
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder = sortingOrder;
        if (material != null)
        {
            renderer.sharedMaterial = material;
        }

        return obj;
    }

    private static GameObject CreateLocalSprite(string name, Sprite sprite, Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Color color, int sortingOrder, Material material)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = localRotation;
        obj.transform.localScale = localScale;

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder = sortingOrder;
        if (material != null)
        {
            renderer.sharedMaterial = material;
        }

        return obj;
    }

    private static GameObject CreateSprite(string name, Sprite sprite, Vector3 position, Vector3 scale, Color color, int sortingOrder, Transform parent, Material material)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = scale;

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder = sortingOrder;
        if (material != null)
        {
            renderer.sharedMaterial = material;
        }

        return obj;
    }

    private static void CreateWarmGlow(Sprite pixel, Vector3 position, Transform parent, Material unlitMaterial)
    {
        CreateRect("Segment Sconce Warm Glow", pixel, position, new Vector2(0.48f, 0.92f), new Color(1f, 0.56f, 0.18f, 0.1f), 48, parent, unlitMaterial);
    }

    private static void TryPlaceWallSconce(Vector3 position, Transform parent, float scale)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WallSconcePrefabPath);
        if (prefab == null)
        {
            return;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            return;
        }

        instance.name = "Prototype Wall Sconce";
        instance.transform.SetParent(parent);
        instance.transform.position = position;
        instance.transform.localScale = Vector3.one * scale;
    }

    private static void ConfigureLayoutInspector(Transform root)
    {
        CastleGalleryPrototypeLayout layout = root.gameObject.AddComponent<CastleGalleryPrototypeLayout>();
        List<CastleGalleryPrototypeLayout.Placement> placements = new List<CastleGalleryPrototypeLayout.Placement>();

        AddPlacement(placements, root, "Door wall plate", "Open Arch Oblique Doorway Candidate", true, true);
        AddPlacement(placements, root, "Back wall tile field", "Hallway Castle Gray Block Wall Tile", true);
        AddPlacement(placements, root, "Deeper blue gold wall tile field", "Deeper Gallery Blue Gold Wall Tile", true);
        AddPlacement(placements, root, "Floor base", "Hallway Floor Base", true);
        AddPlacement(placements, root, "Floor back shadow", "Hallway Floor Back Shadow", true);
        AddPlacement(placements, root, "Front floor lip", "Hallway Front Floor Lip", true);
        AddPlacement(placements, root, "Near black floor groove", "Hallway Near Black Groove", true);

        AddPlacement(placements, root, "West windowwall pier", "Gallery West Windowwall Divider Stone Pier", true);
        AddPlacement(placements, root, "Middle windowwall pier", "Gallery Middle Windowwall Divider Stone Pier", true);
        AddPlacement(placements, root, "Renfield room arch doorwall", "Renfield Private Room Arch Doorwall", true, true);
        AddPlacement(placements, root, "Renfield room wall wash", "Renfield Private Room Wall Wash", true);
        AddPlacement(placements, root, "Renfield room night window", "Renfield Private Room Back Window Night", true);
        AddPlacement(placements, root, "Haunted mirror room arch doorwall", "Haunted Mirror Room Arch Doorwall", true, true);
        AddPlacement(placements, root, "Haunted mirror room wall wash", "Haunted Mirror Room Wall Wash", true);
        AddPlacement(placements, root, "Magic mirror", "Magic Mirror Gold Outer Frame", true);
        AddPlacement(placements, root, "Restored grim reaper statue", "GrimReaper", true);
        AddPlacement(placements, root, "Restored gallery window", "Window", true);
        AddPlacement(placements, root, "Restored architectural fountain", "Fountain", true);

        AddPlacement(placements, root, "East end dark wall face", "East End Dark Wall Face", true);
        AddPlacement(placements, root, "East end brown return", "East End Inner Brown Return", true);
        AddPlacement(placements, root, "East end black side shadow", "East End Black Side Shadow", true);
        AddPlacement(placements, root, "East terminal black mass", "East End Terminal Black Mass", true);
        AddPlacement(placements, root, "East end top return block", "East End Top Return Block", true);
        AddPlacement(placements, root, "East end lower return block", "East End Lower Return Block", true);
        AddPlacement(placements, root, "East end floor stop shadow", "East End Floor Stop Shadow", true);

        AddPlacement(placements, root, "Renfield position", "Renfield Placeholder", false);
        AddPlacement(placements, root, "Renfield ground shadow", "Renfield Ground Shadow", true);

        AddRepeatedPlacements(placements, root, "Wall sconce", "Prototype Wall Sconce", false);
        AddRepeatedPlacements(placements, root, "Back wall vertical block", "Back Wall Vertical Block", true);
        AddRepeatedPlacements(placements, root, "Floor plank break", "Floor Plank Break", true);

        layout.SetPlacements(placements.ToArray());
    }

    private static void AddPlacement(List<CastleGalleryPrototypeLayout.Placement> placements, Transform root, string label, string exactName, bool editScale, bool editRotation = false)
    {
        Transform target = FindDescendant(root, exactName);
        if (target != null)
        {
            placements.Add(new CastleGalleryPrototypeLayout.Placement(label, target, editScale, editRotation));
        }
    }

    private static void AddRepeatedPlacements(List<CastleGalleryPrototypeLayout.Placement> placements, Transform root, string labelPrefix, string namePrefix, bool editScale)
    {
        List<Transform> matches = new List<Transform>();
        CollectDescendants(root, namePrefix, matches);

        for (int i = 0; i < matches.Count; i++)
        {
            placements.Add(new CastleGalleryPrototypeLayout.Placement(labelPrefix + " " + i, matches[i], editScale));
        }
    }

    private static Transform FindDescendant(Transform parent, string exactName)
    {
        if (parent.name == exactName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindDescendant(parent.GetChild(i), exactName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void CollectDescendants(Transform parent, string namePrefix, List<Transform> matches)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.StartsWith(namePrefix, StringComparison.Ordinal))
            {
                matches.Add(child);
            }

            CollectDescendants(child, namePrefix, matches);
        }
    }

    private static void ConfigureSpriteImporter(string path, Vector2 pivot, float pixelsPerUnit, TextureWrapMode wrapMode = TextureWrapMode.Clamp, bool alphaIsTransparency = true)
    {
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new FileNotFoundException("Missing required sprite asset.", path);
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = wrapMode;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.spritePivot = pivot;
        importer.alphaIsTransparency = alphaIsTransparency;

        SerializedObject serializedImporter = new SerializedObject(importer);
        SerializedProperty alignmentProperty = serializedImporter.FindProperty("m_Alignment");
        if (alignmentProperty != null)
        {
            alignmentProperty.intValue = pivot == new Vector2(0.5f, 0.5f)
                ? SpriteAlignmentCenter
                : SpriteAlignmentCustom;
        }

        serializedImporter.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
    }

    private static Type FindLight2DType()
    {
        Type lightType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.2D.Runtime");
        if (lightType != null)
        {
            return lightType;
        }

        lightType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        if (lightType != null)
        {
            return lightType;
        }

        return Type.GetType("UnityEngine.Rendering.Light2D, Unity.RenderPipelines.Universal.Runtime");
    }

    private static void SetFloatProperty(Component component, Type type, string propertyName, float value)
    {
        System.Reflection.PropertyInfo property = type.GetProperty(propertyName);
        if (property != null)
        {
            property.SetValue(component, value, null);
        }
    }

    private static void SetColorProperty(Component component, Type type, string propertyName, Color value)
    {
        System.Reflection.PropertyInfo property = type.GetProperty(propertyName);
        if (property != null)
        {
            property.SetValue(component, value, null);
        }
    }

    private static void SetEnumProperty(Component component, Type type, string propertyName, string valueName)
    {
        System.Reflection.PropertyInfo property = type.GetProperty(propertyName);
        if (property != null && property.PropertyType.IsEnum)
        {
            property.SetValue(component, Enum.Parse(property.PropertyType, valueName), null);
        }
    }
}
