using System;
using UnityEngine;
using TMPro;

public class GamePlayMenu : PeripheralElement
{
    private Dimension size;
    private Vector2 centerPoint;
    private readonly GameObject CANVAS, MENU;
    public enum BUTTONS { BACK_TO_GAME, SETTINGS , QUIT_TO_BUILD_HUB };
    private static readonly int NUMBER_OF_BUTTONS = Enum.GetNames(typeof(BUTTONS)).Length;
    private readonly bool[] BUTTON_STATES;
    private readonly GameObject[] MENU_BUTTONS, MENU_BUTTON_LABELS;
    private readonly string DYNAMIC_CANVAS_NAME = "FieldStaticCanvas", MENU_BUTTON_LABEL_NAME = "Label";
    private readonly Color MENU_COLOR = new Color(0, 0, 0, .65f);
    private SettingPairs settingPairs;
    private string gameMenuColorString;

    public GamePlayMenu(SettingPairs settingPairs)
    {
        this.settingPairs = settingPairs;
        gameMenuColorString = this.settingPairs.game_menu_color;
        CANVAS = GameObject.Find(DYNAMIC_CANVAS_NAME);
        MENU = GameObject.Instantiate(Resources.Load("Prefabs/Menu") as GameObject);
        MENU.transform.SetParent(CANVAS.transform);
        MENU.transform.localPosition = Vector3.zero;
        BUTTON_STATES = new bool[NUMBER_OF_BUTTONS];
        MENU_BUTTONS = new GameObject[NUMBER_OF_BUTTONS];
        MENU_BUTTON_LABELS = new GameObject[NUMBER_OF_BUTTONS];
        for (int buttonIndex = 0; buttonIndex < NUMBER_OF_BUTTONS; ++buttonIndex)
        {
            GameObject button = GameObject.Instantiate(Resources.Load("Prefabs/MenuButton") as GameObject);
            GameObject buttonLabel = button.transform.Find(MENU_BUTTON_LABEL_NAME).gameObject;
            buttonLabel.GetComponent<TextMeshProUGUI>().text = processButtonLabel(buttonIndex);
            button.transform.SetParent(MENU.transform);
            button.transform.localScale = Vector3.one;
            MENU_BUTTONS[buttonIndex] = button;
            MENU_BUTTON_LABELS[buttonIndex] = buttonLabel;
            MENU_BUTTONS[buttonIndex].GetComponent<UnityEngine.UI.Image>().color = ImageTools.getColorFromString(gameMenuColorString);
        }
        MENU.SetActive(false);
        calculatePoints();
    }

    protected override void calculatePoints()
    {
        
    }

    public override void update()
    {
        
    }

    public void update(SettingPairs settingPairs)
    {
        if (MENU != null)
        {
            this.settingPairs = settingPairs;
            Color gameMenuColor = default;
            if (gameMenuColorString != this.settingPairs.game_menu_color)
            {
                gameMenuColorString = this.settingPairs.game_menu_color;
                gameMenuColor = ImageTools.getColorFromString(gameMenuColorString);
            }
            MENU.SetActive(base.enabled);
            if (base.enabled)
            {
                MENU.transform.SetAsLastSibling();
                for (int buttonIndex = 0; buttonIndex < NUMBER_OF_BUTTONS; ++buttonIndex)
                {
                    BUTTON_STATES[buttonIndex] = MENU_BUTTONS[buttonIndex].GetComponent<ButtonListener>().isClicked();
                    if (gameMenuColor != default)
                        MENU_BUTTONS[buttonIndex].GetComponent<UnityEngine.UI.Image>().color = gameMenuColor;
                }
            }
        }
    }

    private string processButtonLabel(int buttonIndex)
    {
        string buttonLabel = ((BUTTONS)buttonIndex).ToString();
        buttonLabel = buttonLabel.Replace('_', ' ').ToLower();
        buttonLabel = buttonLabel[0].ToString().ToUpper() + buttonLabel.Substring(1);
        return buttonLabel;
    }

    public bool[] getButtonStates()
    {
        return BUTTON_STATES;
    }

    public void resetButtonStates()
    {
        for (int buttonIndex = 0; buttonIndex < NUMBER_OF_BUTTONS; ++buttonIndex)
        {
            BUTTON_STATES[buttonIndex] = false;
            if (MENU_BUTTONS[buttonIndex] != null)
                MENU_BUTTONS[buttonIndex].GetComponent<ButtonListener>().resetClick();
        }
    }
}