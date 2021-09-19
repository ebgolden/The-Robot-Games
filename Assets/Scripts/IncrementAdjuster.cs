using System;
using UnityEngine;
using TMPro;

public class IncrementAdjuster : Adjuster
{
    private readonly int MIN_VALUE, MAX_VALUE;
    private int currentValue;

    public IncrementAdjuster(Color colorScheme, string labelText, string description, int minValue, int maxValue, int currentValue) : base(colorScheme, labelText, description)
    {
        MIN_VALUE = minValue;
        MAX_VALUE = maxValue;
        this.currentValue = currentValue;
        changeText();
        changeButtonColors();
    }

    private void changeText()
    {
        base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text = currentValue.ToString();
    }

    private void changeButtonColors()
    {
        if (base.GAME_OBJECT.transform.Find("Decrease").gameObject.GetComponent<UnityEngine.UI.Image>().color != (currentValue <= MIN_VALUE ? base.INACTIVE_COLOR : base.colorScheme))
            base.GAME_OBJECT.transform.Find("Decrease").gameObject.GetComponent<UnityEngine.UI.Image>().color = (currentValue <= MIN_VALUE ? base.INACTIVE_COLOR : base.colorScheme);
        if (base.GAME_OBJECT.transform.Find("Increase").gameObject.GetComponent<UnityEngine.UI.Image>().color != (currentValue >= MAX_VALUE ? base.INACTIVE_COLOR : base.colorScheme))
            base.GAME_OBJECT.transform.Find("Increase").gameObject.GetComponent<UnityEngine.UI.Image>().color = (currentValue >= MAX_VALUE ? base.INACTIVE_COLOR : base.colorScheme);
    }

    public override string getValue()
    {
        return currentValue.ToString();
    }

    public override void update(Color colorScheme)
    {
        base.colorScheme = colorScheme;
        changeButtonColors();
        if (base.GAME_OBJECT.transform.Find("Decrease").gameObject.GetComponent<ButtonListener>().isClicked())
        {
            if (currentValue > MIN_VALUE)
            {
                --currentValue;
                changeText();
            }
            base.GAME_OBJECT.transform.Find("Decrease").gameObject.GetComponent<ButtonListener>().resetClick();
        }
        if (base.GAME_OBJECT.transform.Find("Increase").gameObject.GetComponent<ButtonListener>().isClicked())
        {
            if (currentValue < MAX_VALUE)
            {
                ++currentValue;
                changeText();
            }
            base.GAME_OBJECT.transform.Find("Increase").gameObject.GetComponent<ButtonListener>().resetClick();
        }
        if (base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text != currentValue.ToString())
        {
            int currentTextValue = 0;
            try
            {
                currentTextValue = int.Parse(base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text);
            }
            catch
            {
                currentTextValue = currentValue;
            }
            if ((currentTextValue < currentValue && currentTextValue >= MIN_VALUE) || (currentTextValue > currentValue && currentTextValue <= MAX_VALUE))
                currentValue = currentTextValue;
            changeText();
        }
    }
}