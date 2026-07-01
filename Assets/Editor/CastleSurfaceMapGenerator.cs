#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CastleSurfaceMapGenerator
{
    private const string OutputFolder = "Assets/Art/Castle/GeneratedSurfaceMaps";
    private const string NormalSecondaryTextureName = "_NormalMap";

    private static readonly SurfaceSpec[] EastGallerySpecs =
    {
        new SurfaceSpec("Assets/Art/Castle/Tiles/wall_castle_gray_block_tile.png", 3.2f, 1, 1.12f),
        new SurfaceSpec("Assets/Art/Castle/Tiles/floor3.png", 2.6f, 1, 1.08f),
        new SurfaceSpec("Assets/Art/Castle/Prototype/castle_gallery_oblique_doorwall_user_exact.png", 3.0f, 1, 1.10f),
        new SurfaceSpec("Assets/Art/Castle/Tiles/LowerEdge.png", 1.8f, 1, 1.00f),
        new SurfaceSpec("Assets/Art/Castle/Props/Pillar1.png", 2.2f, 1, 1.00f),
        new SurfaceSpec("Assets/Art/Castle/Props/GrimReaperStatue.png", 2.0f, 1, 0.95f),
        new SurfaceSpec("Assets/Art/Castle/Props/Fountain.png", 1.9f, 1, 0.95f),
        new SurfaceSpec("Assets/Art/Castle/Props/wall_sconce_candle.png", 1.6f, 1, 0.95f),
        new SurfaceSpec("Assets/Art/Castle/Prototype/castle_blockout_pixel.png", 1.2f, 1, 0.85f),
        new SurfaceSpec("Assets/Art/Characters/Renfield/Renfield.png", 1.1f, 1, 0.80f),
    };

    [MenuItem("Tools/Dracula2/Castle/Regenerate East Gallery Surface Maps")]
    public static void RegenerateEastGallerySurfaceMaps()
    {
        Directory.CreateDirectory(ToAbsolutePath(OutputFolder));

        var generated = 0;
        foreach (var spec in EastGallerySpecs)
        {
            if (!File.Exists(ToAbsolutePath(spec.SourcePath)))
            {
                Debug.LogWarning($"Surface map source not found: {spec.SourcePath}");
                continue;
            }

            var sourceName = Path.GetFileNameWithoutExtension(spec.SourcePath);
            var normalPath = $"{OutputFolder}/{sourceName}_normal.png";
            var heightPath = $"{OutputFolder}/{sourceName}_height.png";

            GenerateMaps(spec, normalPath, heightPath);
            ConfigureGeneratedTexture(normalPath, true);
            ConfigureGeneratedTexture(heightPath, false);
            AttachNormalMap(spec.SourcePath, normalPath);
            generated++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Regenerated {generated} East Gallery surface map set(s) in {OutputFolder}.");
    }

    private static void GenerateMaps(SurfaceSpec spec, string normalAssetPath, string heightAssetPath)
    {
        var sourceTexture = LoadPng(spec.SourcePath);
        var width = sourceTexture.width;
        var height = sourceTexture.height;
        var pixels = sourceTexture.GetPixels32();
        var alpha = new float[pixels.Length];
        var heights = new float[pixels.Length];
        var blurred = new float[pixels.Length];

        for (var i = 0; i < pixels.Length; i++)
        {
            var p = pixels[i];
            alpha[i] = p.a / 255f;
            var luma = ((0.299f * p.r) + (0.587f * p.g) + (0.114f * p.b)) / 255f;
            heights[i] = alpha[i] <= 0.01f ? 0.5f : Mathf.Clamp01(0.5f + ((luma - 0.5f) * spec.HeightContrast));
        }

        Array.Copy(heights, blurred, heights.Length);
        for (var pass = 0; pass < spec.BlurPasses; pass++)
        {
            BlurHeight(width, height, alpha, blurred, heights);
            var swap = blurred;
            blurred = heights;
            heights = swap;
        }

        var normalPixels = new Color32[pixels.Length];
        var heightPixels = new Color32[pixels.Length];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = ToIndex(x, y, width);
                var a = pixels[index].a;
                if (alpha[index] <= 0.01f)
                {
                    normalPixels[index] = new Color32(128, 128, 255, 0);
                    heightPixels[index] = new Color32(0, 0, 0, 0);
                    continue;
                }

                var left = SampleHeight(x - 1, y, width, height, alpha, heights, heights[index]);
                var right = SampleHeight(x + 1, y, width, height, alpha, heights, heights[index]);
                var down = SampleHeight(x, y - 1, width, height, alpha, heights, heights[index]);
                var up = SampleHeight(x, y + 1, width, height, alpha, heights, heights[index]);

                var dx = (right - left) * 0.5f;
                var dy = (up - down) * 0.5f;
                var normal = new Vector3(-dx * spec.NormalStrength, -dy * spec.NormalStrength, 1f).normalized;

                normalPixels[index] = new Color32(
                    ToByte((normal.x * 0.5f) + 0.5f),
                    ToByte((normal.y * 0.5f) + 0.5f),
                    ToByte((normal.z * 0.5f) + 0.5f),
                    a);

                var h = ToByte(heights[index]);
                heightPixels[index] = new Color32(h, h, h, a);
            }
        }

        SavePng(width, height, normalPixels, normalAssetPath, true);
        SavePng(width, height, heightPixels, heightAssetPath, true);
    }

    private static void BlurHeight(int width, int height, float[] alpha, float[] input, float[] output)
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = ToIndex(x, y, width);
                if (alpha[index] <= 0.01f)
                {
                    output[index] = input[index];
                    continue;
                }

                var center = input[index];
                var sum = center * 4f;
                sum += SampleHeight(x - 1, y, width, height, alpha, input, center);
                sum += SampleHeight(x + 1, y, width, height, alpha, input, center);
                sum += SampleHeight(x, y - 1, width, height, alpha, input, center);
                sum += SampleHeight(x, y + 1, width, height, alpha, input, center);
                output[index] = sum * 0.125f;
            }
        }
    }

    private static Texture2D LoadPng(string assetPath)
    {
        var bytes = File.ReadAllBytes(ToAbsolutePath(assetPath));
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        if (!ImageConversion.LoadImage(texture, bytes))
        {
            throw new InvalidOperationException($"Could not load PNG data from {assetPath}.");
        }

        return texture;
    }

    private static void SavePng(int width, int height, Color32[] pixels, string assetPath, bool linear)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, linear);
        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        File.WriteAllBytes(ToAbsolutePath(assetPath), texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }

    private static void ConfigureGeneratedTexture(string assetPath, bool isNormalMap)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = isNormalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.npotScale = TextureImporterNPOTScale.None;
        if (!isNormalMap)
        {
            importer.sRGBTexture = false;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
        }

        importer.SaveAndReimport();
    }

    private static void AttachNormalMap(string sourceAssetPath, string normalAssetPath)
    {
        var sourceImporter = AssetImporter.GetAtPath(sourceAssetPath) as TextureImporter;
        var normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(normalAssetPath);
        if (sourceImporter == null || normalTexture == null)
        {
            Debug.LogWarning($"Could not attach normal map {normalAssetPath} to {sourceAssetPath}.");
            return;
        }

        var secondaryTextures = new List<SecondarySpriteTexture>();
        var existing = sourceImporter.secondarySpriteTextures;
        if (existing != null)
        {
            for (var i = 0; i < existing.Length; i++)
            {
                if (existing[i].name != NormalSecondaryTextureName)
                {
                    secondaryTextures.Add(existing[i]);
                }
            }
        }

        secondaryTextures.Add(new SecondarySpriteTexture
        {
            name = NormalSecondaryTextureName,
            texture = normalTexture
        });

        sourceImporter.secondarySpriteTextures = secondaryTextures.ToArray();
        sourceImporter.SaveAndReimport();
    }

    private static float SampleHeight(int x, int y, int width, int height, float[] alpha, float[] heights, float fallback)
    {
        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Clamp(y, 0, height - 1);
        var index = ToIndex(x, y, width);
        return alpha[index] <= 0.01f ? fallback : heights[index];
    }

    private static int ToIndex(int x, int y, int width)
    {
        return (y * width) + x;
    }

    private static byte ToByte(float value)
    {
        return (byte)Mathf.RoundToInt(Mathf.Clamp01(value) * 255f);
    }

    private static string ToAbsolutePath(string assetPath)
    {
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
    }

    private readonly struct SurfaceSpec
    {
        public readonly string SourcePath;
        public readonly float NormalStrength;
        public readonly int BlurPasses;
        public readonly float HeightContrast;

        public SurfaceSpec(string sourcePath, float normalStrength, int blurPasses, float heightContrast)
        {
            SourcePath = sourcePath;
            NormalStrength = normalStrength;
            BlurPasses = blurPasses;
            HeightContrast = heightContrast;
        }
    }
}
#endif
