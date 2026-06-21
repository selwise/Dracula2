using NUnit.Framework;
using UnityEngine;

public sealed class OutlineImageCanvasTests
{
    [Test]
    public void FindOpaqueBoundsReturnsTightAlphaRect()
    {
        Color32[] pixels = Fill(6, 5, new Color32(0, 0, 0, 0));
        pixels[Index(2, 1, 6)] = new Color32(20, 20, 20, 255);
        pixels[Index(4, 3, 6)] = new Color32(20, 20, 20, 255);

        bool found = OutlineImageCanvas.TryFindOpaqueBounds(pixels, 6, 5, out RectInt bounds);

        Assert.IsTrue(found);
        Assert.AreEqual(new RectInt(2, 1, 3, 3), bounds);
    }

    [Test]
    public void PaddedCropCanPreserveBottomBaseline()
    {
        RectInt bounds = new RectInt(312, 0, 1080, 1976);

        RectInt crop = OutlineImageCanvas.BuildPaddedCropRect(bounds, 2048, 2048, 32, 32, 0);

        Assert.AreEqual(new RectInt(280, 0, 1144, 2008), crop);
    }

    [Test]
    public void CropCopiesOnlyRequestedPixels()
    {
        Color32[] pixels = Fill(4, 4, new Color32(0, 0, 0, 0));
        pixels[Index(1, 1, 4)] = new Color32(10, 11, 12, 255);
        pixels[Index(2, 2, 4)] = new Color32(20, 21, 22, 255);

        Color32[] cropped = OutlineImageCanvas.Crop(pixels, 4, 4, new RectInt(1, 1, 2, 2));

        Assert.AreEqual(4, cropped.Length);
        AssertPixel(cropped[0], 10, 11, 12, 255);
        AssertPixel(cropped[3], 20, 21, 22, 255);
    }

    private static Color32[] Fill(int width, int height, Color32 color)
    {
        Color32[] pixels = new Color32[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        return pixels;
    }

    private static int Index(int x, int y, int width)
    {
        return y * width + x;
    }

    private static void AssertPixel(Color32 pixel, byte r, byte g, byte b, byte a)
    {
        Assert.AreEqual(r, pixel.r);
        Assert.AreEqual(g, pixel.g);
        Assert.AreEqual(b, pixel.b);
        Assert.AreEqual(a, pixel.a);
    }
}
