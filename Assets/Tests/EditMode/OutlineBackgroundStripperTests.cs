using NUnit.Framework;
using UnityEngine;

public sealed class OutlineBackgroundStripperTests
{
    [Test]
    public void StripClearsOnlyBorderConnectedLightNeutralPixels()
    {
        Color32[] source = Fill(8, 8, new Color32(255, 255, 255, 255));

        for (int x = 2; x < 6; x++)
        {
            source[Index(x, 2, 8)] = new Color32(5, 4, 12, 255);
            source[Index(x, 5, 8)] = new Color32(5, 4, 12, 255);
        }

        for (int y = 2; y < 6; y++)
        {
            source[Index(2, y, 8)] = new Color32(5, 4, 12, 255);
            source[Index(5, y, 8)] = new Color32(5, 4, 12, 255);
        }

        source[Index(1, 1, 8)] = new Color32(190, 190, 196, 255);
        source[Index(3, 3, 8)] = new Color32(255, 255, 255, 255);
        source[Index(4, 3, 8)] = new Color32(177, 174, 193, 255);
        source[Index(3, 4, 8)] = new Color32(70, 20, 30, 255);

        OutlineBackgroundStripResult result = OutlineBackgroundStripper.Strip(source, 8, 8, OutlineBackgroundStripSettings.Default);

        Assert.AreEqual(0, result.Pixels[Index(0, 0, 8)].a);
        Assert.AreEqual(0, result.Pixels[Index(1, 1, 8)].a);
        AssertPixel(result.Pixels[Index(2, 2, 8)], 5, 4, 12, 255);
        AssertPixel(result.Pixels[Index(3, 3, 8)], 255, 255, 255, 255);
        AssertPixel(result.Pixels[Index(4, 3, 8)], 177, 174, 193, 255);
        AssertPixel(result.Pixels[Index(3, 4, 8)], 70, 20, 30, 255);
        Assert.Greater(result.RemovedPixelCount, 0);
    }

    [Test]
    public void StripDoesNotMutateInputPixels()
    {
        Color32[] source = Fill(5, 5, new Color32(255, 255, 255, 255));
        Color32[] original = (Color32[])source.Clone();

        OutlineBackgroundStripper.Strip(source, 5, 5, OutlineBackgroundStripSettings.Default);

        CollectionAssert.AreEqual(original, source);
    }

    [Test]
    public void StripReportsZeroRemovedPixelsWhenOutlineTouchesEveryBorderPixel()
    {
        Color32[] source = Fill(3, 3, new Color32(5, 4, 12, 255));
        source[Index(1, 1, 3)] = new Color32(255, 255, 255, 255);

        OutlineBackgroundStripResult result = OutlineBackgroundStripper.Strip(source, 3, 3, OutlineBackgroundStripSettings.Default);

        Assert.AreEqual(0, result.RemovedPixelCount);
        AssertPixel(result.Pixels[Index(1, 1, 3)], 255, 255, 255, 255);
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
