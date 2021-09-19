using System.Collections.Generic;
using UnityEngine;

public class ColorAdjuster : Adjuster
{
    private readonly List<Color> COLORS;
    private string currentColor;
    private readonly List<GameObject> COLOR_BUTTONS;

    public ColorAdjuster(Color colorScheme, string labelText, string description, string[] colors, string currentColor) : base(colorScheme, labelText, description)
    {
        COLORS = new List<Color>();
        foreach (string color in colors)
            COLORS.Add(ImageTools.getColorFromString(color));
        this.currentColor = currentColor;
        COLOR_BUTTONS = new List<GameObject>();
        foreach (Color color in COLORS)
        {
            GameObject colorButton = GameObject.Instantiate(Resources.Load("Prefabs/ColorButton") as GameObject);
            colorButton.GetComponent<UnityEngine.UI.Image>().enabled = false;
            colorButton.transform.Find("ColorIcon").gameObject.GetComponent<UnityEngine.UI.Image>().color = color;
            colorButton.transform.SetParent(base.GAME_OBJECT.transform.Find("ColorButtons"));
            colorButton.transform.localPosition = new Vector3(colorButton.transform.localPosition.x, colorButton.transform.localPosition.y, 0);
            COLOR_BUTTONS.Add(colorButton);
        }
        changeColorBorders();
    }

    private void changeColorBorders()
    {
        for (int colorButtonIndex = 0; colorButtonIndex < COLOR_BUTTONS.Count; ++colorButtonIndex)
        {
            COLOR_BUTTONS[colorButtonIndex].GetComponent<UnityEngine.UI.Image>().enabled = getStringFromColor(COLORS[colorButtonIndex]) == currentColor;
            if (COLOR_BUTTONS[colorButtonIndex].GetComponent<UnityEngine.UI.Image>().color != (getStringFromColor(base.colorScheme) == currentColor ? Color.white : base.colorScheme))
                COLOR_BUTTONS[colorButtonIndex].GetComponent<UnityEngine.UI.Image>().color = getStringFromColor(base.colorScheme) == currentColor ? Color.white : base.colorScheme;
        }
    }

    private string getStringFromColor(Color color)
    {
        string colorString = "";
        string[] colorAttributes = new string[4];
        colorAttributes[0] = Mathf.RoundToInt(255 * color.r).ToString("X");
        colorAttributes[1] = Mathf.RoundToInt(255 * color.g).ToString("X");
        colorAttributes[2] = Mathf.RoundToInt(255 * color.b).ToString("X");
        colorAttributes[3] = Mathf.RoundToInt(255 * color.a).ToString("X");
        foreach (string colorAttribute in colorAttributes)
            colorString += (colorAttribute.Length == 1 ? "0" : "") + colorAttribute;
        return colorString;
    }

    public override string getValue()
    {
        return currentColor;
    }

    public override void update(Color colorScheme)
    {
        if (base.colorScheme != colorScheme)
        {
            base.colorScheme = colorScheme;
            changeColorBorders();
        }
        for (int colorButtonIndex = 0; colorButtonIndex < COLOR_BUTTONS.Count; ++colorButtonIndex)
        {
            if (COLOR_BUTTONS[colorButtonIndex].GetComponent<ButtonListener>().isClicked())
            {
                currentColor = getStringFromColor(COLORS[colorButtonIndex]);
                COLOR_BUTTONS[colorButtonIndex].GetComponent<ButtonListener>().resetClick();
                changeColorBorders();
                break;
            }
        }
    }
}