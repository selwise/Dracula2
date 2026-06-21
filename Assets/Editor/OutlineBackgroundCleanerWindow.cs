using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class OutlineBackgroundCleanerWindow : EditorWindow
{
    private enum PreviewMode
    {
        Cleaned,
        Original,
        RemovedMask,
        AlphaMask
    }

    private readonly List<PngEntry> entries = new List<PngEntry>();
    private Vector2 fileScrollPosition;
    private Vector2 previewScrollPosition;
    private int selectedIndex = -1;

    private bool stripExterior = true;
    private int minLightness = 140;
    private int neutralTolerance = 28;
    private int alphaCutoff;
    private bool connectDiagonals;
    private bool trimTransparentCanvas = true;
    private int horizontalPadding = 32;
    private int topPadding = 32;
    private int bottomPadding;
    private PreviewMode previewMode = PreviewMode.Cleaned;

    private string loadedPath;
    private Texture2D sourceTexture;
    private Texture2D previewTexture;
    private ProcessedImage processedImage;
    private bool previewDirty = true;

    [MenuItem("Tools/Dracula/Outline Background Cleaner")]
    public static void Open()
    {
        OutlineBackgroundCleanerWindow window = GetWindow<OutlineBackgroundCleanerWindow>("Outline Cleaner");
        window.minSize = new Vector2(920f, 560f);
        window.Show();
    }

    private void OnDisable()
    {
        ClearPreviewTextures();
    }

    private void OnGUI()
    {
        DrawDropZone();

        using (new EditorGUILayout.HorizontalScope())
        {
            DrawFilePanel();
            DrawMainPanel();
        }
    }

    private void DrawDropZone()
    {
        Rect dropZone = GUILayoutUtility.GetRect(0f, 58f, GUILayout.ExpandWidth(true));
        GUI.Box(dropZone, "Drop PNG files or folders here");

        Event current = Event.current;
        if (!dropZone.Contains(current.mousePosition))
        {
            return;
        }

        if (current.type == EventType.DragUpdated || current.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                AddPaths(DragAndDrop.paths);
            }

            current.Use();
        }
    }

    private void DrawFilePanel()
    {
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(260f)))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add PNG"))
                {
                    string path = EditorUtility.OpenFilePanel("Add PNG", string.Empty, "png");
                    AddFile(path);
                }

                if (GUILayout.Button("Add Folder"))
                {
                    string path = EditorUtility.OpenFolderPanel("Add PNG Folder", string.Empty, string.Empty);
                    if (!string.IsNullOrEmpty(path))
                    {
                        AddPaths(new[] { path });
                    }
                }
            }

            EditorGUILayout.LabelField(entries.Count + " file(s)", EditorStyles.miniBoldLabel);
            fileScrollPosition = EditorGUILayout.BeginScrollView(fileScrollPosition);

            for (int i = 0; i < entries.Count; i++)
            {
                PngEntry entry = entries[i];
                bool selected = i == selectedIndex;
                GUIStyle style = selected ? EditorStyles.helpBox : EditorStyles.label;
                using (new EditorGUILayout.HorizontalScope(style))
                {
                    if (GUILayout.Button(Path.GetFileName(entry.Path), EditorStyles.label))
                    {
                        SelectIndex(i);
                    }

                    if (GUILayout.Button("X", GUILayout.Width(26f)))
                    {
                        entries.RemoveAt(i);
                        if (selectedIndex >= entries.Count)
                        {
                            selectedIndex = entries.Count - 1;
                        }

                        previewDirty = true;
                        GUIUtility.ExitGUI();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawMainPanel()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            DrawSettings();
            DrawPreview();
            DrawSaveButtons();
        }
    }

    private void DrawSettings()
    {
        EditorGUI.BeginChangeCheck();
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Previewed Cleanup", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Safe Grey Halo"))
                {
                    ApplyPreset(true, 140, 28, 0, false);
                }

                if (GUILayout.Button("Light Halo"))
                {
                    ApplyPreset(true, 115, 36, 0, false);
                }

                if (GUILayout.Button("Trim Only"))
                {
                    ApplyPreset(false, 140, 28, 0, false);
                }
            }

            stripExterior = EditorGUILayout.Toggle("Strip Exterior Pixels", stripExterior);
            using (new EditorGUI.DisabledScope(!stripExterior))
            {
                minLightness = EditorGUILayout.IntSlider("Min Removable Lightness", minLightness, 0, 255);
                neutralTolerance = EditorGUILayout.IntSlider("Neutral RGB Tolerance", neutralTolerance, 0, 255);
                alphaCutoff = EditorGUILayout.IntSlider("Alpha Cutoff", alphaCutoff, 0, 255);
                connectDiagonals = EditorGUILayout.Toggle("Connect Diagonals", connectDiagonals);
            }

            trimTransparentCanvas = EditorGUILayout.Toggle("Trim Transparent Canvas", trimTransparentCanvas);
            using (new EditorGUI.DisabledScope(!trimTransparentCanvas))
            {
                horizontalPadding = EditorGUILayout.IntSlider("Left/Right Padding", horizontalPadding, 0, 256);
                topPadding = EditorGUILayout.IntSlider("Top Padding", topPadding, 0, 256);
                bottomPadding = EditorGUILayout.IntSlider("Bottom Padding", bottomPadding, 0, 256);
            }

            previewMode = (PreviewMode)EditorGUILayout.EnumPopup("Preview", previewMode);
        }

        if (EditorGUI.EndChangeCheck())
        {
            previewDirty = true;
        }
    }

    private void DrawPreview()
    {
        PngEntry entry = CurrentEntry;
        if (entry == null)
        {
            EditorGUILayout.HelpBox("Drop or add a PNG to preview cleanup before saving.", MessageType.Info);
            return;
        }

        EnsurePreview(entry);

        if (processedImage == null || previewTexture == null)
        {
            EditorGUILayout.HelpBox(entry.Status, MessageType.Warning);
            return;
        }

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField(Path.GetFileName(entry.Path), EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(entry.Path, EditorStyles.miniLabel, GUILayout.Height(18f));
            EditorGUILayout.LabelField(processedImage.Summary, EditorStyles.wordWrappedMiniLabel);

            Rect previewRect = GUILayoutUtility.GetRect(320f, 420f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            previewScrollPosition = GUI.BeginScrollView(previewRect, previewScrollPosition, new Rect(0f, 0f, previewTexture.width, previewTexture.height));
            EditorGUI.DrawTextureTransparent(new Rect(0f, 0f, previewTexture.width, previewTexture.height), previewTexture, ScaleMode.ScaleToFit);
            GUI.EndScrollView();
        }
    }

    private void DrawSaveButtons()
    {
        PngEntry entry = CurrentEntry;
        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.enabled = entry != null && processedImage != null;

            if (GUILayout.Button("Save Fixed Copy Beside Source"))
            {
                SaveCurrent(BuildCopyPath(entry.Path));
            }

            if (GUILayout.Button("Save Fixed Copy As..."))
            {
                string defaultName = Path.GetFileName(BuildCopyPath(entry.Path));
                string destination = EditorUtility.SaveFilePanel("Save Fixed PNG", Path.GetDirectoryName(entry.Path), defaultName, "png");
                if (!string.IsNullOrEmpty(destination))
                {
                    SaveCurrent(destination);
                }
            }

            if (GUILayout.Button("Overwrite Original..."))
            {
                ConfirmAndOverwrite(entry);
            }

            GUI.enabled = true;
        }
    }

    private void ApplyPreset(bool shouldStripExterior, int lightness, int tolerance, int alpha, bool diagonals)
    {
        stripExterior = shouldStripExterior;
        minLightness = lightness;
        neutralTolerance = tolerance;
        alphaCutoff = alpha;
        connectDiagonals = diagonals;
        previewDirty = true;
    }

    private void AddPaths(IEnumerable<string> droppedPaths)
    {
        foreach (string droppedPath in droppedPaths)
        {
            if (Directory.Exists(droppedPath))
            {
                foreach (string pngPath in Directory.GetFiles(droppedPath, "*.png", SearchOption.AllDirectories))
                {
                    AddFile(pngPath);
                }

                continue;
            }

            AddFile(droppedPath);
        }
    }

    private void AddFile(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path) || !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string fullPath = Path.GetFullPath(path);
        for (int i = 0; i < entries.Count; i++)
        {
            if (string.Equals(entries[i].Path, fullPath, StringComparison.Ordinal))
            {
                SelectIndex(i);
                return;
            }
        }

        entries.Add(new PngEntry(fullPath));
        SelectIndex(entries.Count - 1);
    }

    private void SelectIndex(int index)
    {
        selectedIndex = Mathf.Clamp(index, -1, entries.Count - 1);
        previewScrollPosition = Vector2.zero;
        previewDirty = true;
    }

    private void ConfirmAndOverwrite(PngEntry entry)
    {
        if (!EditorUtility.DisplayDialog(
                "Overwrite PNG",
                "Overwrite " + Path.GetFileName(entry.Path) + "? This cannot be undone by the cleaner.",
                "Overwrite",
                "Cancel"))
        {
            return;
        }

        SaveCurrent(entry.Path);
    }

    private void SaveCurrent(string destination)
    {
        PngEntry entry = CurrentEntry;
        if (entry == null || processedImage == null)
        {
            return;
        }

        try
        {
            Texture2D texture = CreateTexture(processedImage.Pixels, processedImage.Width, processedImage.Height, "outline_cleaner_save");
            try
            {
                File.WriteAllBytes(destination, texture.EncodeToPNG());
            }
            finally
            {
                DestroyImmediate(texture);
            }

            entry.Status = "Saved " + destination;
            RefreshAssetIfNeeded(destination);
        }
        catch (Exception ex)
        {
            entry.Status = "Save failed: " + ex.Message;
        }
    }

    private void EnsurePreview(PngEntry entry)
    {
        if (!previewDirty && string.Equals(entry.Path, loadedPath, StringComparison.Ordinal))
        {
            return;
        }

        ClearPreviewTextures();
        loadedPath = entry.Path;

        try
        {
            sourceTexture = LoadPng(entry.Path);
            processedImage = ProcessTexture(sourceTexture, CurrentSettings);
            previewTexture = CreatePreviewTexture(sourceTexture, processedImage);
            entry.Status = processedImage.Summary;
        }
        catch (Exception ex)
        {
            processedImage = null;
            previewTexture = null;
            entry.Status = "Preview failed: " + ex.Message;
        }

        previewDirty = false;
    }

    private ProcessedImage ProcessTexture(Texture2D source, OutlineBackgroundStripSettings settings)
    {
        Color32[] sourcePixels = source.GetPixels32();
        Color32[] strippedPixels;
        int removedPixels;

        if (stripExterior)
        {
            OutlineBackgroundStripResult stripResult = OutlineBackgroundStripper.Strip(sourcePixels, source.width, source.height, settings);
            strippedPixels = stripResult.Pixels;
            removedPixels = stripResult.RemovedPixelCount;
        }
        else
        {
            strippedPixels = (Color32[])sourcePixels.Clone();
            removedPixels = 0;
        }

        RectInt cropRect = new RectInt(0, 0, source.width, source.height);
        Color32[] outputPixels = strippedPixels;
        int outputWidth = source.width;
        int outputHeight = source.height;

        if (trimTransparentCanvas && OutlineImageCanvas.TryFindOpaqueBounds(strippedPixels, source.width, source.height, out RectInt bounds))
        {
            cropRect = OutlineImageCanvas.BuildPaddedCropRect(bounds, source.width, source.height, horizontalPadding, topPadding, bottomPadding);
            outputPixels = OutlineImageCanvas.Crop(strippedPixels, source.width, source.height, cropRect);
            outputWidth = cropRect.width;
            outputHeight = cropRect.height;
        }

        NormalizeTransparentRgb(outputPixels);

        int opaqueBorderPixels = CountOpaqueBorderPixels(outputPixels, outputWidth, outputHeight);
        string summary = "Source " + source.width + "x" + source.height
            + " -> Output " + outputWidth + "x" + outputHeight
            + " | opaque exterior removed: " + removedPixels
            + " | output opaque border pixels: " + opaqueBorderPixels
            + " | crop: " + cropRect;

        return new ProcessedImage(
            outputPixels,
            outputWidth,
            outputHeight,
            sourcePixels,
            strippedPixels,
            source.width,
            source.height,
            removedPixels,
            cropRect,
            summary);
    }

    private Texture2D CreatePreviewTexture(Texture2D source, ProcessedImage image)
    {
        switch (previewMode)
        {
            case PreviewMode.Original:
                return CreateTexture(source.GetPixels32(), source.width, source.height, "outline_cleaner_original_preview");
            case PreviewMode.RemovedMask:
                return CreateRemovedMaskTexture(image);
            case PreviewMode.AlphaMask:
                return CreateAlphaMaskTexture(image.Pixels, image.Width, image.Height);
            default:
                return CreateTexture(image.Pixels, image.Width, image.Height, "outline_cleaner_cleaned_preview");
        }
    }

    private static Texture2D CreateRemovedMaskTexture(ProcessedImage image)
    {
        Color32[] mask = new Color32[image.SourceWidth * image.SourceHeight];
        for (int i = 0; i < mask.Length; i++)
        {
            bool removed = image.SourcePixels[i].a != 0 && image.StrippedSourcePixels[i].a == 0;
            mask[i] = removed ? new Color32(255, 50, 50, 255) : new Color32(0, 0, 0, 255);
        }

        return CreateTexture(mask, image.SourceWidth, image.SourceHeight, "outline_cleaner_removed_mask");
    }

    private static Texture2D CreateAlphaMaskTexture(Color32[] pixels, int width, int height)
    {
        Color32[] mask = new Color32[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            byte alpha = pixels[i].a;
            mask[i] = new Color32(alpha, alpha, alpha, 255);
        }

        return CreateTexture(mask, width, height, "outline_cleaner_alpha_mask");
    }

    private static Texture2D CreateTexture(Color32[] pixels, int width, int height, string name)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = name;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels32(pixels);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
        return texture;
    }

    private static Texture2D LoadPng(string path)
    {
        byte[] pngBytes = File.ReadAllBytes(path);
        Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        source.filterMode = FilterMode.Point;
        if (!ImageConversion.LoadImage(source, pngBytes, markNonReadable: false))
        {
            DestroyImmediate(source);
            throw new InvalidDataException("Unity could not decode this PNG.");
        }

        return source;
    }

    private OutlineBackgroundStripSettings CurrentSettings =>
        new OutlineBackgroundStripSettings(
            (byte)minLightness,
            (byte)neutralTolerance,
            (byte)alphaCutoff,
            connectDiagonals,
            new Color32(0, 0, 0, 0));

    private void ClearPreviewTextures()
    {
        if (sourceTexture != null)
        {
            DestroyImmediate(sourceTexture);
            sourceTexture = null;
        }

        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
            previewTexture = null;
        }

        processedImage = null;
    }

    private static void NormalizeTransparentRgb(Color32[] pixels)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a == 0)
            {
                pixels[i] = new Color32(0, 0, 0, 0);
            }
        }
    }

    private static int CountOpaqueBorderPixels(Color32[] pixels, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return 0;
        }

        int count = 0;
        for (int x = 0; x < width; x++)
        {
            if (pixels[x].a != 0)
            {
                count++;
            }

            int topIndex = (height - 1) * width + x;
            if (height > 1 && pixels[topIndex].a != 0)
            {
                count++;
            }
        }

        for (int y = 1; y < height - 1; y++)
        {
            if (pixels[y * width].a != 0)
            {
                count++;
            }

            if (width > 1 && pixels[y * width + width - 1].a != 0)
            {
                count++;
            }
        }

        return count;
    }

    private static string BuildCopyPath(string sourcePath)
    {
        string directory = Path.GetDirectoryName(sourcePath);
        string filename = Path.GetFileNameWithoutExtension(sourcePath);
        return Path.Combine(directory ?? string.Empty, filename + "_fixed.png");
    }

    private static void RefreshAssetIfNeeded(string path)
    {
        if (TryGetAssetPath(path, out string assetPath))
        {
            AssetDatabase.ImportAsset(assetPath);
            return;
        }

        AssetDatabase.Refresh();
    }

    private static bool TryGetAssetPath(string fullPath, out string assetPath)
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string normalizedPath = Path.GetFullPath(fullPath);
        string prefix = projectRoot + Path.DirectorySeparatorChar;

        if (normalizedPath.StartsWith(prefix, StringComparison.Ordinal))
        {
            assetPath = normalizedPath.Substring(prefix.Length).Replace(Path.DirectorySeparatorChar, '/');
            return assetPath.StartsWith("Assets/", StringComparison.Ordinal);
        }

        assetPath = string.Empty;
        return false;
    }

    private PngEntry CurrentEntry
    {
        get
        {
            if (selectedIndex < 0 || selectedIndex >= entries.Count)
            {
                return null;
            }

            return entries[selectedIndex];
        }
    }

    private sealed class PngEntry
    {
        public PngEntry(string path)
        {
            Path = path;
        }

        public string Path { get; }
        public string Status { get; set; }
    }

    private sealed class ProcessedImage
    {
        public ProcessedImage(
            Color32[] pixels,
            int width,
            int height,
            Color32[] sourcePixels,
            Color32[] strippedSourcePixels,
            int sourceWidth,
            int sourceHeight,
            int removedPixelCount,
            RectInt cropRect,
            string summary)
        {
            Pixels = pixels;
            Width = width;
            Height = height;
            SourcePixels = sourcePixels;
            StrippedSourcePixels = strippedSourcePixels;
            SourceWidth = sourceWidth;
            SourceHeight = sourceHeight;
            RemovedPixelCount = removedPixelCount;
            CropRect = cropRect;
            Summary = summary;
        }

        public Color32[] Pixels { get; }
        public int Width { get; }
        public int Height { get; }
        public Color32[] SourcePixels { get; }
        public Color32[] StrippedSourcePixels { get; }
        public int SourceWidth { get; }
        public int SourceHeight { get; }
        public int RemovedPixelCount { get; }
        public RectInt CropRect { get; }
        public string Summary { get; }
    }
}
