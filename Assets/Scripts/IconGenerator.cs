using UnityEngine;

public class IconGenerator
{
	public GameObject gameObjectOfIcon;
    public Camera camera;
    public Part part;
    private Texture2D icon;

    public IconGenerator()
    {
        gameObjectOfIcon = null;
        camera = null;
    }
    
	public void initialize()
	{
        if (gameObjectOfIcon != null && camera != null)
        {
            Texture2D texture = (Texture2D)gameObjectOfIcon.GetComponent<Renderer>().material.mainTexture;
            icon = new Texture2D(256, 256, TextureFormat.RGB24, false);
            RenderTexture renderTexture = camera.targetTexture;
            RenderTexture.active = renderTexture;
            icon.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            icon.Apply();
            icon = ImageTools.removeBadColors(icon);
        }
	}

    public Texture2D getIcon()
    {
        return icon;
    }
}