using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class CryptPrototypeBuilder
{
    private const string ArtFolder = "Assets/Art/Crypt";
    private const string ScenePath = "Assets/Scenes/CryptPrototype.unity";
    private const string Bg2BackgroundPath = ArtFolder + "/bg2_downscaled.png";
    private const string Bg2CoffinOccluderPath = ArtFolder + "/bg2_coffin_occluder.png";
    private const string PaintedBackgroundPath = ArtFolder + "/crypt_room_painted_background.png";
    private const string PaintedForegroundOccludersPath = ArtFolder + "/crypt_room_foreground_occluders.png";
    private const string PaintedCoffinOccludersPath = ArtFolder + "/crypt_room_coffin_occluders.png";
    private const string RoomLayerBackWallsPath = ArtFolder + "/crypt_room_layer_back_walls.png";
    private const string RoomLayerFloorDressingPath = ArtFolder + "/crypt_room_layer_floor_dressing.png";
    private const string RoomLayerCoffinPlatformPath = ArtFolder + "/crypt_room_layer_coffin_platform.png";
    private const string RoomLayerForegroundRailPath = ArtFolder + "/crypt_room_layer_foreground_rail.png";
    private const string PaintedCandleGlowPath = ArtFolder + "/crypt_room_candle_glow_overlay.png";
    private const string PaintedCandleHeatPath = ArtFolder + "/crypt_room_candle_heat_overlay.png";
    private const string PaintedShadowGradePath = ArtFolder + "/crypt_room_shadow_grade_overlay.png";
    private const string PaintedSigilPulsePath = ArtFolder + "/crypt_room_sigil_pulse_overlay.png";
    private const string PaintedCoffinPropPath = ArtFolder + "/coffin.png";
    private const float PaintedCoffinPropScale = 0.225f;
    private const string PromptPanelPath = ArtFolder + "/ui_gothic_prompt_panel.png";
    private const string HudShellPath = ArtFolder + "/ui_gothic_hud_shell.png";
    private const int Ppu = 32;
    private const float PaintedBackgroundPpu = 114.06f;
    private const float CharacterPpu = 64f;
    private const float DraculaV4DownPpu = 175.625f;

    private static readonly Color32 ClearColor = new Color32(0, 0, 0, 0);
    private static readonly Color32 Ink = new Color32(5, 6, 9, 255);
    private static readonly Color32 Floor = new Color32(72, 76, 87, 255);
    private static readonly Color32 FloorDark = new Color32(39, 43, 54, 255);
    private static readonly Color32 FloorLight = new Color32(105, 110, 121, 255);
    private static readonly Color32 Wall = new Color32(47, 48, 58, 255);
    private static readonly Color32 WallDark = new Color32(25, 26, 34, 255);
    private static readonly Color32 WallLight = new Color32(83, 84, 94, 255);
    private static readonly Color32 DoorBlack = new Color32(2, 3, 7, 255);
    private static readonly Color32 DoorGlow = new Color32(72, 94, 132, 255);
    private static readonly Color32 Wood = new Color32(82, 37, 32, 255);
    private static readonly Color32 WoodDark = new Color32(42, 17, 19, 255);
    private static readonly Color32 Red = new Color32(138, 18, 31, 255);
    private static readonly Color32 RedDark = new Color32(71, 9, 21, 255);
    private static readonly Color32 Pale = new Color32(211, 207, 194, 255);
    private static readonly Color32 White = new Color32(231, 226, 211, 255);
    private static readonly Color32 Gold = new Color32(188, 143, 62, 255);

    private delegate void TexturePainter(Texture2D texture);

    private static bool regenerateSprites;

    [MenuItem("Dracula/Build Crypt Prototype")]
    public static void BuildCryptPrototype()
    {
        BuildCryptPrototype(regenerateExistingSprites: false);
    }

    [MenuItem("Dracula/Regenerate Placeholder Sprites And Build Crypt Prototype")]
    public static void RegeneratePlaceholderSpritesAndBuildCryptPrototype()
    {
        BuildCryptPrototype(regenerateExistingSprites: true);
    }

    private static void BuildCryptPrototype(bool regenerateExistingSprites)
    {
        EnsureFolders();
        regenerateSprites = regenerateExistingSprites;

        try
        {
            BuildCryptPrototypeScene();
        }
        finally
        {
            regenerateSprites = false;
        }
    }

    private static void BuildCryptPrototypeScene()
    {
        Sprite floor = CreateSprite("crypt_floor_tile", 64, 32, new Vector2(0.5f, 0.5f), DrawFloorTile);
        Sprite crackedFloor = CreateSprite("crypt_floor_cracked_tile", 64, 32, new Vector2(0.5f, 0.5f), DrawCrackedFloorTile);
        Sprite darkFloor = CreateSprite("crypt_floor_dark_tile", 64, 32, new Vector2(0.5f, 0.5f), DrawDarkFloorTile);
        Sprite exitTile = CreateSprite("crypt_exit_threshold", 64, 32, new Vector2(0.5f, 0.5f), DrawExitThreshold);
        Sprite floorShadow = CreateSprite("crypt_floor_edge_shadow", 512, 276, new Vector2(0.5f, 0.5f), DrawFloorEdgeShadow);
        Sprite vignette = CreateSprite("crypt_vignette_overlay", 512, 288, new Vector2(0.5f, 0.5f), DrawVignetteOverlay);
        Sprite exitGlow = CreateSprite("crypt_exit_blue_glow", 128, 96, new Vector2(0.5f, 0.2f), DrawExitGlow);
        Sprite exitSteps = CreateSprite("crypt_exit_steps", 128, 64, new Vector2(0.5f, 0.34f), DrawExitSteps);
        Sprite candlePool = CreateSprite("crypt_candle_light_pool", 96, 48, new Vector2(0.5f, 0.5f), DrawCandleLightPool);
        Sprite floorScuffs = CreateSprite("crypt_floor_scuffs", 512, 276, new Vector2(0.5f, 0.5f), DrawFloorScuffs);
        Sprite floorTombInsets = CreateSprite("crypt_floor_tomb_insets", 512, 276, new Vector2(0.5f, 0.5f), DrawFloorTombInsets);
        Sprite wallFloorBase = CreateSprite("crypt_wall_floor_base", 512, 276, new Vector2(0.5f, 0.5f), DrawWallFloorBase);
        Sprite foregroundLedge = CreateSprite("crypt_foreground_ledge", 512, 112, new Vector2(0.5f, 0f), DrawForegroundLedge);
        Sprite wall = CreateSprite("crypt_back_wall_exit", 384, 128, new Vector2(0.5f, 0f), DrawBackWall);
        Sprite leftWall = CreateSprite("crypt_left_side_wall", 128, 136, new Vector2(1f, 0f), DrawLeftSideWall);
        Sprite rightWall = CreateSprite("crypt_right_side_wall", 128, 136, new Vector2(0f, 0f), DrawRightSideWall);
        Sprite pillar = CreateSprite("crypt_pillar", 28, 92, new Vector2(0.5f, 0f), DrawPillar);
        Sprite wallRelief = CreateSprite("crypt_wall_relief", 58, 84, new Vector2(0.5f, 0f), DrawWallRelief);
        Sprite coffinGlow = CreateSprite("coffin_glow", 160, 88, new Vector2(0.5f, 0.33f), DrawCoffinGlow);
        Sprite coffin = CreateSprite("coffin_iso", 120, 76, new Vector2(0.5f, 0.31f), DrawCoffin);
        Sprite sealEffect0 = CreateSprite("coffin_seal_effect_0", 160, 88, new Vector2(0.5f, 0.5f), DrawCoffinGlow);
        Sprite sealEffect1 = CreateSprite("coffin_seal_effect_1", 160, 88, new Vector2(0.5f, 0.5f), DrawCoffinGlow);
        Sprite sealEffect2 = CreateSprite("coffin_seal_effect_2", 160, 88, new Vector2(0.5f, 0.5f), DrawCoffinGlow);
        Sprite sealEffect3 = CreateSprite("coffin_seal_effect_3", 160, 88, new Vector2(0.5f, 0.5f), DrawCoffinGlow);
        Sprite urn = CreateSprite("crypt_funeral_urn", 38, 50, new Vector2(0.5f, 0f), DrawUrn);
        Sprite brokenUrn = CreateSprite("crypt_broken_urn", 54, 34, new Vector2(0.5f, 0.25f), DrawBrokenUrn);
        Sprite candles = CreateSprite("crypt_candelabra", 44, 44, new Vector2(0.5f, 0f), DrawCandles);
        Sprite bones = CreateSprite("crypt_bone_pile", 60, 32, new Vector2(0.5f, 0.25f), DrawBones);
        Sprite chain = CreateSprite("crypt_chain", 24, 72, new Vector2(0.5f, 1f), DrawChain);
        Sprite rug = CreateSprite("crypt_rug_runner", 96, 48, new Vector2(0.5f, 0.5f), DrawRug);
        Sprite rubble = CreateSprite("crypt_carved_rubble", 66, 34, new Vector2(0.5f, 0.35f), DrawRubble);
        Sprite shadow = CreateSprite("dracula_shadow", 88, 28, new Vector2(0.5f, 0.5f), DrawShadow, CharacterPpu);
        Sprite down0 = LoadSprite(ArtFolder + "/draculav4_down_0.png", new Vector2(0.5f, 0f), DraculaV4DownPpu);
        Sprite down1 = LoadSprite(ArtFolder + "/draculav4_down_1.png", new Vector2(0.5f, 0f), DraculaV4DownPpu);
        Sprite down2 = LoadSprite(ArtFolder + "/draculav4_down_2.png", new Vector2(0.5f, 0f), DraculaV4DownPpu);
        Sprite down3 = LoadSprite(ArtFolder + "/draculav4_down_3.png", new Vector2(0.5f, 0f), DraculaV4DownPpu);
        Sprite down4 = LoadSprite(ArtFolder + "/draculav4_down_4.png", new Vector2(0.5f, 0f), DraculaV4DownPpu);
        Sprite down5 = LoadSprite(ArtFolder + "/draculav4_down_5.png", new Vector2(0.5f, 0f), DraculaV4DownPpu);
        Sprite up0 = LoadSprite(ArtFolder + "/draculav3_up_0.png", new Vector2(0.5f, 0f), CharacterPpu);
        Sprite up1 = LoadSprite(ArtFolder + "/draculav3_up_1.png", new Vector2(0.5f, 0f), CharacterPpu);
        Sprite up2 = LoadSprite(ArtFolder + "/draculav3_up_2.png", new Vector2(0.5f, 0f), CharacterPpu);
        Sprite up3 = LoadSprite(ArtFolder + "/draculav3_up_3.png", new Vector2(0.5f, 0f), CharacterPpu);
        Sprite up4 = LoadSprite(ArtFolder + "/draculav3_up_4.png", new Vector2(0.5f, 0f), CharacterPpu);
        Sprite up5 = LoadSprite(ArtFolder + "/draculav3_up_5.png", new Vector2(0.5f, 0f), CharacterPpu);
        Sprite side0 = CreateSprite("dracula_side_0", 80, 128, new Vector2(0.5f, 0f), delegate(Texture2D t) { DrawDracula(t, 2, 0); }, CharacterPpu);
        Sprite side1 = CreateSprite("dracula_side_1", 80, 128, new Vector2(0.5f, 0f), delegate(Texture2D t) { DrawDracula(t, 2, 1); }, CharacterPpu);
        Sprite side2 = CreateSprite("dracula_side_2", 80, 128, new Vector2(0.5f, 0f), delegate(Texture2D t) { DrawDracula(t, 2, 2); }, CharacterPpu);
        Sprite side3 = CreateSprite("dracula_side_3", 80, 128, new Vector2(0.5f, 0f), delegate(Texture2D t) { DrawDracula(t, 2, 3); }, CharacterPpu);
        Sprite side4 = CreateSprite("dracula_side_4", 80, 128, new Vector2(0.5f, 0f), delegate(Texture2D t) { DrawDracula(t, 2, 4); }, CharacterPpu);
        Sprite side5 = CreateSprite("dracula_side_5", 80, 128, new Vector2(0.5f, 0f), delegate(Texture2D t) { DrawDracula(t, 2, 5); }, CharacterPpu);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "CryptPrototype";

        GameObject root = new GameObject("Crypt Prototype");
        GameObject room = new GameObject("Isometric Crypt Room");
        room.transform.SetParent(root.transform);

        ConfigureLightingEnvironment();
        CreateCamera();
        CreateLight();

        Vector3 coffinPosition;
        Vector3 exitPosition;
        SpriteRenderer interactionGlowRenderer = null;
        SpriteRenderer interactionEffectRenderer = null;
        SpriteRenderer candleGlowRenderer = null;
        SpriteRenderer candleHeatRenderer = null;
        SpriteRenderer shadowGradeRenderer = null;
        SpriteRenderer sigilPulseRenderer = null;
        if (HasPaintedRoom())
        {
            CreatePaintedRoom(room.transform, out candleGlowRenderer, out candleHeatRenderer, out shadowGradeRenderer, out sigilPulseRenderer);
            bool useBg2Room = UsesBg2Room();
            coffinPosition = useBg2Room ? new Vector3(1.62f, -0.72f, 0f) : new Vector3(2.58f, 0.74f, 0f);
            exitPosition = useBg2Room ? new Vector3(-1.94f, 1.48f, 0f) : new Vector3(-3.58f, 1.36f, 0f);
            if (!useBg2Room)
            {
                Sprite coffinPropSprite = File.Exists(PaintedCoffinPropPath)
                    ? LoadSprite(PaintedCoffinPropPath, new Vector2(0.5f, 0.36f), PaintedBackgroundPpu)
                    : coffin;
                GameObject coffinProp = CreateSpriteObject("Coffin Prop", coffinPropSprite, coffinPosition, 270, room.transform);
                coffinProp.transform.localScale = Vector3.one * (File.Exists(PaintedCoffinPropPath) ? PaintedCoffinPropScale : 0.72f);
            }

            GameObject interactionGlow = CreateSpriteObject("Coffin Interaction Glow", coffinGlow, coffinPosition + new Vector3(0f, -0.04f, 0f), 96, room.transform);
            interactionGlow.transform.localScale = Vector3.one * 0.72f;
            interactionGlowRenderer = interactionGlow.GetComponent<SpriteRenderer>();
            interactionGlowRenderer.color = new Color(1f, 1f, 1f, 0f);
            GameObject interactionEffect = CreateSpriteObject("Coffin Seal Effect", sealEffect0, coffinPosition + new Vector3(0f, -0.02f, 0f), 132, room.transform);
            interactionEffect.transform.localScale = Vector3.one * 0.72f;
            interactionEffectRenderer = interactionEffect.GetComponent<SpriteRenderer>();
            interactionEffectRenderer.color = new Color(1f, 1f, 1f, 0f);
            interactionEffectRenderer.enabled = false;
        }
        else
        {
            GameObject backWall = CreateSpriteObject("Back Wall With Exit", wall, new Vector3(1.18f, 3.05f, 0f), -80, room.transform);
            backWall.transform.localScale = Vector3.one;

            CreateSpriteObject("Left Side Wall Rear", leftWall, new Vector3(-1.98f, 1.72f, 0f), -70, room.transform);
            CreateSpriteObject("Left Side Wall Front", leftWall, new Vector3(-4.22f, 0.36f, 0f), -69, room.transform);
            CreateSpriteObject("Right Side Wall Rear", rightWall, new Vector3(4.36f, 1.72f, 0f), -70, room.transform);
            CreateSpriteObject("Right Side Wall Front", rightWall, new Vector3(6.42f, 0.36f, 0f), -69, room.transform);

            CreateSpriteObject("Left Pillar", pillar, new Vector3(-4.12f, 2.64f, 0f), -40, room.transform);
            CreateSpriteObject("Center Left Pillar", pillar, new Vector3(-1.12f, 2.64f, 0f), -39, room.transform);
            CreateSpriteObject("Exit Pillar Left", pillar, new Vector3(3.55f, 2.64f, 0f), -35, room.transform);
            CreateSpriteObject("Exit Pillar Right", pillar, new Vector3(4.95f, 2.64f, 0f), -35, room.transform);
            CreateSpriteObject("Right Pillar", pillar, new Vector3(6.45f, 2.64f, 0f), -40, room.transform);
            CreateWallReliefs(room.transform, wallRelief);

            for (int y = 0; y < 7; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Vector3 position = IsoPosition(x, y);
                    Sprite tileSprite = PickFloorSprite(x, y, floor, crackedFloor, darkFloor, exitTile);
                    GameObject tile = CreateSpriteObject("Floor Tile " + x + "," + y, tileSprite, position, -20 + x + y, room.transform);
                    tile.transform.localScale = Vector3.one;
                }
            }

            CreateSpriteObject("Floor Edge Shadow", floorShadow, new Vector3(1.16f, 0.56f, 0f), 20, room.transform);
            CreateSpriteObject("Floor Scuffs And Dust", floorScuffs, new Vector3(1.16f, 0.56f, 0f), 58, room.transform);
            CreateSpriteObject("Floor Tomb Inlays", floorTombInsets, new Vector3(1.16f, 0.56f, 0f), 60, room.transform);
            CreateSpriteObject("Wall Floor Base Alignment", wallFloorBase, new Vector3(1.16f, 0.56f, 0f), 64, room.transform);
            GameObject exitStepObject = CreateSpriteObject("Exit Raised Steps", exitSteps, new Vector3(4.22f, 2.38f, 0f), 65, room.transform);
            exitStepObject.transform.localScale = Vector3.one * 0.62f;
            CreateSpriteObject("Rug Runner", rug, new Vector3(0.75f, -0.68f, 0f), 72, room.transform);
            coffinPosition = IsoPosition(3, 2);
            exitPosition = new Vector3(4.24f, 2.78f, 0f);
            CreateSpriteObject("Coffin Red Spill", coffinGlow, coffinPosition + new Vector3(0f, -0.04f, 0f), 95, room.transform);
            CreateSpriteObject("Coffin", coffin, coffinPosition + new Vector3(0f, -0.03f, 0f), 130, room.transform);
            CreateCoffinDressing(room.transform, candles, coffinPosition);

            CreateWallDressing(room.transform, torch: candles, chain: chain);
            CreateFloorDressing(room.transform, urn, brokenUrn, candles, bones, rubble);
            CreateLightingOverlays(room.transform, vignette, exitGlow, candlePool);
            CreateSpriteObject("Foreground Broken Masonry", foregroundLedge, new Vector3(1.15f, -2.94f, 0f), 254, room.transform);
        }

        CreateWalkBoundaryColliders(room.transform);

        GameObject exit = new GameObject("Main Arch Exit Trigger");
        exit.transform.SetParent(room.transform);
        exit.transform.position = exitPosition;
        BoxCollider2D exitCollider = exit.AddComponent<BoxCollider2D>();
        exitCollider.isTrigger = true;
        exitCollider.size = new Vector2(0.78f, 0.78f);

        Vector3 playerPosition = UsesBg2Room() ? new Vector3(0.42f, -1.74f, 0f) : new Vector3(0.25f, -1.35f, 0f);
        GameObject player = CreateSpriteObject("Dracula", down0, playerPosition, 318, root.transform);
        Rigidbody2D playerBody = player.AddComponent<Rigidbody2D>();
        playerBody.bodyType = RigidbodyType2D.Dynamic;
        playerBody.gravityScale = 0f;
        playerBody.freezeRotation = true;
        playerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        playerBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        CapsuleCollider2D playerCollider = player.AddComponent<CapsuleCollider2D>();
        playerCollider.direction = CapsuleDirection2D.Horizontal;
        playerCollider.size = new Vector2(0.42f, 0.28f);
        playerCollider.offset = new Vector2(0f, 0.16f);
        DraculaWalker walker = player.AddComponent<DraculaWalker>();
        walker.spriteRenderer = player.GetComponent<SpriteRenderer>();
        walker.body = playerBody;
        walker.walkDown = new Sprite[] { down0, down1, down2, down3, down4, down5 };
        walker.walkUp = new Sprite[] { up0, up1, up2, up3, up4, up5 };
        walker.walkSide = new Sprite[] { side0, side1, side2, side3, side4, side5 };
        walker.frameTime = 0.115f;
        walker.baseSortingOrder = 280;
        walker.ySortMultiplier = 28f;
        walker.minSortingOrder = 180;
        walker.maxSortingOrder = 340;
        walker.minBounds = UsesBg2Room() ? new Vector2(-5.35f, -3.28f) : new Vector2(-4.78f, -1.66f);
        walker.maxBounds = UsesBg2Room() ? new Vector2(7.12f, 2.10f) : new Vector2(6.48f, 2.74f);

        CryptPrototypeInteraction interaction = player.AddComponent<CryptPrototypeInteraction>();
        interaction.coffinPosition = coffinPosition;
        interaction.coffinGlowRenderer = interactionGlowRenderer;
        interaction.coffinEffectRenderer = interactionEffectRenderer;
        interaction.coffinEffectFrames = new Sprite[] { sealEffect0, sealEffect1, sealEffect2, sealEffect3 };
        interaction.promptPanelTexture = File.Exists(PromptPanelPath) ? LoadTexture(PromptPanelPath) : null;
        interaction.promptBottomOffset = File.Exists(HudShellPath) ? 280f : 94f;

        GameObject playerShadow = CreateSpriteObject("Shadow", shadow, Vector3.zero, 198, player.transform);
        playerShadow.transform.localPosition = new Vector3(0f, 0.08f, 0f);

        CryptAmbientAnimator ambientAnimator = root.AddComponent<CryptAmbientAnimator>();
        ambientAnimator.flickerRenderers = new SpriteRenderer[0];
        ambientAnimator.sigilRenderer = sigilPulseRenderer;
        CreateCandleLightingRig(root.transform, new SpriteRenderer[] { candleGlowRenderer, candleHeatRenderer }, shadowGradeRenderer);

        if (!UsesBg2Room() && File.Exists(HudShellPath))
        {
            GameObject hud = new GameObject("Reference Style HUD");
            hud.transform.SetParent(root.transform);
            CryptHudOverlay hudOverlay = hud.AddComponent<CryptHudOverlay>();
            hudOverlay.frameTexture = LoadTexture(HudShellPath);
            hudOverlay.visible = true;
            hudOverlay.visibleInEditMode = true;
            hudOverlay.editModeOpacity = 1f;
        }

        Selection.activeGameObject = player;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Built Dracula crypt prototype scene at " + ScenePath);
    }

    private static void EnsureFolders()
    {
        if (!Directory.Exists("Assets/Scenes"))
        {
            Directory.CreateDirectory("Assets/Scenes");
        }

        if (!Directory.Exists(ArtFolder))
        {
            Directory.CreateDirectory(ArtFolder);
        }
    }

    private static GameObject CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = UsesBg2Room() ? 4.4f : 3.82f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.035f, 0.038f, 0.048f, 1f);
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = UsesBg2Room() ? new Vector3(1.15f, 0.75f, -10f) : new Vector3(0.78f, 0.28f, -10f);
        cameraObject.AddComponent<AudioListener>();
        return cameraObject;
    }

    private static GameObject CreateLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.24f;
        light.color = new Color(0.52f, 0.68f, 0.92f, 1f);
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.32f;
        lightObject.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
        return lightObject;
    }

    private static void ConfigureLightingEnvironment()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.08f, 0.105f, 0.15f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.035f, 0.045f, 0.058f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.012f, 0.011f, 0.014f, 1f);
        RenderSettings.ambientIntensity = 0.68f;
        RenderSettings.reflectionIntensity = 0.08f;
    }

    private static GameObject CreateSpriteObject(string name, Sprite sprite, Vector3 position, int sortingOrder, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        return obj;
    }

    private static void CreatePaintedRoom(Transform room, out SpriteRenderer candleGlowRenderer, out SpriteRenderer candleHeatRenderer, out SpriteRenderer shadowGradeRenderer, out SpriteRenderer sigilPulseRenderer)
    {
        candleGlowRenderer = null;
        candleHeatRenderer = null;
        shadowGradeRenderer = null;
        sigilPulseRenderer = null;
        if (UsesBg2Room())
        {
            CreatePaintedLayer("BG2 Room Base Layer", Bg2BackgroundPath, -220, room);
            if (File.Exists(Bg2CoffinOccluderPath))
            {
                CreatePaintedLayer("BG2 Coffin Occluder Layer", Bg2CoffinOccluderPath, 286, room);
            }
            return;
        }

        bool useLayeredRoom = HasLayeredRoom();
        if (useLayeredRoom)
        {
            CreatePaintedLayer("Painted Base Room Layer", PaintedBackgroundPath, -220, room);
        }
        else
        {
            CreatePaintedLayer("Painted Crypt Room Background", PaintedBackgroundPath, -200, room);
        }

        if (File.Exists(PaintedShadowGradePath))
        {
            Sprite shadowGrade = LoadSprite(PaintedShadowGradePath, new Vector2(0.5f, 0.5f), PaintedBackgroundPpu);
            GameObject shadowGradeObject = CreateSpriteObject("Painted Shadow Grade Overlay", shadowGrade, new Vector3(1.15f, 0.75f, 0f), -120, room);
            shadowGradeRenderer = shadowGradeObject.GetComponent<SpriteRenderer>();
            shadowGradeRenderer.color = new Color(1f, 1f, 1f, 0.88f);
        }

        if (File.Exists(PaintedCandleHeatPath))
        {
            Sprite candleHeat = LoadSprite(PaintedCandleHeatPath, new Vector2(0.5f, 0.5f), PaintedBackgroundPpu);
            GameObject candleHeatObject = CreateSpriteObject("Painted Candle Heat Overlay", candleHeat, new Vector3(1.15f, 0.75f, 0f), 156, room);
            candleHeatRenderer = candleHeatObject.GetComponent<SpriteRenderer>();
            candleHeatRenderer.color = new Color(1f, 1f, 1f, 0.52f);
        }

        if (File.Exists(PaintedCandleGlowPath))
        {
            Sprite candleGlow = LoadSprite(PaintedCandleGlowPath, new Vector2(0.5f, 0.5f), PaintedBackgroundPpu);
            GameObject candleGlowObject = CreateSpriteObject("Painted Candle Flicker Overlay", candleGlow, new Vector3(1.15f, 0.75f, 0f), 160, room);
            candleGlowRenderer = candleGlowObject.GetComponent<SpriteRenderer>();
            candleGlowRenderer.color = new Color(1f, 1f, 1f, 0.62f);
        }

        if (File.Exists(PaintedSigilPulsePath))
        {
            Sprite sigilPulse = LoadSprite(PaintedSigilPulsePath, new Vector2(0.5f, 0.5f), PaintedBackgroundPpu);
            GameObject sigilPulseObject = CreateSpriteObject("Painted Sigil Pulse Overlay", sigilPulse, new Vector3(1.15f, 0.75f, 0f), 164, room);
            sigilPulseRenderer = sigilPulseObject.GetComponent<SpriteRenderer>();
            sigilPulseRenderer.color = new Color(1f, 1f, 1f, 0.68f);
        }

        if (!useLayeredRoom && File.Exists(PaintedCoffinOccludersPath))
        {
            CreatePaintedLayer("Painted Coffin Depth Occluder", PaintedCoffinOccludersPath, 198, room);
        }

        string foregroundPath = useLayeredRoom ? RoomLayerForegroundRailPath : PaintedForegroundOccludersPath;
        if (File.Exists(foregroundPath))
        {
            string foregroundName = useLayeredRoom ? "Painted Foreground Rail Layer" : "Painted Foreground Rail Occluders";
            CreatePaintedLayer(foregroundName, foregroundPath, 282, room);
        }
    }

    private static bool HasPaintedRoom()
    {
        return UsesBg2Room() || File.Exists(PaintedBackgroundPath);
    }

    private static bool UsesBg2Room()
    {
        return File.Exists(Bg2BackgroundPath);
    }

    private static bool HasLayeredRoom()
    {
        return File.Exists(RoomLayerBackWallsPath)
            && File.Exists(RoomLayerFloorDressingPath)
            && File.Exists(RoomLayerCoffinPlatformPath)
            && File.Exists(RoomLayerForegroundRailPath);
    }

    private static GameObject CreatePaintedLayer(string name, string path, int sortingOrder, Transform parent)
    {
        Sprite sprite = LoadSprite(path, new Vector2(0.5f, 0.5f), PaintedBackgroundPpu);
        GameObject obj = CreateSpriteObject(name, sprite, new Vector3(1.15f, 0.75f, 0f), sortingOrder, parent);
        obj.transform.localScale = Vector3.one;
        return obj;
    }

    private static void CreateWalkBoundaryColliders(Transform room)
    {
        GameObject boundary = new GameObject("Walk Boundary Colliders");
        boundary.transform.SetParent(room);

        if (UsesBg2Room())
        {
            CreateBg2WalkBoundaryColliders(boundary.transform);
            return;
        }

        CreateEdgeBoundary(boundary.transform, "Sealed Back Wall Walk Limit", new Vector2[]
        {
            new Vector2(-5.00f, -0.58f),
            new Vector2(-4.26f, 0.20f),
            new Vector2(-1.08f, 1.98f),
            new Vector2(1.10f, 2.46f),
            new Vector2(3.38f, 2.02f),
            new Vector2(6.18f, 0.62f)
        });

        CreateEdgeBoundary(boundary.transform, "Sealed Right Wall Walk Limit", new Vector2[]
        {
            new Vector2(6.22f, 0.58f),
            new Vector2(6.48f, 0.05f),
            new Vector2(6.46f, -0.68f),
            new Vector2(6.12f, -1.08f),
            new Vector2(5.42f, -1.32f)
        });

        CreateEdgeBoundary(boundary.transform, "Sealed Lower Rail Walk Limit", new Vector2[]
        {
            new Vector2(-5.08f, -0.62f),
            new Vector2(-4.72f, -1.18f),
            new Vector2(-3.28f, -1.42f),
            new Vector2(-1.18f, -1.52f),
            new Vector2(0.62f, -1.64f),
            new Vector2(2.34f, -1.72f),
            new Vector2(4.42f, -1.44f),
            new Vector2(5.48f, -1.28f)
        });

        CreateEdgeBoundary(boundary.transform, "Coffin Platform Back Edge", new Vector2[]
        {
            new Vector2(1.10f, 0.18f),
            new Vector2(2.58f, 1.03f),
            new Vector2(4.24f, 0.24f)
        });

        CreateEdgeBoundary(boundary.transform, "Coffin Platform Right Edge", new Vector2[]
        {
            new Vector2(4.24f, 0.24f),
            new Vector2(4.44f, -0.16f),
            new Vector2(3.06f, -0.80f)
        });

        CreateEdgeBoundary(boundary.transform, "Coffin Platform Left Edge", new Vector2[]
        {
            new Vector2(1.10f, 0.18f),
            new Vector2(0.98f, -0.18f),
            new Vector2(1.30f, -0.38f)
        });

        CreateEdgeBoundary(boundary.transform, "Coffin Platform Front Lip Left Of Stairs", new Vector2[]
        {
            new Vector2(1.30f, -0.38f),
            new Vector2(1.52f, -0.46f)
        });

        CreateEdgeBoundary(boundary.transform, "Coffin Platform Front Lip Right Of Stairs", new Vector2[]
        {
            new Vector2(2.36f, -0.74f),
            new Vector2(3.06f, -0.80f)
        });

        CreatePolygonBoundary(boundary.transform, "Coffin Prop Collider", new Vector2[]
        {
            new Vector2(1.59f, 0.58f),
            new Vector2(1.93f, 1.03f),
            new Vector2(3.23f, 1.05f),
            new Vector2(3.57f, 0.69f),
            new Vector2(3.36f, 0.33f),
            new Vector2(1.86f, 0.29f)
        });
    }

    private static void CreateBg2WalkBoundaryColliders(Transform parent)
    {
        CreateEdgeBoundary(parent, "BG2 Room Outer Walk Perimeter", new Vector2[]
        {
            new Vector2(-5.43f, 0.22f),
            new Vector2(-5.43f, -1.30f),
            new Vector2(-4.14f, -1.72f),
            new Vector2(-2.52f, -2.30f),
            new Vector2(-0.60f, -3.18f),
            new Vector2(1.12f, -3.82f),
            new Vector2(2.84f, -3.18f),
            new Vector2(4.40f, -2.54f),
            new Vector2(6.14f, -1.84f),
            new Vector2(7.10f, -1.18f),
            new Vector2(7.45f, 0.22f),
            new Vector2(6.26f, 0.30f),
            new Vector2(5.08f, 0.18f),
            new Vector2(4.12f, 0.18f),
            new Vector2(3.18f, 0.56f),
            new Vector2(2.18f, 1.08f),
            new Vector2(1.05f, 1.48f),
            new Vector2(0.05f, 1.58f),
            new Vector2(-1.04f, 1.44f),
            new Vector2(-2.18f, 0.98f),
            new Vector2(-3.18f, 0.36f),
            new Vector2(-4.40f, 0.10f),
            new Vector2(-5.43f, 0.22f)
        });

        CreateEdgeBoundary(parent, "BG2 Raised Platform Back Edge", new Vector2[]
        {
            new Vector2(-1.84f, -0.72f),
            new Vector2(-0.25f, -0.05f),
            new Vector2(1.92f, 0.18f),
            new Vector2(4.08f, -0.54f)
        });

        CreateEdgeBoundary(parent, "BG2 Raised Platform Right Edge", new Vector2[]
        {
            new Vector2(4.08f, -0.54f),
            new Vector2(3.94f, -1.38f),
            new Vector2(3.10f, -1.88f)
        });

        CreateEdgeBoundary(parent, "BG2 Raised Platform Left Edge", new Vector2[]
        {
            new Vector2(-1.84f, -0.72f),
            new Vector2(-1.76f, -1.06f),
            new Vector2(-1.24f, -1.24f)
        });

        CreateEdgeBoundary(parent, "BG2 Raised Platform Front Lip Left Of Stairs", new Vector2[]
        {
            new Vector2(-1.24f, -1.24f),
            new Vector2(-0.76f, -1.44f)
        });

        CreateEdgeBoundary(parent, "BG2 Raised Platform Front Lip Right Of Stairs", new Vector2[]
        {
            new Vector2(1.30f, -2.34f),
            new Vector2(3.10f, -1.88f)
        });

        CreatePolygonBoundary(parent, "BG2 Painted Coffin Collider", new Vector2[]
        {
            new Vector2(0.18f, 0.04f),
            new Vector2(1.14f, 0.46f),
            new Vector2(2.88f, 0.02f),
            new Vector2(3.18f, -0.28f),
            new Vector2(2.02f, -0.82f),
            new Vector2(0.26f, -0.34f)
        });
    }

    private static EdgeCollider2D CreateEdgeBoundary(Transform parent, string name, Vector2[] points)
    {
        GameObject colliderObject = new GameObject(name);
        colliderObject.transform.SetParent(parent);
        EdgeCollider2D collider = colliderObject.AddComponent<EdgeCollider2D>();
        collider.edgeRadius = 0.035f;
        collider.points = points;
        return collider;
    }

    private static PolygonCollider2D CreatePolygonBoundary(Transform parent, string name, Vector2[] points)
    {
        GameObject colliderObject = new GameObject(name);
        colliderObject.transform.SetParent(parent);
        PolygonCollider2D collider = colliderObject.AddComponent<PolygonCollider2D>();
        collider.points = points;
        return collider;
    }

    private static void CreateCandleLightingRig(Transform parent, SpriteRenderer[] glowRenderers, SpriteRenderer shadowGradeRenderer)
    {
        GameObject rig = new GameObject("Realtime Candle Light Rig");
        rig.transform.SetParent(parent);

        Light[] candleLights = new Light[]
        {
            CreateCandlePointLight(rig.transform, "Rear Door Candle Light", new Vector3(-2.34f, 1.88f, -2.2f), 0.72f, 2.55f),
            CreateCandlePointLight(rig.transform, "Center Candelabra Light", new Vector3(0.15f, 1.93f, -2.2f), 1.1f, 3.35f),
            CreateCandlePointLight(rig.transform, "Right Candelabra Light", new Vector3(4.42f, 1.16f, -2.2f), 1.0f, 3.2f),
            CreateCandlePointLight(rig.transform, "Sigil Candle Circle Light", new Vector3(0.22f, -1.04f, -2.2f), 1.18f, 3.55f)
        };

        CryptCandleLightFlicker flicker = rig.AddComponent<CryptCandleLightFlicker>();
        flicker.candleLights = candleLights;
        flicker.glowRenderers = glowRenderers;
        flicker.shadowGradeRenderer = shadowGradeRenderer;
        flicker.flickerSpeed = 3.9f;
        flicker.lightIntensityPulse = 0.32f;
        flicker.lightRangePulse = 0.18f;
        flicker.glowAlphaPulse = 0.18f;
        flicker.glowScalePulse = 0.018f;
        flicker.shadowBreath = 0.045f;
    }

    private static Light CreateCandlePointLight(Transform parent, string name, Vector3 position, float intensity, float range)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent);
        lightObject.transform.position = position;

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.54f, 0.18f, 1f);
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
        light.renderMode = LightRenderMode.ForcePixel;
        return light;
    }

    private static Vector3 IsoPosition(int x, int y)
    {
        return new Vector3((x - y) * 0.78f, (x + y) * 0.38f - 2.15f, 0f);
    }

    private static Sprite PickFloorSprite(int x, int y, Sprite floor, Sprite crackedFloor, Sprite darkFloor, Sprite exitTile)
    {
        if (x == 9 && y == 4)
        {
            return exitTile;
        }

        if ((x == 0 && y > 2) || (y == 6 && x < 4) || (x == 9 && y > 1))
        {
            return darkFloor;
        }

        if ((x + y) % 5 == 0 || (x == 6 && y == 2) || (x == 2 && y == 5))
        {
            return crackedFloor;
        }

        return floor;
    }

    private static void CreateWallDressing(Transform room, Sprite torch, Sprite chain)
    {
        GameObject leftTorch = CreateSpriteObject("Rear Left Candle Sconce", torch, new Vector3(-3.42f, 2.52f, 0f), -28, room);
        leftTorch.transform.localScale = Vector3.one * 0.62f;
        GameObject centerTorch = CreateSpriteObject("Rear Center Candle Sconce", torch, new Vector3(0.58f, 2.52f, 0f), -28, room);
        centerTorch.transform.localScale = Vector3.one * 0.62f;
        GameObject exitTorch = CreateSpriteObject("Exit Candle Sconce", torch, new Vector3(5.7f, 2.52f, 0f), -28, room);
        exitTorch.transform.localScale = Vector3.one * 0.62f;
        CreateSpriteObject("Left Hanging Chain", chain, new Vector3(-2.08f, 3.24f, 0f), -27, room);
        CreateSpriteObject("Right Hanging Chain", chain, new Vector3(2.22f, 3.21f, 0f), -27, room);
    }

    private static void CreateWallReliefs(Transform room, Sprite wallRelief)
    {
        GameObject leftRelief = CreateSpriteObject("Rear Ancestral Relief Left", wallRelief, new Vector3(-2.62f, 2.42f, 0f), -31, room);
        leftRelief.transform.localScale = Vector3.one * 0.84f;
        GameObject rightRelief = CreateSpriteObject("Rear Ancestral Relief Right", wallRelief, new Vector3(1.68f, 2.4f, 0f), -31, room);
        rightRelief.transform.localScale = new Vector3(-0.84f, 0.84f, 1f);
    }

    private static void CreateFloorDressing(Transform room, Sprite urn, Sprite brokenUrn, Sprite candles, Sprite bones, Sprite rubble)
    {
        PlaceFloorPropScaled("Left Funeral Urn", urn, new Vector3(-3.42f, -0.5f, 0f), room, 4, 0.88f);
        PlaceFloorPropScaled("Broken Funeral Urn", brokenUrn, new Vector3(5.82f, 0.36f, 0f), room, 4, 0.9f);
        PlaceFloorPropScaled("Rear Funeral Urn", urn, new Vector3(-1.75f, 1.38f, 0f), room, 4, 0.76f);
        PlaceFloorPropScaled("Front Bone Pile", bones, new Vector3(-2.66f, -1.45f, 0f), room, 2, 0.72f);
        PlaceFloorPropScaled("Right Bone Pile", bones, new Vector3(4.6f, -1.0f, 0f), room, 2, 0.72f);
        PlaceFloorPropScaled("Rear Carved Rubble", rubble, new Vector3(2.2f, 1.58f, 0f), room, 1, 0.78f);
        PlaceFloorPropScaled("Left Carved Rubble", rubble, new Vector3(-4.0f, 0.08f, 0f), room, 1, 0.78f);
    }

    private static void CreateCoffinDressing(Transform room, Sprite candles, Vector3 coffinPosition)
    {
        GameObject rearCandles = CreateSpriteObject("Coffin Rear Candles", candles, coffinPosition + new Vector3(-0.42f, 0.03f, 0f), 124, room);
        rearCandles.transform.localScale = Vector3.one * 0.46f;
        GameObject frontCandles = CreateSpriteObject("Coffin Front Candles", candles, coffinPosition + new Vector3(0.82f, -0.43f, 0f), 154, room);
        frontCandles.transform.localScale = Vector3.one * 0.48f;
    }

    private static void CreateLightingOverlays(Transform room, Sprite vignette, Sprite exitGlow, Sprite candlePool)
    {
        GameObject exitGlowObject = CreateSpriteObject("Blue Exit Glow", exitGlow, new Vector3(4.22f, 2.62f, 0f), -22, room);
        exitGlowObject.transform.localScale = new Vector3(1.25f, 1.08f, 1f);

        GameObject leftPool = CreateSpriteObject("Left Candle Glow Pool", candlePool, new Vector3(-3.44f, 2.11f, 0f), 62, room);
        leftPool.transform.localScale = new Vector3(0.78f, 0.55f, 1f);
        GameObject rightPool = CreateSpriteObject("Right Candle Glow Pool", candlePool, new Vector3(5.62f, 2.08f, 0f), 62, room);
        rightPool.transform.localScale = new Vector3(0.78f, 0.55f, 1f);
        GameObject coffinLeftPool = CreateSpriteObject("Coffin Candle Glow Left", candlePool, new Vector3(0.38f, -0.29f, 0f), 88, room);
        coffinLeftPool.transform.localScale = new Vector3(0.52f, 0.38f, 1f);
        GameObject coffinRightPool = CreateSpriteObject("Coffin Candle Glow Right", candlePool, new Vector3(1.58f, -0.74f, 0f), 88, room);
        coffinRightPool.transform.localScale = new Vector3(0.5f, 0.36f, 1f);

        GameObject vignetteObject = CreateSpriteObject("Camera Vignette", vignette, new Vector3(1.15f, 0.75f, 0f), 900, room);
        vignetteObject.transform.localScale = Vector3.one;
    }

    private static GameObject PlaceFloorProp(string name, Sprite sprite, Vector3 position, Transform parent, int sortOffset)
    {
        return PlaceFloorPropScaled(name, sprite, position, parent, sortOffset, 1f);
    }

    private static GameObject PlaceFloorPropScaled(string name, Sprite sprite, Vector3 position, Transform parent, int sortOffset, float scale)
    {
        GameObject obj = CreateSpriteObject(name, sprite, position, SortForY(position.y, sortOffset), parent);
        obj.transform.localScale = Vector3.one * scale;
        return obj;
    }

    private static int SortForY(float y, int offset)
    {
        return 185 - Mathf.RoundToInt(y * 20f) + offset;
    }

    private static Sprite CreateSprite(string name, int width, int height, Vector2 pivot, TexturePainter painter, float pixelsPerUnit = Ppu)
    {
        string path = ArtFolder + "/" + name + ".png";
        if (!regenerateSprites && File.Exists(path))
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            ConfigureSpriteImporter(path, pivot, pixelsPerUnit);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Clear(texture);
        painter(texture);
        texture.Apply(false);
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        ConfigureSpriteImporter(path, pivot, pixelsPerUnit);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite LoadSprite(string path, Vector2 pivot, float pixelsPerUnit)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        ConfigureSpriteImporter(path, pivot, pixelsPerUnit);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Texture2D LoadTexture(string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static void ConfigureSpriteImporter(string path, Vector2 pivot, float pixelsPerUnit)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.spritePivot = pivot;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static void DrawFloorTile(Texture2D texture)
    {
        FillDiamond(texture, 32, 16, 30, 14, Floor);
        DrawDiamond(texture, 32, 16, 30, 14, Ink);
        DrawLine(texture, 4, 16, 32, 2, FloorLight);
        DrawLine(texture, 32, 2, 60, 16, FloorDark);
        DrawLine(texture, 32, 30, 60, 16, FloorDark);
        DrawLine(texture, 10, 16, 32, 26, FloorDark);
        DrawLine(texture, 28, 4, 42, 11, new Color32(90, 95, 108, 255));
        DrawLine(texture, 18, 21, 25, 24, new Color32(52, 57, 68, 255));

        for (int y = 6; y < 26; y += 4)
        {
            for (int x = 10 + (y % 8); x < 55; x += 11)
            {
                if (InsideDiamond(x, y, 32, 16, 29, 13))
                {
                    Set(texture, x, y, new Color32(91, 94, 105, 255));
                }
            }
        }
    }

    private static void DrawCrackedFloorTile(Texture2D texture)
    {
        DrawFloorTile(texture);
        DrawLine(texture, 22, 8, 27, 13, WallDark);
        DrawLine(texture, 27, 13, 25, 18, WallDark);
        DrawLine(texture, 25, 18, 33, 22, WallDark);
        DrawLine(texture, 42, 10, 36, 14, new Color32(30, 33, 41, 255));
        DrawLine(texture, 36, 14, 40, 19, new Color32(30, 33, 41, 255));
        DrawLine(texture, 12, 17, 20, 15, new Color32(118, 123, 135, 255));
        Set(texture, 31, 23, new Color32(24, 26, 34, 255));
        Set(texture, 32, 23, new Color32(24, 26, 34, 255));
    }

    private static void DrawDarkFloorTile(Texture2D texture)
    {
        FillDiamond(texture, 32, 16, 30, 14, new Color32(48, 52, 63, 255));
        DrawDiamond(texture, 32, 16, 30, 14, Ink);
        DrawLine(texture, 4, 16, 32, 2, new Color32(73, 77, 88, 255));
        DrawLine(texture, 32, 2, 60, 16, new Color32(24, 27, 35, 255));
        DrawLine(texture, 32, 30, 60, 16, new Color32(24, 27, 35, 255));
        DrawLine(texture, 10, 16, 32, 26, new Color32(29, 32, 41, 255));
        DrawLine(texture, 15, 13, 26, 8, new Color32(68, 72, 83, 255));
        DrawLine(texture, 39, 22, 49, 17, new Color32(30, 33, 42, 255));
    }

    private static void DrawExitThreshold(Texture2D texture)
    {
        DrawFloorTile(texture);
        FillDiamond(texture, 32, 16, 17, 7, new Color32(42, 55, 79, 230));
        DrawDiamond(texture, 32, 16, 17, 7, DoorGlow);
        DrawLine(texture, 21, 16, 32, 11, White);
        DrawLine(texture, 32, 11, 43, 16, DoorGlow);
    }

    private static void DrawFloorEdgeShadow(Texture2D texture)
    {
        int cx = texture.width / 2;
        int top = 20;
        int mid = texture.height / 2 - 2;
        int bottom = texture.height - 22;
        int left = 20;
        int right = texture.width - 22;
        Vector2[] floorShape = new Vector2[]
        {
            new Vector2(cx, top),
            new Vector2(right, mid),
            new Vector2(cx, bottom),
            new Vector2(left, mid)
        };

        DrawPolygon(texture, floorShape, new Color32(0, 0, 0, 155));
        DrawPolygon(texture, new Vector2[]
        {
            new Vector2(cx, top + 6),
            new Vector2(right - 13, mid),
            new Vector2(cx, bottom - 8),
            new Vector2(left + 13, mid)
        }, new Color32(22, 24, 31, 115));

        FillPolygon(texture, new Vector2[]
        {
            new Vector2(left + 8, mid - 8),
            new Vector2(cx, top + 2),
            new Vector2(cx + 2, top + 22),
            new Vector2(left + 44, mid + 8)
        }, new Color32(0, 0, 0, 70));

        FillPolygon(texture, new Vector2[]
        {
            new Vector2(cx, top + 2),
            new Vector2(right - 8, mid - 8),
            new Vector2(right - 46, mid + 9),
            new Vector2(cx + 2, top + 22)
        }, new Color32(0, 0, 0, 58));

        FillPolygon(texture, new Vector2[]
        {
            new Vector2(left + 1, mid),
            new Vector2(cx, bottom - 4),
            new Vector2(cx, bottom - 30),
            new Vector2(left + 48, mid)
        }, new Color32(0, 0, 0, 45));

        FillPolygon(texture, new Vector2[]
        {
            new Vector2(right - 1, mid),
            new Vector2(cx, bottom - 4),
            new Vector2(cx, bottom - 30),
            new Vector2(right - 48, mid)
        }, new Color32(0, 0, 0, 55));

        for (int y = top + 36; y < bottom - 28; y += 9)
        {
            for (int x = left + 35; x < right - 35; x += 17)
            {
                if (PointInPolygon(new Vector2(x, y), floorShape))
                {
                    Set(texture, x, y, new Color32(150, 154, 164, 45));
                }
            }
        }
    }

    private static void DrawFloorScuffs(Texture2D texture)
    {
        int cx = texture.width / 2;
        int top = 20;
        int mid = texture.height / 2 - 2;
        int bottom = texture.height - 22;
        int left = 20;
        int right = texture.width - 22;
        Vector2[] floorShape = new Vector2[]
        {
            new Vector2(cx, top),
            new Vector2(right, mid),
            new Vector2(cx, bottom),
            new Vector2(left, mid)
        };

        FillSoftEllipse(texture, 165, 113, 82, 10, new Color32(193, 198, 198, 22), 0.52f);
        FillSoftEllipse(texture, 330, 154, 105, 12, new Color32(154, 170, 190, 18), 0.55f);
        FillSoftEllipse(texture, 243, 198, 96, 11, new Color32(210, 205, 181, 15), 0.58f);

        for (int i = 0; i < 7; i++)
        {
            int y = 83 + i * 21;
            DrawLineClipped(texture, 71 + i * 15, y, 178 + i * 25, y + 22, new Color32(181, 184, 190, 32), floorShape);
            DrawLineClipped(texture, 356 - i * 12, y + 11, 454 - i * 19, y + 27, new Color32(20, 23, 31, 36), floorShape);
        }

        DrawLineClipped(texture, 210, 83, 225, 92, new Color32(13, 15, 22, 54), floorShape);
        DrawLineClipped(texture, 225, 92, 217, 104, new Color32(13, 15, 22, 54), floorShape);
        DrawLineClipped(texture, 217, 104, 239, 116, new Color32(13, 15, 22, 54), floorShape);
        DrawLineClipped(texture, 347, 201, 362, 193, new Color32(13, 15, 22, 48), floorShape);
        DrawLineClipped(texture, 362, 193, 379, 204, new Color32(13, 15, 22, 48), floorShape);
        DrawLineClipped(texture, 130, 175, 113, 184, new Color32(90, 94, 104, 34), floorShape);
        DrawLineClipped(texture, 113, 184, 125, 193, new Color32(90, 94, 104, 34), floorShape);

        for (int y = 58; y < 236; y += 11)
        {
            for (int x = 54 + (y % 23); x < 467; x += 31)
            {
                if (PointInPolygon(new Vector2(x, y), floorShape))
                {
                    Set(texture, x, y, new Color32(215, 211, 190, 28));
                    if ((x + y) % 3 == 0)
                    {
                        Set(texture, x + 1, y, new Color32(18, 20, 28, 24));
                    }
                }
            }
        }
    }

    private static void DrawFloorTombInsets(Texture2D texture)
    {
        DrawInsetFloorSlab(texture, 166, 156, 43, 20, new Color32(55, 59, 69, 176), new Color32(126, 128, 134, 135));
        DrawInsetFloorSlab(texture, 340, 111, 48, 22, new Color32(45, 50, 64, 168), new Color32(101, 126, 160, 150));
        DrawInsetFloorSlab(texture, 372, 202, 40, 19, new Color32(49, 52, 62, 160), new Color32(121, 119, 123, 130));

        DrawLine(texture, 145, 156, 166, 147, new Color32(18, 19, 25, 120));
        DrawLine(texture, 166, 147, 188, 156, new Color32(96, 100, 111, 120));
        DrawLine(texture, 315, 111, 340, 100, new Color32(123, 150, 183, 110));
        DrawLine(texture, 340, 100, 365, 111, new Color32(27, 38, 57, 132));
        DrawLine(texture, 353, 202, 372, 194, new Color32(86, 88, 97, 120));
        DrawLine(texture, 372, 194, 391, 202, new Color32(20, 22, 29, 126));

        FillRect(texture, 158, 154, 16, 3, new Color32(17, 18, 24, 120));
        FillRect(texture, 334, 109, 13, 3, new Color32(23, 30, 44, 120));
        FillRect(texture, 366, 201, 12, 3, new Color32(16, 17, 23, 110));
        Set(texture, 166, 158, Gold);
        Set(texture, 340, 113, DoorGlow);
        Set(texture, 372, 204, WallLight);
    }

    private static void DrawWallFloorBase(Texture2D texture)
    {
        int cx = texture.width / 2;
        int top = 20;
        int mid = texture.height / 2 - 2;
        int bottom = texture.height - 22;
        int left = 20;
        int right = texture.width - 22;

        Vector2[] rearLeftBase = new Vector2[]
        {
            new Vector2(left + 2, mid - 4),
            new Vector2(cx, top - 7),
            new Vector2(cx, top + 14),
            new Vector2(left + 42, mid + 12)
        };
        Vector2[] rearRightBase = new Vector2[]
        {
            new Vector2(cx, top - 7),
            new Vector2(right - 2, mid - 4),
            new Vector2(right - 42, mid + 12),
            new Vector2(cx, top + 14)
        };
        FillPolygon(texture, rearLeftBase, new Color32(27, 29, 38, 210));
        FillPolygon(texture, rearRightBase, new Color32(23, 25, 34, 218));
        DrawPolygon(texture, rearLeftBase, Ink);
        DrawPolygon(texture, rearRightBase, Ink);

        DrawLine(texture, left + 2, mid - 4, cx, top - 7, new Color32(92, 94, 103, 210));
        DrawLine(texture, cx, top - 7, right - 2, mid - 4, new Color32(72, 75, 87, 215));
        DrawLine(texture, left + 42, mid + 12, cx, top + 14, new Color32(10, 11, 16, 230));
        DrawLine(texture, cx, top + 14, right - 42, mid + 12, new Color32(8, 9, 14, 235));

        for (int i = 0; i < 7; i++)
        {
            int x = left + 54 + i * 29;
            int y = mid - 21 - i * 14;
            DrawLine(texture, x, y, x + 4, y + 20, new Color32(13, 14, 20, 180));
        }

        for (int i = 0; i < 7; i++)
        {
            int x = right - 54 - i * 29;
            int y = mid - 21 - i * 14;
            DrawLine(texture, x, y, x - 4, y + 20, new Color32(10, 11, 17, 188));
        }

        Vector2[] leftDrop = new Vector2[]
        {
            new Vector2(left + 2, mid),
            new Vector2(cx, bottom - 1),
            new Vector2(cx, bottom - 22),
            new Vector2(left + 43, mid + 4)
        };
        Vector2[] rightDrop = new Vector2[]
        {
            new Vector2(right - 2, mid),
            new Vector2(cx, bottom - 1),
            new Vector2(cx, bottom - 22),
            new Vector2(right - 43, mid + 4)
        };
        FillPolygon(texture, leftDrop, new Color32(7, 8, 13, 60));
        FillPolygon(texture, rightDrop, new Color32(4, 5, 9, 72));
        DrawLine(texture, left + 2, mid, cx, bottom - 1, new Color32(4, 5, 9, 128));
        DrawLine(texture, right - 2, mid, cx, bottom - 1, new Color32(3, 4, 8, 148));

        FillSoftEllipse(texture, 256, 25, 130, 12, new Color32(0, 0, 0, 48), 0.55f);
    }

    private static void DrawInsetFloorSlab(Texture2D texture, int cx, int cy, int halfWidth, int halfHeight, Color32 fill, Color32 trim)
    {
        FillDiamond(texture, cx, cy, halfWidth, halfHeight, fill);
        DrawDiamond(texture, cx, cy, halfWidth, halfHeight, Ink);
        DrawDiamond(texture, cx, cy, Mathf.Max(halfWidth - 8, 4), Mathf.Max(halfHeight - 5, 3), trim);
        DrawLine(texture, cx - halfWidth + 7, cy, cx, cy - halfHeight + 4, new Color32(trim.r, trim.g, trim.b, 105));
        DrawLine(texture, cx, cy - halfHeight + 4, cx + halfWidth - 7, cy, new Color32(13, 15, 22, 135));
        DrawLine(texture, cx - 7, cy + 1, cx + 7, cy + 1, new Color32(12, 13, 18, 135));
        DrawLine(texture, cx, cy - 6, cx, cy + 6, new Color32(12, 13, 18, 125));
    }

    private static void DrawForegroundLedge(Texture2D texture)
    {
        FillSoftEllipse(texture, 256, 10, 246, 10, new Color32(0, 0, 0, 115), 0.45f);

        Vector2[] leftFace = new Vector2[]
        {
            new Vector2(0, 6),
            new Vector2(214, 6),
            new Vector2(214, 48),
            new Vector2(110, 76),
            new Vector2(0, 52)
        };
        FillPolygon(texture, leftFace, new Color32(25, 27, 35, 255));
        DrawPolygon(texture, leftFace, Ink);

        Vector2[] leftTop = new Vector2[]
        {
            new Vector2(0, 52),
            new Vector2(108, 30),
            new Vector2(214, 52),
            new Vector2(164, 84),
            new Vector2(44, 86)
        };
        FillPolygon(texture, leftTop, new Color32(58, 61, 70, 255));
        DrawPolygon(texture, leftTop, Ink);

        Vector2[] rightFace = new Vector2[]
        {
            new Vector2(298, 6),
            new Vector2(512, 6),
            new Vector2(512, 52),
            new Vector2(402, 76),
            new Vector2(298, 48)
        };
        FillPolygon(texture, rightFace, new Color32(22, 24, 32, 255));
        DrawPolygon(texture, rightFace, Ink);

        Vector2[] rightTop = new Vector2[]
        {
            new Vector2(298, 52),
            new Vector2(404, 30),
            new Vector2(512, 52),
            new Vector2(468, 86),
            new Vector2(348, 84)
        };
        FillPolygon(texture, rightTop, new Color32(54, 57, 67, 255));
        DrawPolygon(texture, rightTop, Ink);

        DrawLine(texture, 29, 19, 86, 31, new Color32(71, 74, 84, 255));
        DrawLine(texture, 95, 13, 164, 13, new Color32(11, 12, 18, 255));
        DrawLine(texture, 16, 46, 99, 65, new Color32(83, 86, 96, 255));
        DrawLine(texture, 111, 76, 178, 55, new Color32(18, 20, 27, 255));
        DrawLine(texture, 358, 13, 434, 13, new Color32(10, 11, 17, 255));
        DrawLine(texture, 426, 70, 498, 48, new Color32(80, 83, 93, 255));
        DrawLine(texture, 348, 84, 404, 30, new Color32(17, 19, 26, 255));

        DrawLine(texture, 70, 62, 86, 72, new Color32(12, 13, 19, 255));
        DrawLine(texture, 86, 72, 80, 82, new Color32(12, 13, 19, 255));
        DrawLine(texture, 382, 58, 367, 66, new Color32(12, 13, 19, 255));
        DrawLine(texture, 367, 66, 377, 77, new Color32(12, 13, 19, 255));

        FillPolygon(texture, new Vector2[] { new Vector2(220, 10), new Vector2(246, 19), new Vector2(235, 32), new Vector2(207, 24) }, new Color32(60, 62, 70, 255));
        DrawPolygon(texture, new Vector2[] { new Vector2(220, 10), new Vector2(246, 19), new Vector2(235, 32), new Vector2(207, 24) }, Ink);
        FillPolygon(texture, new Vector2[] { new Vector2(263, 8), new Vector2(297, 18), new Vector2(280, 35), new Vector2(252, 23) }, new Color32(42, 45, 55, 255));
        DrawPolygon(texture, new Vector2[] { new Vector2(263, 8), new Vector2(297, 18), new Vector2(280, 35), new Vector2(252, 23) }, Ink);
        Set(texture, 231, 24, WallLight);
        Set(texture, 276, 25, WallLight);
        DrawLine(texture, 254, 23, 269, 13, new Color32(81, 84, 94, 255));
    }

    private static void DrawVignetteOverlay(Texture2D texture)
    {
        float cx = texture.width * 0.5f;
        float cy = texture.height * 0.51f;
        float maxDistance = Mathf.Sqrt(cx * cx + cy * cy);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float dx = (x - cx) / cx;
                float dy = (y - cy) / cy;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float edge = Mathf.Clamp01((distance - 0.54f) / 0.52f);
                float topShade = Mathf.Clamp01((y - texture.height * 0.68f) / (texture.height * 0.32f)) * 0.22f;
                float sideShade = Mathf.Abs(x - cx) / cx * 0.18f;
                byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((edge * 108f) + (topShade + sideShade) * 42f), 0, 135);
                Set(texture, x, y, new Color32(0, 0, 0, alpha));
            }
        }

        for (int x = 0; x < texture.width; x += 2)
        {
            Set(texture, x, texture.height - 3, new Color32(0, 0, 0, 160));
        }
    }

    private static void DrawExitGlow(Texture2D texture)
    {
        FillSoftEllipse(texture, 64, 20, 50, 19, new Color32(46, 83, 139, 78), 0.55f);
        FillSoftEllipse(texture, 64, 31, 38, 26, new Color32(32, 65, 119, 58), 0.6f);
        FillSoftEllipse(texture, 64, 44, 54, 20, new Color32(32, 65, 119, 35), 0.65f);
        DrawLine(texture, 36, 20, 64, 10, new Color32(104, 137, 185, 95));
        DrawLine(texture, 64, 10, 93, 20, new Color32(104, 137, 185, 70));
        DrawLine(texture, 42, 36, 84, 48, new Color32(73, 103, 158, 48));
    }

    private static void DrawExitSteps(Texture2D texture)
    {
        FillSoftEllipse(texture, 64, 25, 54, 17, new Color32(0, 0, 0, 80), 0.65f);
        Vector2[] lower = new Vector2[]
        {
            new Vector2(18, 23),
            new Vector2(64, 8),
            new Vector2(110, 23),
            new Vector2(64, 48)
        };
        FillPolygon(texture, lower, new Color32(39, 43, 55, 235));
        DrawPolygon(texture, lower, Ink);
        DrawLine(texture, 28, 23, 64, 42, new Color32(24, 26, 34, 255));
        DrawLine(texture, 64, 42, 100, 23, new Color32(24, 26, 34, 255));

        Vector2[] upper = new Vector2[]
        {
            new Vector2(38, 23),
            new Vector2(64, 14),
            new Vector2(90, 23),
            new Vector2(64, 36)
        };
        FillPolygon(texture, upper, new Color32(66, 73, 91, 235));
        DrawPolygon(texture, upper, Ink);
        DrawLine(texture, 44, 23, 64, 16, new Color32(103, 125, 162, 185));
        DrawLine(texture, 64, 16, 84, 23, new Color32(59, 89, 132, 185));
        DrawLine(texture, 48, 29, 80, 29, new Color32(22, 25, 34, 255));
        Set(texture, 63, 21, new Color32(137, 165, 205, 170));
        Set(texture, 64, 21, new Color32(137, 165, 205, 170));
    }

    private static void DrawCandleLightPool(Texture2D texture)
    {
        FillSoftEllipse(texture, 48, 20, 42, 17, new Color32(164, 78, 29, 52), 0.55f);
        FillSoftEllipse(texture, 48, 22, 25, 9, new Color32(206, 141, 54, 36), 0.65f);
        for (int x = 18; x < 79; x += 7)
        {
            Set(texture, x, 21 + (x % 3), new Color32(224, 157, 58, 48));
        }
    }

    private static void DrawLeftSideWall(Texture2D texture)
    {
        Vector2[] wallFace = new Vector2[]
        {
            new Vector2(118, 10),
            new Vector2(118, 122),
            new Vector2(13, 79),
            new Vector2(13, 28)
        };
        FillPolygon(texture, wallFace, new Color32(34, 36, 45, 255));
        DrawPolygon(texture, wallFace, Ink);

        Vector2[] baseLip = new Vector2[]
        {
            new Vector2(118, 10),
            new Vector2(118, 24),
            new Vector2(13, 42),
            new Vector2(13, 28)
        };
        FillPolygon(texture, baseLip, new Color32(21, 23, 31, 255));
        DrawPolygon(texture, baseLip, Ink);

        for (int i = 0; i < 7; i++)
        {
            int y = 32 + i * 13;
            DrawLineClipped(texture, 18, y, 116, y + 38, WallDark, wallFace);
        }

        for (int i = 0; i < 5; i++)
        {
            int x0 = 34 + i * 18;
            DrawLineClipped(texture, x0, 33, x0 + 3, 107, WallDark, wallFace);
        }

        DrawLine(texture, 112, 20, 112, 116, WallLight);
        DrawLineClipped(texture, 20, 40, 106, 76, new Color32(79, 82, 93, 255), wallFace);
        FillRect(texture, 56, 60, 20, 26, WallDark);
        DrawRect(texture, 56, 60, 20, 26, Ink);
        DrawLine(texture, 60, 82, 72, 63, WallLight);
        DrawSideWallDrawer(texture, new Vector2[] { new Vector2(29, 45), new Vector2(79, 63), new Vector2(79, 75), new Vector2(29, 57) }, true);
        DrawSideWallDrawer(texture, new Vector2[] { new Vector2(26, 82), new Vector2(67, 98), new Vector2(67, 108), new Vector2(26, 93) }, false);
        FillRect(texture, 84, 32, 17, 4, new Color32(12, 13, 19, 170));
        DrawRect(texture, 84, 32, 17, 4, Ink);
        Set(texture, 95, 35, Red);
        DrawCobweb(texture, 18, 118, 1);
    }

    private static void DrawRightSideWall(Texture2D texture)
    {
        Vector2[] wallFace = new Vector2[]
        {
            new Vector2(10, 10),
            new Vector2(10, 122),
            new Vector2(115, 79),
            new Vector2(115, 28)
        };
        FillPolygon(texture, wallFace, new Color32(30, 31, 40, 255));
        DrawPolygon(texture, wallFace, Ink);

        Vector2[] baseLip = new Vector2[]
        {
            new Vector2(10, 10),
            new Vector2(10, 24),
            new Vector2(115, 42),
            new Vector2(115, 28)
        };
        FillPolygon(texture, baseLip, new Color32(18, 19, 27, 255));
        DrawPolygon(texture, baseLip, Ink);

        for (int i = 0; i < 7; i++)
        {
            int y = 32 + i * 13;
            DrawLineClipped(texture, 112, y, 12, y + 38, WallDark, wallFace);
        }

        for (int i = 0; i < 5; i++)
        {
            int x0 = 92 - i * 18;
            DrawLineClipped(texture, x0, 33, x0 - 3, 107, WallDark, wallFace);
        }

        DrawLine(texture, 16, 20, 16, 116, new Color32(86, 88, 99, 255));
        FillRect(texture, 50, 49, 18, 30, DoorBlack);
        DrawRect(texture, 50, 49, 18, 30, Ink);
        Set(texture, 66, 62, DoorGlow);
        Set(texture, 66, 63, DoorGlow);
        DrawSideWallDrawer(texture, new Vector2[] { new Vector2(99, 45), new Vector2(49, 63), new Vector2(49, 75), new Vector2(99, 57) }, false);
        DrawSideWallDrawer(texture, new Vector2[] { new Vector2(102, 82), new Vector2(61, 98), new Vector2(61, 108), new Vector2(102, 93) }, true);
        FillRect(texture, 28, 33, 17, 4, new Color32(12, 13, 19, 170));
        DrawRect(texture, 28, 33, 17, 4, Ink);
        Set(texture, 32, 36, Red);
        DrawCobweb(texture, 110, 118, -1);
    }

    private static void DrawSideWallDrawer(Texture2D texture, Vector2[] shape, bool leftLit)
    {
        FillPolygon(texture, shape, new Color32(38, 40, 50, 245));
        DrawPolygon(texture, shape, Ink);

        Color32 lit = leftLit ? new Color32(80, 83, 94, 225) : new Color32(18, 19, 27, 235);
        Color32 shade = leftLit ? new Color32(17, 18, 25, 235) : new Color32(73, 76, 87, 215);
        DrawLine(texture, Mathf.RoundToInt(shape[0].x) + 3, Mathf.RoundToInt(shape[0].y) + 2, Mathf.RoundToInt(shape[1].x) - 3, Mathf.RoundToInt(shape[1].y) + 2, lit);
        DrawLine(texture, Mathf.RoundToInt(shape[3].x) + 4, Mathf.RoundToInt(shape[3].y) - 2, Mathf.RoundToInt(shape[2].x) - 4, Mathf.RoundToInt(shape[2].y) - 2, shade);

        int cx = Mathf.RoundToInt((shape[0].x + shape[1].x + shape[2].x + shape[3].x) * 0.25f);
        int cy = Mathf.RoundToInt((shape[0].y + shape[1].y + shape[2].y + shape[3].y) * 0.25f);
        DrawLine(texture, cx - 5, cy, cx + 5, cy, new Color32(13, 14, 20, 220));
        Set(texture, cx, cy + 2, Gold);
    }

    private static void DrawCobweb(Texture2D texture, int anchorX, int anchorY, int directionX)
    {
        Color32 web = new Color32(197, 200, 190, 46);
        Color32 faint = new Color32(197, 200, 190, 25);
        DrawLine(texture, anchorX, anchorY, anchorX + directionX * 25, anchorY - 5, web);
        DrawLine(texture, anchorX, anchorY, anchorX + directionX * 20, anchorY - 19, web);
        DrawLine(texture, anchorX, anchorY, anchorX + directionX * 7, anchorY - 28, faint);
        DrawLine(texture, anchorX + directionX * 6, anchorY - 3, anchorX + directionX * 9, anchorY - 12, faint);
        DrawLine(texture, anchorX + directionX * 13, anchorY - 4, anchorX + directionX * 15, anchorY - 16, faint);
        DrawLine(texture, anchorX + directionX * 21, anchorY - 5, anchorX + directionX * 19, anchorY - 19, faint);
        DrawLine(texture, anchorX + directionX * 5, anchorY - 11, anchorX + directionX * 18, anchorY - 14, faint);
        DrawLine(texture, anchorX + directionX * 3, anchorY - 18, anchorX + directionX * 12, anchorY - 23, faint);
    }

    private static void DrawCoffinGlow(Texture2D texture)
    {
        FillSoftEllipse(texture, 77, 29, 62, 18, new Color32(0, 0, 0, 92), 0.62f);
        FillSoftEllipse(texture, 76, 36, 60, 22, new Color32(79, 4, 17, 46), 0.56f);
        FillPolygon(texture, new Vector2[]
        {
            new Vector2(21, 30),
            new Vector2(61, 14),
            new Vector2(116, 43),
            new Vector2(76, 64)
        }, new Color32(134, 14, 29, 30));
        DrawLine(texture, 27, 31, 61, 15, new Color32(184, 39, 43, 48));
        DrawLine(texture, 61, 15, 112, 42, new Color32(72, 84, 119, 42));
        DrawLine(texture, 27, 43, 76, 65, new Color32(138, 18, 31, 42));
    }

    private static void DrawBackWall(Texture2D texture)
    {
        int w = texture.width;
        int wallWidth = w - 6;
        int doorCenter = w - 92;
        FillRect(texture, 3, 5, wallWidth, 112, new Color32(43, 44, 54, 255));
        DrawRect(texture, 3, 5, wallWidth, 112, Ink);
        FillRect(texture, 3, 5, wallWidth, 13, WallDark);
        FillRect(texture, 3, 106, wallWidth, 11, new Color32(92, 92, 101, 255));
        FillRect(texture, 3, 18, wallWidth, 8, new Color32(31, 33, 43, 255));

        for (int y = 18; y < 108; y += 13)
        {
            DrawLine(texture, 6, y, w - 8, y, WallDark);
        }

        for (int row = 0; row < 8; row++)
        {
            int y0 = 16 + row * 13;
            int offset = row % 2 == 0 ? 0 : 18;
            for (int x = 18 + offset; x < w - 10; x += 36)
            {
                DrawLine(texture, x, y0, x, Mathf.Min(y0 + 12, 109), WallDark);
            }
        }

        FillRect(texture, 6, 7, w - 12, 7, new Color32(9, 10, 15, 170));
        DrawArch(texture, doorCenter, 7, 54, 78);
        DrawLine(texture, doorCenter - 25, 7, doorCenter - 25, 53, Ink);
        DrawLine(texture, doorCenter + 25, 7, doorCenter + 25, 53, Ink);
        DrawLine(texture, doorCenter - 22, 13, doorCenter - 22, 52, DoorGlow);
        DrawLine(texture, doorCenter + 21, 13, doorCenter + 21, 52, new Color32(30, 49, 77, 255));

        FillRect(texture, 12, 22, 32, 24, WallDark);
        DrawRect(texture, 12, 22, 32, 24, Ink);
        DrawLine(texture, 16, 42, 39, 26, WallLight);
        DrawLine(texture, 16, 26, 39, 42, WallDark);

        FillRect(texture, 78, 39, 28, 32, WallDark);
        DrawRect(texture, 78, 39, 28, 32, Ink);
        DrawLine(texture, 83, 66, 100, 43, WallLight);

        FillRect(texture, 143, 20, 38, 8, WallDark);
        DrawRect(texture, 143, 20, 38, 8, Ink);
        Set(texture, 165, 25, Red);
        Set(texture, 167, 25, Red);

        FillRect(texture, 218, 34, 30, 34, WallDark);
        DrawRect(texture, 218, 34, 30, 34, Ink);
        DrawLine(texture, 224, 62, 242, 39, WallLight);

        FillRect(texture, 50, 94, 58, 4, new Color32(15, 16, 22, 130));
        DrawLine(texture, 151, 103, 219, 103, new Color32(18, 19, 25, 170));
        DrawLine(texture, doorCenter + 28, 103, w - 26, 103, new Color32(18, 19, 25, 130));

        DrawWallNiche(texture, 54, 46, 25, 41);
        DrawWallNiche(texture, 198, 48, 26, 39);
        DrawTombSlab(texture, 116, 51, 42, 48);
        DrawTombSlab(texture, 284, 49, 36, 46);
        DrawWallCrack(texture, 34, 81, true);
        DrawWallCrack(texture, 255, 35, false);
        DrawWallCrack(texture, w - 46, 89, true);
        DrawCobweb(texture, 14, 113, 1);
        DrawCobweb(texture, w - 15, 113, -1);
        FillRect(texture, 3, 5, wallWidth, 5, new Color32(13, 14, 20, 210));
        for (int x = 16; x < w - 32; x += 42)
        {
            DrawLine(texture, x, 5, x + 8, 13, new Color32(82, 84, 94, 185));
            DrawLine(texture, x + 20, 9, x + 34, 9, new Color32(11, 12, 18, 210));
        }
        DrawLine(texture, doorCenter - 35, 89, doorCenter + 37, 89, new Color32(7, 8, 12, 180));
        DrawLine(texture, doorCenter - 28, 96, doorCenter + 32, 96, new Color32(8, 9, 14, 150));
    }

    private static void DrawWallRelief(Texture2D texture)
    {
        FillEllipse(texture, 29, 6, 23, 5, new Color32(0, 0, 0, 88));
        FillRect(texture, 7, 12, 44, 50, new Color32(47, 49, 58, 255));
        FillEllipse(texture, 29, 61, 22, 16, new Color32(47, 49, 58, 255));
        FillRect(texture, 8, 12, 42, 50, new Color32(47, 49, 58, 255));

        DrawLine(texture, 7, 12, 7, 61, Ink);
        DrawLine(texture, 51, 12, 51, 61, Ink);
        DrawLine(texture, 7, 12, 51, 12, Ink);
        for (int a = 0; a <= 180; a += 4)
        {
            float radians = a * Mathf.Deg2Rad;
            int x = Mathf.RoundToInt(29 + Mathf.Cos(radians) * 22);
            int y = Mathf.RoundToInt(61 + Mathf.Sin(radians) * 16);
            Set(texture, x, y, Ink);
        }

        FillRect(texture, 12, 17, 34, 40, new Color32(28, 30, 39, 255));
        DrawRect(texture, 12, 17, 34, 40, Ink);
        FillRect(texture, 16, 21, 26, 29, new Color32(38, 40, 49, 255));
        DrawRect(texture, 16, 21, 26, 29, new Color32(10, 11, 17, 255));
        DrawLine(texture, 16, 50, 42, 50, new Color32(80, 83, 94, 255));
        DrawLine(texture, 17, 23, 41, 23, new Color32(12, 13, 19, 255));

        FillPolygon(texture, new Vector2[] { new Vector2(28, 25), new Vector2(21, 39), new Vector2(28, 47), new Vector2(36, 39) }, new Color32(61, 63, 71, 255));
        DrawPolygon(texture, new Vector2[] { new Vector2(28, 25), new Vector2(21, 39), new Vector2(28, 47), new Vector2(36, 39) }, Ink);
        FillPolygon(texture, new Vector2[] { new Vector2(21, 37), new Vector2(10, 42), new Vector2(17, 30) }, new Color32(82, 10, 24, 255));
        DrawPolygon(texture, new Vector2[] { new Vector2(21, 37), new Vector2(10, 42), new Vector2(17, 30) }, Ink);
        FillPolygon(texture, new Vector2[] { new Vector2(36, 37), new Vector2(48, 42), new Vector2(40, 30) }, new Color32(82, 10, 24, 255));
        DrawPolygon(texture, new Vector2[] { new Vector2(36, 37), new Vector2(48, 42), new Vector2(40, 30) }, Ink);
        FillRect(texture, 27, 28, 4, 18, WallDark);
        DrawLine(texture, 21, 37, 36, 37, WallDark);
        FillEllipse(texture, 29, 41, 6, 5, new Color32(123, 122, 125, 255));
        Set(texture, 26, 41, Ink);
        Set(texture, 32, 41, Ink);
        DrawLine(texture, 26, 38, 32, 38, Ink);
        Set(texture, 28, 65, Red);
        Set(texture, 29, 65, Red);
        Set(texture, 30, 65, Red);
        DrawLine(texture, 14, 58, 24, 67, WallLight);
        DrawLine(texture, 43, 57, 35, 47, WallDark);
        DrawLine(texture, 17, 18, 24, 25, new Color32(82, 85, 96, 255));
    }

    private static void DrawWallNiche(Texture2D texture, int x, int y, int width, int height)
    {
        FillRect(texture, x, y, width, height - 7, new Color32(22, 23, 31, 255));
        FillEllipse(texture, x + width / 2, y + height - 8, width / 2, 10, new Color32(22, 23, 31, 255));
        DrawRect(texture, x, y, width, height - 7, Ink);
        for (int a = 0; a <= 180; a += 6)
        {
            float radians = a * Mathf.Deg2Rad;
            int px = Mathf.RoundToInt(x + width / 2 + Mathf.Cos(radians) * width / 2);
            int py = Mathf.RoundToInt(y + height - 8 + Mathf.Sin(radians) * 10);
            Set(texture, px, py, Ink);
        }
        DrawLine(texture, x + 3, y + 3, x + 3, y + height - 13, new Color32(74, 78, 91, 255));
        DrawLine(texture, x + width - 3, y + 4, x + width - 3, y + height - 13, new Color32(13, 14, 20, 255));
        FillRect(texture, x + 6, y + 8, width - 12, 4, new Color32(37, 39, 49, 255));
    }

    private static void DrawTombSlab(Texture2D texture, int x, int y, int width, int height)
    {
        FillRect(texture, x, y, width, height, new Color32(37, 39, 48, 255));
        DrawRect(texture, x, y, width, height, Ink);
        DrawLine(texture, x + 5, y + height - 7, x + width - 6, y + height - 7, WallDark);
        DrawLine(texture, x + 7, y + 9, x + width - 8, y + 9, new Color32(76, 79, 91, 255));
        DrawLine(texture, x + width / 2, y + 14, x + width / 2, y + height - 17, WallDark);
        DrawLine(texture, x + width / 2 - 5, y + height - 25, x + width / 2 + 5, y + height - 25, WallDark);
        Set(texture, x + width - 8, y + 12, DoorGlow);
    }

    private static void DrawWallCrack(Texture2D texture, int x, int y, bool mirrored)
    {
        int sign = mirrored ? -1 : 1;
        DrawLine(texture, x, y, x + sign * 8, y - 6, new Color32(13, 14, 20, 255));
        DrawLine(texture, x + sign * 8, y - 6, x + sign * 4, y - 14, new Color32(13, 14, 20, 255));
        DrawLine(texture, x + sign * 4, y - 14, x + sign * 13, y - 19, new Color32(13, 14, 20, 255));
        DrawLine(texture, x + sign * 6, y - 7, x + sign * 17, y - 8, new Color32(21, 23, 30, 255));
        DrawLine(texture, x + sign * 3, y - 13, x + sign * 1, y - 22, new Color32(21, 23, 30, 255));
    }

    private static void DrawPillar(Texture2D texture)
    {
        FillRect(texture, 8, 4, 12, 82, new Color32(59, 60, 69, 255));
        FillRect(texture, 5, 4, 18, 7, WallDark);
        FillRect(texture, 4, 82, 20, 7, WallLight);
        DrawRect(texture, 8, 4, 12, 82, Ink);
        DrawRect(texture, 5, 4, 18, 7, Ink);
        DrawRect(texture, 4, 82, 20, 7, Ink);
        DrawLine(texture, 11, 13, 11, 79, WallLight);
        DrawLine(texture, 18, 13, 18, 79, WallDark);

        for (int y = 20; y < 77; y += 14)
        {
            DrawLine(texture, 8, y, 19, y, WallDark);
        }
    }

    private static void DrawCoffin(Texture2D texture)
    {
        FillSoftEllipse(texture, 67, 20, 52, 13, new Color32(0, 0, 0, 138), 0.58f);

        Vector2[] lowerCase = new Vector2[]
        {
            new Vector2(15, 29),
            new Vector2(48, 13),
            new Vector2(71, 18),
            new Vector2(112, 43),
            new Vector2(86, 68),
            new Vector2(53, 61),
            new Vector2(16, 44)
        };
        FillPolygon(texture, lowerCase, WoodDark);
        DrawPolygon(texture, lowerCase, Ink);

        Vector2[] nearSide = new Vector2[]
        {
            new Vector2(16, 44),
            new Vector2(53, 61),
            new Vector2(86, 68),
            new Vector2(112, 43),
            new Vector2(106, 51),
            new Vector2(86, 74),
            new Vector2(51, 67),
            new Vector2(15, 51)
        };
        FillPolygon(texture, nearSide, new Color32(55, 20, 22, 255));
        DrawPolygon(texture, nearSide, Ink);

        Vector2[] farSide = new Vector2[]
        {
            new Vector2(15, 29),
            new Vector2(48, 13),
            new Vector2(71, 18),
            new Vector2(112, 43),
            new Vector2(105, 44),
            new Vector2(70, 24),
            new Vector2(49, 20),
            new Vector2(20, 34)
        };
        FillPolygon(texture, farSide, new Color32(115, 55, 40, 255));
        DrawPolygon(texture, farSide, Ink);

        Vector2[] lid = new Vector2[]
        {
            new Vector2(24, 33),
            new Vector2(50, 20),
            new Vector2(69, 24),
            new Vector2(101, 43),
            new Vector2(80, 61),
            new Vector2(54, 56),
            new Vector2(24, 42)
        };
        FillPolygon(texture, lid, new Color32(96, 43, 35, 255));
        DrawPolygon(texture, lid, Ink);

        Vector2[] raisedInset = new Vector2[]
        {
            new Vector2(34, 36),
            new Vector2(53, 26),
            new Vector2(68, 29),
            new Vector2(88, 42),
            new Vector2(73, 54),
            new Vector2(55, 51),
            new Vector2(34, 41)
        };
        FillPolygon(texture, raisedInset, new Color32(118, 52, 39, 255));
        DrawPolygon(texture, raisedInset, new Color32(161, 78, 52, 255));

        DrawLine(texture, 24, 33, 50, 20, new Color32(168, 86, 57, 255));
        DrawLine(texture, 50, 20, 69, 24, new Color32(198, 108, 68, 255));
        DrawLine(texture, 70, 25, 100, 43, new Color32(154, 72, 49, 255));
        DrawLine(texture, 101, 43, 80, 61, new Color32(43, 14, 17, 255));
        DrawLine(texture, 54, 56, 80, 61, new Color32(49, 16, 18, 255));
        DrawLine(texture, 20, 43, 52, 61, new Color32(92, 36, 30, 255));
        DrawLine(texture, 108, 45, 86, 71, new Color32(34, 10, 13, 255));
        DrawLine(texture, 36, 39, 75, 56, new Color32(74, 25, 24, 255));
        DrawLine(texture, 44, 32, 84, 50, new Color32(146, 67, 45, 255));

        FillPolygon(texture, new Vector2[]
        {
            new Vector2(49, 39),
            new Vector2(70, 44),
            new Vector2(67, 49),
            new Vector2(46, 44)
        }, RedDark);
        DrawPolygon(texture, new Vector2[]
        {
            new Vector2(49, 39),
            new Vector2(70, 44),
            new Vector2(67, 49),
            new Vector2(46, 44)
        }, Ink);
        FillPolygon(texture, new Vector2[]
        {
            new Vector2(58, 32),
            new Vector2(64, 34),
            new Vector2(59, 55),
            new Vector2(53, 53)
        }, RedDark);
        DrawPolygon(texture, new Vector2[]
        {
            new Vector2(58, 32),
            new Vector2(64, 34),
            new Vector2(59, 55),
            new Vector2(53, 53)
        }, Ink);
        DrawLine(texture, 51, 42, 66, 46, new Color32(135, 13, 29, 255));
        DrawLine(texture, 58, 35, 56, 51, new Color32(135, 13, 29, 255));

        DrawLine(texture, 31, 34, 38, 36, Gold);
        DrawLine(texture, 84, 44, 92, 48, Gold);
        DrawLine(texture, 44, 58, 52, 60, Gold);
        DrawLine(texture, 80, 65, 88, 67, Gold);
        Set(texture, 34, 36, White);
        Set(texture, 88, 47, White);
        Set(texture, 59, 44, new Color32(194, 32, 42, 255));
        Set(texture, 60, 45, new Color32(194, 32, 42, 255));
        DrawLine(texture, 46, 16, 72, 22, new Color32(54, 20, 20, 255));
        DrawLine(texture, 28, 38, 56, 52, new Color32(54, 20, 20, 255));
    }

    private static void DrawUrn(Texture2D texture)
    {
        FillEllipse(texture, 19, 6, 15, 5, new Color32(0, 0, 0, 102));
        FillRect(texture, 14, 8, 10, 5, new Color32(45, 42, 48, 255));
        DrawRect(texture, 14, 8, 10, 5, Ink);
        FillEllipse(texture, 19, 16, 9, 7, new Color32(88, 84, 91, 255));
        DrawLine(texture, 11, 16, 27, 16, Ink);
        FillEllipse(texture, 19, 27, 13, 16, new Color32(83, 79, 87, 255));
        FillEllipse(texture, 19, 29, 9, 13, new Color32(55, 52, 62, 255));
        DrawLine(texture, 9, 21, 4, 30, Ink);
        DrawLine(texture, 29, 21, 34, 30, Ink);
        DrawLine(texture, 4, 30, 10, 36, Ink);
        DrawLine(texture, 34, 30, 28, 36, Ink);
        DrawLine(texture, 8, 24, 12, 31, new Color32(94, 91, 99, 255));
        DrawLine(texture, 30, 24, 26, 31, new Color32(32, 30, 38, 255));
        FillRect(texture, 11, 43, 16, 5, new Color32(40, 37, 45, 255));
        DrawRect(texture, 11, 43, 16, 5, Ink);
        DrawLine(texture, 13, 38, 24, 17, new Color32(123, 118, 123, 255));
        DrawLine(texture, 22, 39, 28, 25, WallDark);
        DrawLine(texture, 15, 26, 23, 32, new Color32(113, 92, 55, 255));
        DrawLine(texture, 15, 32, 23, 26, new Color32(113, 92, 55, 255));
        Set(texture, 18, 31, Red);
        Set(texture, 19, 31, Red);
        DrawLine(texture, 12, 45, 26, 45, WallLight);
        DrawLine(texture, 25, 17, 30, 20, new Color32(21, 22, 30, 170));
    }

    private static void DrawBrokenUrn(Texture2D texture)
    {
        FillEllipse(texture, 27, 8, 23, 7, new Color32(0, 0, 0, 95));
        FillPolygon(texture, new Vector2[] { new Vector2(13, 14), new Vector2(26, 9), new Vector2(39, 14), new Vector2(35, 25), new Vector2(20, 27), new Vector2(9, 22) }, new Color32(73, 69, 76, 255));
        DrawPolygon(texture, new Vector2[] { new Vector2(13, 14), new Vector2(26, 9), new Vector2(39, 14), new Vector2(35, 25), new Vector2(20, 27), new Vector2(9, 22) }, Ink);
        FillPolygon(texture, new Vector2[] { new Vector2(16, 15), new Vector2(26, 12), new Vector2(35, 15), new Vector2(31, 22), new Vector2(21, 24), new Vector2(13, 21) }, new Color32(49, 47, 57, 255));
        DrawLine(texture, 14, 15, 21, 20, WallLight);
        DrawLine(texture, 27, 12, 21, 24, WallDark);
        DrawLine(texture, 36, 15, 30, 22, new Color32(23, 23, 31, 255));
        FillPolygon(texture, new Vector2[] { new Vector2(6, 19), new Vector2(14, 23), new Vector2(10, 30), new Vector2(3, 27) }, new Color32(70, 67, 75, 255));
        DrawPolygon(texture, new Vector2[] { new Vector2(6, 19), new Vector2(14, 23), new Vector2(10, 30), new Vector2(3, 27) }, Ink);
        FillPolygon(texture, new Vector2[] { new Vector2(40, 18), new Vector2(51, 21), new Vector2(47, 29), new Vector2(37, 25) }, new Color32(58, 55, 64, 255));
        DrawPolygon(texture, new Vector2[] { new Vector2(40, 18), new Vector2(51, 21), new Vector2(47, 29), new Vector2(37, 25) }, Ink);
        DrawLine(texture, 16, 27, 43, 28, new Color32(181, 176, 158, 74));
        DrawLine(texture, 19, 29, 34, 31, new Color32(181, 176, 158, 48));
        Set(texture, 25, 20, Red);
        Set(texture, 26, 20, Red);
    }

    private static void DrawCandles(Texture2D texture)
    {
        FillEllipse(texture, 22, 6, 18, 5, new Color32(0, 0, 0, 82));
        FillEllipse(texture, 22, 24, 19, 13, new Color32(123, 62, 24, 35));
        DrawLine(texture, 10, 11, 34, 11, new Color32(94, 73, 38, 255));
        DrawLine(texture, 22, 8, 22, 28, new Color32(94, 73, 38, 255));
        DrawLine(texture, 13, 11, 13, 20, new Color32(94, 73, 38, 255));
        DrawLine(texture, 31, 11, 31, 20, new Color32(70, 52, 31, 255));
        FillRect(texture, 8, 20, 5, 17, White);
        FillRect(texture, 16, 17, 5, 20, new Color32(220, 213, 188, 255));
        FillRect(texture, 24, 15, 5, 22, new Color32(229, 222, 199, 255));
        FillRect(texture, 33, 21, 4, 15, new Color32(201, 194, 171, 255));
        DrawRect(texture, 8, 20, 5, 17, Ink);
        DrawRect(texture, 16, 17, 5, 20, Ink);
        DrawRect(texture, 24, 15, 5, 22, Ink);
        DrawRect(texture, 33, 21, 4, 15, Ink);
        Set(texture, 11, 25, new Color32(201, 194, 171, 255));
        Set(texture, 19, 24, new Color32(190, 183, 164, 255));
        Set(texture, 27, 23, new Color32(202, 194, 171, 255));
        DrawFlame(texture, 10, 38);
        DrawFlame(texture, 18, 39);
        DrawFlame(texture, 26, 41);
        DrawFlame(texture, 35, 37);
        FillRect(texture, 7, 6, 30, 4, new Color32(76, 62, 39, 255));
        DrawLine(texture, 7, 6, 37, 6, Ink);
        FillRect(texture, 16, 2, 13, 5, new Color32(62, 50, 34, 255));
        DrawRect(texture, 16, 2, 13, 5, Ink);
        Set(texture, 22, 7, Gold);
    }

    private static void DrawBones(Texture2D texture)
    {
        FillEllipse(texture, 30, 8, 25, 6, new Color32(0, 0, 0, 86));
        DrawLine(texture, 8, 14, 35, 20, White);
        DrawLine(texture, 8, 13, 35, 19, Ink);
        FillEllipse(texture, 7, 14, 4, 3, White);
        FillEllipse(texture, 36, 20, 4, 3, White);
        DrawLine(texture, 19, 10, 46, 13, new Color32(213, 207, 185, 255));
        DrawLine(texture, 19, 9, 46, 12, Ink);
        FillEllipse(texture, 18, 10, 4, 3, White);
        FillEllipse(texture, 47, 13, 4, 3, White);
        FillEllipse(texture, 43, 17, 8, 6, Pale);
        DrawRect(texture, 37, 13, 13, 10, Ink);
        Set(texture, 41, 18, Ink);
        Set(texture, 47, 18, Ink);
        DrawLine(texture, 42, 15, 48, 15, Ink);
        DrawLine(texture, 39, 23, 49, 23, new Color32(189, 181, 159, 255));
        DrawLine(texture, 22, 18, 25, 25, Pale);
        DrawLine(texture, 27, 18, 30, 25, Pale);
        DrawLine(texture, 32, 18, 35, 24, Pale);
        DrawLine(texture, 21, 18, 24, 25, Ink);
        DrawLine(texture, 26, 18, 29, 25, Ink);
        DrawLine(texture, 31, 18, 34, 24, Ink);
        DrawLine(texture, 14, 22, 24, 28, new Color32(207, 201, 179, 255));
        DrawLine(texture, 14, 21, 24, 27, Ink);
        Set(texture, 44, 21, new Color32(160, 151, 131, 255));
    }

    private static void DrawChain(Texture2D texture)
    {
        for (int y = 66; y > 12; y -= 9)
        {
            DrawRect(texture, 9, y - 5, 6, 8, new Color32(102, 105, 112, 255));
            Set(texture, 10, y - 4, Ink);
            Set(texture, 14, y + 2, Ink);
        }

        FillRect(texture, 8, 66, 8, 4, WallDark);
        DrawRect(texture, 8, 66, 8, 4, Ink);
        DrawLine(texture, 12, 13, 5, 3, new Color32(82, 84, 91, 255));
        DrawLine(texture, 12, 13, 19, 3, new Color32(82, 84, 91, 255));
        FillEllipse(texture, 12, 8, 8, 5, new Color32(0, 0, 0, 65));
    }

    private static void DrawRug(Texture2D texture)
    {
        Vector2[] rugShape = new Vector2[]
        {
            new Vector2(48, 5),
            new Vector2(90, 24),
            new Vector2(48, 43),
            new Vector2(6, 24)
        };
        FillPolygon(texture, rugShape, new Color32(77, 10, 24, 235));
        DrawPolygon(texture, rugShape, Ink);
        DrawPolygon(texture, new Vector2[]
        {
            new Vector2(48, 10),
            new Vector2(78, 24),
            new Vector2(48, 38),
            new Vector2(18, 24)
        }, new Color32(143, 26, 35, 255));
        DrawLine(texture, 25, 24, 48, 14, Gold);
        DrawLine(texture, 48, 14, 72, 24, Gold);
        DrawLine(texture, 25, 24, 48, 34, RedDark);
        DrawLine(texture, 48, 34, 72, 24, RedDark);
        for (int x = 12; x <= 84; x += 8)
        {
            Set(texture, x, 24, new Color32(197, 151, 67, 255));
        }
    }

    private static void DrawRubble(Texture2D texture)
    {
        FillEllipse(texture, 33, 9, 29, 7, new Color32(0, 0, 0, 88));
        FillPolygon(texture, new Vector2[] { new Vector2(8, 10), new Vector2(22, 14), new Vector2(18, 27), new Vector2(4, 23) }, new Color32(64, 66, 76, 255));
        DrawPolygon(texture, new Vector2[] { new Vector2(8, 10), new Vector2(22, 14), new Vector2(18, 27), new Vector2(4, 23) }, Ink);
        FillPolygon(texture, new Vector2[] { new Vector2(24, 7), new Vector2(43, 13), new Vector2(37, 29), new Vector2(18, 23) }, new Color32(84, 86, 96, 255));
        DrawPolygon(texture, new Vector2[] { new Vector2(24, 7), new Vector2(43, 13), new Vector2(37, 29), new Vector2(18, 23) }, Ink);
        FillPolygon(texture, new Vector2[] { new Vector2(45, 12), new Vector2(62, 16), new Vector2(55, 29), new Vector2(39, 25) }, new Color32(55, 58, 68, 255));
        DrawPolygon(texture, new Vector2[] { new Vector2(45, 12), new Vector2(62, 16), new Vector2(55, 29), new Vector2(39, 25) }, Ink);
        FillPolygon(texture, new Vector2[] { new Vector2(26, 12), new Vector2(38, 16), new Vector2(34, 25), new Vector2(22, 21) }, new Color32(47, 51, 64, 255));
        DrawPolygon(texture, new Vector2[] { new Vector2(26, 12), new Vector2(38, 16), new Vector2(34, 25), new Vector2(22, 21) }, Ink);
        DrawLine(texture, 27, 16, 34, 19, DoorGlow);
        DrawLine(texture, 31, 14, 29, 23, DoorGlow);
        DrawLine(texture, 24, 12, 38, 16, WallLight);
        DrawLine(texture, 10, 17, 18, 20, WallLight);
        DrawLine(texture, 47, 17, 56, 19, WallLight);
        DrawLine(texture, 31, 28, 51, 28, new Color32(14, 15, 20, 190));
        Set(texture, 13, 21, new Color32(118, 122, 133, 255));
        Set(texture, 52, 20, new Color32(101, 105, 116, 255));
    }

    private static void DrawFlame(Texture2D texture, int x, int y)
    {
        FillEllipse(texture, x, y, 3, 4, new Color32(255, 167, 54, 85));
        Set(texture, x, y + 2, Gold);
        Set(texture, x - 1, y + 1, Gold);
        Set(texture, x + 1, y + 1, Gold);
        Set(texture, x, y + 1, White);
        Set(texture, x, y + 3, Red);
    }

    private static void DrawShadow(Texture2D texture)
    {
        FillEllipse(texture, 22, 7, 20, 6, new Color32(0, 0, 0, 126));
        FillEllipse(texture, 22, 7, 13, 4, new Color32(0, 0, 0, 162));
    }

    private static void DrawDracula(Texture2D texture, int direction, int frame)
    {
        if (direction == 1)
        {
            DrawDraculaUp(texture, frame);
        }
        else if (direction == 2)
        {
            DrawDraculaSide(texture, frame);
        }
        else
        {
            DrawDraculaDown(texture, frame);
        }

        AddDraculaFinish(texture, direction, frame);
    }

    private static void AddDraculaFinish(Texture2D texture, int direction, int frame)
    {
        int step = frame == 0 ? -1 : 1;

        if (direction == 0)
        {
            // Narrower face, harsher hairline, high collar, and a walking cape hem.
            FillRect(texture, 16, 60, 8, 2, Ink);
            Set(texture, 19, 59, Ink);
            Set(texture, 20, 58, Ink);
            Set(texture, 21, 59, Ink);
            Set(texture, 15, 57, Ink);
            Set(texture, 24, 57, Ink);
            DrawLine(texture, 17, 55, 19, 55, Ink);
            DrawLine(texture, 21, 55, 23, 55, Ink);
            Set(texture, 18, 53, Red);
            Set(texture, 22, 53, Red);
            DrawLine(texture, 18, 51, 22, 51, new Color32(96, 74, 68, 255));
            Set(texture, 16, 52, new Color32(177, 170, 158, 255));

            FillPolygon(texture, new Vector2[] { new Vector2(12, 46), new Vector2(16, 50), new Vector2(17, 43), new Vector2(14, 41) }, White);
            DrawPolygon(texture, new Vector2[] { new Vector2(12, 46), new Vector2(16, 50), new Vector2(17, 43), new Vector2(14, 41) }, Ink);
            FillPolygon(texture, new Vector2[] { new Vector2(28, 46), new Vector2(24, 50), new Vector2(23, 43), new Vector2(26, 41) }, White);
            DrawPolygon(texture, new Vector2[] { new Vector2(28, 46), new Vector2(24, 50), new Vector2(23, 43), new Vector2(26, 41) }, Ink);
            DrawLine(texture, 10, 38, 16, 45, new Color32(154, 17, 35, 255));
            DrawLine(texture, 30, 38, 24, 45, new Color32(60, 5, 16, 255));
            DrawLine(texture, 16, 23, 16, 48, new Color32(120, 13, 29, 255));
            DrawLine(texture, 24, 23, 24, 48, new Color32(41, 4, 13, 255));
            DrawLine(texture, 12 + step, 7, 20, 4, new Color32(132, 12, 29, 255));
            DrawLine(texture, 28 + step, 7, 20, 4, Ink);
        }
        else if (direction == 1)
        {
            // Rear view: stronger cloak folds and moving hem, but no face detail.
            FillRect(texture, 17, 45, 6, 7, new Color32(18, 18, 24, 255));
            DrawLine(texture, 17, 51, 20, 55, Ink);
            DrawLine(texture, 23, 51, 20, 55, Ink);
            DrawLine(texture, 10, 37, 17, 44, new Color32(150, 16, 34, 255));
            DrawLine(texture, 30, 37, 23, 44, new Color32(55, 5, 15, 255));
            DrawLine(texture, 15, 20, 17, 52, new Color32(118, 13, 29, 255));
            DrawLine(texture, 25, 20, 23, 52, new Color32(37, 4, 12, 255));
            DrawLine(texture, 13 + step, 7, 20, 3, new Color32(132, 12, 29, 255));
            DrawLine(texture, 27 + step, 7, 20, 3, Ink);
        }
        else
        {
            // Profile: severe nose/eye, sideburn shape, shoulder spike, and cape trail.
            FillRect(texture, 17, 60, 9, 2, Ink);
            Set(texture, 21, 59, Ink);
            Set(texture, 22, 58, Ink);
            Set(texture, 17, 57, Ink);
            Set(texture, 25, 55, Red);
            Set(texture, 27, 53, Pale);
            DrawLine(texture, 18, 56, 24, 56, Ink);
            DrawLine(texture, 21, 50, 26, 50, new Color32(96, 74, 68, 255));
            FillPolygon(texture, new Vector2[] { new Vector2(15, 47), new Vector2(19, 51), new Vector2(20, 43), new Vector2(17, 41) }, White);
            DrawPolygon(texture, new Vector2[] { new Vector2(15, 47), new Vector2(19, 51), new Vector2(20, 43), new Vector2(17, 41) }, Ink);
            DrawLine(texture, 10, 39, 17, 46, new Color32(154, 17, 35, 255));
            DrawLine(texture, 32, 38, 25, 45, new Color32(54, 5, 15, 255));
            DrawLine(texture, 14, 28, 15, 52, new Color32(124, 13, 29, 255));
            DrawLine(texture, 30 + step, 8, 20, 3, Ink);
            DrawLine(texture, 12 + step, 8, 20, 3, new Color32(132, 12, 29, 255));
        }
    }

    private static void DrawDraculaDown(Texture2D texture, int frame)
    {
        int step = frame == 0 ? -1 : 1;
        FillRect(texture, 15 + step, 5, 4, 20, Ink);
        FillRect(texture, 21 - step, 5, 4, 20, Ink);
        FillRect(texture, 12 + step, 3, 8, 4, Ink);
        FillRect(texture, 21 - step, 3, 8, 4, Ink);
        Set(texture, 16 + step, 6, new Color32(39, 32, 34, 255));
        Set(texture, 23 - step, 6, new Color32(39, 32, 34, 255));

        Vector2[] cape = new Vector2[]
        {
            new Vector2(5, 5),
            new Vector2(35, 5),
            new Vector2(33, 36),
            new Vector2(29, 53),
            new Vector2(24, 44),
            new Vector2(21, 37),
            new Vector2(20, 34),
            new Vector2(19, 37),
            new Vector2(16, 44),
            new Vector2(11, 53),
            new Vector2(7, 36)
        };
        FillPolygon(texture, cape, RedDark);
        DrawPolygon(texture, cape, Ink);
        DrawLine(texture, 7, 7, 9, 35, new Color32(149, 16, 34, 255));
        DrawLine(texture, 33, 7, 31, 35, new Color32(62, 5, 16, 255));
        DrawLine(texture, 11, 52, 20, 34, Ink);
        DrawLine(texture, 29, 52, 20, 34, Ink);
        FillPolygon(texture, new Vector2[]
        {
            new Vector2(8, 35),
            new Vector2(13, 54),
            new Vector2(18, 43),
            new Vector2(20, 37),
            new Vector2(22, 43),
            new Vector2(27, 54),
            new Vector2(32, 35),
            new Vector2(25, 38),
            new Vector2(20, 35),
            new Vector2(15, 38)
        }, new Color32(126, 13, 29, 255));
        DrawPolygon(texture, new Vector2[]
        {
            new Vector2(8, 35),
            new Vector2(13, 54),
            new Vector2(18, 43),
            new Vector2(20, 37),
            new Vector2(22, 43),
            new Vector2(27, 54),
            new Vector2(32, 35)
        }, Ink);

        FillRect(texture, 16, 22, 9, 28, Ink);
        FillRect(texture, 19, 24, 3, 24, White);
        FillRect(texture, 18, 42, 5, 8, new Color32(226, 221, 202, 255));
        Set(texture, 20, 33, Red);
        Set(texture, 20, 34, Red);
        Set(texture, 17, 23, new Color32(94, 8, 21, 255));
        Set(texture, 23, 23, new Color32(94, 8, 21, 255));

        FillRect(texture, 15, 49, 10, 12, Pale);
        DrawRect(texture, 15, 49, 10, 12, Ink);
        FillRect(texture, 15, 60, 10, 2, Ink);
        FillRect(texture, 17, 62, 6, 1, Ink);
        Set(texture, 20, 63, Ink);
        Set(texture, 16, 59, Ink);
        Set(texture, 24, 59, Ink);
        Set(texture, 17, 54, Ink);
        Set(texture, 23, 54, Ink);
        Set(texture, 18, 53, Red);
        Set(texture, 22, 53, Red);
        Set(texture, 20, 51, new Color32(112, 81, 72, 255));
        DrawLine(texture, 17, 50, 23, 50, new Color32(181, 176, 163, 255));
        Set(texture, 18, 60, White);
        Set(texture, 19, 60, White);

        DrawLine(texture, 15, 48, 8, 37, Ink);
        DrawLine(texture, 25, 48, 32, 37, Ink);
        FillRect(texture, 9, 39, 5, 4, White);
        FillRect(texture, 26, 39, 5, 4, White);
        DrawLine(texture, 7, 8, 16, 3, new Color32(108, 10, 24, 255));
        DrawLine(texture, 33, 8, 24, 3, new Color32(108, 10, 24, 255));
    }

    private static void DrawDraculaUp(Texture2D texture, int frame)
    {
        int step = frame == 0 ? -1 : 1;
        FillRect(texture, 14 - step, 4, 5, 20, Ink);
        FillRect(texture, 21 + step, 4, 5, 20, Ink);
        FillRect(texture, 11 - step, 3, 8, 4, Ink);
        FillRect(texture, 21 + step, 3, 8, 4, Ink);

        Vector2[] cape = new Vector2[]
        {
            new Vector2(5, 5),
            new Vector2(35, 5),
            new Vector2(33, 37),
            new Vector2(28, 55),
            new Vector2(20, 61),
            new Vector2(12, 55),
            new Vector2(7, 37)
        };
        FillPolygon(texture, cape, new Color32(94, 8, 21, 255));
        DrawPolygon(texture, cape, Ink);
        DrawLine(texture, 7, 7, 9, 36, new Color32(145, 15, 33, 255));
        DrawLine(texture, 33, 7, 31, 36, new Color32(55, 5, 14, 255));
        FillPolygon(texture, new Vector2[]
        {
            new Vector2(10, 36),
            new Vector2(15, 52),
            new Vector2(20, 58),
            new Vector2(25, 52),
            new Vector2(30, 36),
            new Vector2(23, 40),
            new Vector2(17, 40)
        }, new Color32(126, 13, 29, 255));
        DrawPolygon(texture, new Vector2[]
        {
            new Vector2(10, 36),
            new Vector2(15, 52),
            new Vector2(20, 58),
            new Vector2(25, 52),
            new Vector2(30, 36),
            new Vector2(23, 40),
            new Vector2(17, 40)
        }, Ink);
        FillRect(texture, 16, 23, 9, 25, Ink);
        FillPolygon(texture, new Vector2[]
        {
            new Vector2(14, 47),
            new Vector2(26, 47),
            new Vector2(25, 56),
            new Vector2(20, 61),
            new Vector2(15, 56)
        }, Ink);
        FillRect(texture, 17, 45, 6, 6, new Color32(25, 24, 31, 255));
        Set(texture, 20, 55, new Color32(44, 37, 39, 255));
        Set(texture, 19, 54, new Color32(44, 37, 39, 255));
        DrawLine(texture, 8, 8, 17, 3, new Color32(116, 11, 26, 255));
        DrawLine(texture, 32, 8, 23, 3, new Color32(116, 11, 26, 255));
        DrawLine(texture, 10, 32, 16, 45, Ink);
        DrawLine(texture, 30, 32, 24, 45, Ink);
        FillRect(texture, 10, 34, 5, 4, White);
        FillRect(texture, 25, 34, 5, 4, White);
    }

    private static void DrawDraculaSide(Texture2D texture, int frame)
    {
        int step = frame == 0 ? -1 : 1;
        FillRect(texture, 15 + step, 4, 5, 20, Ink);
        FillRect(texture, 22 - step, 4, 5, 20, Ink);
        FillRect(texture, 14 + step, 3, 8, 4, Ink);
        FillRect(texture, 22 - step, 3, 7, 4, Ink);

        Vector2[] cape = new Vector2[]
        {
            new Vector2(6, 6),
            new Vector2(34, 5),
            new Vector2(32, 37),
            new Vector2(25, 55),
            new Vector2(11, 40)
        };
        FillPolygon(texture, cape, RedDark);
        DrawPolygon(texture, cape, Ink);
        DrawLine(texture, 8, 7, 11, 39, new Color32(150, 16, 34, 255));
        DrawLine(texture, 33, 7, 30, 36, new Color32(58, 6, 16, 255));
        FillPolygon(texture, new Vector2[]
        {
            new Vector2(11, 38),
            new Vector2(19, 56),
            new Vector2(26, 42),
            new Vector2(32, 37),
            new Vector2(24, 39)
        }, new Color32(129, 13, 29, 255));
        DrawLine(texture, 11, 38, 19, 56, Ink);
        DrawLine(texture, 19, 56, 26, 42, Ink);
        FillRect(texture, 17, 23, 9, 25, Ink);
        FillRect(texture, 22, 25, 3, 20, White);
        FillRect(texture, 16, 49, 12, 12, Pale);
        DrawRect(texture, 16, 49, 12, 12, Ink);
        FillRect(texture, 17, 60, 10, 2, Ink);
        FillRect(texture, 19, 62, 6, 1, Ink);
        Set(texture, 22, 63, Ink);
        Set(texture, 22, 59, new Color32(44, 37, 39, 255));
        Set(texture, 23, 58, new Color32(44, 37, 39, 255));
        Set(texture, 24, 54, Ink);
        Set(texture, 25, 54, Red);
        Set(texture, 27, 53, Pale);
        Set(texture, 23, 60, White);
        DrawLine(texture, 18, 49, 24, 49, new Color32(177, 173, 161, 255));
        DrawLine(texture, 16, 47, 8, 36, Ink);
        DrawLine(texture, 25, 47, 31, 38, Ink);
        FillRect(texture, 10, 38, 5, 4, White);
        FillRect(texture, 29, 39, 4, 4, White);
        Set(texture, 22, 49, Red);
        DrawLine(texture, 8, 8, 18, 3, new Color32(108, 10, 24, 255));
        DrawLine(texture, 34, 8, 24, 3, new Color32(108, 10, 24, 255));
    }

    private static void DrawArch(Texture2D texture, int centerX, int bottomY, int width, int height)
    {
        int radius = width / 2;
        int straightTop = bottomY + height - radius;
        for (int y = bottomY; y <= bottomY + height; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                bool inDoor = y <= straightTop && x >= centerX - radius + 3 && x <= centerX + radius - 3;
                if (!inDoor && y > straightTop)
                {
                    int dx = x - centerX;
                    int dy = y - straightTop;
                    inDoor = dx * dx + dy * dy <= radius * radius;
                }

                if (inDoor)
                {
                    Set(texture, x, y, DoorBlack);
                }
            }
        }

        DrawLine(texture, centerX - radius, bottomY, centerX - radius, straightTop, Ink);
        DrawLine(texture, centerX + radius, bottomY, centerX + radius, straightTop, Ink);
        for (int a = 0; a <= 180; a += 3)
        {
            float radians = a * Mathf.Deg2Rad;
            int x = Mathf.RoundToInt(centerX + Mathf.Cos(radians) * radius);
            int y = Mathf.RoundToInt(straightTop + Mathf.Sin(radians) * radius);
            Set(texture, x, y, Ink);
            Set(texture, x, y - 1, DoorGlow);
        }
    }

    private static void Clear(Texture2D texture)
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, ClearColor);
            }
        }
    }

    private static void Set(Texture2D texture, int x, int y, Color32 color)
    {
        if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
        {
            return;
        }

        texture.SetPixel(x, y, color);
    }

    private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color32 color)
    {
        for (int yy = y; yy < y + height; yy++)
        {
            for (int xx = x; xx < x + width; xx++)
            {
                Set(texture, xx, yy, color);
            }
        }
    }

    private static void DrawRect(Texture2D texture, int x, int y, int width, int height, Color32 color)
    {
        DrawLine(texture, x, y, x + width - 1, y, color);
        DrawLine(texture, x, y + height - 1, x + width - 1, y + height - 1, color);
        DrawLine(texture, x, y, x, y + height - 1, color);
        DrawLine(texture, x + width - 1, y, x + width - 1, y + height - 1, color);
    }

    private static void FillDiamond(Texture2D texture, int cx, int cy, int halfWidth, int halfHeight, Color32 color)
    {
        for (int y = cy - halfHeight; y <= cy + halfHeight; y++)
        {
            for (int x = cx - halfWidth; x <= cx + halfWidth; x++)
            {
                if (InsideDiamond(x, y, cx, cy, halfWidth, halfHeight))
                {
                    Set(texture, x, y, color);
                }
            }
        }
    }

    private static bool InsideDiamond(int x, int y, int cx, int cy, int halfWidth, int halfHeight)
    {
        float nx = Mathf.Abs(x - cx) / (float)halfWidth;
        float ny = Mathf.Abs(y - cy) / (float)halfHeight;
        return nx + ny <= 1f;
    }

    private static void DrawDiamond(Texture2D texture, int cx, int cy, int halfWidth, int halfHeight, Color32 color)
    {
        DrawLine(texture, cx, cy + halfHeight, cx + halfWidth, cy, color);
        DrawLine(texture, cx + halfWidth, cy, cx, cy - halfHeight, color);
        DrawLine(texture, cx, cy - halfHeight, cx - halfWidth, cy, color);
        DrawLine(texture, cx - halfWidth, cy, cx, cy + halfHeight, color);
    }

    private static void FillEllipse(Texture2D texture, int cx, int cy, int rx, int ry, Color32 color)
    {
        for (int y = cy - ry; y <= cy + ry; y++)
        {
            for (int x = cx - rx; x <= cx + rx; x++)
            {
                float nx = (x - cx) / (float)rx;
                float ny = (y - cy) / (float)ry;
                if (nx * nx + ny * ny <= 1f)
                {
                    Set(texture, x, y, color);
                }
            }
        }
    }

    private static void FillSoftEllipse(Texture2D texture, int cx, int cy, int rx, int ry, Color32 color, float falloffStart)
    {
        for (int y = cy - ry; y <= cy + ry; y++)
        {
            for (int x = cx - rx; x <= cx + rx; x++)
            {
                float nx = (x - cx) / (float)rx;
                float ny = (y - cy) / (float)ry;
                float distance = Mathf.Sqrt(nx * nx + ny * ny);
                if (distance <= 1f)
                {
                    float fade = distance <= falloffStart ? 1f : Mathf.Clamp01((1f - distance) / (1f - falloffStart));
                    Color32 faded = color;
                    faded.a = (byte)Mathf.RoundToInt(color.a * fade);
                    Set(texture, x, y, faded);
                }
            }
        }
    }

    private static void FillPolygon(Texture2D texture, Vector2[] points, Color32 color)
    {
        int minX = texture.width;
        int maxX = 0;
        int minY = texture.height;
        int maxY = 0;
        for (int i = 0; i < points.Length; i++)
        {
            minX = Mathf.Min(minX, Mathf.FloorToInt(points[i].x));
            maxX = Mathf.Max(maxX, Mathf.CeilToInt(points[i].x));
            minY = Mathf.Min(minY, Mathf.FloorToInt(points[i].y));
            maxY = Mathf.Max(maxY, Mathf.CeilToInt(points[i].y));
        }

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (PointInPolygon(new Vector2(x + 0.5f, y + 0.5f), points))
                {
                    Set(texture, x, y, color);
                }
            }
        }
    }

    private static void DrawPolygon(Texture2D texture, Vector2[] points, Color32 color)
    {
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % points.Length];
            DrawLine(texture, Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), Mathf.RoundToInt(b.x), Mathf.RoundToInt(b.y), color);
        }
    }

    private static bool PointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color32 color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int error = dx + dy;

        while (true)
        {
            Set(texture, x0, y0, color);
            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    private static void DrawLineClipped(Texture2D texture, int x0, int y0, int x1, int y1, Color32 color, Vector2[] clipPolygon)
    {
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int error = dx + dy;

        while (true)
        {
            if (PointInPolygon(new Vector2(x0 + 0.5f, y0 + 0.5f), clipPolygon))
            {
                Set(texture, x0, y0, color);
            }

            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }
}
