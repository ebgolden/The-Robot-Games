using System;
using UnityEngine;
using TMPro;

public class SliderAdjuster : Adjuster
{
    private readonly float MIN_VALUE, MAX_VALUE;
    private readonly string UNIT;
    private float currentValue, currentSliderValue;

    public SliderAdjuster(Color colorScheme, string labelText, string description, float minValue, float maxValue, float currentValue, string unit) : base(colorScheme, labelText, description)
    {
        MIN_VALUE = minValue;
        MAX_VALUE = maxValue;
        UNIT = unit;
        base.GAME_OBJECT.transform.Find("Panel").Find("Unit").gameObject.GetComponent<TextMeshProUGUI>().text = unit;
        this.currentValue = currentValue;
        currentSliderValue = 0;
        changeText();
        changeSlider();
    }

    private void changeText()
    {
        base.GAME_OBJECT.transform.Find("Panel").Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text = StringTools.formatString(currentValue);
        currentSliderValue = base.GAME_OBJECT.transform.Find("Slider").gameObject.GetComponent<UnityEngine.UI.Slider>().value;
    }

    private void changeSlider()
    {
        base.GAME_OBJECT.transform.Find("Slider").gameObject.GetComponent<UnityEngine.UI.Slider>().value = (currentValue - MIN_VALUE) / (MAX_VALUE - MIN_VALUE);
        currentSliderValue = base.GAME_OBJECT.transform.Find("Slider").gameObject.GetComponent<UnityEngine.UI.Slider>().value;
    }

    public override string getValue()
    {
        return currentValue.ToString();
    }

    public override void update(Color colorScheme)
    {
        base.colorScheme = colorScheme;
        if (base.GAME_OBJECT.transform.Find("Slider").Find("Fill Area").Find("Fill").gameObject.GetComponent<UnityEngine.UI.Image>().color != base.colorScheme)
            base.GAME_OBJECT.transform.Find("Slider").Find("Fill Area").Find("Fill").gameObject.GetComponent<UnityEngine.UI.Image>().color = base.colorScheme;
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2) || Input.GetKey(KeyCode.Tab) || Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
        {
            base.GAME_OBJECT.transform.Find("Panel").Find("CurrentValue").Find("Text Area").Find("Text").gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            base.GAME_OBJECT.transform.Find("Panel").Find("CurrentValue").Find("Text Area").Find("Text").gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(50, 0);
        }
        if (base.GAME_OBJECT.transform.Find("Slider").gameObject.GetComponent<UnityEngine.UI.Slider>().value != currentSliderValue)
        {
            currentValue = base.GAME_OBJECT.transform.Find("Slider").gameObject.GetComponent<UnityEngine.UI.Slider>().value * (MAX_VALUE - MIN_VALUE) + MIN_VALUE;
            changeText();
        }
        if (base.GAME_OBJECT.transform.Find("Panel").Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text != currentValue.ToString())
        {
            float currentTextValue = 0;
            try
            {
                currentTextValue = float.Parse(base.GAME_OBJECT.transform.Find("Panel").Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text);
                if (currentTextValue >= MIN_VALUE && currentTextValue <= MAX_VALUE)
                    currentValue = currentTextValue;
                else throw new FormatException();
            }
            catch
            {
                changeText();
            }
            changeSlider();
        }
    }
}