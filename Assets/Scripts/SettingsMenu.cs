using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsMenu : PeripheralElement
{
    private readonly List<Setting> CURRENT_SETTINGS;
    private readonly GameObject GAME_OBJECT, CANVAS, SETTINGS_INFO, BACK_BUTTON, SCROLLBAR;
    private readonly RectTransform SETTINGS_CONTAINER, SETTINGS_PANEL;
    private readonly ScrollRect SCROLL_RECT;
    private readonly List<GameObject> SETTINGS_TABS;
    private readonly List<GameObject>[] SETTINGS_IN_TABS;
    private readonly List<Adjuster>[] ADJUSTERS_IN_TABS;
    private KeyCode menuToggle = KeyCode.Escape;
    private readonly int DEFAULT_TAB_INDEX = 0;
    private readonly Color ACTIVE_TAB_COLOR = new Color(0, 0, 0, 0.2f), INACTIVE_TAB_COLOR = new Color(0, 0, 0, 0.39215686274f);
    private readonly Point MIN_SETTING_INFO_BUILD_HUB = new Point(-446.5115, -80), MAX_SETTING_INFO_BUILD_HUB = new Point(432.4064, 80), MIN_SETTING_INFO_FIELD = new Point(-446.5115, -80), MAX_SETTING_INFO_FIELD = new Point(432.4064, 80);
    private int currentSettingFilterIndex;
    private Color colorScheme;
    private bool resetSettings;

    public SettingsMenu(List<Setting> settingList, Color colorScheme, KeyCode menuToggle, bool resetSettings)
    {
        CURRENT_SETTINGS = settingList;
        this.colorScheme = colorScheme;
        this.menuToggle = menuToggle;
        this.resetSettings = resetSettings;
        GAME_OBJECT = GameObject.Instantiate(Resources.Load("Prefabs/Settings") as GameObject);
        SETTINGS_INFO = GAME_OBJECT.transform.Find("SettingInfo").gameObject;
        SETTINGS_CONTAINER = GAME_OBJECT.transform.Find("SettingsCard").Find("SettingsContainer").gameObject.GetComponent<RectTransform>();
        SETTINGS_PANEL = GAME_OBJECT.transform.Find("SettingsCard").Find("SettingsContainer").Find("SettingsPanel").gameObject.GetComponent<RectTransform>();
        SCROLLBAR = GAME_OBJECT.transform.Find("SettingsCard").Find("SettingsScrollbar").gameObject;
        GameObject SCROLL_VIEW = GAME_OBJECT.transform.Find("SettingsCard").gameObject;
        SCROLL_RECT = SCROLL_VIEW.GetComponent<ScrollRect>();
        BACK_BUTTON = GAME_OBJECT.transform.Find("BackButton").gameObject;
        SETTINGS_TABS = new List<GameObject>();
        foreach (Transform child in GAME_OBJECT.transform.Find("SettingsTabs").Find("SettingsTabMask"))
            SETTINGS_TABS.Add(child.gameObject);
        SETTINGS_IN_TABS = new List<GameObject>[SETTINGS_TABS.Count];
        ADJUSTERS_IN_TABS = new List<Adjuster>[SETTINGS_TABS.Count];
        for (int tabIndex = 0; tabIndex < SETTINGS_TABS.Count; ++tabIndex)
            createGameObjectsForSettings(CURRENT_SETTINGS, tabIndex);
        SETTINGS_INFO.SetActive(false);
        GAME_OBJECT.SetActive(false);
        GameObject[] sceneGameObject = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject gameObject in sceneGameObject)
        {
            if (gameObject.name == "BuildHubCanvas" || gameObject.name == "FieldDynamicCanvas")
            {
                CANVAS = gameObject;
                break;
            }
        }
        currentSettingFilterIndex = DEFAULT_TAB_INDEX;
        update();
        filterSettings(currentSettingFilterIndex);
        updateSettings(colorScheme, menuToggle, resetSettings);
    }

    public void updateSettings(Color colorScheme, KeyCode menuToggle, bool resetSettings)
    {
        this.colorScheme = colorScheme;
        this.menuToggle = menuToggle;
        this.resetSettings = resetSettings;
        if (resetSettings)
        {
            CURRENT_SETTINGS.Clear();
            SettingsManager settingsManager = new SettingsManager();
            List<Setting> settingList = settingsManager.getSettingList(SettingsManager.DEFAULT_SETTINGS);
            CURRENT_SETTINGS.AddRange(settingList);
            for (int tabIndex = 0; tabIndex < SETTINGS_TABS.Count; ++tabIndex)
            {
                foreach (GameObject gameObject in SETTINGS_IN_TABS[tabIndex])
                    GameObject.Destroy(gameObject);
                foreach (Adjuster adjuster in ADJUSTERS_IN_TABS[tabIndex])
                    GameObject.Destroy(adjuster.GAME_OBJECT);
                SETTINGS_IN_TABS[tabIndex].Clear();
                ADJUSTERS_IN_TABS[tabIndex].Clear();
                createGameObjectsForSettings(CURRENT_SETTINGS, tabIndex);
            }
            filterSettings(currentSettingFilterIndex);
        }
    }

    private void createGameObjectsForSettings(List<Setting> settings, int tabIndex)
    {
        List<GameObject> settingsInTab = new List<GameObject>();
        List<Adjuster> adjustersInTab = new List<Adjuster>();
        foreach(Setting setting in settings)
        {
            if (setting.category == SETTINGS_TABS[tabIndex].transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text)
            {
                GameObject settingGameObject = GameObject.Instantiate(Resources.Load("Prefabs/Setting") as GameObject);
                settingGameObject.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = setting.name;
                Adjuster adjuster = null;
                switch (setting.adjustmentMethod)
                {
                    case "Color":
                        adjuster = new ColorAdjuster(colorScheme, setting.name, setting.description, setting.options != null ? setting.options : new string[] { }, setting.currentValue != null ? setting.currentValue : "");
                        break;
                    case "Dropdown":
                        adjuster = new DropdownAdjuster(colorScheme, setting.name, setting.description, setting.defaultValue != null ? setting.defaultValue : "", setting.options != null ? setting.options : new string[] { }, setting.currentValue != null ? int.Parse(setting.currentValue) : 0);
                        break;
                    case "Image":
                        adjuster = new ImageAdjuster(colorScheme, setting.name, setting.description, setting.options != null ? setting.options : new string[] { }, setting.currentValue != null ? setting.currentValue : "");
                        break;
                    case "Increment":
                        adjuster = new IncrementAdjuster(colorScheme, setting.name, setting.description, setting.options != null && setting.options.Length >= 1 ? int.Parse(setting.options[0]) : 0, setting.options != null && setting.options.Length >= 2 ? int.Parse(setting.options[1]) : 0, setting.currentValue != null ? int.Parse(setting.currentValue) : 0);
                        break;
                    case "Input":
                        adjuster = new InputAdjuster(colorScheme, setting.name, setting.description, setting.placeholder != null ? setting.placeholder : "", setting.currentValue != null ? setting.currentValue : "", setting.options != null && setting.options.Length >= 1 ? bool.Parse(setting.options[0]) : false);
                        break;
                    case "Slider":
                        adjuster = new SliderAdjuster(colorScheme, setting.name, setting.description, setting.options != null && setting.options.Length >= 1 ? float.Parse(setting.options[0]) : 0, setting.options != null && setting.options.Length >= 2 ? float.Parse(setting.options[1]) : 0, setting.currentValue != null ? float.Parse(setting.currentValue) : 0, setting.options != null && setting.options.Length >= 3 ? setting.options[2] : "");
                        break;
                    case "Switch":
                        adjuster = new SwitchAdjuster(colorScheme, setting.name, setting.description, setting.options != null && setting.options.Length >= 1 ? setting.options[0] : "", setting.options != null && setting.options.Length >= 2 ? setting.options[1] : "", setting.currentValue != null && bool.Parse(setting.currentValue.ToLower()));
                        break;
                    case "Button":
                    default:
                        adjuster = new ButtonAdjuster(colorScheme, setting.name, setting.description, setting.options != null && setting.options.Length >= 1 ? setting.options[0] : "", setting.currentValue != null && bool.Parse(setting.currentValue.ToLower()));
                        break;
                }
                adjuster.GAME_OBJECT.transform.SetParent(settingGameObject.transform.Find("AdjustmentMethod"));
                settingsInTab.Add(settingGameObject);
                adjustersInTab.Add(adjuster);
            }
        }
        SETTINGS_IN_TABS[tabIndex] = settingsInTab;
        ADJUSTERS_IN_TABS[tabIndex] = adjustersInTab;
    }

    protected override void calculatePoints()
    {

    }

    public override void update()
    {
        if (base.enabled)
        {
            if (!GAME_OBJECT.activeInHierarchy)
            {
                GAME_OBJECT.SetActive(true);
                GAME_OBJECT.transform.SetParent(default);
                GAME_OBJECT.transform.SetParent(CANVAS.transform);
                GAME_OBJECT.transform.SetAsLastSibling();
                GAME_OBJECT.transform.localPosition = new Vector3(GAME_OBJECT.transform.localPosition.x, GAME_OBJECT.transform.localPosition.y, 0);
                GAME_OBJECT.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                GAME_OBJECT.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                GAME_OBJECT.GetComponent<RectTransform>().sizeDelta = Vector3.one;
                GAME_OBJECT.transform.localScale = Vector3.one;
            }
            if ((SETTINGS_PANEL.offsetMax.y - SETTINGS_PANEL.offsetMin.y) > (SETTINGS_CONTAINER.offsetMax.y - SETTINGS_CONTAINER.offsetMin.y))
            {
                SCROLLBAR.SetActive(true);
                SCROLL_RECT.vertical = true;
            }
            else
            {
                SCROLLBAR.SetActive(false);
                SCROLL_RECT.vertical = false;
            }
            for (int settingIndex = 0; settingIndex < SETTINGS_IN_TABS[currentSettingFilterIndex].Count; ++settingIndex)
            {
                ButtonListener buttonListener = SETTINGS_IN_TABS[currentSettingFilterIndex][settingIndex].GetComponent<ButtonListener>();
                if (buttonListener.isMouseOver())
                {
                    Adjuster adjuster = ADJUSTERS_IN_TABS[currentSettingFilterIndex][settingIndex];
                    SETTINGS_INFO.transform.Find("SettingName").gameObject.GetComponent<TextMeshProUGUI>().text = adjuster.getName();
                    SETTINGS_INFO.transform.Find("SettingDescription").gameObject.GetComponent<TextMeshProUGUI>().text = adjuster.getDescription();
                    SETTINGS_INFO.SetActive(true);
                    break;
                }
                else SETTINGS_INFO.SetActive(false);
            }
            bool switchTabs = false;
            for (int tabIndex = 0; tabIndex < SETTINGS_TABS.Count; ++tabIndex)
            {
                GameObject tab = SETTINGS_TABS[tabIndex];
                if (tab.GetComponent<ButtonListener>().isClicked() && currentSettingFilterIndex != tabIndex)
                {
                    switchTabs = true;
                    filterSettings(tabIndex);
                    break;
                }
                else tab.GetComponent<ButtonListener>().resetClick();
            }
            if (switchTabs)
            {
                foreach (GameObject tab in SETTINGS_TABS)
                {
                    if (tab.GetComponent<ButtonListener>().isClicked())
                    {
                        tab.GetComponent<ButtonListener>().resetClick();
                        tab.GetComponent<UnityEngine.UI.Image>().color = ACTIVE_TAB_COLOR;
                    }
                    else tab.GetComponent<UnityEngine.UI.Image>().color = INACTIVE_TAB_COLOR;
                }
            }
            for (int adjusterIndex = 0; adjusterIndex < ADJUSTERS_IN_TABS[currentSettingFilterIndex].Count; ++adjusterIndex)
            {
                Adjuster adjuster = ADJUSTERS_IN_TABS[currentSettingFilterIndex][adjusterIndex];
                adjuster.update(colorScheme);
                for (int settingIndex = 0; settingIndex < CURRENT_SETTINGS.Count; ++settingIndex)
                {
                    if (CURRENT_SETTINGS[settingIndex].name == adjuster.getName())
                    {
                        if (CURRENT_SETTINGS[settingIndex].currentValue != adjuster.getValue())
                            CURRENT_SETTINGS[settingIndex].currentValue = adjuster.getValue();
                        break;
                    }
                }
            }
            if (!CANVAS.name.Contains("BuildHub") && (Input.GetKey(menuToggle)) || BACK_BUTTON.GetComponent<ButtonListener>().isClicked())
            {
                BACK_BUTTON.GetComponent<ButtonListener>().resetClick();
                GAME_OBJECT.transform.SetParent(null);
                GAME_OBJECT.SetActive(false);
                base.disable();
            }
        }
        else if (GAME_OBJECT.activeInHierarchy)
        {
            GAME_OBJECT.SetActive(false);
            GAME_OBJECT.transform.SetParent(default);
        }
    }

    private void filterSettings(int tabIndex)
    {
        currentSettingFilterIndex = tabIndex;
        List<Transform> settingsToRemoveFromPanel = new List<Transform>();
        foreach (Transform child in SETTINGS_PANEL.transform)
        {
            settingsToRemoveFromPanel.Add(child);
            child.gameObject.SetActive(false);
        }
        foreach (Transform child in settingsToRemoveFromPanel)
            child.SetParent(null);
        foreach (GameObject setting in SETTINGS_IN_TABS[currentSettingFilterIndex])
        {
            setting.SetActive(true);
            setting.transform.SetParent(null);
            setting.transform.SetParent(SETTINGS_PANEL.transform);
            setting.transform.localPosition = new Vector3(setting.transform.localPosition.x, setting.transform.localPosition.y, 0);
            setting.transform.localScale = Vector3.one;
        }
    }

    public void onGUI()
    {
        
    }

    public List<Setting> getSettings()
    {
        return CURRENT_SETTINGS;
    }

    public List<Setting> getSettingObjectList()
    {
        return CURRENT_SETTINGS;
    }

    public bool isEnabled()
    {
        return base.enabled;
    }
}