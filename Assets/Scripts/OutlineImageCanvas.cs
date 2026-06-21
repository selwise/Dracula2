using System;
using UnityEngine;

public static class OutlineImageCanvas
{
    public static bool TryFindOpaqueBounds(Color32[] pixels, int width, int height, out RectInt bounds)
    {
        ValidatePixels(pixels, width, height);

        int minX = width;
        int minY = height;
        int maxX = -1;
        int maxY = -1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pixels[ToIndex(x, y, width)].a == 0)
                {
                    continue;
                }

                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        if (maxX < minX || maxY < minY)
        {
            bounds = new RectInt(0, 0, 0, 0);
            return false;
        }

        bounds = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        return true;
    }

    public static RectInt BuildPaddedCropRect(
        RectInt opaqueBounds,
        int sourceWidth,
        int sourceHeight,
        int horizontalPadding,
        int topPadding,
        int bottomPadding)
    {
        if (sourceWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceWidth), "Width must be positive.");
        }

        if (sourceHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceHeight), "Height must be positive.");
        }

        int xMin = Mathf.Clamp(opaqueBounds.xMin - Mathf.Max(0, horizontalPadding), 0, sourceWidth);
        int xMax = Mathf.Clamp(opaqueBounds.xMax + Mathf.Max(0, horizontalPadding), 0, sourceWidth);
        int yMin = Mathf.Clamp(opaqueBounds.yMin - Mathf.Max(0, bottomPadding), 0, sourceHeight);
        int yMax = Mathf.Clamp(opaqueBounds.yMax + Mathf.Max(0, topPadding), 0, sourceHeight);
        return new RectInt(xMin, yMin, Mathf.Max(0, xMax - xMin), Mathf.Max(0, yMax - yMin));
    }

    public static Color32[] Crop(Color32[] pixels, int sourceWidth, int sourceHeight, RectInt cropRect)
    {
        ValidatePixels(pixels, sourceWidth, sourceHeight);

        if (cropRect.xMin < 0 || cropRect.yMin < 0 || cropRect.xMax > sourceWidth || cropRect.yMax > sourceHeight)
        {
            throw new ArgumentOutOfRangeException(nameof(cropRect), "Crop rect must be inside the source image.");
        }

        Color32[] cropped = new Color32[cropRect.width * cropRect.height];
        for (int y = 0; y < cropRect.height; y++)
        {
            Array.Copy(
                pixels,
                ToIndex(cropRect.xMin, cropRect.yMin + y, sourceWidth),
                cropped,
                y * cropRect.width,
                cropRect.width);
        }

        return cropped;
    }

    private static void ValidatePixels(Color32[] pixels, int width, int height)
    {
        if (pixels == null)
        {
            throw new ArgumentNullException(nameof(pixels));
        }

        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        if (pixels.Length != width * height)
        {
            throw new ArgumentException("Pixel buffer length must equal width * height.", nameof(pixels));
        }
    }

    private static int ToIndex(int x, int y, int width)
    {
        return y * width + x;
    }
}
