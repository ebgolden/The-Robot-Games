using System;
using UnityEngine;

public static class ImageTools
{
    private static readonly Color[] BAD_COLORS = { new Color(1, 1, 1, 1), new Color(0.321568627f, 0.321568627f, 0.321568627f, 1) };

    public static Texture2D removeColor(Texture2D i, Color c)
    {
        Color[] pixels = i.GetPixels(0, 0, i.width, i.height, 0);
        for (int p = 0; p < pixels.Length; ++p)
        {
            if (pixels[p] == c)
                pixels[p] = new Color(0, 0, 0, 0);
        }
        Texture2D n = new Texture2D(i.width, i.height);
        n.SetPixels(0, 0, i.width, i.height, pixels, 0);
        n.Apply();
        return n;
    }

    public static Texture2D fixBadColors(Texture2D image)
    {
        Texture2D fixedImage = image;
        foreach (Color color in BAD_COLORS)
            fixedImage = changeColor(fixedImage, color);
        return fixedImage;
    }

    public static Texture2D removeBadColors(Texture2D image)
    {
        Texture2D fixedImage = image;
        foreach (Color color in BAD_COLORS)
            fixedImage = removeColor(fixedImage, color);
        return fixedImage;
    }

    private static Texture2D changeColor(Texture2D i, Color c)
    {
        Color[] pixels = i.GetPixels(0, 0, i.width, i.height, 0);
        for (int p = 0; p < pixels.Length; ++p)
        {
            if (pixels[p] == c)
                pixels[p] = new Color(c.r - 0.00392156862f, c.g - 0.00392156862f, c.b - 0.00392156862f, c.a);
        }
        Texture2D n = new Texture2D(i.width, i.height);
        n.SetPixels(0, 0, i.width, i.height, pixels, 0);
        n.Apply();
        return n;
    }

    public static Sprite getSpriteFromString(string imageString)
    {
        Image image = new Image(imageString);
        return Sprite.Create(image.getTexture(), new Rect(0, 0, image.getTexture().width, image.getTexture().height), new Vector2(0.5f, 0.5f), 100);
    }

    public static Color getColorFromString(string colorString)
    {
        string[] colorAttributes = new string[colorString.Length / 2];
        for (int characterIndex = 0; characterIndex < colorString.Length / 2; ++characterIndex)
        {
            colorAttributes[characterIndex] = colorString.Substring(characterIndex * 2, 2);
            if (colorAttributes[characterIndex] == "00")
                colorAttributes[characterIndex] = "0";
        }
        return new Color((float)Convert.ToInt32(colorAttributes[0], 16) / 255, (float)Convert.ToInt32(colorAttributes[1], 16) / 255, (float)Convert.ToInt32(colorAttributes[2], 16) / 255, (float)Convert.ToInt32(colorAttributes[3], 16) / 255);
    }
}