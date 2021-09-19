using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropdownAdjuster : Adjuster
{
    private readonly string DEFAULT_OPTION;
    private readonly string[] OPTIONS;
    private int currentOptionIndex;

    public DropdownAdjuster(Color colorScheme, string labelText, string description, string defaultOption, string[] options, int currentOptionIndex) : base(colorScheme, labelText, description)
    {
        DEFAULT_OPTION = defaultOption;
        OPTIONS = options;
        this.currentOptionIndex = currentOptionIndex;
        base.GAME_OBJECT.transform.Find("Dropdown").gameObject.GetComponent<TMP_Dropdown>().ClearOptions();
        List<TMP_Dropdown.OptionData> sortOptions = new List<TMP_Dropdown.OptionData>();
        sortOptions.Add(new TMP_Dropdown.OptionData(DEFAULT_OPTION));
        foreach (string option in OPTIONS)
            sortOptions.Add(new TMP_Dropdown.OptionData(option));
        base.GAME_OBJECT.transform.Find("Dropdown").gameObject.GetComponent<TMP_Dropdown>().AddOptions(sortOptions);
        base.GAME_OBJECT.transform.Find("Dropdown").gameObject.GetComponent<TMP_Dropdown>().value = this.currentOptionIndex + 1;
    }

    public override string getValue()
    {
        return OPTIONS[currentOptionIndex];
    }

    public override void update(Color colorScheme)
    {
        base.colorScheme = colorScheme;
        if (base.GAME_OBJECT.transform.Find("Dropdown").gameObject.GetComponent<UnityEngine.UI.Image>().color != base.colorScheme)
            base.GAME_OBJECT.transform.Find("Dropdown").gameObject.GetComponent<UnityEngine.UI.Image>().color = base.colorScheme;
        if (base.GAME_OBJECT.transform.Find("Dropdown").gameObject.GetComponent<TMP_Dropdown>().value != 0 && base.GAME_OBJECT.transform.Find("Dropdown").gameObject.GetComponent<TMP_Dropdown>().value != currentOptionIndex + 1)
            currentOptionIndex = base.GAME_OBJECT.transform.Find("Dropdown").gameObject.GetComponent<TMP_Dropdown>().value - 1;
    }
}