using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class InputAdjuster : Adjuster
{
    private readonly string PLACEHOLDER;
    private readonly bool MAP_KEYS;
    private string currentValue;

    public InputAdjuster(Color colorScheme, string labelText, string description, string placeholder, string currentValue, bool mapsKeys) : base(colorScheme, labelText, description)
    {
        PLACEHOLDER = placeholder;
        MAP_KEYS = mapsKeys;
        this.currentValue = currentValue;
        base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text = currentValue;
        base.GAME_OBJECT.transform.Find("CurrentValue").Find("Text Area").Find("Placeholder").gameObject.GetComponent<TextMeshProUGUI>().text = PLACEHOLDER;
    }

    public override string getValue()
    {
        return currentValue;
    }

    public override void update(Color colorScheme)
    {
        base.colorScheme = colorScheme;
        if (base.GAME_OBJECT.transform.Find("CurrentValue").GetComponent<UnityEngine.UI.Image>().color != base.colorScheme)
            base.GAME_OBJECT.transform.Find("CurrentValue").GetComponent<UnityEngine.UI.Image>().color = base.colorScheme;
        if (!MAP_KEYS)
        {
            if (base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text == default || base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text.Length == 0 || (base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text.Length == 1 && (int)(base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text[0]) == 8203))
                base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text = currentValue;
            else if (base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text != currentValue)
                currentValue = base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text;
        }
        else if (EventSystem.current.currentSelectedGameObject == base.GAME_OBJECT.transform.Find("CurrentValue").gameObject && !(Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) && Input.anyKey)
        {
            if (base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text == default || base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text.Length == 0 || (base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text.Length == 1 && (int)(base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text[0]) == 8203))
                base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text = currentValue;
            else
            {
                foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKey(vKey))
                    {
                        currentValue = vKey.ToString();
                        base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text = currentValue;
                        break;
                    }
                }
            }
        }
        base.GAME_OBJECT.transform.Find("CurrentValue").gameObject.GetComponent<TMP_InputField>().text = currentValue;
    }
}