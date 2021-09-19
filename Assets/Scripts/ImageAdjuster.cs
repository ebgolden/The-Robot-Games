using System.Collections.Generic;
using System;
using UnityEngine;

public class ImageAdjuster : Adjuster
{
    private readonly List<Image> IMAGES;
    private string currentImage;
    private readonly List<GameObject> IMAGE_BUTTONS;

    public ImageAdjuster(Color colorScheme, string labelText, string description, string[] images, string currentImage) : base(colorScheme, labelText, description)
    {
        IMAGES = new List<Image>();
        foreach (string image in images)
            IMAGES.Add(new Image(image));
        this.currentImage = currentImage;
        IMAGE_BUTTONS = new List<GameObject>();
        foreach (Image image in IMAGES)
        {
            GameObject imageButton = GameObject.Instantiate(Resources.Load("Prefabs/ImageButton") as GameObject);
            imageButton.GetComponent<UnityEngine.UI.Image>().enabled = false;
            imageButton.transform.Find("Icon").gameObject.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(image.getTexture(), new Rect(0, 0, image.getTexture().width, image.getTexture().height), new Vector2(0.5f, 0.5f), 100);
            imageButton.transform.SetParent(base.GAME_OBJECT.transform.Find("ImageButtons"));
            imageButton.transform.localPosition = new Vector3(imageButton.transform.localPosition.x, imageButton.transform.localPosition.y, 0);
            IMAGE_BUTTONS.Add(imageButton);
        }
        changeImageBorders();
    }

    private void changeImageBorders()
    {
        for (int imageButtonIndex = 0; imageButtonIndex < IMAGE_BUTTONS.Count; ++imageButtonIndex)
        {
            IMAGE_BUTTONS[imageButtonIndex].GetComponent<UnityEngine.UI.Image>().enabled = Convert.ToBase64String(IMAGES[imageButtonIndex].getTexture().EncodeToPNG()) == currentImage;
            if (IMAGE_BUTTONS[imageButtonIndex].GetComponent<UnityEngine.UI.Image>().color != base.colorScheme)
                IMAGE_BUTTONS[imageButtonIndex].GetComponent<UnityEngine.UI.Image>().color = base.colorScheme;
        }
    }

    public override string getValue()
    {
        return currentImage;
    }

    public override void update(Color colorScheme)
    {
        if (base.colorScheme != colorScheme)
        {
            base.colorScheme = colorScheme;
            changeImageBorders();
        }
        for (int imageButtonIndex = 0; imageButtonIndex < IMAGE_BUTTONS.Count; ++imageButtonIndex)
        {
            if (IMAGE_BUTTONS[imageButtonIndex].GetComponent<ButtonListener>().isClicked())
            {
                currentImage = Convert.ToBase64String(IMAGES[imageButtonIndex].getTexture().EncodeToPNG());
                IMAGE_BUTTONS[imageButtonIndex].GetComponent<ButtonListener>().resetClick();
                changeImageBorders();
                break;
            }
        }
    }
}