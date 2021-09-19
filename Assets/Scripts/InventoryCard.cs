using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryCard : PeripheralElement
{
    private long credits;
    private Part[] humanParts, robotParts;
    private readonly PerformanceMetricCalculator PERFORMANCE_METRIC_CALCULATOR;
    private readonly string INVENTORY_CARD_NAME = "InventoryCard", PARTS_CONTAINER_NAME = "PartsContainer", PARTS_PANEL_NAME = "PartsPanel", INVENTORY_SCROLLBAR_NAME = "InventoryScrollbar", PART_INFO_NAME = "WorkshopPartInfo", PART_TYPE_NAME = "WorkshopPartType", DURABILITY_BAR_NAME = "DurabilityBar", DURABILITY_BAR_LABEL_NAME = "DurabilityBarLabel", PART_STATS_NAME = "WorkshopPartStats", STAT_BETTER_COLOR_PREFIX = "<color=#07D20C>", STAT_WORSE_COLOR_PREFIX = "<color=#FFA300>", FORMAT_COLOR_SUFFIX = "</color>", PART_TAB_SUFFIX = "PartTab", DEFAULT_PART_FILTER = "All", ATTACHMENT_PART_FILTER = "Attachment", HEAD_PART_FILTER = "Head", DEFAULT_SORT_STRING = "Sort", WEIGHT_NAME = "Weight", TIME_NAME = "time", USAGE_NAME = "usage", REMAINING_DURABILITY_NAME = "Remaining durability", DURABILITY_NAME = "Durability";
    private readonly Color ACTIVE_TAB_COLOR = new Color(0, 0, 0, 0.2f), INACTIVE_TAB_COLOR = new Color(0, 0, 0, 0.39215686274f);
    private readonly GameObject SCROLL_VIEW, SCROLLBAR_OBJECT, MASKED_INVENTORY_PART_BUTTON, INVENTORY_SORT;
    private readonly List<GameObject> PART_TABS;
    private GameObject partInfo, partType, durabilityBar, durabilityBarLabel, partStats;
    private List<GameObject> partButtons, partStatsList;
    private readonly ScrollRect SCROLL_RECT;
    private readonly Scrollbar SCROLLBAR;
    private readonly RectTransform PARTS_CONTAINER, PARTS_PANEL;
    private readonly int DEFAULT_SORT = 0;
    public enum MODES { VIEW_PART_STATS, PREVIEW_PART, EQUIPT_PART };
    private MODES mode;
    private Part partBeingPreviewed, partBeingEquipt;
    private string currentPartFilter;
    private int currentSort;

    public InventoryCard(long credits, Part[] humanParts, Part[] robotParts)
    {
        this.credits = credits;
        this.humanParts = humanParts;
        this.robotParts = robotParts;
        mode = MODES.VIEW_PART_STATS;
        partBeingPreviewed = null;
        partBeingEquipt = null;
        currentPartFilter = DEFAULT_PART_FILTER;
        MASKED_INVENTORY_PART_BUTTON = GameObject.Find("Workshop").transform.Find("MaskedInventoryPartButton").gameObject;
        MASKED_INVENTORY_PART_BUTTON.SetActive(false);
        INVENTORY_SORT = GameObject.Find("InventorySort");
        currentSort = DEFAULT_SORT;
        PERFORMANCE_METRIC_CALCULATOR = new PerformanceMetricCalculator();
        SCROLL_VIEW = GameObject.Find(INVENTORY_CARD_NAME);
        GameObject partsContainerGameObject = GameObject.Find(PARTS_CONTAINER_NAME);
        PARTS_CONTAINER = partsContainerGameObject.GetComponent<RectTransform>();
        SCROLL_RECT = SCROLL_VIEW.GetComponent<ScrollRect>();
        PARTS_PANEL = GameObject.Find(PARTS_PANEL_NAME).GetComponent<RectTransform>();
        SCROLLBAR_OBJECT = GameObject.Find(INVENTORY_CARD_NAME).transform.Find(INVENTORY_SCROLLBAR_NAME).gameObject;
        SCROLLBAR = SCROLLBAR_OBJECT.GetComponent<Scrollbar>();
        SCROLLBAR_OBJECT.SetActive(false);
        SCROLL_RECT.vertical = false;
        partButtons = new List<GameObject>();
        partInfo = GameObject.Find("Workshop").transform.Find(PART_INFO_NAME).gameObject;
        partType = partInfo.transform.Find(PART_TYPE_NAME).gameObject;
        durabilityBar = partInfo.transform.Find(DURABILITY_BAR_NAME).gameObject;
        durabilityBarLabel = partInfo.transform.Find(DURABILITY_BAR_LABEL_NAME).gameObject;
        partStats = partInfo.transform.Find(PART_STATS_NAME).gameObject;
        partInfo.SetActive(false);
        partStatsList = new List<GameObject>();
        initialize();
        PART_TABS = new List<GameObject>();
        foreach (Transform childTransform in GameObject.Find("InventoryTabs").transform.Find("InventoryTabMask"))
            if (childTransform.gameObject.name.Contains(PART_TAB_SUFFIX))
                PART_TABS.Add(childTransform.gameObject);
        updateSorting();
    }

    public void initialize()
    {
        partButtons = resetGameObjectList(partButtons);
        foreach (Part part in this.humanParts)
        {
            GameObject inventoryPartButton = GameObject.Instantiate(Resources.Load("Prefabs/PartButton") as GameObject);
            Texture2D partIcon = part.getIcon();
            if (partIcon != null)
                inventoryPartButton.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(partIcon, new Rect(0, 0, partIcon.width, partIcon.height), new Vector2(0.5f, 0.5f), 100);
            inventoryPartButton.transform.SetParent(PARTS_PANEL);
            inventoryPartButton.GetComponent<RectTransform>().localScale = Vector3.one;
            inventoryPartButton.transform.localPosition = new Vector3(inventoryPartButton.transform.localPosition.x, inventoryPartButton.transform.localPosition.y, 0);
            partButtons.Add(inventoryPartButton);
        }
        updateSorting();
    }

    private List<GameObject> resetGameObjectList(List<GameObject> gameObjectList)
    {
        if (gameObjectList != null)
            foreach (GameObject gameObject in gameObjectList)
                GameObject.Destroy(gameObject);
        return new List<GameObject>();
    }

    protected override void calculatePoints()
    {

    }

    public override void update()
    {

    }

    public void update(long credits, Part[] humanParts, Part[] robotParts)
    {
        this.credits = credits;
        if (this.humanParts.Length != humanParts.Length || this.robotParts.Length != robotParts.Length)
        {
            this.humanParts = humanParts;
            this.robotParts = robotParts;
            initialize();
        }
        if (base.enabled)
        {
            if (Input.GetMouseButton(0) && mode == MODES.PREVIEW_PART)
            {
                mode = MODES.VIEW_PART_STATS;
                partBeingPreviewed = null;
                partBeingEquipt = null;
                MASKED_INVENTORY_PART_BUTTON.SetActive(false);
            }
            if ((PARTS_PANEL.offsetMax.y - PARTS_PANEL.offsetMin.y) > (PARTS_CONTAINER.offsetMax.y - PARTS_CONTAINER.offsetMin.y))
            {
                SCROLLBAR_OBJECT.SetActive(true);
                SCROLL_RECT.vertical = true;
            }
            else
            {
                SCROLLBAR_OBJECT.SetActive(false);
                SCROLL_RECT.vertical = false;
            }
            for (int partButtonIndex = 0; partButtonIndex < partButtons.Count; ++partButtonIndex)
            {
                ButtonListener buttonListener = partButtons[partButtonIndex].GetComponent<ButtonListener>();
                if ((buttonListener.isClicked() || buttonListener.isControlClicked()) && mode != MODES.PREVIEW_PART)
                {
                    bool ignoreClick = false;
                    foreach (Part part in robotParts)
                    {
                        if (humanParts[partButtonIndex].getID() == part.getID())
                        {
                            ignoreClick = true;
                            break;
                        }
                    }
                    if (!ignoreClick)
                    {
                        if (buttonListener.isClicked())
                        {
                            mode = MODES.PREVIEW_PART;
                            partInfo.SetActive(false);
                            partBeingPreviewed = humanParts[partButtonIndex];
                            Texture2D partIcon = humanParts[partButtonIndex].getIcon();
                            MASKED_INVENTORY_PART_BUTTON.transform.Find("Icon").gameObject.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(partIcon, new Rect(0, 0, partIcon.width, partIcon.height), new Vector2(0.5f, 0.5f), 100);
                            MASKED_INVENTORY_PART_BUTTON.GetComponent<RectTransform>().localScale = Vector3.one;
                            MASKED_INVENTORY_PART_BUTTON.transform.position = partButtons[partButtonIndex].transform.position;
                            MASKED_INVENTORY_PART_BUTTON.SetActive(true);
                        }
                        else if (buttonListener.isControlClicked())
                        {
                            mode = MODES.EQUIPT_PART;
                            partBeingEquipt = humanParts[partButtonIndex];
                        }
                        buttonListener.resetClick();
                        break;
                    }
                    buttonListener.resetClick();
                }
                if (buttonListener.isMouseOver() && mode != MODES.PREVIEW_PART)
                {
                    Part robotPart = null;
                    foreach (Part part in robotParts)
                    {
                        if (part.GetType() == humanParts[partButtonIndex].GetType())
                        {
                            robotPart = part;
                            break;
                        }
                    }
                    float positionY = Input.mousePosition.y / (UnityEngine.Screen.height) * GameObject.Find("BuildHubCanvas").GetComponent<RectTransform>().sizeDelta.y - partInfo.GetComponent<RectTransform>().sizeDelta.y * 2;
                    positionY = Mathf.Clamp(positionY, -partInfo.GetComponent<RectTransform>().sizeDelta.y * 2, GameObject.Find("BuildHubCanvas").GetComponent<RectTransform>().sizeDelta.y - partInfo.GetComponent<RectTransform>().sizeDelta.y * 2);
                    partInfo.transform.localPosition = new Vector3(partInfo.transform.localPosition.x, positionY, partInfo.transform.localPosition.z);
                    partType.GetComponent<TextMeshProUGUI>().text = humanParts[partButtonIndex].GetType().ToString();
                    double[] differenceInStats = (robotPart != null ? robotPart.compareTo(humanParts[partButtonIndex]) : new double[0]);
                    durabilityBar.GetComponent<RectTransform>().localScale = new Vector3((humanParts[partButtonIndex].getDurability() > 0 ? (float)(humanParts[partButtonIndex].getRemainingDurability() / humanParts[partButtonIndex].getDurability()) : 0), durabilityBar.GetComponent<RectTransform>().localScale.y, 0);
                    durabilityBarLabel.GetComponent<TextMeshProUGUI>().text = ConfigurationCard.DURABILITY_BAR_LABEL_PREFIX + StringTools.formatString(humanParts[partButtonIndex].getRemainingDurability()) + " / " + StringTools.formatString(humanParts[partButtonIndex].getDurability()) + (differenceInStats != null && differenceInStats.Length > 0 ? " (" + applyStatDifferenceFormatting(differenceInStats[0], false) + " / " + applyStatDifferenceFormatting(differenceInStats[1], false) + ")" : "");
                    foreach (GameObject partStat in partStatsList)
                        GameObject.Destroy(partStat);
                    partStatsList.Clear();
                    string[] partStatStrings = humanParts[partButtonIndex].getStatStrings();
                    for (int partStatIndex = 0; partStatIndex < partStatStrings.Length; ++partStatIndex)
                    {
                        GameObject partStat = GameObject.Instantiate(Resources.Load("Prefabs/PartStat") as GameObject);
                        string differenceInStatString = "";
                        int differenceInStatsIndex = partStatIndex + 2;
                        if (!(humanParts[partButtonIndex] is Attachment))
                            differenceInStatString = applyStatDifferenceFormatting(differenceInStats[differenceInStatsIndex], differenceInStatsIndex == 2);
                        else if (differenceInStats != null && differenceInStats.Length > 0)
                        {
                            if (differenceInStatsIndex == 3 || (differenceInStatsIndex >= 8 && differenceInStatsIndex < 11))
                                differenceInStatsIndex = -1;
                            else if (differenceInStatsIndex > 3 && differenceInStatsIndex < 8)
                                differenceInStatsIndex -= 1;
                            else if (differenceInStatsIndex == 11)
                                differenceInStatsIndex -= 4;
                            if (differenceInStatsIndex != -1)
                            {
                                if (differenceInStatsIndex >= 3 && differenceInStatsIndex <= 6)
                                    differenceInStats[differenceInStatsIndex] /= 1000;
                                differenceInStatString = applyStatDifferenceFormatting(differenceInStats[differenceInStatsIndex], differenceInStatsIndex == 2 || differenceInStatsIndex == 3 || differenceInStatsIndex == 4 || differenceInStatsIndex == 6);
                            }
                        }
                        partStat.GetComponent<TextMeshProUGUI>().text = partStatStrings[partStatIndex] + (differenceInStatString != "" ? " (" + differenceInStatString + ")" : "");
                        partStat.transform.SetParent(partStats.GetComponent<RectTransform>());
                        partStat.transform.localPosition = new Vector3(partStat.transform.localPosition.x, partStat.transform.localPosition.y, 0);
                        partStatsList.Add(partStat);
                    }
                    partInfo.SetActive(true);
                    break;
                }
                else partInfo.SetActive(false);
            }
            bool switchTabs = false;
            foreach (GameObject tab in PART_TABS)
            {
                if (tab.GetComponent<ButtonListener>().isClicked() && currentPartFilter != tab.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text)
                {
                    switchTabs = true;
                    filterParts(tab.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text);
                    updateSorting();
                    break;
                }
                else tab.GetComponent<ButtonListener>().resetClick();
            }
            if (switchTabs)
            {
                foreach (GameObject tab in PART_TABS)
                {
                    if (tab.GetComponent<ButtonListener>().isClicked())
                    {
                        tab.GetComponent<ButtonListener>().resetClick();
                        tab.GetComponent<UnityEngine.UI.Image>().color = ACTIVE_TAB_COLOR;
                    }
                    else tab.GetComponent<UnityEngine.UI.Image>().color = INACTIVE_TAB_COLOR;
                }
            }
            if (currentSort != INVENTORY_SORT.GetComponent<TMP_Dropdown>().value)
            {
                currentSort = INVENTORY_SORT.GetComponent<TMP_Dropdown>().value;
                applySorting();
            }
        }
    }

    private void filterParts(string filterPartType)
    {
        currentPartFilter = filterPartType;
        for (int partButtonIndex = 0; partButtonIndex < partButtons.Count; ++partButtonIndex)
        {
            if (filterPartType == DEFAULT_PART_FILTER || ((humanParts[partButtonIndex].GetType().ToString() == filterPartType || (filterPartType == ATTACHMENT_PART_FILTER && humanParts[partButtonIndex] is Attachment))))
            {
                partButtons[partButtonIndex].transform.SetParent(null);
                partButtons[partButtonIndex].transform.SetParent(PARTS_PANEL);
            }
            else partButtons[partButtonIndex].transform.SetParent(null);
        }
    }

    private void updateSorting()
    {
        Part partOfTypeFilter = getPartOfType(currentPartFilter), headPart = getPartOfType(HEAD_PART_FILTER);
        string[] partStatStrings = partOfTypeFilter.getStatStrings();
        INVENTORY_SORT.GetComponent<TMP_Dropdown>().ClearOptions();
        List<TMP_Dropdown.OptionData> sortOptions = new List<TMP_Dropdown.OptionData>();
        sortOptions.Add(new TMP_Dropdown.OptionData(DEFAULT_SORT_STRING));
        sortOptions.Add(new TMP_Dropdown.OptionData(REMAINING_DURABILITY_NAME));
        sortOptions.Add(new TMP_Dropdown.OptionData(DURABILITY_NAME));
        foreach (string statString in partOfTypeFilter.getStatStrings())
            sortOptions.Add(new TMP_Dropdown.OptionData(statString.Substring(0, statString.IndexOf(":"))));
        INVENTORY_SORT.GetComponent<TMP_Dropdown>().AddOptions(sortOptions);
        if (currentSort > headPart.getStatStrings().Length)
            currentSort = 0;
        applySorting();
    }

    private Part getPartOfType(string partFilter)
    {
        Part partOfTypeFilter = null;
        foreach (Part part in humanParts)
        {
            if (part.GetType().ToString() == partFilter || (partFilter == ATTACHMENT_PART_FILTER && part is Attachment) || (partFilter == DEFAULT_PART_FILTER && part is Head))
            {
                partOfTypeFilter = part;
                break;
            }
        }
        return partOfTypeFilter;
    }

    private void applySorting()
    {
        Part partOfTypeFilter = getPartOfType(currentPartFilter);
        List<GameObject> partButtonsToSort = new List<GameObject>();
        GameObject[] sortedPartButtons = null;
        List<Part> partsToSort = new List<Part>();
        for (int partButtonIndex = 0; partButtonIndex < partButtons.Count; ++partButtonIndex)
        {
            if (partButtons[partButtonIndex].transform.parent != null)
            {
                partButtonsToSort.Add(partButtons[partButtonIndex]);
                partButtons[partButtonIndex].transform.SetParent(null);
                partsToSort.Add(humanParts[partButtonIndex]);
            }
        }
        sortedPartButtons = new GameObject[partButtonsToSort.Count];
        if (currentSort > 0)
        {
            partsToSort.Sort((x, y) => x.getStats()[currentSort - 1].CompareTo(y.getStats()[currentSort - 1]));
            if (!(INVENTORY_SORT.GetComponent<TMP_Dropdown>().captionText.text.Contains(WEIGHT_NAME) || (INVENTORY_SORT.GetComponent<TMP_Dropdown>().captionText.text.Contains(TIME_NAME) && INVENTORY_SORT.GetComponent<TMP_Dropdown>().captionText.text.Contains(USAGE_NAME))))
                partsToSort.Reverse();
        }
        foreach (GameObject partButton in partButtonsToSort)
            sortedPartButtons[partsToSort.IndexOf(humanParts[partButtons.IndexOf(partButton)])] = partButton;
        foreach (GameObject partButton in sortedPartButtons)
            partButton.transform.SetParent(PARTS_PANEL);
    }

    public MODES getMode()
    {
        return mode;
    }

    public Part getPartBeingPreviewed()
    {
        return partBeingPreviewed;
    }

    public void exitPartPreview()
    {
        mode = MODES.VIEW_PART_STATS;
        partBeingPreviewed = null;
        MASKED_INVENTORY_PART_BUTTON.SetActive(false);
    }

    public Part getPartBeingEquipt()
    {
        Part tempPartBeingEquipt = partBeingEquipt;
        partBeingEquipt = null;
        mode = MODES.VIEW_PART_STATS;
        return tempPartBeingEquipt;
    }

    private string applyStatDifferenceFormatting(double statDifference, bool reverse)
    {
        string formattedStatDifference = "";
        if (!reverse)
            formattedStatDifference = (statDifference > 0 ? STAT_BETTER_COLOR_PREFIX + "+" + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : (statDifference < 0 ? STAT_WORSE_COLOR_PREFIX + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX: StringTools.formatString(statDifference)));
        else formattedStatDifference = (statDifference > 0 ? STAT_WORSE_COLOR_PREFIX + "+" + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : (statDifference < 0 ? STAT_BETTER_COLOR_PREFIX + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : StringTools.formatString(statDifference)));
        return formattedStatDifference;
    }

    public Part[] getHumanParts()
    {
        return humanParts;
    }

    public void destroyAllGeneratedObjects()
    {
        foreach (GameObject partButton in partButtons)
            GameObject.Destroy(partButton);
        foreach (GameObject partStat in partStatsList)
            GameObject.Destroy(partStat);
    }
}