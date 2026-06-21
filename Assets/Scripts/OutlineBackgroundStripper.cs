using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct OutlineBackgroundStripSettings
{
    public OutlineBackgroundStripSettings(
        byte minLightness,
        byte neutralTolerance,
        byte alphaCutoff,
        bool connectDiagonals,
        Color32 transparentColor)
    {
        MinLightness = minLightness;
        NeutralTolerance = neutralTolerance;
        AlphaCutoff = alphaCutoff;
        ConnectDiagonals = connectDiagonals;
        TransparentColor = transparentColor;
    }

    public byte MinLightness { get; }
    public byte NeutralTolerance { get; }
    public byte AlphaCutoff { get; }
    public bool ConnectDiagonals { get; }
    public Color32 TransparentColor { get; }

    public static OutlineBackgroundStripSettings Default =>
        new OutlineBackgroundStripSettings(140, 28, 0, false, new Color32(0, 0, 0, 0));
}

public readonly struct OutlineBackgroundStripResult
{
    public OutlineBackgroundStripResult(Color32[] pixels, int removedPixelCount)
    {
        Pixels = pixels;
        RemovedPixelCount = removedPixelCount;
    }

    public Color32[] Pixels { get; }
    public int RemovedPixelCount { get; }
}

public static class OutlineBackgroundStripper
{
    private static readonly Vector2Int[] CardinalOffsets =
    {
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1)
    };

    private static readonly Vector2Int[] DiagonalOffsets =
    {
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1)
    };

    public static OutlineBackgroundStripResult Strip(
        Color32[] sourcePixels,
        int width,
        int height,
        OutlineBackgroundStripSettings settings)
    {
        if (sourcePixels == null)
        {
            throw new ArgumentNullException(nameof(sourcePixels));
        }

        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        if (sourcePixels.Length != width * height)
        {
            throw new ArgumentException("Pixel buffer length must equal width * height.", nameof(sourcePixels));
        }

        Color32[] output = (Color32[])sourcePixels.Clone();
        bool[] visited = new bool[sourcePixels.Length];
        bool[] removable = new bool[sourcePixels.Length];
        Queue<int> queue = new Queue<int>();

        EnqueueBorderSeeds(sourcePixels, width, height, settings, visited, queue);

        Vector2Int[] offsets = settings.ConnectDiagonals ? DiagonalOffsets : CardinalOffsets;
        while (queue.Count > 0)
        {
            int index = queue.Dequeue();
            removable[index] = true;
            int x = index % width;
            int y = index / width;

            for (int i = 0; i < offsets.Length; i++)
            {
                int nx = x + offsets[i].x;
                int ny = y + offsets[i].y;
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                {
                    continue;
                }

                int nextIndex = ToIndex(nx, ny, width);
                if (visited[nextIndex] || !IsRemovable(sourcePixels[nextIndex], settings))
                {
                    continue;
                }

                visited[nextIndex] = true;
                queue.Enqueue(nextIndex);
            }
        }

        int removed = 0;
        for (int i = 0; i < output.Length; i++)
        {
            if (!removable[i])
            {
                continue;
            }

            if (output[i].a != 0)
            {
                removed++;
            }

            output[i] = settings.TransparentColor;
        }

        return new OutlineBackgroundStripResult(output, removed);
    }

    public static bool IsRemovable(Color32 pixel, OutlineBackgroundStripSettings settings)
    {
        if (pixel.a <= settings.AlphaCutoff)
        {
            return true;
        }

        byte high = Math.Max(pixel.r, Math.Max(pixel.g, pixel.b));
        byte low = Math.Min(pixel.r, Math.Min(pixel.g, pixel.b));
        return high >= settings.MinLightness && high - low <= settings.NeutralTolerance;
    }

    private static void EnqueueBorderSeeds(
        Color32[] pixels,
        int width,
        int height,
        OutlineBackgroundStripSettings settings,
        bool[] visited,
        Queue<int> queue)
    {
        for (int x = 0; x < width; x++)
        {
            TryEnqueue(x, 0, width, pixels, settings, visited, queue);
            if (height > 1)
            {
                TryEnqueue(x, height - 1, width, pixels, settings, visited, queue);
            }
        }

        for (int y = 1; y < height - 1; y++)
        {
            TryEnqueue(0, y, width, pixels, settings, visited, queue);
            if (width > 1)
            {
                TryEnqueue(width - 1, y, width, pixels, settings, visited, queue);
            }
        }
    }

    private static void TryEnqueue(
        int x,
        int y,
        int width,
        Color32[] pixels,
        OutlineBackgroundStripSettings settings,
        bool[] visited,
        Queue<int> queue)
    {
        int index = ToIndex(x, y, width);
        if (visited[index] || !IsRemovable(pixels[index], settings))
        {
            return;
        }

        visited[index] = true;
        queue.Enqueue(index);
    }

    private static int ToIndex(int x, int y, int width)
    {
        return y * width + x;
    }
}
