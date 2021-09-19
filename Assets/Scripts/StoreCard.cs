using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreCard : PeripheralElement
{
    private long credits;
    private Part[] storeParts, humanParts, robotParts;
    private readonly PerformanceMetricCalculator PERFORMANCE_METRIC_CALCULATOR;
    private readonly string STORE_CARD_NAME = "StoreCard", STORE_CONTAINER_NAME = "StoreContainer", STORE_PANEL_NAME = "StorePanel", STORE_SCROLLBAR_NAME = "StoreScrollbar", PART_INFO_NAME = "StorePartInfo", PART_TYPE_NAME = "StorePartType", PART_STATS_NAME = "StorePartStats", STAT_BETTER_COLOR_PREFIX = "<color=#07D20C>", STAT_WORSE_COLOR_PREFIX = "<color=#FFA300>", FORMAT_COLOR_SUFFIX = "</color>", STORE_TAB_SUFFIX = "StoreTab", DEFAULT_STORE_FILTER = "Featured", ATTACHMENT_PART_FILTER = "Attachment", HEAD_PART_FILTER = "Head", DEFAULT_SORT_STRING = "Sort", WEIGHT_NAME = "Weight", TIME_NAME = "time", USAGE_NAME = "usage", DURABILITY_NAME = "Durability";
    private readonly string[] FEATURED_PARTS_ROWS_LABELS = { "Head", "Body", "Mobility", "Attachment" };
    private readonly List<string> ATTACHMENT_PARTS_ROWS_LABELS;
    private readonly Color ACTIVE_TAB_COLOR = new Color(0, 0, 0, 0.2f), INACTIVE_TAB_COLOR = new Color(0, 0, 0, 0.39215686274f), AFFORDABLE_COLOR = new Color(1f, 0.45f, 0f, .8f), UNAFFORDABLE_COLOR = new Color(0, 0, 0, .4f), AFFORDABLE_TEXT_COLOR = new Color(.02745098039f, .82352941176f, .04705882352f, 1), UNAFFORDABLE_TEXT_COLOR = new Color(0, 0, 0, .4f);
    private readonly List<GameObject> STORE_TABS, FEATURED_PARTS_ROWS, ATTACHMENT_PARTS_ROWS;
    private readonly GameObject STORE_SORT, SCROLL_VIEW, SCROLLBAR, BUY_PART_BUTTON, STORE_CREDIT, STORE_CREDITS_SPENT;
    private GameObject partInfo, partType, partStats;
    private List<GameObject> storeButtons, partStatsList;
    private readonly int DEFAULT_SORT = 0, STORE_COLUMN_COUNT = 4, STORE_FEATURED_COLUMN_COUNT = 1, STORE_ATTACHMENT_COLUMN_COUNT = 1;
    public enum MODES { VIEW_PART_STATS, PREVIEW_PART, BUY_PART };
    private MODES mode;
    private Part partBeingPreviewed, partBeingBought;
    private readonly Vector3 STORE_CREDITS_SPENT_HOME_POSITION;
    private readonly float STORE_CREDITS_SPENT_ANIMATION_POSITION_SPEED = .2f, STORE_CREDITS_SPENT_ANIMATION_COLOR_SPEED = .01f;
    private readonly ScrollRect SCROLL_RECT;
    private readonly RectTransform STORE_CONTAINER, STORE_PANEL;
    private readonly Vector2 STORE_CELL_SIZE = new Vector2(160, 160), STORE_FEATURED_CELL_SIZE = new Vector2(690, 140), STORE_ATTACHMENT_CELL_SIZE = new Vector2(690, 190);
    private string currentPartFilter;
    private int currentSort;
    private Color colorScheme;
    private bool enableCreditsSpentAnimation;

    public StoreCard(long credits, Part[] storeParts, Part[] humanParts, Part[] robotParts, Color colorScheme, bool enableCreditsSpentAnimation)
    {
        this.credits = credits;
        this.storeParts = storeParts;
        this.humanParts = humanParts;
        this.robotParts = robotParts;
        this.colorScheme = colorScheme;
        this.enableCreditsSpentAnimation = enableCreditsSpentAnimation;
        mode = MODES.VIEW_PART_STATS;
        partBeingPreviewed = null;
        partBeingBought = null;
        currentPartFilter = DEFAULT_STORE_FILTER;
        STORE_SORT = GameObject.Find("StoreSort");
        currentSort = DEFAULT_SORT;
        PERFORMANCE_METRIC_CALCULATOR = new PerformanceMetricCalculator();
        SCROLL_VIEW = GameObject.Find(STORE_CARD_NAME);
        GameObject storeContainerGameObject = GameObject.Find(STORE_CONTAINER_NAME);
        STORE_CONTAINER = storeContainerGameObject.GetComponent<RectTransform>();
        SCROLL_RECT = SCROLL_VIEW.GetComponent<ScrollRect>();
        STORE_PANEL = GameObject.Find(STORE_PANEL_NAME).GetComponent<RectTransform>();
        SCROLLBAR = GameObject.Find(STORE_CARD_NAME).transform.Find(STORE_SCROLLBAR_NAME).gameObject;
        SCROLLBAR.SetActive(false);
        SCROLL_RECT.vertical = false;
        storeButtons = new List<GameObject>();
        partInfo = GameObject.Find("Store").transform.Find(PART_INFO_NAME).gameObject;
        partType = partInfo.transform.Find(PART_TYPE_NAME).gameObject;
        partStats = partInfo.transform.Find(PART_STATS_NAME).gameObject;
        partInfo.SetActive(false);
        BUY_PART_BUTTON = GameObject.Find("Store").transform.Find("BuyPart").gameObject;
        BUY_PART_BUTTON.SetActive(false);
        STORE_CREDIT = GameObject.Find("StoreCredit");
        STORE_CREDIT.GetComponent<TextMeshProUGUI>().color = colorScheme;
        updateCredits();
        STORE_CREDITS_SPENT = GameObject.Find("Store").transform.Find("StoreCreditsSpent").gameObject;
        STORE_CREDITS_SPENT_HOME_POSITION = STORE_CREDITS_SPENT.transform.localPosition;
        STORE_CREDITS_SPENT.SetActive(false);
        partStatsList = new List<GameObject>();
        ATTACHMENT_PARTS_ROWS_LABELS = new List<string>();
        foreach (Part part in this.storeParts)
        {
            if (part is Attachment && !ATTACHMENT_PARTS_ROWS_LABELS.Contains(part.GetType().ToString()))
                ATTACHMENT_PARTS_ROWS_LABELS.Add(part.GetType().ToString());
            GameObject storeButton = GameObject.Instantiate(Resources.Load("Prefabs/FeaturedPartButton") as GameObject);
            Texture2D partIcon = part.getIcon();
            if (partIcon != null)
                storeButton.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(partIcon, new Rect(0, 0, partIcon.width, partIcon.height), new Vector2(0.5f, 0.5f), 100);
            storeButton.GetComponent<RectTransform>().localScale = Vector3.one;
            storeButton.transform.localPosition = new Vector3(storeButton.transform.localPosition.x, storeButton.transform.localPosition.y, 0);
            storeButton.transform.Find("CostLabel").GetComponent<TextMeshProUGUI>().text += PERFORMANCE_METRIC_CALCULATOR.calculateCost(part);
            storeButton.transform.Find("CostLabel").GetComponent<TextMeshProUGUI>().color = (PERFORMANCE_METRIC_CALCULATOR.calculateCost(part) <= this.credits ? AFFORDABLE_TEXT_COLOR : UNAFFORDABLE_TEXT_COLOR);
            storeButton.SetActive(false);
            storeButtons.Add(storeButton);
        }
        STORE_TABS = new List<GameObject>();
        foreach (Transform childTransform in GameObject.Find("StoreTabs").transform.Find("StoreTabMask"))
            if (childTransform.gameObject.name.Contains(STORE_TAB_SUFFIX))
                STORE_TABS.Add(childTransform.gameObject);
        FEATURED_PARTS_ROWS = new List<GameObject>();
        foreach (string featuredPartsRowLabel in FEATURED_PARTS_ROWS_LABELS)
        {
            GameObject featuredPartsRow = GameObject.Instantiate(Resources.Load("Prefabs/FeaturedPartsRow") as GameObject);
            string featuredPartsRowLabelText = featuredPartsRow.transform.Find("AllParts").Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text;
            featuredPartsRow.transform.Find("AllParts").Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text = featuredPartsRowLabelText.Substring(0, featuredPartsRowLabelText.IndexOf(" ") + 1) + featuredPartsRowLabel + featuredPartsRowLabelText.Substring(featuredPartsRowLabelText.IndexOf(" "));
            featuredPartsRow.transform.localPosition = new Vector3(featuredPartsRow.transform.localPosition.x, featuredPartsRow.transform.localPosition.y, 0);
            featuredPartsRow.SetActive(false);
            FEATURED_PARTS_ROWS.Add(featuredPartsRow);
        }
        ATTACHMENT_PARTS_ROWS = new List<GameObject>();
        foreach (string attachmentPartsRowLabel in ATTACHMENT_PARTS_ROWS_LABELS)
        {
            GameObject attachmentPartsRow = GameObject.Instantiate(Resources.Load("Prefabs/AttachmentPartsRow") as GameObject);
            string attachmentPartsRowLabelText = attachmentPartsRow.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text;
            attachmentPartsRow.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text = attachmentPartsRowLabel;
            attachmentPartsRow.transform.localPosition = new Vector3(attachmentPartsRow.transform.localPosition.x, attachmentPartsRow.transform.localPosition.y, 0);
            attachmentPartsRow.SetActive(false);
            ATTACHMENT_PARTS_ROWS.Add(attachmentPartsRow);
        }
        filterFeaturedParts();
        updateSorting();
    }

    public void updateCredits()
    {
        string storeCreditText = STORE_CREDIT.GetComponent<TextMeshProUGUI>().text;
        STORE_CREDIT.GetComponent<TextMeshProUGUI>().text = storeCreditText.Substring(0, storeCreditText.LastIndexOf(" ") + 1) + credits.ToString();
    }

    protected override void calculatePoints()
    {

    }

    public override void update()
    {
        
    }

    public void update(long credits, Part[] humanParts, Part[] robotParts, Color colorScheme, bool enableCreditsSpentAnimation)
    {
        this.credits = credits;
        this.humanParts = humanParts;
        this.robotParts = robotParts;
        this.colorScheme = colorScheme;
        this.enableCreditsSpentAnimation = enableCreditsSpentAnimation;
        if (STORE_CREDIT.GetComponent<TextMeshProUGUI>().color != this.colorScheme)
            STORE_CREDIT.GetComponent<TextMeshProUGUI>().color = this.colorScheme;
        if (base.enabled)
        {
            if (BUY_PART_BUTTON.activeSelf && BUY_PART_BUTTON.GetComponent<ButtonListener>().isClicked() && PERFORMANCE_METRIC_CALCULATOR.calculateCost(partBeingPreviewed) <= credits)
            {
                BUY_PART_BUTTON.GetComponent<ButtonListener>().resetClick();
                mode = MODES.BUY_PART;
                partBeingBought = partBeingPreviewed;
                partBeingPreviewed = null;
                BUY_PART_BUTTON.SetActive(false);
                credits -= PERFORMANCE_METRIC_CALCULATOR.calculateCost(partBeingBought);
                updateCredits();
                if (enableCreditsSpentAnimation)
                {
                    STORE_CREDITS_SPENT.SetActive(true);
                    STORE_CREDITS_SPENT.transform.localPosition = STORE_CREDITS_SPENT_HOME_POSITION;
                    Color storeCreditsSpentColor = STORE_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color;
                    STORE_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color = new Color(storeCreditsSpentColor.r, storeCreditsSpentColor.g, storeCreditsSpentColor.b, 1);
                    STORE_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().text = "-" + PERFORMANCE_METRIC_CALCULATOR.calculateCost(partBeingBought).ToString();
                }
            }
            if (Input.GetMouseButton(0) && mode == MODES.PREVIEW_PART && !BUY_PART_BUTTON.GetComponent<ButtonListener>().isMouseOver())
            {
                mode = MODES.VIEW_PART_STATS;
                partBeingPreviewed = null;
                partBeingBought = null;
                BUY_PART_BUTTON.SetActive(false);
            }
            if ((STORE_PANEL.offsetMax.y - STORE_PANEL.offsetMin.y) > (STORE_CONTAINER.offsetMax.y - STORE_CONTAINER.offsetMin.y))
            {
                SCROLLBAR.SetActive(true);
                SCROLL_RECT.vertical = true;
            }
            else
            {
                SCROLLBAR.SetActive(false);
                SCROLL_RECT.vertical = false;
            }
            for (int storeButtonIndex = 0; storeButtonIndex < storeButtons.Count; ++storeButtonIndex)
            {
                ButtonListener buttonListener = storeButtons[storeButtonIndex].GetComponent<ButtonListener>();
                if ((buttonListener.isClicked()) && mode != MODES.PREVIEW_PART)
                {
                    bool ignoreClick = false;
                    foreach (Part part in humanParts)
                    {
                        if (storeParts[storeButtonIndex].getID() == part.getID())
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
                            partBeingPreviewed = storeParts[storeButtonIndex];
                            BUY_PART_BUTTON.SetActive(true);
                            BUY_PART_BUTTON.GetComponent<UnityEngine.UI.Image>().color = (PERFORMANCE_METRIC_CALCULATOR.calculateCost(partBeingPreviewed) <= credits ? colorScheme : UNAFFORDABLE_COLOR);
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
                        if (part.GetType() == storeParts[storeButtonIndex].GetType())
                        {
                            robotPart = part;
                            break;
                        }
                    }
                    Vector2 position;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(GameObject.Find("BuildHubCanvas").GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out position);
                    position.x += partInfo.GetComponent<RectTransform>().sizeDelta.x / 2;
                    partInfo.transform.localPosition = position;
                    partType.GetComponent<TextMeshProUGUI>().text = storeParts[storeButtonIndex].GetType().ToString();
                    double[] differenceInStats = robotPart != null ? robotPart.compareTo(storeParts[storeButtonIndex]) : (new Blaster("", null, 0, null, Shape.SHAPES.CYLINDER, 0, 0, 0, 0, 0, 0)).compareTo(storeParts[storeButtonIndex]);
                    foreach (GameObject partStat in partStatsList)
                        GameObject.Destroy(partStat);
                    partStatsList.Clear();
                    GameObject durabilityPartStat = GameObject.Instantiate(Resources.Load("Prefabs/PartStat") as GameObject);
                    durabilityPartStat.GetComponent<TextMeshProUGUI>().text = "Durability: " + StringTools.formatString(storeParts[storeButtonIndex].getDurability()) + (differenceInStats.Length > 0 ? " (" + applyStatDifferenceFormatting(differenceInStats[1], false) + ")": "");
                    durabilityPartStat.transform.SetParent(partStats.GetComponent<RectTransform>());
                    durabilityPartStat.transform.localPosition = new Vector3(durabilityPartStat.transform.localPosition.x, durabilityPartStat.transform.localPosition.y, 0);
                    partStatsList.Add(durabilityPartStat);
                    string[] partStatStrings = storeParts[storeButtonIndex].getStatStrings();
                    for (int partStatIndex = 0; partStatIndex < partStatStrings.Length; ++partStatIndex)
                    {
                        GameObject partStat = GameObject.Instantiate(Resources.Load("Prefabs/PartStat") as GameObject);
                        string differenceInStatString = "";
                        int differenceInStatsIndex = partStatIndex + 2;
                        if (!(storeParts[storeButtonIndex] is Attachment))
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
            foreach (GameObject tab in STORE_TABS)
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
            foreach (GameObject featuredPartsRow in FEATURED_PARTS_ROWS)
            {
                if (featuredPartsRow.GetComponent<ButtonListener>().isClicked())
                {
                    switchTabs = true;
                    string allPartsButtonLabelText = featuredPartsRow.transform.Find("AllParts").Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text;
                    foreach (GameObject tab in STORE_TABS)
                    {
                        if (allPartsButtonLabelText.Contains(tab.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text))
                        {
                            tab.GetComponent<ButtonListener>().click();
                            break;
                        }
                    }
                    featuredPartsRow.GetComponent<ButtonListener>().resetClick();
                    filterParts(allPartsButtonLabelText.Substring(allPartsButtonLabelText.IndexOf(" ") + 1, allPartsButtonLabelText.LastIndexOf(" ") - (allPartsButtonLabelText.IndexOf(" ") + 1) - 1));
                    updateSorting();
                    break;
                }
                else featuredPartsRow.GetComponent<ButtonListener>().resetClick();
            }
            if (switchTabs)
            {
                foreach (GameObject tab in STORE_TABS)
                {
                    if (tab.GetComponent<ButtonListener>().isClicked())
                    {
                        tab.GetComponent<ButtonListener>().resetClick();
                        tab.GetComponent<UnityEngine.UI.Image>().color = ACTIVE_TAB_COLOR;
                    }
                    else tab.GetComponent<UnityEngine.UI.Image>().color = INACTIVE_TAB_COLOR;
                }
            }
            if (currentSort != STORE_SORT.GetComponent<TMP_Dropdown>().value)
            {
                currentSort = STORE_SORT.GetComponent<TMP_Dropdown>().value;
                applySorting();
            }
            if (enableCreditsSpentAnimation && STORE_CREDITS_SPENT.activeSelf)
            {
                Vector3 position = STORE_CREDITS_SPENT.transform.localPosition;
                STORE_CREDITS_SPENT.transform.localPosition = new Vector3(position.x, position.y + STORE_CREDITS_SPENT_ANIMATION_POSITION_SPEED, position.z);
                Color storeCreditsSpentColor = STORE_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color;
                STORE_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color = new Color(storeCreditsSpentColor.r, storeCreditsSpentColor.g, storeCreditsSpentColor.b, storeCreditsSpentColor.a - STORE_CREDITS_SPENT_ANIMATION_COLOR_SPEED);
                if (STORE_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color.a <= 0)
                    STORE_CREDITS_SPENT.SetActive(false);
            }
            else
            {
                if (STORE_CREDITS_SPENT.transform.localPosition != STORE_CREDITS_SPENT_HOME_POSITION)
                    STORE_CREDITS_SPENT.transform.localPosition = STORE_CREDITS_SPENT_HOME_POSITION;
                Color storeCreditsSpentColor = STORE_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color;
                if (STORE_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color.a != 1)
                    STORE_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color = new Color(storeCreditsSpentColor.r, storeCreditsSpentColor.g, storeCreditsSpentColor.b, 1);
                if (STORE_CREDITS_SPENT.activeInHierarchy)
                    STORE_CREDITS_SPENT.SetActive(false);
            }
        }
    }

    private void filterFeaturedParts()
    {
        STORE_PANEL.GetComponent<GridLayoutGroup>().constraintCount = STORE_FEATURED_COLUMN_COUNT;
        STORE_PANEL.GetComponent<GridLayoutGroup>().cellSize = STORE_FEATURED_CELL_SIZE;
        foreach (GameObject featuredPartsRow in FEATURED_PARTS_ROWS)
        {
            featuredPartsRow.SetActive(true);
            featuredPartsRow.transform.SetParent(STORE_PANEL);
            featuredPartsRow.transform.localPosition = new Vector3(featuredPartsRow.transform.localPosition.x, featuredPartsRow.transform.localPosition.y, 0);
            featuredPartsRow.transform.localScale = Vector3.one;
        }
        for (int partIndex = 0; partIndex < storeButtons.Count; ++partIndex)
        {
            int cost = PERFORMANCE_METRIC_CALCULATOR.calculateCost(storeParts[partIndex]);
            if (cost <= credits)
            {
                for (int featuredPartsRowIndex = 0; featuredPartsRowIndex < FEATURED_PARTS_ROWS_LABELS.Length; ++featuredPartsRowIndex)
                {
                    if (FEATURED_PARTS_ROWS_LABELS[featuredPartsRowIndex] == storeParts[partIndex].GetType().ToString() || (storeParts[partIndex] is Attachment && FEATURED_PARTS_ROWS_LABELS[featuredPartsRowIndex] == "Attachment"))
                    {
                        FEATURED_PARTS_ROWS[featuredPartsRowIndex].transform.localScale = Vector3.one;
                        storeButtons[partIndex].SetActive(true);
                        storeButtons[partIndex].transform.SetParent(FEATURED_PARTS_ROWS[featuredPartsRowIndex].transform.Find("PartsPanel"));
                        storeButtons[partIndex].transform.localPosition = new Vector3(storeButtons[partIndex].transform.localPosition.x, storeButtons[partIndex].transform.localPosition.y, 0);
                        storeButtons[partIndex].transform.localScale = Vector3.one;
                        break;
                    }
                }
            }
            else
            {
                storeButtons[partIndex].SetActive(false);
                storeButtons[partIndex].transform.SetParent(null);
            }
        }
        foreach (GameObject featuredPartsRow in FEATURED_PARTS_ROWS)
        {
            if (featuredPartsRow.transform.Find("PartsPanel").childCount == 0)
            {
                featuredPartsRow.transform.SetParent(null);
                featuredPartsRow.SetActive(false);
            }
        }
    }

    private void filterAttachmentParts()
    {
        STORE_PANEL.GetComponent<GridLayoutGroup>().constraintCount = STORE_ATTACHMENT_COLUMN_COUNT;
        STORE_PANEL.GetComponent<GridLayoutGroup>().cellSize = STORE_ATTACHMENT_CELL_SIZE;
        foreach (GameObject attachmentPartsRow in ATTACHMENT_PARTS_ROWS)
        {
            attachmentPartsRow.SetActive(true);
            attachmentPartsRow.transform.SetParent(STORE_PANEL);
            attachmentPartsRow.transform.localPosition = new Vector3(attachmentPartsRow.transform.localPosition.x, attachmentPartsRow.transform.localPosition.y, 0);
            attachmentPartsRow.transform.localScale = Vector3.one;
        }
        for (int partIndex = 0; partIndex < storeButtons.Count; ++partIndex)
        {
            for (int attachmentPartsRowIndex = 0; attachmentPartsRowIndex < ATTACHMENT_PARTS_ROWS_LABELS.Count; ++attachmentPartsRowIndex)
            {
                if (ATTACHMENT_PARTS_ROWS_LABELS[attachmentPartsRowIndex] == storeParts[partIndex].GetType().ToString())
                {
                    ATTACHMENT_PARTS_ROWS[attachmentPartsRowIndex].transform.localScale = Vector3.one;
                    storeButtons[partIndex].SetActive(true);
                    storeButtons[partIndex].transform.SetParent(ATTACHMENT_PARTS_ROWS[attachmentPartsRowIndex].transform.Find("PartsPanel"));
                    storeButtons[partIndex].transform.localPosition = new Vector3(storeButtons[partIndex].transform.localPosition.x, storeButtons[partIndex].transform.localPosition.y, 0);
                    storeButtons[partIndex].transform.localScale = Vector3.one;
                    break;
                }
            }
        }
        foreach (GameObject attachmentPartsRow in ATTACHMENT_PARTS_ROWS)
        {
            if (attachmentPartsRow.transform.Find("PartsPanel").childCount == 0)
            {
                attachmentPartsRow.transform.SetParent(null);
                attachmentPartsRow.SetActive(false);
            }
        }
    }

    private void updateSorting()
    {
        Part partOfTypeFilter = getPartOfType(currentPartFilter), headPart = getPartOfType(HEAD_PART_FILTER);
        string[] partStatStrings = partOfTypeFilter.getStatStrings();
        STORE_SORT.GetComponent<TMP_Dropdown>().ClearOptions();
        List<TMP_Dropdown.OptionData> sortOptions = new List<TMP_Dropdown.OptionData>();
        sortOptions.Add(new TMP_Dropdown.OptionData(DEFAULT_SORT_STRING));
        sortOptions.Add(new TMP_Dropdown.OptionData(DURABILITY_NAME));
        foreach (string statString in partOfTypeFilter.getStatStrings())
            sortOptions.Add(new TMP_Dropdown.OptionData(statString.Substring(0, statString.IndexOf(":"))));
        STORE_SORT.GetComponent<TMP_Dropdown>().AddOptions(sortOptions);
        if (currentSort > headPart.getStatStrings().Length)
            currentSort = 0;
        applySorting();
    }

    private Part getPartOfType(string partFilter)
    {
        Part partOfTypeFilter = null;
        foreach (Part part in storeParts)
        {
            if (part.GetType().ToString() == partFilter || (partFilter == ATTACHMENT_PART_FILTER && part is Attachment) || (partFilter == DEFAULT_STORE_FILTER && part is Head))
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
        for (int storeButtonIndex = 0; storeButtonIndex < storeButtons.Count; ++storeButtonIndex)
        {
            if (storeButtons[storeButtonIndex].transform.parent != null)
            {
                partButtonsToSort.Add(storeButtons[storeButtonIndex]);
                storeButtons[storeButtonIndex].transform.SetParent(null);
                partsToSort.Add(storeParts[storeButtonIndex]);
            }
        }
        sortedPartButtons = new GameObject[partButtonsToSort.Count];
        if (currentSort > 0)
        {
            partsToSort.Sort((x, y) => x.getStats()[currentSort].CompareTo(y.getStats()[currentSort]));
            if (!(STORE_SORT.GetComponent<TMP_Dropdown>().captionText.text.Contains(WEIGHT_NAME) || (STORE_SORT.GetComponent<TMP_Dropdown>().captionText.text.Contains(TIME_NAME) && STORE_SORT.GetComponent<TMP_Dropdown>().captionText.text.Contains(USAGE_NAME))))
                partsToSort.Reverse();
        }
        foreach (GameObject partButton in partButtonsToSort)
            sortedPartButtons[partsToSort.IndexOf(storeParts[storeButtons.IndexOf(partButton)])] = partButton;
        if (currentPartFilter == DEFAULT_STORE_FILTER)
        {
            for (int storeButtonIndex = 0; storeButtonIndex < sortedPartButtons.Length; ++storeButtonIndex)
            {
                for (int featuredPartsRowIndex = 0; featuredPartsRowIndex < FEATURED_PARTS_ROWS_LABELS.Length; ++featuredPartsRowIndex)
                {
                    if (FEATURED_PARTS_ROWS_LABELS[featuredPartsRowIndex] == partsToSort[storeButtonIndex].GetType().ToString() || (partsToSort[storeButtonIndex] is Attachment && FEATURED_PARTS_ROWS_LABELS[featuredPartsRowIndex] == "Attachment"))
                    {
                        sortedPartButtons[storeButtonIndex].SetActive(true);
                        sortedPartButtons[storeButtonIndex].transform.SetParent(FEATURED_PARTS_ROWS[featuredPartsRowIndex].transform.Find("PartsPanel"));
                        sortedPartButtons[storeButtonIndex].transform.localPosition = new Vector3(sortedPartButtons[storeButtonIndex].transform.localPosition.x, sortedPartButtons[storeButtonIndex].transform.localPosition.y, 0);
                        break;
                    }
                }
            }
        }
        else if (currentPartFilter == ATTACHMENT_PART_FILTER)
        {
            for (int storeButtonIndex = 0; storeButtonIndex < sortedPartButtons.Length; ++storeButtonIndex)
            {
                for (int attachmentPartsRowIndex = 0; attachmentPartsRowIndex < ATTACHMENT_PARTS_ROWS_LABELS.Count; ++attachmentPartsRowIndex)
                {
                    if (ATTACHMENT_PARTS_ROWS_LABELS[attachmentPartsRowIndex] == partsToSort[storeButtonIndex].GetType().ToString())
                    {
                        sortedPartButtons[storeButtonIndex].SetActive(true);
                        sortedPartButtons[storeButtonIndex].transform.SetParent(ATTACHMENT_PARTS_ROWS[attachmentPartsRowIndex].transform.Find("PartsPanel"));
                        sortedPartButtons[storeButtonIndex].transform.localPosition = new Vector3(sortedPartButtons[storeButtonIndex].transform.localPosition.x, sortedPartButtons[storeButtonIndex].transform.localPosition.y, 0);
                        break;
                    }
                }
            }
        }
        else foreach (GameObject partButton in sortedPartButtons)
                partButton.transform.SetParent(STORE_PANEL);
    }

    private void filterParts(string filterPartType)
    {
        currentPartFilter = filterPartType;
        if (currentPartFilter == DEFAULT_STORE_FILTER)
        {
            foreach (GameObject attachmentPartsRow in ATTACHMENT_PARTS_ROWS)
            {
                attachmentPartsRow.transform.SetParent(null);
                attachmentPartsRow.SetActive(false);
            }
            filterFeaturedParts();
        }
        else if (currentPartFilter == ATTACHMENT_PART_FILTER)
        {
            foreach (GameObject featuredPartsRow in FEATURED_PARTS_ROWS)
            {
                featuredPartsRow.transform.SetParent(null);
                featuredPartsRow.SetActive(false);
            }
            filterAttachmentParts();
        }
        else
        {
            foreach (GameObject featuredPartsRow in FEATURED_PARTS_ROWS)
            {
                featuredPartsRow.transform.SetParent(null);
                featuredPartsRow.SetActive(false);
            }
            foreach (GameObject attachmentPartsRow in ATTACHMENT_PARTS_ROWS)
            {
                attachmentPartsRow.transform.SetParent(null);
                attachmentPartsRow.SetActive(false);
            }
            STORE_PANEL.GetComponent<GridLayoutGroup>().constraintCount = STORE_COLUMN_COUNT;
            STORE_PANEL.GetComponent<GridLayoutGroup>().cellSize = STORE_CELL_SIZE;
            for (int storeButtonIndex = 0; storeButtonIndex < storeButtons.Count; ++storeButtonIndex)
            {
                if (storeParts[storeButtonIndex].GetType().ToString() == filterPartType || (filterPartType == ATTACHMENT_PART_FILTER && storeParts[storeButtonIndex] is Attachment))
                {
                    storeButtons[storeButtonIndex].SetActive(true);
                    storeButtons[storeButtonIndex].transform.SetParent(null);
                    storeButtons[storeButtonIndex].transform.SetParent(STORE_PANEL);
                    storeButtons[storeButtonIndex].transform.localPosition = new Vector3(storeButtons[storeButtonIndex].transform.localPosition.x, storeButtons[storeButtonIndex].transform.localPosition.y, 0);
                    storeButtons[storeButtonIndex].transform.localScale = Vector3.one;
                }
                else
                {
                    storeButtons[storeButtonIndex].transform.SetParent(null);
                    storeButtons[storeButtonIndex].SetActive(false);
                }
            }
        }
    }

    public MODES getMode()
    {
        return mode;
    }

    public Part getPartBeingPreviewed()
    {
        return partBeingPreviewed;
    }

    public Part getPartBeingBought()
    {
        Part tempPartBeingEquipt = partBeingBought;
        partBeingBought = null;
        mode = MODES.VIEW_PART_STATS;
        return tempPartBeingEquipt;
    }

    public void goToDefaultTab()
    {
        STORE_TABS[0].GetComponent<ButtonListener>().click();
    }

    private string applyStatDifferenceFormatting(double statDifference, bool reverse)
    {
        string formattedStatDifference = "";
        if (!reverse)
            formattedStatDifference = (statDifference > 0 ? STAT_BETTER_COLOR_PREFIX + "+" + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : (statDifference < 0 ? STAT_WORSE_COLOR_PREFIX + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : StringTools.formatString(statDifference)));
        else formattedStatDifference = (statDifference > 0 ? STAT_WORSE_COLOR_PREFIX + "+" + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : (statDifference < 0 ? STAT_BETTER_COLOR_PREFIX + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : StringTools.formatString(statDifference)));
        return formattedStatDifference;
    }

    public void destroyAllGeneratedObjects()
    {
        foreach (GameObject featuredPartsRow in FEATURED_PARTS_ROWS)
            GameObject.Destroy(featuredPartsRow);
        foreach (GameObject attachmentPartsRow in ATTACHMENT_PARTS_ROWS)
            GameObject.Destroy(attachmentPartsRow);
        foreach (GameObject storeButton in storeButtons)
            GameObject.Destroy(storeButton);
        foreach (GameObject partStat in partStatsList)
            GameObject.Destroy(partStat);
    }
}