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
    private const string RenfieldSpritePath = "Assets/Art/Characters/Renfield/Renfield.png";
    private const string WallSconcePrefabPath = "Assets/Prefabs/Castle/WallSconceCandle.prefab";
    private const string SpriteLitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";
    private const string SpriteUnlitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
    private const float CastleWallTilePpu = 240f;
    private const float ObliqueDoorwayPpu = 264f;
    private const float RenfieldPpu = 220f;
    private const int SpriteAlignmentCenter = 0;
    private const int SpriteAlignmentCustom = 9;

    private static readonly Vector3 CameraOffset = new Vector3(-3.72f, 1.32f, -10f);
    private static readonly CastleObliqueWallPlacementRule ObliqueDoorwayPlacementRule =
        CastleObliqueWallPlacementRule.CastleProperEastGalleryLeftReturn();

    [MenuItem("Dracula/Build Castle East Gallery Prototype")]
    public static void BuildCastleEastGalleryPrototype()
    {
        Sprite pixel = EnsureBlockoutPixel();
        Sprite castleWallTile = LoadCastleWallTile();
        Sprite obliqueDoorwaySprite = LoadObliqueDoorwaySprite();
        Sprite renfieldSprite = LoadRenfieldSprite();
        CastleWallSconcePrefabBuilder.EnsurePrefabExists();
        Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteLitMaterialPath);
        Material unlitMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteUnlitMaterialPath);

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

        BuildCorridorBase(pixel, castleWallTile, background, litMaterial);
        BuildSegmentBreak(obliqueDoorwaySprite, background, litMaterial);
        BuildWallSconceRun(props);
        BuildEastEndStop(pixel, background, props, litMaterial);

        GameObject renfield = CreateRenfield(renfieldSprite, litMaterial, unlitMaterial, root.transform);
        CreateGameOptions(renfield.GetComponent<AdventureActor>(), cameraObject.GetComponent<Camera>(), root.transform);
        SnapCameraToActor(cameraObject.transform, renfield.GetComponent<AdventureActor>());
        ConfigureLayoutInspector(root.transform);

        Selection.activeGameObject = root;
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("Built castle horizontal row prototype at " + ScenePath + ".");
    }

    private static void BuildCorridorBase(Sprite pixel, Sprite castleWallTile, Transform parent, Material litMaterial)
    {
        CreateRect("Hallway Deep Background", pixel, new Vector3(1.1f, 0.02f, 0f), new Vector2(20.7f, 7.95f), new Color(0.06f, 0.11f, 0.11f, 1f), -50, parent, null);
        CreateRect("Hallway Upper Warm Wall", pixel, new Vector3(1.1f, 2.04f, 0f), new Vector2(20.7f, 2.42f), new Color(0.38f, 0.27f, 0.16f, 1f), -40, parent, litMaterial);
        CreateRect("Hallway Top Shadow Cap", pixel, new Vector3(1.1f, 3.22f, 0f), new Vector2(20.7f, 0.32f), new Color(0.2f, 0.16f, 0.11f, 1f), -38, parent, litMaterial);
        CreateRect("Hallway Upper Cornice", pixel, new Vector3(1.1f, 2.68f, 0f), new Vector2(20.7f, 0.24f), new Color(0.48f, 0.34f, 0.18f, 1f), -37, parent, litMaterial);

        CreateRect("Hallway Middle Brown Field", pixel, new Vector3(1.1f, 0.88f, 0f), new Vector2(20.7f, 1.82f), new Color(0.31f, 0.22f, 0.14f, 1f), -35, parent, litMaterial);
        CreateRect("Hallway Warm Horizontal Band", pixel, new Vector3(1.1f, 0.28f, 0f), new Vector2(20.7f, 0.26f), new Color(0.62f, 0.39f, 0.18f, 1f), -34, parent, litMaterial);
        CreateRect("Hallway Lower Green Wall", pixel, new Vector3(1.1f, -0.66f, 0f), new Vector2(20.7f, 1.14f), new Color(0.17f, 0.23f, 0.2f, 1f), -33, parent, litMaterial);
        CreateRect("Hallway Lower Dark Rail", pixel, new Vector3(1.1f, -1.18f, 0f), new Vector2(20.7f, 0.16f), new Color(0.07f, 0.06f, 0.06f, 1f), -32, parent, litMaterial);
        CreateRect("Hallway Red Rail Hairline", pixel, new Vector3(1.1f, -1.05f, 0f), new Vector2(20.7f, 0.035f), new Color(0.26f, 0.1f, 0.07f, 1f), -31, parent, litMaterial);

        CreateTiledSprite("Hallway Castle Gray Block Wall Tile", castleWallTile, new Vector3(-4.95f, 0.88f, 0f), new Vector2(26.4f, 4.12f), Color.white, -27, parent, litMaterial);

        CreateRect("Hallway Floor Base", pixel, new Vector3(1.1f, -2.78f, 0f), new Vector2(20.7f, 3.12f), new Color(0.25f, 0.2f, 0.13f, 1f), -30, parent, litMaterial);
        CreateRect("Hallway Floor Back Shadow", pixel, new Vector3(1.1f, -1.55f, 0f), new Vector2(20.7f, 0.32f), new Color(0.16f, 0.18f, 0.14f, 1f), -29, parent, litMaterial);
        CreateRect("Hallway Front Floor Lip", pixel, new Vector3(1.1f, -4.17f, 0f), new Vector2(20.7f, 0.38f), new Color(0.17f, 0.14f, 0.09f, 1f), -24, parent, litMaterial);
        CreateRect("Hallway Near Black Groove", pixel, new Vector3(1.1f, -3.9f, 0f), new Vector2(20.7f, 0.07f), new Color(0.055f, 0.048f, 0.038f, 1f), -23, parent, null);

        for (int i = 0; i < 9; i++)
        {
            float x = -6.8f + i * 1.92f;
            CreateRect("Back Wall Vertical Block " + i, pixel, new Vector3(x, 1.06f, 0f), new Vector2(0.04f, 3.55f), new Color(0.16f, 0.13f, 0.1f, 0.45f), -28, parent, null);
        }

        for (int i = 0; i < 16; i++)
        {
            float x = -7.18f + i * 1.12f;
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
        GameObject doorway = CreateSprite(
            "Open Arch Oblique Doorway Candidate",
            obliqueDoorwaySprite,
            CalculateObliqueDoorwayPosition(obliqueDoorwaySprite),
            ObliqueDoorwayPlacementRule.perspectiveScale,
            Color.white,
            22,
            background,
            litMaterial);

        ObliqueDoorwayPlacementRule.ApplyTo(doorway.transform, obliqueDoorwaySprite);
        CastleObliqueWallPlacement placement = doorway.AddComponent<CastleObliqueWallPlacement>();
        placement.Configure(doorway.GetComponent<SpriteRenderer>(), ObliqueDoorwayPlacementRule);
    }

    private static Vector3 CalculateObliqueDoorwayPosition(Sprite obliqueDoorwaySprite)
    {
        return ObliqueDoorwayPlacementRule.CalculatePosition(obliqueDoorwaySprite);
    }

    private static void BuildWallSconceRun(Transform props)
    {
        const float sconceY = 1.12f;
        const float scale = 0.94f;
        TryPlaceWallSconce(new Vector3(-12.2f, sconceY, 0f), props, scale);
        TryPlaceWallSconce(new Vector3(-6.25f, sconceY, 0f), props, scale);
        TryPlaceWallSconce(new Vector3(-0.3f, sconceY, 0f), props, scale);
        TryPlaceWallSconce(new Vector3(3.55f, sconceY, 0f), props, scale);
    }

    private static void BuildEastEndStop(Sprite pixel, Transform background, Transform props, Material litMaterial)
    {
        CreateRect("East End Dark Wall Face", pixel, new Vector3(7.42f, 0.02f, 0f), new Vector2(1.02f, 6.8f), new Color(0.03f, 0.14f, 0.14f, 1f), 24, background, litMaterial);
        CreateRect("East End Inner Brown Return", pixel, new Vector3(6.74f, 0.48f, 0f), new Vector2(0.44f, 4.9f), new Color(0.44f, 0.3f, 0.17f, 1f), 23, background, litMaterial);
        CreateRect("East End Black Side Shadow", pixel, new Vector3(6.99f, -0.1f, 0f), new Vector2(0.16f, 4.35f), new Color(0.02f, 0.024f, 0.024f, 1f), 25, background, null);
        CreateRect("East End Top Return Block", pixel, new Vector3(6.8f, 2.45f, 0f), new Vector2(0.88f, 0.9f), new Color(0.35f, 0.27f, 0.18f, 1f), 26, background, litMaterial);
        CreateRect("East End Lower Return Block", pixel, new Vector3(6.78f, -1.05f, 0f), new Vector2(0.82f, 0.5f), new Color(0.12f, 0.14f, 0.12f, 1f), 26, background, litMaterial);
        CreateRect("East End Floor Stop Shadow", pixel, new Vector3(6.46f, -2.38f, 0f), new Vector2(1.32f, 0.28f), new Color(0.09f, 0.08f, 0.055f, 1f), 46, background, null);
        CreateRect("Renfield Ground Shadow", pixel, new Vector3(CastleRoomLayout.CastleProperEastGalleryEndStart.x + 0.08f, -2.86f, 0f), new Vector2(0.95f, 0.13f), new Color(0.02f, 0.018f, 0.014f, 0.65f), 55, props, null);
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

    private static AdventureLoopController CreateGameOptions(AdventureActor renfield, Camera sceneCamera, Transform parent)
    {
        GameObject optionsObject = new GameObject("GameOptions");
        optionsObject.transform.SetParent(parent);

        GameOptions options = optionsObject.AddComponent<GameOptions>();
        options.startAsRenfield = true;

        AdventureLoopController loopController = optionsObject.AddComponent<AdventureLoopController>();
        loopController.gameOptions = options;
        loopController.startDayWithRenfield = true;
        loopController.dracula = null;
        loopController.renfield = renfield;
        loopController.sceneCamera = sceneCamera;
        loopController.cameraOffset = CameraOffset;
        loopController.cameraFollowSpeed = 13f;
        loopController.draculaSleepsDuringDay = false;
        loopController.allowEmergencyDayWake = false;
        loopController.renfieldStations = new AdventureActionStation[0];

        if (renfield != null)
        {
            renfield.ApplyControl(true, true);
        }

        return loopController;
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
        AddPlacement(placements, root, "Floor base", "Hallway Floor Base", true);
        AddPlacement(placements, root, "Floor back shadow", "Hallway Floor Back Shadow", true);
        AddPlacement(placements, root, "Front floor lip", "Hallway Front Floor Lip", true);
        AddPlacement(placements, root, "Near black floor groove", "Hallway Near Black Groove", true);

        AddPlacement(placements, root, "East end dark wall face", "East End Dark Wall Face", true);
        AddPlacement(placements, root, "East end brown return", "East End Inner Brown Return", true);
        AddPlacement(placements, root, "East end black side shadow", "East End Black Side Shadow", true);
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
