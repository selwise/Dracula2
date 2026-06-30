using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CastleWallSconcePrefabBuilder
{
    private const string PrefabFolder = "Assets/Prefabs/Castle";
    private const string PrefabPath = PrefabFolder + "/WallSconceCandle.prefab";
    private const string SconceArtPath = "Assets/Art/Castle/Props/wall_sconce_candle.png";
    private const string PrototypeFolder = "Assets/Art/Castle/Prototype";
    private const string PixelPath = PrototypeFolder + "/castle_blockout_pixel.png";
    private const string CandleFlamePrefabPath = "Assets/Prefabs/CandleFlame.prefab";
    private const string SpriteLitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";
    private const string SpriteUnlitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

    [MenuItem("Dracula/Create Castle Wall Sconce Prefab")]
    public static GameObject CreateCastleWallSconcePrefab()
    {
        Sprite pixel = EnsureBlockoutPixel();
        Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteLitMaterialPath);
        Material unlitMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteUnlitMaterialPath);

        Directory.CreateDirectory(PrefabFolder);

        GameObject root = new GameObject("WallSconceCandle");
        try
        {
            BuildSconce(root.transform, pixel, litMaterial, unlitMaterial);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created castle wall sconce prefab at " + PrefabPath + ".");
            return prefab;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    public static void EnsurePrefabExists()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
        {
            CreateCastleWallSconcePrefab();
        }
    }

    private static void BuildSconce(Transform parent, Sprite pixel, Material litMaterial, Material unlitMaterial)
    {
        Sprite authoredSconce = LoadAuthoredSconceSprite();

        if (authoredSconce != null)
        {
            CreateSprite("Authored Sconce Art", authoredSconce, Vector3.zero, Vector3.one, Color.white, 84, parent, litMaterial);
            PlaceFlame(parent);
            return;
        }

        CreateRect("Dark Recess Shadow", pixel, new Vector3(0f, 0f, 0f), new Vector2(0.62f, 1.04f), new Color(0.025f, 0.021f, 0.018f, 0.92f), 70, parent, null);

        CreateRect("Bronze Frame Left", pixel, new Vector3(-0.34f, 0f, 0f), new Vector2(0.07f, 1.0f), new Color(0.42f, 0.25f, 0.09f, 1f), 72, parent, litMaterial);
        CreateRect("Bronze Frame Right", pixel, new Vector3(0.34f, 0f, 0f), new Vector2(0.07f, 1.0f), new Color(0.52f, 0.31f, 0.11f, 1f), 72, parent, litMaterial);
        CreateRect("Bronze Frame Top", pixel, new Vector3(0f, 0.51f, 0f), new Vector2(0.74f, 0.08f), new Color(0.48f, 0.3f, 0.12f, 1f), 73, parent, litMaterial);
        CreateRect("Bronze Frame Bottom", pixel, new Vector3(0f, -0.51f, 0f), new Vector2(0.74f, 0.08f), new Color(0.32f, 0.2f, 0.08f, 1f), 73, parent, litMaterial);

        CreateRect("Inner Slate Panel", pixel, new Vector3(0f, 0.03f, 0f), new Vector2(0.44f, 0.78f), new Color(0.08f, 0.095f, 0.09f, 1f), 71, parent, litMaterial);
        CreateRect("Panel Upper Catchlight", pixel, new Vector3(0f, 0.39f, 0f), new Vector2(0.36f, 0.035f), new Color(0.23f, 0.26f, 0.24f, 1f), 74, parent, litMaterial);

        CreateRect("Iron Wall Peg", pixel, new Vector3(0f, 0.02f, 0f), new Vector2(0.14f, 0.42f), new Color(0.08f, 0.075f, 0.07f, 1f), 78, parent, litMaterial);
        CreateRect("Iron Arm Left", pixel, new Vector3(-0.11f, -0.13f, 0f), new Vector2(0.24f, 0.055f), new Color(0.1f, 0.085f, 0.072f, 1f), 79, parent, litMaterial);
        CreateRect("Iron Arm Right", pixel, new Vector3(0.11f, -0.13f, 0f), new Vector2(0.24f, 0.055f), new Color(0.13f, 0.105f, 0.08f, 1f), 79, parent, litMaterial);
        CreateRect("Bronze Candle Cup", pixel, new Vector3(0f, -0.2f, 0f), new Vector2(0.34f, 0.12f), new Color(0.5f, 0.29f, 0.09f, 1f), 82, parent, litMaterial);
        CreateRect("Cup Dark Lip", pixel, new Vector3(0f, -0.14f, 0f), new Vector2(0.38f, 0.035f), new Color(0.11f, 0.075f, 0.045f, 1f), 83, parent, litMaterial);

        CreateRect("Candle Wax Body", pixel, new Vector3(0f, 0.04f, 0f), new Vector2(0.17f, 0.48f), new Color(0.88f, 0.79f, 0.58f, 1f), 84, parent, litMaterial);
        CreateRect("Candle Wax Shadow", pixel, new Vector3(0.06f, 0.04f, 0f), new Vector2(0.045f, 0.42f), new Color(0.54f, 0.44f, 0.28f, 1f), 85, parent, litMaterial);
        CreateRect("Candle Wax Highlight", pixel, new Vector3(-0.045f, 0.1f, 0f), new Vector2(0.03f, 0.32f), new Color(1f, 0.91f, 0.68f, 1f), 86, parent, litMaterial);
        CreateRect("Black Wick", pixel, new Vector3(0f, 0.32f, 0f), new Vector2(0.026f, 0.1f), new Color(0.02f, 0.014f, 0.01f, 1f), 87, parent, null);

        PlaceFlame(parent);
    }

    private static void PlaceFlame(Transform parent)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CandleFlamePrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("Candle flame prefab was not found, so the wall sconce was created without a flame.");
            return;
        }

        GameObject flame = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (flame == null)
        {
            return;
        }

        flame.name = "Animated Candle Flame";
        flame.transform.SetParent(parent, false);
        flame.transform.localPosition = new Vector3(0f, 0.56f, 0f);
        flame.transform.localScale = Vector3.one * 0.2f;

        SpriteRenderer renderer = flame.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = 92;
        }

        CandleFlame candle = flame.GetComponent<CandleFlame>();
        if (candle != null)
        {
            candle.flameSize = 0.62f;
            candle.lightIntensity = 1.85f;
            candle.lightRange = 2.15f;
            candle.lightFlickerIntensity = 0.28f;
            candle.animationFramerate = 5.2f;
            candle.flameOffset = Vector2.zero;
            candle.positionJitter = 0.012f;
            candle.hideBaseSpriteAtRuntime = true;
        }
    }

    private static GameObject CreateRect(string name, Sprite sprite, Vector3 position, Vector2 size, Color color, int sortingOrder, Transform parent, Material material)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
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

    private static GameObject CreateSprite(string name, Sprite sprite, Vector3 position, Vector3 scale, Color color, int sortingOrder, Transform parent, Material material)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
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

    private static Sprite LoadAuthoredSconceSprite()
    {
        if (!File.Exists(SconceArtPath))
        {
            return null;
        }

        AssetDatabase.ImportAsset(SconceArtPath);
        TextureImporter importer = AssetImporter.GetAtPath(SconceArtPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 220f;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(SconceArtPath);
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

        AssetDatabase.ImportAsset(PixelPath);
        TextureImporter importer = AssetImporter.GetAtPath(PixelPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 1f;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(PixelPath);
    }
}
