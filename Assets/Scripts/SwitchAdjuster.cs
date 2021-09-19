using UnityEngine;
using TMPro;

public class SwitchAdjuster : Adjuster
{
    private readonly string FIRST_STATE_LABEL, SECOND_STATE_LABEL;
    private bool inFirstState;

    public SwitchAdjuster(Color colorScheme, string labelText, string description, string firstStateLabel, string secondStateLabel, bool inFirstState) : base(colorScheme, labelText, description)
    {
        FIRST_STATE_LABEL = firstStateLabel;
        SECOND_STATE_LABEL = secondStateLabel;
        this.inFirstState = inFirstState;
        base.GAME_OBJECT.transform.Find("FirstState").Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text = firstStateLabel;
        base.GAME_OBJECT.transform.Find("SecondState").Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text = secondStateLabel;
        base.colorScheme = colorScheme;
        changeButtonColors();
    }

    private void changeButtonColors()
    {
        if (base.GAME_OBJECT.transform.Find("FirstState").gameObject.GetComponent<UnityEngine.UI.Image>().color != (!inFirstState ? base.INACTIVE_COLOR : base.colorScheme))
            base.GAME_OBJECT.transform.Find("FirstState").gameObject.GetComponent<UnityEngine.UI.Image>().color = (!inFirstState ? base.INACTIVE_COLOR : base.colorScheme);
        if (base.GAME_OBJECT.transform.Find("SecondState").gameObject.GetComponent<UnityEngine.UI.Image>().color != (inFirstState ? base.INACTIVE_COLOR : base.colorScheme))
            base.GAME_OBJECT.transform.Find("SecondState").gameObject.GetComponent<UnityEngine.UI.Image>().color = (inFirstState ? base.INACTIVE_COLOR : base.colorScheme);
    }

    public override string getValue()
    {
        return inFirstState.ToString().ToLower();
    }

    public override void update(Color colorScheme)
    {
        base.colorScheme = colorScheme;
        changeButtonColors();
        if (base.GAME_OBJECT.transform.Find("FirstState").gameObject.GetComponent<ButtonListener>().isClicked())
        {
            inFirstState = true;
            base.GAME_OBJECT.transform.Find("FirstState").gameObject.GetComponent<ButtonListener>().resetClick();
        }
        if (base.GAME_OBJECT.transform.Find("SecondState").gameObject.GetComponent<ButtonListener>().isClicked())
        {
            inFirstState = false;
            base.GAME_OBJECT.transform.Find("SecondState").gameObject.GetComponent<ButtonListener>().resetClick();
        }
    }
}