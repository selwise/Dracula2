using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SpectrumDoorPrototypeBuilder
{
    private const string ScenePath = "Assets/Scenes/SpectrumDoorPrototype.unity";
    private const string ArtFolder = "Assets/Art/SpectrumPrototype";
    private const string CharacterFolder = "Assets/Art/Characters/Dracula/SpectrumPlaceholder/WalkRight";
    private const string RoomBasePath = ArtFolder + "/spectrum_mockup_room_black_recess.png";
    private const string DoorFullRawPath = ArtFolder + "/spectrum_statue_door_full_raw.png";
    private const string WalkRightPrefix = CharacterFolder + "/walking_right_";
    private const int WalkRightFrameCount = 12;
    private const float RoomPpu = 180f;
    private const float CharacterPpu = 650f;
    private const int DoorCenterX = 1130;
    private const int DoorHalfWidth = 230;
    private const int DoorX = DoorCenterX - DoorHalfWidth;
    private const int DoorY = 94;
    private const int DoorWidth = DoorHalfWidth * 2;
    private const int DoorHeight = 755;
    private const int SpriteAlignmentCustom = 9;
    private const int SpriteMeshTypeFullRect = 0;

    [MenuItem("Dracula/Build Spectrum Door Prototype")]
    public static void BuildSpectrumDoorPrototype()
    {
        EnsureRequiredAssets();

        Sprite roomBase = LoadSprite(RoomBasePath, new Vector2(0.5f, 0.5f), RoomPpu);
        ConfigureSpriteImporter(DoorFullRawPath, new Vector2(0.5f, 0.5f), RoomPpu);
        Sprite[] walkRight = LoadSpriteSequence(WalkRightPrefix, WalkRightFrameCount, new Vector2(0.5f, 0f), CharacterPpu);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "SpectrumDoorPrototype";

        GameObject root = new GameObject("Spectrum Door Prototype");
        GameObject room = new GameObject("Mockup Room Plate");
        room.transform.SetParent(root.transform);

        CreateCamera(roomBase);
        CreateLight();

        CreateSpriteObject("Room Base - Door Area Blacked Out", roomBase, Vector3.zero, 0, room.transform);

        Vector3 doorCenter = PixelCenterToWorld(DoorX + DoorWidth * 0.5f, DoorY + DoorHeight * 0.5f, roomBase);

        GameObject player = CreateDracula(walkRight, doorCenter + new Vector3(-1.45f, -1.55f, 0f), root.transform);
        AdventureActor actor = player.GetComponent<AdventureActor>();

        Selection.activeGameObject = root;
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("Built doorless Spectrum prototype scene at " + ScenePath + " using actor " + actor.name + ".");
    }

    private static void EnsureRequiredAssets()
    {
        string[] requiredPaths =
        {
            RoomBasePath,
            DoorFullRawPath,
            WalkRightPrefix + "00.png"
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

    private static GameObject CreateDracula(Sprite[] walkRight, Vector3 position, Transform parent)
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
        walker.walkDown = walkRight;
        walker.walkUp = walkRight;
        walker.walkSide = walkRight;
        walker.idleDown = new[] { walkRight[0] };
        walker.idleUp = new[] { walkRight[0] };
        walker.idleSide = new[] { walkRight[0] };
        walker.moveSpeed = 2.25f;
        walker.sideFrameTime = 0.15f;
        walker.idleFrameTime = 0.24f;
        walker.baseSortingOrder = 280;
        walker.ySortMultiplier = 28f;
        walker.minSortingOrder = 180;
        walker.maxSortingOrder = 340;
        walker.minBounds = new Vector2(-6.2f, -2.8f);
        walker.maxBounds = new Vector2(6.2f, 0.35f);

        AdventureActor actor = player.AddComponent<AdventureActor>();
        actor.character = AdventureCharacter.Dracula;
        actor.roomName = "Spectrum Mockup Room";
        actor.walker = walker;
        actor.spriteRenderer = walker.spriteRenderer;

        return player;
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

    private static Sprite LoadSprite(string path, Vector2 pivot, float pixelsPerUnit)
    {
        ConfigureSpriteImporter(path, pivot, pixelsPerUnit);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite[] LoadSpriteSequence(string pathPrefix, int frameCount, Vector2 pivot, float pixelsPerUnit)
    {
        Sprite[] sprites = new Sprite[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            string path = pathPrefix + i.ToString("00") + ".png";
            sprites[i] = LoadSprite(path, pivot, pixelsPerUnit);
            if (sprites[i] == null)
            {
                throw new FileNotFoundException("Unity failed to import required Spectrum Dracula frame.", path);
            }
        }

        return sprites;
    }

    private static void ConfigureSpriteImporter(string path, Vector2 pivot, float pixelsPerUnit)
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
            || importer.filterMode != FilterMode.Point
            || importer.textureCompression != TextureImporterCompression.Uncompressed
            || importer.spritePixelsPerUnit != pixelsPerUnit
            || importer.spritePivot != pivot;

        if (changed)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.spritePivot = pivot;
            importer.alphaIsTransparency = true;
        }

        ApplySpriteImporterSerialization(importer);
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
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
