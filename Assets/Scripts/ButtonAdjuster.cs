using UnityEngine;
using TMPro;

public class ButtonAdjuster : Adjuster
{
    private readonly string BUTTON_LABEL;
    private bool isActive;

    public ButtonAdjuster(Color colorScheme, string labelText, string description, string buttonLabel, bool isActive) : base(colorScheme, labelText, description)
    {
        BUTTON_LABEL = buttonLabel != null ? buttonLabel : "";
        this.isActive = isActive;
        base.GAME_OBJECT.transform.Find("Button").Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text = buttonLabel;
        base.GAME_OBJECT.transform.Find("Button").GetComponent<UnityEngine.UI.Image>().color = base.colorScheme;
    }

    public override string getValue()
    {
        return isActive.ToString().ToLower();
    }

    public override void update(Color colorScheme)
    {
        base.colorScheme = colorScheme;
        if (base.GAME_OBJECT.transform.Find("Button").GetComponent<UnityEngine.UI.Image>().color != base.colorScheme)
            base.GAME_OBJECT.transform.Find("Button").GetComponent<UnityEngine.UI.Image>().color = base.colorScheme;
        if (base.GAME_OBJECT.transform.Find("Button").gameObject.GetComponent<ButtonListener>().isClicked())
        {
            isActive = !isActive;
            base.GAME_OBJECT.transform.Find("Button").gameObject.GetComponent<ButtonListener>().resetClick();
        }
    }
}