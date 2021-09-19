using System;
using UnityEngine;

public class Image
{
    private readonly string TEXTURE_STRING;
    private Texture2D texture;
    public readonly int WIDTH;
    public readonly int HEIGHT;
    public readonly int DEPTH;

    public Image(string textureString)
    {
        TEXTURE_STRING = textureString;
        texture = new Texture2D(1, 1);
        try
        {
            texture.LoadImage(Convert.FromBase64String(textureString));
        }
        catch {}
        if (texture != null)
        {
            WIDTH = texture.width;
            HEIGHT = texture.height;
            DEPTH = texture.height;
        }
        else
        {
            WIDTH = 0;
            HEIGHT = 0;
            DEPTH = 0;
        }
    }

    public Texture2D getTexture()
    {
        return texture;
    }

    public void setTexture(Texture2D texture)
    {
        this.texture = texture;
    }

    public int getWidth()
    {
        return WIDTH;
    }

    public int getHeight()
    {
        return HEIGHT;
    }

    public int getDepth()
    {
        return DEPTH;
    }

    public override string ToString()
    {
        return TEXTURE_STRING;
    }
}