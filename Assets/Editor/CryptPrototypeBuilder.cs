using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CryptPrototypeBuilder
{
    private const string ScenePath = "Assets/Scenes/CryptPrototype.unity";
    private const string ArtFolder = "Assets/Art/SpectrumPrototype";
    private const string CharacterSheetFolder = "Assets/Art/Characters/Dracula/Sheets";
    private const string RoomBasePath = ArtFolder + "/spectrum_mockup_room_black_recess.png";
    private const string DoorFullRawPath = ArtFolder + "/spectrum_statue_door_full_raw.png";
    private const string ClassicWalkRightSheetPath = CharacterSheetFolder + "/dracula_classic_walk_right_sheet.png";
    private const string ClassicWalkLeftSheetPath = CharacterSheetFolder + "/dracula_classic_walk_left_sheet.png";
    private const string ClassicWalkDownSheetPath = CharacterSheetFolder + "/dracula_classic_walk_down_sheet.png";
    private const string SpectrumWalkRightSheetPath = CharacterSheetFolder + "/dracula_spectrum_walk_right_sheet.png";
    private const string SpectrumWalkLeftSheetPath = CharacterSheetFolder + "/dracula_spectrum_walk_left_sheet.png";
    private const string SpectrumWalkDownSheetPath = CharacterSheetFolder + "/dracula_spectrum_walk_down_sheet.png";
    private const int WalkRightFrameCount = 16;
    private const int WalkDownFrameCount = 14;
    private const int SheetColumns = 4;
    private const int SheetRows = 4;
    private const float RoomPpu = 180f;
    private const float CharacterPpu = 160f;
    private const float SpectrumSidePpu = 80f;
    private const int DoorCenterX = 1130;
    private const int DoorHalfWidth = 230;
    private const int DoorX = DoorCenterX - DoorHalfWidth;
    private const int DoorY = 94;
    private const int DoorWidth = DoorHalfWidth * 2;
    private const int DoorHeight = 755;
    private const int SpriteAlignmentCustom = 9;
    private const int SpriteMeshTypeFullRect = 0;

    [MenuItem("Dracula/Build Crypt Prototype")]
    public static void BuildCryptPrototype()
    {
        EnsureRequiredAssets();

        Sprite roomBase = LoadSprite(RoomBasePath, new Vector2(0.5f, 0.5f), RoomPpu);
        ConfigureSpriteImporter(DoorFullRawPath, new Vector2(0.5f, 0.5f), RoomPpu);
        Sprite[] walkRight = LoadSpriteSheetSequence(ClassicWalkRightSheetPath, "classic_walk_right", WalkRightFrameCount, SheetColumns, SheetRows, new Vector2(0.5f, 0f), CharacterPpu);
        Sprite[] walkLeft = LoadSpriteSheetSequence(ClassicWalkLeftSheetPath, "classic_walk_left", WalkRightFrameCount, SheetColumns, SheetRows, new Vector2(0.5f, 0f), CharacterPpu);
        Sprite[] walkDown = LoadSpriteSheetSequence(ClassicWalkDownSheetPath, "classic_walk_down", WalkDownFrameCount, SheetColumns, SheetRows, new Vector2(0.5f, 0f), CharacterPpu);
        Sprite[] walkUp = FirstFrame(walkDown);
        Sprite[] idleDown = FirstFrame(walkDown);
        Sprite[] idleUp = FirstFrame(walkDown);
        Sprite[] idleRight = FirstFrame(walkRight);
        Sprite[] idleLeft = FirstFrame(walkLeft);
        Sprite[] spectrumWalkRight = LoadSpriteSheetSequence(SpectrumWalkRightSheetPath, "spectrum_walk_right", WalkRightFrameCount, SheetColumns, SheetRows, new Vector2(0.5f, 0f), SpectrumSidePpu);
        Sprite[] spectrumWalkLeft = LoadSpriteSheetSequence(SpectrumWalkLeftSheetPath, "spectrum_walk_left", WalkRightFrameCount, SheetColumns, SheetRows, new Vector2(0.5f, 0f), SpectrumSidePpu);
        Sprite[] spectrumWalkDown = LoadSpriteSheetSequence(SpectrumWalkDownSheetPath, "spectrum_walk_down", WalkDownFrameCount, SheetColumns, SheetRows, new Vector2(0.5f, 0f), CharacterPpu);
        Sprite[] spectrumWalkUp = FirstFrame(spectrumWalkDown);
        Sprite[] spectrumIdleDown = FirstFrame(spectrumWalkDown);
        Sprite[] spectrumIdleUp = FirstFrame(spectrumWalkDown);
        Sprite[] spectrumIdleRight = FirstFrame(spectrumWalkRight);
        Sprite[] spectrumIdleLeft = FirstFrame(spectrumWalkLeft);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "CryptPrototype";

        GameObject root = new GameObject("Crypt Prototype");
        GameObject room = new GameObject("Isometric Crypt Room");
        room.transform.SetParent(root.transform);

        CreateCamera(roomBase);
        CreateLight();

        CreateSpriteObject("Room Base - Door Area Blacked Out", roomBase, Vector3.zero, 0, room.transform);

        Vector3 doorCenter = PixelCenterToWorld(DoorX + DoorWidth * 0.5f, DoorY + DoorHeight * 0.5f, roomBase);

        GameObject player = CreateDracula(
            walkRight,
            walkDown,
            walkUp,
            walkLeft,
            idleDown,
            idleUp,
            idleRight,
            idleLeft,
            spectrumWalkRight,
            spectrumWalkDown,
            spectrumWalkUp,
            spectrumWalkLeft,
            spectrumIdleDown,
            spectrumIdleUp,
            spectrumIdleRight,
            spectrumIdleLeft,
            doorCenter + new Vector3(-1.45f, -1.55f, 0f),
            root.transform);
        AdventureActor actor = player.GetComponent<AdventureActor>();

        Selection.activeGameObject = root;
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("Built Crypt prototype scene at " + ScenePath + " using actor " + actor.name + ".");
    }

    private static void EnsureRequiredAssets()
    {
        string[] requiredPaths =
        {
            RoomBasePath,
            DoorFullRawPath,
            ClassicWalkRightSheetPath,
            ClassicWalkLeftSheetPath,
            ClassicWalkDownSheetPath,
            SpectrumWalkRightSheetPath,
            SpectrumWalkLeftSheetPath,
            SpectrumWalkDownSheetPath
        };

        for (int i = 0; i < requiredPaths.Length; i++)
        {
            if (!File.Exists(requiredPaths[i]))
            {
                throw new FileNotFoundException("Missing required Spectrum prototype asset.", requiredPaths[i]);
            }
        }
    }

    private static GameObject CreateCamera(Sprite roomSprite)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographicSize = roomSprite.bounds.size.y * 0.54f;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.tag = "MainCamera";
        return cameraObject;
    }

    private static void CreateLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.8f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static GameObject CreateDracula(
        Sprite[] walkRight,
        Sprite[] walkDown,
        Sprite[] walkUp,
        Sprite[] walkLeft,
        Sprite[] idleDown,
        Sprite[] idleUp,
        Sprite[] idleRight,
        Sprite[] idleLeft,
        Sprite[] spectrumWalkRight,
        Sprite[] spectrumWalkDown,
        Sprite[] spectrumWalkUp,
        Sprite[] spectrumWalkLeft,
        Sprite[] spectrumIdleDown,
        Sprite[] spectrumIdleUp,
        Sprite[] spectrumIdleRight,
        Sprite[] spectrumIdleLeft,
        Vector3 position,
        Transform parent)
    {
        GameObject player = CreateSpriteObject("Spectrum Dracula Placeholder", walkRight[0], position, 40, parent);
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
        walker.spriteRenderer = player.GetComponent<SpriteRenderer>();
        walker.body = body;
        walker.walkDown = walkDown;
        walker.walkUp = walkUp;
        walker.walkRight = walkRight;
        walker.walkLeft = walkLeft;
        walker.idleDown = idleDown;
        walker.idleUp = idleUp;
        walker.idleSide = idleRight;
        walker.idleLeft = idleLeft;
        walker.moveSpeed = 2.25f;
        walker.sideFrameTime = 0.15f;
        walker.idleFrameTime = 0.24f;
        walker.baseSortingOrder = 280;
        walker.ySortMultiplier = 28f;
        walker.minSortingOrder = 180;
        walker.maxSortingOrder = 340;
        walker.minBounds = new Vector2(-6.2f, -2.8f);
        walker.maxBounds = new Vector2(6.2f, 0.35f);

        DraculaSpriteStyleSwitcher styleSwitcher = player.AddComponent<DraculaSpriteStyleSwitcher>();
        styleSwitcher.walker = walker;
        styleSwitcher.spriteRenderer = walker.spriteRenderer;
        AssignSpriteSet(styleSwitcher.classic, walkDown, walkUp, walkRight, walkLeft, idleDown, idleUp, idleRight, idleLeft);
        AssignSpriteSet(styleSwitcher.spectrumInspired, spectrumWalkDown, spectrumWalkUp, spectrumWalkRight, spectrumWalkLeft, spectrumIdleDown, spectrumIdleUp, spectrumIdleRight, spectrumIdleLeft);
        styleSwitcher.useSpectrumInspired = false;
        styleSwitcher.ApplySelectedStyle();

        AdventureActor actor = player.AddComponent<AdventureActor>();
        actor.character = AdventureCharacter.Dracula;
        actor.roomName = "Crypt Room";
        actor.walker = walker;
        actor.spriteRenderer = walker.spriteRenderer;

        return player;
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

    private static GameObject CreateSpriteObject(string name, Sprite sprite, Vector3 position, int sortingOrder, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        renderer.sortingLayerName = "Default";
        return obj;
    }

    private static Vector3 PixelCenterToWorld(float pixelX, float pixelY, Sprite roomSprite)
    {
        Texture2D texture = roomSprite.texture;
        float x = (pixelX - texture.width * 0.5f) / RoomPpu;
        float y = (texture.height * 0.5f - pixelY) / RoomPpu;
        return new Vector3(x, y, 0f);
    }

    private static Sprite LoadSprite(string path, Vector2 pivot, float pixelsPerUnit, FilterMode filterMode = FilterMode.Point)
    {
        ConfigureSpriteImporter(path, pivot, pixelsPerUnit, filterMode);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite[] LoadSpriteSheetSequence(
        string path,
        string spriteNamePrefix,
        int frameCount,
        int columns,
        int rows,
        Vector2 pivot,
        float pixelsPerUnit)
    {
        ConfigureSpriteSheetImporter(path, spriteNamePrefix, frameCount, columns, rows, pivot, pixelsPerUnit);

        Dictionary<string, Sprite> spritesByName = new Dictionary<string, Sprite>();
        Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        for (int i = 0; i < assets.Length; i++)
        {
            Sprite sprite = assets[i] as Sprite;
            if (sprite != null)
            {
                spritesByName[sprite.name] = sprite;
            }
        }

        Sprite[] sprites = new Sprite[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            string spriteName = spriteNamePrefix + "_" + i.ToString("00");
            if (!spritesByName.TryGetValue(spriteName, out sprites[i]) || sprites[i] == null)
            {
                throw new FileNotFoundException("Unity failed to import required Spectrum Dracula sheet sprite " + spriteName + ".", path);
            }
        }

        return sprites;
    }

    private static Sprite[] FirstFrame(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0 || sprites[0] == null)
        {
            return new Sprite[0];
        }

        return new[] { sprites[0] };
    }

    private static void ConfigureSpriteImporter(string path, Vector2 pivot, float pixelsPerUnit, FilterMode filterMode = FilterMode.Point)
    {
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new FileNotFoundException("Missing required sprite asset.", path);
        }

        bool changed = importer.textureType != TextureImporterType.Sprite
            || importer.spriteImportMode != SpriteImportMode.Single
            || importer.mipmapEnabled
            || importer.filterMode != filterMode
            || importer.textureCompression != TextureImporterCompression.Uncompressed
            || importer.spritePixelsPerUnit != pixelsPerUnit
            || importer.spritePivot != pivot;

        if (changed)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = filterMode;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.spritePivot = pivot;
            importer.alphaIsTransparency = true;
        }

        ApplySpriteImporterSerialization(importer);
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
    }

    private static void ConfigureSpriteSheetImporter(
        string path,
        string spriteNamePrefix,
        int frameCount,
        int columns,
        int rows,
        Vector2 pivot,
        float pixelsPerUnit,
        FilterMode filterMode = FilterMode.Point)
    {
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new FileNotFoundException("Missing required sprite sheet asset.", path);
        }

        int width;
        int height;
        importer.GetSourceTextureWidthAndHeight(out width, out height);
        int cellWidth = width / columns;
        int cellHeight = height / rows;
        SpriteMetaData[] metadata = new SpriteMetaData[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            int column = i % columns;
            int row = i / columns;
            SpriteMetaData spriteMetaData = new SpriteMetaData();
            spriteMetaData.name = spriteNamePrefix + "_" + i.ToString("00");
            spriteMetaData.alignment = SpriteAlignmentCustom;
            spriteMetaData.pivot = pivot;
            spriteMetaData.rect = new Rect(column * cellWidth, height - ((row + 1) * cellHeight), cellWidth, cellHeight);
            metadata[i] = spriteMetaData;
        }

        bool changed = importer.textureType != TextureImporterType.Sprite
            || importer.spriteImportMode != SpriteImportMode.Multiple
            || importer.mipmapEnabled
            || importer.filterMode != filterMode
            || importer.textureCompression != TextureImporterCompression.Uncompressed
            || importer.spritePixelsPerUnit != pixelsPerUnit;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false;
        importer.filterMode = filterMode;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.alphaIsTransparency = true;
        importer.spritesheet = metadata;

        ApplySpriteImporterSerialization(importer);
        EditorUtility.SetDirty(importer);
        if (changed)
        {
            importer.SaveAndReimport();
        }
        else
        {
            importer.SaveAndReimport();
        }
    }

    private static void ApplySpriteImporterSerialization(TextureImporter importer)
    {
        SerializedObject serializedImporter = new SerializedObject(importer);
        SetSerializedInt(serializedImporter, "m_Alignment", SpriteAlignmentCustom);
        SetSerializedInt(serializedImporter, "m_SpriteMeshType", SpriteMeshTypeFullRect);
        serializedImporter.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerializedInt(SerializedObject serializedObject, string propertyPath, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyPath);
        if (property != null)
        {
            property.intValue = value;
        }
    }
}
