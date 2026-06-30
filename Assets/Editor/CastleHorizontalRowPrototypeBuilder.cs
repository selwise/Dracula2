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
    private const string CharacterSheetPath = "Assets/Art/Characters/Dracula/Sheets/dracula_spectrum_walk_down_sheet.png";
    private const string CandleFlamePrefabPath = "Assets/Prefabs/CandleFlame.prefab";
    private const string SpriteLitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";
    private const string SpriteUnlitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
    private const float CharacterScale = 0.88f;
    private const int SpriteAlignmentCenter = 0;

    [MenuItem("Dracula/Build Castle East Gallery Prototype")]
    public static void BuildCastleEastGalleryPrototype()
    {
        Sprite pixel = EnsureBlockoutPixel();
        Sprite[] draculaFrames = LoadDraculaFrames();
        Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteLitMaterialPath);
        Material unlitMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteUnlitMaterialPath);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "CastleProperEastGalleryPrototype";

        GameObject root = new GameObject("Castle Proper East Gallery Prototype");
        GameObject row = new GameObject("Horizontal Row - Upper East");
        row.transform.SetParent(root.transform);

        GameObject cameraObject = CreateCamera(root.transform);
        CreateReferenceLight(root.transform);
        CreateGlobal2DLight(root.transform);

        Transform background = CreateChild(row.transform, "Painted Blockout");
        Transform colliders = CreateChild(row.transform, "Walk Boundaries");
        Transform propLayer = CreateChild(row.transform, "Props");
        Transform foreground = CreateChild(row.transform, "Foreground Parallax Pillars");
        HorizontalParallaxLayer parallax = foreground.gameObject.AddComponent<HorizontalParallaxLayer>();
        parallax.cameraTransform = cameraObject.transform;
        parallax.screenTravelFactor = 1.16f;

        BuildLongGalleryShell(pixel, background, litMaterial);
        BuildRightmostRoomDetail(pixel, background, propLayer, foreground, litMaterial, unlitMaterial);
        BuildWalkBoundaries(pixel, colliders);

        GameObject dracula = CreateDracula(draculaFrames, litMaterial, unlitMaterial, root.transform);
        HorizontalRowCameraFollow follow = cameraObject.AddComponent<HorizontalRowCameraFollow>();
        follow.target = dracula.transform;
        follow.cameraMin = CastleRoomLayout.CastleProperEastGalleryCameraMin;
        follow.cameraMax = CastleRoomLayout.CastleProperEastGalleryCameraMax;
        follow.offset = new Vector3(0f, 1.34f, -10f);
        follow.followSpeed = 13f;
        follow.SnapToTarget();
        parallax.CaptureAnchors();

        CreateAtmosphere(root.transform, pixel);
        Selection.activeGameObject = root;
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("Built castle horizontal row prototype at " + ScenePath + ".");
    }

    private static void BuildLongGalleryShell(Sprite pixel, Transform parent, Material litMaterial)
    {
        CreateRect("Full Gallery Darkness", pixel, new Vector3(-5f, 0.05f, 0f), new Vector2(27.2f, 8.2f), new Color(0.015f, 0.017f, 0.024f, 1f), -40, parent, null);
        CreateRect("Back Stone Wall", pixel, new Vector3(-5f, 1.72f, 0f), new Vector2(27.2f, 3.62f), new Color(0.23f, 0.235f, 0.255f, 1f), -30, parent, litMaterial);
        CreateRect("Upper Wall Shadow", pixel, new Vector3(-5f, 3.28f, 0f), new Vector2(27.2f, 0.58f), new Color(0.075f, 0.07f, 0.085f, 1f), -20, parent, litMaterial);
        CreateRect("Back Walkable Ledge", pixel, new Vector3(-5f, 0.42f, 0f), new Vector2(27.2f, 0.62f), new Color(0.12f, 0.115f, 0.13f, 1f), -18, parent, litMaterial);
        CreateRect("Floor Base", pixel, new Vector3(-5f, -1.58f, 0f), new Vector2(27.2f, 3.98f), new Color(0.29f, 0.285f, 0.27f, 1f), -10, parent, litMaterial);
        CreateRect("Floor Near Edge Shadow", pixel, new Vector3(-5f, -3.18f, 0f), new Vector2(27.2f, 0.48f), new Color(0.095f, 0.088f, 0.085f, 1f), -7, parent, litMaterial);
        CreateRect("Left Continuation Shade", pixel, new Vector3(-12.3f, -0.72f, 0f), new Vector2(10.9f, 5.55f), new Color(0.045f, 0.048f, 0.058f, 0.72f), -4, parent, null);

        for (int i = 0; i < 9; i++)
        {
            float x = -17f + i * 3.05f;
            Color stripeColor = i % 2 == 0
                ? new Color(0.24f, 0.235f, 0.24f, 1f)
                : new Color(0.19f, 0.188f, 0.2f, 1f);
            CreateRect("Wall Stone Bay " + i, pixel, new Vector3(x, 1.72f, 0f), new Vector2(0.08f, 3.06f), stripeColor, -12, parent, litMaterial);
        }

        for (int i = 0; i < 11; i++)
        {
            float y = -2.94f + i * 0.36f;
            Color seam = new Color(0.13f, 0.125f, 0.13f, Mathf.Lerp(0.2f, 0.46f, i / 10f));
            CreateRect("Floor Horizontal Joint " + i, pixel, new Vector3(-5f, y, 0f), new Vector2(27.2f, 0.028f), seam, -2, parent, null);
        }

        for (int i = 0; i < 14; i++)
        {
            float x = -18.2f + i * 2.0f;
            CreateRect("Floor Vertical Joint " + i, pixel, new Vector3(x, -1.55f, 0f), new Vector2(0.024f, 3.05f), new Color(0.12f, 0.115f, 0.12f, 0.32f), -1, parent, null);
        }
    }

    private static void BuildRightmostRoomDetail(Sprite pixel, Transform background, Transform propLayer, Transform foreground, Material litMaterial, Material unlitMaterial)
    {
        CreateRect("Left Arch Opening Darkness", pixel, new Vector3(-6.2f, 0.48f, 0f), new Vector2(2.18f, 4.72f), new Color(0.005f, 0.006f, 0.012f, 1f), 2, background, null);
        CreateRect("Left Arch Threshold", pixel, new Vector3(-6.2f, -2.72f, 0f), new Vector2(2.72f, 0.34f), new Color(0.38f, 0.36f, 0.32f, 1f), 3, background, litMaterial);
        CreateRect("Left Arch Left Pier", pixel, new Vector3(-7.58f, 0.22f, 0f), new Vector2(0.46f, 4.62f), new Color(0.34f, 0.33f, 0.35f, 1f), 9, background, litMaterial);
        CreateRect("Left Arch Right Pier", pixel, new Vector3(-4.82f, 0.22f, 0f), new Vector2(0.46f, 4.62f), new Color(0.34f, 0.33f, 0.35f, 1f), 9, background, litMaterial);
        CreateRect("Left Arch Head", pixel, new Vector3(-6.2f, 2.56f, 0f), new Vector2(3.22f, 0.64f), new Color(0.37f, 0.36f, 0.38f, 1f), 10, background, litMaterial);
        CreateRect("Left Arch Inner Lip", pixel, new Vector3(-6.2f, 1.92f, 0f), new Vector2(2.08f, 0.28f), new Color(0.17f, 0.165f, 0.19f, 1f), 11, background, litMaterial);

        CreateRect("Right Blocking Wall Mass", pixel, new Vector3(7.92f, 0.28f, 0f), new Vector2(1.22f, 6.5f), new Color(0.22f, 0.215f, 0.235f, 1f), 18, background, litMaterial);
        CreateRect("Right Wall Black Return", pixel, new Vector3(8.46f, -0.5f, 0f), new Vector2(0.36f, 5.1f), new Color(0.018f, 0.018f, 0.026f, 1f), 19, background, null);
        CreateRect("Right Wall Face Highlight", pixel, new Vector3(7.38f, 0.2f, 0f), new Vector2(0.18f, 5.75f), new Color(0.42f, 0.39f, 0.35f, 1f), 20, background, litMaterial);
        CreateRect("Right Wall Foot Block", pixel, new Vector3(7.26f, -2.64f, 0f), new Vector2(1.16f, 0.48f), new Color(0.31f, 0.29f, 0.27f, 1f), 25, background, litMaterial);

        for (int i = 0; i < 3; i++)
        {
            float x = -1.8f + i * 3.1f;
            CreateRect("Back Blind Arch Shadow " + i, pixel, new Vector3(x, 1.14f, 0f), new Vector2(1.42f, 2.36f), new Color(0.075f, 0.074f, 0.09f, 1f), 4, background, null);
            CreateRect("Back Blind Arch Cap " + i, pixel, new Vector3(x, 2.38f, 0f), new Vector2(1.72f, 0.28f), new Color(0.38f, 0.365f, 0.35f, 1f), 7, background, litMaterial);
            CreateRect("Back Blind Arch Left Rib " + i, pixel, new Vector3(x - 0.86f, 1.1f, 0f), new Vector2(0.18f, 2.48f), new Color(0.33f, 0.315f, 0.32f, 1f), 8, background, litMaterial);
            CreateRect("Back Blind Arch Right Rib " + i, pixel, new Vector3(x + 0.86f, 1.1f, 0f), new Vector2(0.18f, 2.48f), new Color(0.33f, 0.315f, 0.32f, 1f), 8, background, litMaterial);
        }

        CreateRect("Depth Ramp Left Edge", pixel, new Vector3(-3.2f, -0.14f, 0f), new Vector2(4.3f, 0.08f), new Color(0.43f, 0.405f, 0.35f, 0.7f), 5, background, null);
        CreateRect("Depth Ramp Right Edge", pixel, new Vector3(2.7f, -0.14f, 0f), new Vector2(4.3f, 0.08f), new Color(0.43f, 0.405f, 0.35f, 0.7f), 5, background, null);

        CreateRect("Right Room Foreground Ledge Left", pixel, new Vector3(-1.15f, -3.35f, 0f), new Vector2(7.4f, 0.36f), new Color(0.11f, 0.102f, 0.1f, 0.95f), 360, foreground, null);
        CreateRect("Right Room Foreground Ledge Right", pixel, new Vector3(5.75f, -3.35f, 0f), new Vector2(2.4f, 0.36f), new Color(0.11f, 0.102f, 0.1f, 0.95f), 360, foreground, null);

        CreateForegroundPillar(pixel, new Vector3(-2.85f, -1.42f, 0f), 1.12f, foreground, litMaterial);
        CreateForegroundPillar(pixel, new Vector3(4.9f, -1.2f, 0f), 1.22f, foreground, litMaterial);

        CreateRect("Small Wall Sconce Marker Left", pixel, new Vector3(-0.58f, 1.18f, 0f), new Vector2(0.16f, 0.38f), new Color(0.47f, 0.33f, 0.18f, 1f), 52, propLayer, litMaterial);
        CreateRect("Small Wall Sconce Marker Right", pixel, new Vector3(3.08f, 1.18f, 0f), new Vector2(0.16f, 0.38f), new Color(0.47f, 0.33f, 0.18f, 1f), 52, propLayer, litMaterial);
        CreateWarmGlow(pixel, new Vector3(-0.58f, 1.28f, 0f), propLayer, unlitMaterial);
        CreateWarmGlow(pixel, new Vector3(3.08f, 1.28f, 0f), propLayer, unlitMaterial);
        TryPlaceCandleFlame(new Vector3(-0.58f, 1.16f, 0f), propLayer);
        TryPlaceCandleFlame(new Vector3(3.08f, 1.16f, 0f), propLayer);
    }

    private static void CreateForegroundPillar(Sprite pixel, Vector3 position, float scale, Transform parent, Material litMaterial)
    {
        CreateRect("Foreground Pillar Shadow " + position.x.ToString("0.0"), pixel, position + new Vector3(0.24f, -0.08f, 0f), new Vector2(0.86f * scale, 4.92f * scale), new Color(0.018f, 0.018f, 0.024f, 0.88f), 390, parent, null);
        CreateRect("Foreground Pillar Body " + position.x.ToString("0.0"), pixel, position, new Vector2(0.74f * scale, 4.8f * scale), new Color(0.25f, 0.245f, 0.265f, 1f), 400, parent, litMaterial);
        CreateRect("Foreground Pillar Highlight " + position.x.ToString("0.0"), pixel, position + new Vector3(-0.24f * scale, 0f, 0f), new Vector2(0.09f * scale, 4.62f * scale), new Color(0.48f, 0.45f, 0.39f, 0.9f), 401, parent, litMaterial);
        CreateRect("Foreground Pillar Base " + position.x.ToString("0.0"), pixel, position + new Vector3(0f, -2.52f * scale, 0f), new Vector2(1.34f * scale, 0.48f * scale), new Color(0.18f, 0.17f, 0.18f, 1f), 405, parent, litMaterial);
        CreateRect("Foreground Pillar Capital " + position.x.ToString("0.0"), pixel, position + new Vector3(0f, 2.46f * scale, 0f), new Vector2(1.18f * scale, 0.42f * scale), new Color(0.31f, 0.295f, 0.29f, 1f), 405, parent, litMaterial);
    }

    private static void CreateWarmGlow(Sprite pixel, Vector3 position, Transform parent, Material unlitMaterial)
    {
        CreateRect("Warm Sconce Glow " + position.x.ToString("0.0"), pixel, position, new Vector2(1.45f, 1.15f), new Color(1f, 0.55f, 0.2f, 0.12f), 48, parent, unlitMaterial);
    }

    private static void BuildWalkBoundaries(Sprite pixel, Transform parent)
    {
        CreateCollider("Back Walk Limit", new Vector2(-5.05f, 1.02f), new Vector2(25.1f, 0.36f), parent);
        CreateCollider("Front Walk Limit", new Vector2(-5.05f, -3.28f), new Vector2(25.1f, 0.38f), parent);
        CreateCollider("Right End Wall Collider", new Vector2(7.72f, -1.04f), new Vector2(0.55f, 4.55f), parent);
        CreateTriggerMarker("Future Upper Row Transition Slot", pixel, new Vector3(0.98f, 0.66f, 0f), new Vector2(1.28f, 0.16f), parent);
    }

    private static GameObject CreateCamera(Transform parent)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.transform.SetParent(parent);
        cameraObject.transform.position = new Vector3(0f, -0.2f, -10f);
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 4.72f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
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
        SetFloatProperty(light, lightType, "intensity", 0.31f);
        SetColorProperty(light, lightType, "color", new Color(0.58f, 0.6f, 0.72f, 1f));
    }

    private static void CreateAtmosphere(Transform parent, Sprite dustSprite)
    {
        GameObject dustObject = new GameObject("Castle Hall Dust");
        dustObject.transform.SetParent(parent);
        RoomDustParticles dust = dustObject.AddComponent<RoomDustParticles>();
        dust.particleCount = 90;
        dust.particleSize = 0.55f;
        dust.baseBrightness = 0.16f;
        dust.brightnessVariation = 0.12f;
        dust.driftSpeed = 0.08f;
        dust.driftAmplitude = 0.12f;
        dust.verticalRiseSpeed = 0.015f;
        dust.boundsMin = new Vector2(-17.5f, -2.75f);
        dust.boundsMax = new Vector2(7.2f, 2.05f);
        dust.sortingLayerName = "Default";
        dust.sortingOrder = 46;
        dust.dustFrames = new[] { dustSprite };
    }

    private static GameObject CreateDracula(Sprite[] frames, Material litMaterial, Material unlitMaterial, Transform parent)
    {
        Sprite startupSprite = frames.Length > 0 ? frames[0] : null;
        GameObject player = new GameObject("Spectrum Dracula Placeholder");
        player.transform.SetParent(parent);
        player.transform.position = new Vector3(CastleRoomLayout.CastleProperEastGalleryStart.x, CastleRoomLayout.CastleProperEastGalleryStart.y, 0f);
        player.transform.localScale = Vector3.one * CharacterScale;

        SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = startupSprite;
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

        Sprite[] idle = FirstFrame(frames);
        DraculaWalker walker = player.AddComponent<DraculaWalker>();
        walker.spriteRenderer = renderer;
        walker.body = body;
        walker.walkDown = frames;
        walker.walkUp = idle;
        walker.walkRight = frames;
        walker.walkLeft = new Sprite[0];
        walker.idleDown = idle;
        walker.idleUp = idle;
        walker.idleSide = idle;
        walker.idleLeft = new Sprite[0];
        walker.moveSpeed = 2.35f;
        walker.frameTime = 0.12f;
        walker.sideFrameTime = 0.12f;
        walker.idleFrameTime = 0.24f;
        walker.baseSortingOrder = 278;
        walker.ySortMultiplier = 30f;
        walker.minSortingOrder = 180;
        walker.maxSortingOrder = 345;
        walker.minBounds = CastleRoomLayout.CastleProperEastGalleryMinBounds;
        walker.maxBounds = CastleRoomLayout.CastleProperEastGalleryMaxBounds;

        DraculaSpriteStyleSwitcher styleSwitcher = player.AddComponent<DraculaSpriteStyleSwitcher>();
        styleSwitcher.walker = walker;
        styleSwitcher.spriteRenderer = renderer;
        AssignSpriteSet(styleSwitcher.classic, frames, idle, frames, new Sprite[0], idle, idle, idle, new Sprite[0]);
        AssignSpriteSet(styleSwitcher.spectrumInspired, frames, idle, frames, new Sprite[0], idle, idle, idle, new Sprite[0]);
        styleSwitcher.useSpectrumInspired = false;

        AdventureActor actor = player.AddComponent<AdventureActor>();
        actor.character = AdventureCharacter.Dracula;
        actor.roomName = CastleRoomLayout.CastleProperEastGalleryRoomName;
        actor.walker = walker;
        actor.spriteRenderer = renderer;

        CharacterReadabilityOverlay readability = player.AddComponent<CharacterReadabilityOverlay>();
        readability.sourceRenderer = renderer;
        readability.overlayMaterial = unlitMaterial;
        readability.alpha = 0.17f;
        readability.tint = new Color(0.84f, 0.88f, 0.96f, 1f);

        return player;
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

    private static Sprite[] LoadDraculaFrames()
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(CharacterSheetPath);
        List<Sprite> sprites = new List<Sprite>();
        for (int i = 0; i < assets.Length; i++)
        {
            Sprite sprite = assets[i] as Sprite;
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }

        sprites.Sort((a, b) => ExtractTrailingNumber(a.name).CompareTo(ExtractTrailingNumber(b.name)));
        if (sprites.Count == 0)
        {
            throw new FileNotFoundException("No Dracula sprites were found in the sliced sheet.", CharacterSheetPath);
        }

        return sprites.ToArray();
    }

    private static int ExtractTrailingNumber(string value)
    {
        int index = value.Length - 1;
        while (index >= 0 && char.IsDigit(value[index]))
        {
            index--;
        }

        if (index == value.Length - 1)
        {
            return 0;
        }

        int number;
        return int.TryParse(value.Substring(index + 1), out number) ? number : 0;
    }

    private static Sprite[] FirstFrame(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0 || sprites[0] == null)
        {
            return new Sprite[0];
        }

        return new[] { sprites[0] };
    }

    private static void AssignSpriteSet(
        DraculaSpriteStyleSwitcher.DraculaSpriteSet set,
        Sprite[] walkDown,
        Sprite[] walkUp,
        Sprite[] walkRight,
        Sprite[] walkLeft,
        Sprite[] idleDown,
        Sprite[] idleUp,
        Sprite[] idleSide,
        Sprite[] idleLeft)
    {
        set.walkDown = walkDown;
        set.walkUp = walkUp;
        set.walkRight = walkRight;
        set.walkLeft = walkLeft;
        set.idleDown = idleDown;
        set.idleUp = idleUp;
        set.idleSide = idleSide;
        set.idleLeft = idleLeft;
    }

    private static Transform CreateChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent);
        return child.transform;
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

    private static void CreateCollider(string name, Vector2 position, Vector2 size, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(position.x, position.y, 0f);
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.size = size;
    }

    private static void CreateTriggerMarker(string name, Sprite pixel, Vector3 position, Vector2 size, Transform parent)
    {
        GameObject obj = CreateRect(name, pixel, position, size, new Color(0.25f, 0.55f, 0.95f, 0.08f), 30, parent, null);
        obj.SetActive(false);
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = Vector2.one;
    }

    private static void TryPlaceCandleFlame(Vector3 position, Transform parent)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CandleFlamePrefabPath);
        if (prefab == null)
        {
            return;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            return;
        }

        instance.name = "Prototype Sconce Flame";
        instance.transform.SetParent(parent);
        instance.transform.position = position;
        instance.transform.localScale = Vector3.one * 0.58f;
    }

    private static void ConfigureSpriteImporter(string path, Vector2 pivot, float pixelsPerUnit)
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
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.spritePivot = pivot;
        importer.alphaIsTransparency = true;

        SerializedObject serializedImporter = new SerializedObject(importer);
        SerializedProperty alignmentProperty = serializedImporter.FindProperty("m_Alignment");
        if (alignmentProperty != null)
        {
            alignmentProperty.intValue = SpriteAlignmentCenter;
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
