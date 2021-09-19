using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfigurationCard : PeripheralElement
{
    private string robotName;
    private long credits;
    private double durability, remainingDurability;
    private string[] robotStatStrings;
    private bool maxForceExceeded;
    private List<Part> parts;
    private List<Attachment> attachments;
    private readonly float WIDGET_HEIGHT = 35f, WIDGET_EXPAND_HEIGHT = 140f, WORKSHOP_CREDITS_SPENT_ANIMATION_POSITION_SPEED = .2f, WORKSHOP_CREDITS_SPENT_ANIMATION_COLOR_SPEED = .01f;
    private int numberOfWidgets;
    private List<string>[] widgetStatStrings;
    private List<float> currentWidgetHeights;
    private readonly float WIDGET_EXPAND_RATE = 2;
    private bool[] buttonStates, widgetStates, canAffordRepair, removeParts;
    private int[] repairCosts;
    public static readonly string REPAIR_BUTTON_LABEL = "Repair", DURABILITY_BAR_LABEL_PREFIX = "Durability: ", WEIGHT_STRING = "Weight", STAT_BETTER_COLOR_PREFIX = "<color=#07D20C>", STAT_WORSE_COLOR_PREFIX = "<color=#FFA300>", FORMAT_COLOR_SUFFIX = "</color>";
    public static readonly Color CARD_COLOR = new Color(0, 0, 0, 0f), WIDGET_COLOR = new Color(0, 0, 0, .2f), REPAIR_AFFORDABLE_COLOR = new Color(1f, 0.45f, 0f, .8f), REPAIR_UNAFFORDABLE_COLOR = new Color(0, 0, 0, .4f);
    private readonly GameObject SCROLL_VIEW, SCROLLBAR_OBJECT, WORKSHOP_CREDIT, WORKSHOP_CREDITS_SPENT, MASKED_ROBOT_WIDGET;
    private GameObject robotWidget, robotWidgetLabel, robotWidgetLabelPlaceholder, robotWidgetLabelText, robotWidgetDurabilityBar, robotWidgetDurabilityBarLabel, robotWidgetStats;
    private List<GameObject> robotStatStringLabels, configurationPartWidgets, widgetButtons, widgetIcons, widgetLabels, widgetRepairButtons, widgetRepairButtonLabels, repairDurabilityBars, widgetDurabilityBars, widgetDurabilityBarLabels, widgetStatsList;
    private List<GameObject>[] widgetStatsLabelList;
    private readonly PerformanceMetricCalculator PERFORMANCE_METRIC_CALCULATOR;
    private readonly Point OFFSET = new Point(20, 20), STAT_OFFSET = new Point(10, 10), LIFE_BAR_OFFSET = new Point(10, 40);
    private readonly Dimension STAT_SIZE = new Dimension(180, 20);
    private readonly string CONFIGURATION_CARD_NAME = "ConfigurationCard", ROBOT_WIDGET_NAME = "RobotWidget", ROBOT_WIDGET_LABEL_NAME = "RobotWidgetLabel", ROBOT_WIDGET_LABEL_PLACEHOLDER_NAME = "Placeholder", ROBOT_WIDGET_LABEL_TEXT_NAME = "Text", ROBOT_WIDGET_DURABILITY_BAR_NAME = "RobotWidgetDurabilityBar", ROBOT_WIDGET_DURABILITY_BAR_LABEL_NAME = "RobotWidgetDurabilityBarLabel", ROBOT_WIDGET_STATS_NAME = "RobotWidgetStats", WIDGETS_CARD_NAME = "WidgetsCard", WIDGETS_CONTAINER_NAME = "WidgetsContainer", WIDGETS_PANEL_NAME = "WidgetsPanel", CONFIGURATION_SCROLLBAR_NAME = "ConfigurationScrollbar", ROBOT_NAME_PLACEHOLDER = "Enter Robot Name";
    private readonly Vector3 WORKSHOP_CREDITS_SPENT_HOME_POSITION;
    private readonly ScrollRect SCROLL_RECT;
    private readonly Scrollbar SCROLLBAR;
    private readonly RectTransform WIDGETS_CONTAINER, WIDGETS_PANEL;
    private InventoryCard.MODES mode;
    private Part partBeingPreviewed, partToEquipt;
    private Color colorScheme;
    private bool enableCreditsSpentAnimation;

    public ConfigurationCard(string robotName, long credits, double durability, double remainingDurability, string[] robotStatStrings, bool maxForceExceeded, Part[] parts, Color colorScheme, bool enableCreditsSpentAnimation)
    {
        this.robotName = robotName;
        this.credits = credits;
        this.durability = durability;
        this.remainingDurability = remainingDurability;
        this.robotStatStrings = robotStatStrings;
        this.maxForceExceeded = maxForceExceeded;
        this.parts = new List<Part>();
        attachments = new List<Attachment>();
        this.colorScheme = colorScheme;
        this.enableCreditsSpentAnimation = enableCreditsSpentAnimation;
        foreach (Part part in parts)
        {
            if (!(part is Attachment))
                this.parts.Add(part);
            else this.attachments.Add((Attachment)part);
        }
        mode = InventoryCard.MODES.VIEW_PART_STATS;
        MASKED_ROBOT_WIDGET = GameObject.Find("Workshop").transform.Find("MaskedRobotWidget").gameObject;
        MASKED_ROBOT_WIDGET.SetActive(false);
        partBeingPreviewed = null;
        partToEquipt = null;
        PERFORMANCE_METRIC_CALCULATOR = new PerformanceMetricCalculator();
        SCROLL_VIEW = GameObject.Find(CONFIGURATION_CARD_NAME);
        GameObject partsContainerGameObject = GameObject.Find(WIDGETS_CONTAINER_NAME);
        WIDGETS_CONTAINER = partsContainerGameObject.GetComponent<RectTransform>();
        SCROLL_RECT = SCROLL_VIEW.GetComponent<ScrollRect>();
        WIDGETS_PANEL = GameObject.Find(WIDGETS_PANEL_NAME).GetComponent<RectTransform>();
        SCROLLBAR_OBJECT = GameObject.Find(WIDGETS_CARD_NAME).transform.Find(CONFIGURATION_SCROLLBAR_NAME).gameObject;
        SCROLLBAR = SCROLLBAR_OBJECT.GetComponent<Scrollbar>();
        SCROLLBAR_OBJECT.SetActive(false);
        SCROLL_RECT.vertical = false;
        robotWidget = GameObject.Find(CONFIGURATION_CARD_NAME).transform.Find(ROBOT_WIDGET_NAME).gameObject;
        robotWidgetLabel = robotWidget.transform.Find(ROBOT_WIDGET_LABEL_NAME).gameObject;
        robotWidgetLabelPlaceholder = robotWidgetLabel.transform.Find("Text Area").Find(ROBOT_WIDGET_LABEL_PLACEHOLDER_NAME).gameObject;
        robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().enabled = false;
        robotWidgetLabelText = robotWidgetLabel.transform.Find("Text Area").Find(ROBOT_WIDGET_LABEL_TEXT_NAME).gameObject;
        robotWidgetDurabilityBar = robotWidget.transform.Find(ROBOT_WIDGET_DURABILITY_BAR_NAME).gameObject;
        robotWidgetDurabilityBarLabel = robotWidget.transform.Find(ROBOT_WIDGET_DURABILITY_BAR_LABEL_NAME).gameObject;
        robotWidgetStats = robotWidget.transform.Find(ROBOT_WIDGET_STATS_NAME).gameObject;
        WORKSHOP_CREDIT = GameObject.Find("WorkshopCredit");
        updateCredits();
        WORKSHOP_CREDITS_SPENT = GameObject.Find("Workshop").transform.Find("WorkshopCreditsSpent").gameObject;
        WORKSHOP_CREDITS_SPENT_HOME_POSITION = WORKSHOP_CREDITS_SPENT.transform.localPosition;
        WORKSHOP_CREDITS_SPENT.SetActive(false);
        WORKSHOP_CREDIT.GetComponent<TextMeshProUGUI>().color = colorScheme;
        initialize();
    }

    public void updateCredits()
    {
        string workshopCreditText = WORKSHOP_CREDIT.GetComponent<TextMeshProUGUI>().text;
        WORKSHOP_CREDIT.GetComponent<TextMeshProUGUI>().text = workshopCreditText.Substring(0, workshopCreditText.LastIndexOf(" ") + 1) + credits.ToString();
    }

    public void initialize()
    {
        robotStatStringLabels = resetGameObjectList(robotStatStringLabels);
        configurationPartWidgets = resetGameObjectList(configurationPartWidgets);
        widgetButtons = resetGameObjectList(widgetButtons);
        widgetIcons = resetGameObjectList(widgetIcons);
        widgetLabels = resetGameObjectList(widgetLabels);
        widgetRepairButtons = resetGameObjectList(widgetRepairButtons);
        widgetRepairButtonLabels = resetGameObjectList(widgetRepairButtonLabels);
        repairDurabilityBars = resetGameObjectList(repairDurabilityBars);
        widgetDurabilityBars = resetGameObjectList(widgetDurabilityBars);
        widgetDurabilityBarLabels = resetGameObjectList(widgetDurabilityBarLabels);
        widgetStatsList = resetGameObjectList(widgetStatsList);
        numberOfWidgets = parts.Count + attachments.Count;
        widgetStates = new bool[numberOfWidgets];
        widgetStatStrings = new List<string>[numberOfWidgets];
        currentWidgetHeights = new List<float>();
        while (currentWidgetHeights.Count < numberOfWidgets)
            currentWidgetHeights.Add(-WIDGET_HEIGHT);
        buttonStates = new bool[numberOfWidgets];
        canAffordRepair = new bool[numberOfWidgets];
        removeParts = new bool[numberOfWidgets];
        repairCosts = new int[numberOfWidgets];
        if (widgetStatsLabelList != null)
            foreach (List<GameObject> gameObjectList in widgetStatsLabelList)
                resetGameObjectList(gameObjectList);
        widgetStatsLabelList = new List<GameObject>[numberOfWidgets];
        for (int widgetStatsLabelIndex = 0; widgetStatsLabelIndex < widgetStatsLabelList.Length; ++widgetStatsLabelIndex)
            widgetStatsLabelList[widgetStatsLabelIndex] = new List<GameObject>();
        robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().text = (!((robotName.Length == 1 && ((int)robotName[0]) == 8203) || robotName == default || robotName == "" || robotName == Robot.DEFAULT_NAME) ? robotName : ROBOT_NAME_PLACEHOLDER);
        robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().enabled = robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().text == ROBOT_NAME_PLACEHOLDER;
        robotWidgetLabel.GetComponent<TMP_InputField>().text = (!(robotName == "" || robotName == Robot.DEFAULT_NAME) ? robotName : "");
        robotWidgetDurabilityBar.GetComponent<RectTransform>().localScale = new Vector3((float)(remainingDurability / durability), robotWidgetDurabilityBar.GetComponent<RectTransform>().localScale.y, 0);
        robotWidgetDurabilityBarLabel.GetComponent<TextMeshProUGUI>().text = DURABILITY_BAR_LABEL_PREFIX + remainingDurability.ToString() + " / " + durability.ToString();
        foreach (Transform child in robotWidgetStats.GetComponent<RectTransform>())
            GameObject.Destroy(child.gameObject);
        foreach (string robotStatString in robotStatStrings)
        {
            GameObject partStat = GameObject.Instantiate(Resources.Load("Prefabs/PartStat") as GameObject);
            partStat.GetComponent<TextMeshProUGUI>().text = robotStatString;
            if (robotStatString.Contains(WEIGHT_STRING) && maxForceExceeded)
                partStat.GetComponent<TextMeshProUGUI>().color = BAD_COLOR;
            partStat.transform.SetParent(robotWidgetStats.GetComponent<RectTransform>());
            partStat.transform.localPosition = new Vector3(partStat.transform.localPosition.x, partStat.transform.localPosition.y, 0);
            robotStatStringLabels.Add(partStat);
        }
        for (int partIndex = 0; partIndex < numberOfWidgets; ++partIndex)
        {
            Part part = (partIndex < parts.Count ? parts[partIndex] : attachments[partIndex - parts.Count]);
            GameObject configurationPartWidget, widgetButton, widgetIcon, widgetLabel, widgetRepairButton, widgetRepairButtonLabel, repairDurabilityBar, widgetDurabilityBar, widgetDurabilityBarLabel, widgetStats;
            configurationPartWidget = GameObject.Instantiate(Resources.Load("Prefabs/ConfigurationPartWidget") as GameObject);
            widgetButton = configurationPartWidget.transform.Find("WidgetButton").gameObject;
            widgetIcon = widgetButton.transform.Find("Icon").gameObject;
            widgetLabel = widgetButton.transform.Find("Label").gameObject;
            widgetRepairButton = widgetButton.transform.Find("RepairButton").gameObject;
            widgetRepairButtonLabel = widgetRepairButton.transform.Find("Label").gameObject;
            widgetRepairButtonLabel.transform.localPosition = new Vector3(widgetRepairButtonLabel.transform.localPosition.x, widgetRepairButtonLabel.transform.localPosition.y, 0);
            repairDurabilityBar = widgetRepairButton.transform.Find("RepairDurabilityBar").gameObject;
            widgetDurabilityBar = configurationPartWidget.transform.Find("WidgetDurabilityBar").gameObject;
            widgetDurabilityBarLabel = configurationPartWidget.transform.Find("WidgetDurabilityBarLabel").gameObject;
            widgetStats = configurationPartWidget.transform.Find("WidgetStats").gameObject;
            Texture2D partIcon = part.getIcon();
            if (partIcon != null)
                widgetIcon.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(partIcon, new Rect(0, 0, partIcon.width, partIcon.height), new Vector2(0.5f, 0.5f), 100);
            widgetLabel.GetComponent<TextMeshProUGUI>().text = part.GetType().ToString();
            currentWidgetHeights[partIndex] = -configurationPartWidget.GetComponent<RectTransform>().sizeDelta.y;
            if (part.getRemainingDurability() < part.getDurability())
            {
                int costToRepair = 0;
                if (partIndex < this.parts.Count)
                    costToRepair = PERFORMANCE_METRIC_CALCULATOR.calculateCostToRepair(PERFORMANCE_METRIC_CALCULATOR.calculateCost(part), part.getDurability(), part.getRemainingDurability());
                else costToRepair = PERFORMANCE_METRIC_CALCULATOR.calculateCostToRepair(PERFORMANCE_METRIC_CALCULATOR.calculateCost(attachments[partIndex - parts.Count]), attachments[partIndex - parts.Count].getDurability(), attachments[partIndex - parts.Count].getRemainingDurability());
                repairCosts[partIndex] = costToRepair;
                canAffordRepair[partIndex] = costToRepair <= credits;
                widgetRepairButton.GetComponent<UnityEngine.UI.Image>().color = (canAffordRepair[partIndex] ? new Color(colorScheme.r, colorScheme.g - (float)((part.getDurability() - part.getRemainingDurability()) / part.getDurability()) * colorScheme.g, colorScheme.b, colorScheme.a) : REPAIR_UNAFFORDABLE_COLOR);
                string repairButtonLabel = REPAIR_BUTTON_LABEL + " (<sprite=0> " + repairCosts[partIndex].ToString() + ")";
                widgetRepairButtonLabel.GetComponent<TextMeshProUGUI>().text = repairButtonLabel;
                repairDurabilityBar.GetComponent<RectTransform>().localScale = new Vector3((float)(part.getRemainingDurability() / part.getDurability()), repairDurabilityBar.GetComponent<RectTransform>().localScale.y, 0);
                buttonStates[partIndex] = widgetRepairButton.GetComponent<ButtonListener>().isClicked();
                widgetRepairButton.SetActive(true);
                widgetRepairButtonLabel.SetActive(true);
                repairDurabilityBar.SetActive(true);
            }
            else
            {
                canAffordRepair[partIndex] = false;
                widgetRepairButton.SetActive(false);
                widgetRepairButtonLabel.SetActive(false);
                repairDurabilityBar.SetActive(false);
            }
            widgetDurabilityBar.GetComponent<RectTransform>().localScale = new Vector3((part.getDurability() > 0 ? (float)(part.getRemainingDurability() / part.getDurability()) : 0), widgetDurabilityBar.GetComponent<RectTransform>().localScale.y, 0);
            widgetDurabilityBarLabel.GetComponent<TextMeshProUGUI>().text = DURABILITY_BAR_LABEL_PREFIX + part.getRemainingDurability().ToString() + " / " + part.getDurability().ToString(); ;
            widgetStatStrings[partIndex] = new List<string>();
            widgetStatStrings[partIndex].AddRange(part.getStatStrings());
            foreach (string partStatString in widgetStatStrings[partIndex])
            {
                GameObject partStat = GameObject.Instantiate(Resources.Load("Prefabs/PartStat") as GameObject);
                partStat.GetComponent<TextMeshProUGUI>().text = partStatString;
                if (partStatString.Contains(WEIGHT_STRING) && maxForceExceeded)
                    partStat.GetComponent<TextMeshProUGUI>().color = BAD_COLOR;
                partStat.transform.SetParent(widgetStats.GetComponent<RectTransform>());
                partStat.SetActive(false);
                widgetStatsLabelList[partIndex].Add(partStat);
            }
            widgetDurabilityBar.SetActive(false);
            widgetDurabilityBarLabel.SetActive(false);
            widgetStats.SetActive(true);
            for (int widgetStatIndex = 0; widgetStatIndex < widgetStatsLabelList[partIndex].Count; ++widgetStatIndex)
                widgetStatsLabelList[partIndex][widgetStatIndex].SetActive(false);
            configurationPartWidget.transform.SetParent(WIDGETS_PANEL);
            configurationPartWidget.transform.localPosition = new Vector3(configurationPartWidget.transform.localPosition.x, configurationPartWidget.transform.localPosition.y, 0);
            configurationPartWidget.GetComponent<RectTransform>().localScale = Vector3.one;
            configurationPartWidgets.Add(configurationPartWidget);
            widgetButtons.Add(widgetButton);
            widgetIcons.Add(widgetIcon);
            widgetLabels.Add(widgetLabel);
            widgetRepairButtons.Add(widgetRepairButton);
            widgetRepairButtonLabels.Add(widgetRepairButtonLabel);
            repairDurabilityBars.Add(repairDurabilityBar);
            widgetDurabilityBars.Add(widgetDurabilityBar);
            widgetDurabilityBarLabels.Add(widgetDurabilityBarLabel);
            widgetStatsList.Add(widgetStats);
        }
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

    public void update(string robotName, long credits, double durability, double remainingDurability, string[] robotStatStrings, bool maxForceExceeded, Part[] parts, Color colorScheme, bool enableCreditsSpentAnimation)
    {
        this.colorScheme = colorScheme;
        this.enableCreditsSpentAnimation = enableCreditsSpentAnimation;
        if (WORKSHOP_CREDIT.GetComponent<TextMeshProUGUI>().color != this.colorScheme)
            WORKSHOP_CREDIT.GetComponent<TextMeshProUGUI>().color = this.colorScheme;
        if (this.robotName != robotName)
        {
            this.robotName = robotName;
            robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().text = (!((robotName.Length == 1 && ((int)robotName[0]) == 8203) || this.robotName == default || this.robotName == "" || this.robotName == Robot.DEFAULT_NAME) ? this.robotName : ROBOT_NAME_PLACEHOLDER);
            robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().enabled = robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().text == ROBOT_NAME_PLACEHOLDER;
        }
        this.credits = credits;
        updateCredits();
        this.durability = durability;
        this.remainingDurability = remainingDurability;
        this.maxForceExceeded = maxForceExceeded;
        if (this.parts.Count + attachments.Count != parts.Length)
        {
            this.parts = new List<Part>();
            this.attachments = new List<Attachment>();
            foreach (Part part in parts)
            {
                if (!(part is Attachment))
                    this.parts.Add(part);
                else this.attachments.Add((Attachment)part);
            }
            initialize();
        }
        if (base.enabled && mode != InventoryCard.MODES.PREVIEW_PART)
        {
            robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().text = (!(((int)robotName[0]) == 8203 || robotName == default || robotName == "" || robotName == Robot.DEFAULT_NAME) ? robotName : ROBOT_NAME_PLACEHOLDER);
            robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().enabled = robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().text == ROBOT_NAME_PLACEHOLDER;
            robotWidgetDurabilityBar.GetComponent<RectTransform>().localScale = new Vector3((float)(this.remainingDurability / this.durability), robotWidgetDurabilityBar.GetComponent<RectTransform>().localScale.y, 0);
            robotWidgetDurabilityBarLabel.GetComponent<TextMeshProUGUI>().text = DURABILITY_BAR_LABEL_PREFIX + StringTools.formatString(this.remainingDurability) + " / " + StringTools.formatString(this.durability);
            for (int robotStatStringIndex = 0; robotStatStringIndex < robotStatStrings.Length; ++robotStatStringIndex)
            {
                string robotStatString = robotStatStrings[robotStatStringIndex];
                if (this.robotStatStrings[robotStatStringIndex] != robotStatString)
                {
                    this.robotStatStrings[robotStatStringIndex] = robotStatString;
                    GameObject.Destroy(robotStatStringLabels[robotStatStringIndex]);
                    GameObject partStat = GameObject.Instantiate(Resources.Load("Prefabs/PartStat") as GameObject);
                    partStat.GetComponent<TextMeshProUGUI>().text = robotStatString;
                    if (robotStatString.Contains(WEIGHT_STRING) && this.maxForceExceeded)
                        partStat.GetComponent<TextMeshProUGUI>().color = BAD_COLOR;
                    partStat.transform.SetParent(robotWidgetStats.GetComponent<RectTransform>());
                    robotStatStringLabels[robotStatStringIndex] = partStat;
                }
            }
            if ((WIDGETS_PANEL.offsetMax.y - WIDGETS_PANEL.offsetMin.y) > (WIDGETS_CONTAINER.offsetMax.y - WIDGETS_CONTAINER.offsetMin.y))
            {
                SCROLLBAR_OBJECT.SetActive(true);
                SCROLL_RECT.vertical = true;
            }
            else
            {
                SCROLLBAR_OBJECT.SetActive(false);
                SCROLL_RECT.vertical = false;
            }
            for (int widgetButtonIndex = 0; widgetButtonIndex < widgetButtons.Count; ++widgetButtonIndex)
            {
                Part part = (widgetButtonIndex < parts.Length ? parts[widgetButtonIndex] : attachments[widgetButtonIndex - parts.Length]);
                widgetStates[widgetButtonIndex] = widgetButtons[widgetButtonIndex].GetComponent<ButtonListener>().isClicked() || widgetButtons[widgetButtonIndex].GetComponent<ButtonListener>().isControlClicked();
                float widgetYScale = configurationPartWidgets[widgetButtonIndex].GetComponent<RectTransform>().sizeDelta.y;
                if (!widgetStates[widgetButtonIndex] && Math.Abs(currentWidgetHeights[widgetButtonIndex]) < WIDGET_EXPAND_HEIGHT && Math.Abs(currentWidgetHeights[widgetButtonIndex]) > WIDGET_HEIGHT)
                {
                    currentWidgetHeights[widgetButtonIndex] += WIDGET_EXPAND_RATE;
                    if (Math.Abs(currentWidgetHeights[widgetButtonIndex]) > WIDGET_EXPAND_HEIGHT)
                        currentWidgetHeights[widgetButtonIndex] = currentWidgetHeights[widgetButtonIndex] / Math.Abs(currentWidgetHeights[widgetButtonIndex]) * WIDGET_EXPAND_HEIGHT;
                    if (Math.Abs(currentWidgetHeights[widgetButtonIndex]) < WIDGET_HEIGHT)
                        currentWidgetHeights[widgetButtonIndex] = currentWidgetHeights[widgetButtonIndex] / Math.Abs(currentWidgetHeights[widgetButtonIndex]) * WIDGET_HEIGHT;
                }
                if (widgetStates[widgetButtonIndex])
                {
                    if (widgetButtons[widgetButtonIndex].GetComponent<ButtonListener>().isClicked())
                    {
                        currentWidgetHeights[widgetButtonIndex] *= -1;
                        ++currentWidgetHeights[widgetButtonIndex];
                    }
                    else if (widgetButtons[widgetButtonIndex].GetComponent<ButtonListener>().isControlClicked())
                    {
                        removeParts[widgetButtonIndex] = true;
                    }
                    widgetStates[widgetButtonIndex] = false;
                    widgetButtons[widgetButtonIndex].GetComponent<ButtonListener>().resetClick();
                }
                configurationPartWidgets[widgetButtonIndex].GetComponent<RectTransform>().sizeDelta = new Vector3(configurationPartWidgets[widgetButtonIndex].GetComponent<RectTransform>().sizeDelta.x, Math.Abs(currentWidgetHeights[widgetButtonIndex]), 0);
                widgetButtons[widgetButtonIndex].GetComponent<ButtonListener>().resetClick();
                widgetDurabilityBars[widgetButtonIndex].SetActive(-widgetDurabilityBars[widgetButtonIndex].GetComponent<RectTransform>().sizeDelta.y + widgetDurabilityBars[widgetButtonIndex].transform.position.y >= -configurationPartWidgets[widgetButtonIndex].GetComponent<RectTransform>().sizeDelta.y + configurationPartWidgets[widgetButtonIndex].transform.position.y);
                widgetDurabilityBarLabels[widgetButtonIndex].SetActive(-widgetDurabilityBars[widgetButtonIndex].GetComponent<RectTransform>().sizeDelta.y + widgetDurabilityBars[widgetButtonIndex].transform.position.y >= -configurationPartWidgets[widgetButtonIndex].GetComponent<RectTransform>().sizeDelta.y + configurationPartWidgets[widgetButtonIndex].transform.position.y);
                repairDurabilityBars[widgetButtonIndex].SetActive(-widgetDurabilityBars[widgetButtonIndex].GetComponent<RectTransform>().sizeDelta.y + widgetDurabilityBars[widgetButtonIndex].transform.position.y < -configurationPartWidgets[widgetButtonIndex].GetComponent<RectTransform>().sizeDelta.y + configurationPartWidgets[widgetButtonIndex].transform.position.y);
                for (int widgetStatIndex = 0; widgetStatIndex < widgetStatsLabelList[widgetButtonIndex].Count; ++widgetStatIndex)
                    widgetStatsLabelList[widgetButtonIndex][widgetStatIndex].SetActive(-widgetDurabilityBars[widgetButtonIndex].GetComponent<RectTransform>().sizeDelta.y + widgetDurabilityBars[widgetButtonIndex].transform.position.y >= -configurationPartWidgets[widgetButtonIndex].GetComponent<RectTransform>().sizeDelta.y + configurationPartWidgets[widgetButtonIndex].transform.position.y && -widgetStatsLabelList[widgetButtonIndex][widgetStatIndex].GetComponent<RectTransform>().sizeDelta.y + widgetStatsLabelList[widgetButtonIndex][widgetStatIndex].transform.position.y >= -widgetStatsLabelList[widgetButtonIndex][widgetStatIndex].GetComponent<RectTransform>().sizeDelta.y + widgetStatsLabelList[widgetButtonIndex][widgetStatIndex].transform.position.y);
                canAffordRepair[widgetButtonIndex] = credits >= repairCosts[widgetButtonIndex];
                buttonStates[widgetButtonIndex] = widgetRepairButtons[widgetButtonIndex].GetComponent<ButtonListener>().isClicked() && canAffordRepair[widgetButtonIndex];
                widgetRepairButtons[widgetButtonIndex].GetComponent<UnityEngine.UI.Image>().color = (canAffordRepair[widgetButtonIndex] ? new Color(colorScheme.r, colorScheme.g - (float)((part.getDurability() - part.getRemainingDurability()) / part.getDurability()) * colorScheme.g, colorScheme.b, colorScheme.a) : REPAIR_UNAFFORDABLE_COLOR);
                widgetRepairButtons[widgetButtonIndex].GetComponent<ButtonListener>().resetClick();
                string repairButtonLabel = REPAIR_BUTTON_LABEL + " (<sprite=0> " + repairCosts[widgetButtonIndex].ToString() + ")";
                widgetRepairButtonLabels[widgetButtonIndex].GetComponent<TextMeshProUGUI>().text = repairButtonLabel;
                repairDurabilityBars[widgetButtonIndex].GetComponent<RectTransform>().localScale = new Vector3((part.getDurability() > 0 ? (float)(part.getRemainingDurability() / part.getDurability()) : 0), repairDurabilityBars[widgetButtonIndex].GetComponent<RectTransform>().localScale.y, 0);
                widgetDurabilityBars[widgetButtonIndex].GetComponent<RectTransform>().localScale = new Vector3((part.getDurability() > 0 ? (float)(part.getRemainingDurability() / part.getDurability()) : 0), widgetDurabilityBars[widgetButtonIndex].GetComponent<RectTransform>().localScale.y, 0);
                widgetDurabilityBarLabels[widgetButtonIndex].GetComponent<TextMeshProUGUI>().text = DURABILITY_BAR_LABEL_PREFIX + StringTools.formatString(part.getRemainingDurability()) + " / " + StringTools.formatString(part.getDurability());
                if (part.getRemainingDurability() >= part.getDurability())
                {
                    widgetRepairButtons[widgetButtonIndex].SetActive(false);
                    widgetRepairButtonLabels[widgetButtonIndex].SetActive(false);
                    repairDurabilityBars[widgetButtonIndex].SetActive(false);
                    canAffordRepair[widgetButtonIndex] = false;
                }
                else if (buttonStates[widgetButtonIndex])
                {
                    widgetRepairButtons[widgetButtonIndex].SetActive(false);
                    widgetRepairButtonLabels[widgetButtonIndex].SetActive(false);
                    repairDurabilityBars[widgetButtonIndex].SetActive(false);
                    canAffordRepair[widgetButtonIndex] = false;
                    updateCredits();
                    if (enableCreditsSpentAnimation)
                    {
                        WORKSHOP_CREDITS_SPENT.SetActive(true);
                        WORKSHOP_CREDITS_SPENT.transform.localPosition = WORKSHOP_CREDITS_SPENT_HOME_POSITION;
                        Color storeCreditsSpentColor = WORKSHOP_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color;
                        WORKSHOP_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color = new Color(storeCreditsSpentColor.r, storeCreditsSpentColor.g, storeCreditsSpentColor.b, 1);
                        WORKSHOP_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().text = "-" + PERFORMANCE_METRIC_CALCULATOR.calculateCost(part).ToString();
                    }
                }
            }
            if (enableCreditsSpentAnimation && WORKSHOP_CREDITS_SPENT.activeSelf)
            {
                Vector3 position = WORKSHOP_CREDITS_SPENT.transform.localPosition;
                WORKSHOP_CREDITS_SPENT.transform.localPosition = new Vector3(position.x, position.y + WORKSHOP_CREDITS_SPENT_ANIMATION_POSITION_SPEED, position.z);
                Color storeCreditsSpentColor = WORKSHOP_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color;
                WORKSHOP_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color = new Color(storeCreditsSpentColor.r, storeCreditsSpentColor.g, storeCreditsSpentColor.b, storeCreditsSpentColor.a - WORKSHOP_CREDITS_SPENT_ANIMATION_COLOR_SPEED);
                if (WORKSHOP_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color.a <= 0)
                    WORKSHOP_CREDITS_SPENT.SetActive(false);
            }
            else if (!enableCreditsSpentAnimation)
            {
                if (WORKSHOP_CREDITS_SPENT.transform.localPosition != WORKSHOP_CREDITS_SPENT_HOME_POSITION)
                    WORKSHOP_CREDITS_SPENT.transform.localPosition = WORKSHOP_CREDITS_SPENT_HOME_POSITION;
                Color storeCreditsSpentColor = WORKSHOP_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color;
                if (WORKSHOP_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color.a != 1)
                    WORKSHOP_CREDITS_SPENT.GetComponent<TextMeshProUGUI>().color = new Color(storeCreditsSpentColor.r, storeCreditsSpentColor.g, storeCreditsSpentColor.b, 1);
                if (WORKSHOP_CREDITS_SPENT.activeInHierarchy)
                    WORKSHOP_CREDITS_SPENT.SetActive(false);
            }
            checkRobotName();
        }
    }

    public void checkRobotName()
    {
        if (robotWidgetLabelText.GetComponent<TextMeshProUGUI>().text != this.robotName)
        {
            this.robotName = (((int)robotWidgetLabelText.GetComponent<TextMeshProUGUI>().text[0]) != 8203 ? robotWidgetLabelText.GetComponent<TextMeshProUGUI>().text : Robot.DEFAULT_NAME);
            robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().text = (!((robotName.Length == 1 && ((int)robotName[0]) == 8203) || this.robotName == default || this.robotName == "" || this.robotName == Robot.DEFAULT_NAME) ? this.robotName : ROBOT_NAME_PLACEHOLDER);
            robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().enabled = robotWidgetLabelPlaceholder.GetComponent<TextMeshProUGUI>().text == ROBOT_NAME_PLACEHOLDER;
        }
    }

    public string getRobotName()
    {
        return this.robotName;
    }

    public int[] getRepairCosts()
    {
        return repairCosts;
    }

    public bool[] getButtonStates()
    {
        return buttonStates;
    }

    public void resetButtonStates()
    {
        for (int buttonIndex = 0; buttonIndex < numberOfWidgets; ++buttonIndex)
            buttonStates[buttonIndex] = false;
    }

    public bool[] getRemoveParts()
    {
        return removeParts;
    }

    public void resetRemoveParts()
    {
        for (int buttonIndex = 0; buttonIndex < numberOfWidgets; ++buttonIndex)
            removeParts[buttonIndex] = false;
    }

    public void setMode(InventoryCard.MODES mode, Part partBeingPreviewed, Part partToEquipt, string[] previewRobotStats)
    {
        this.partBeingPreviewed = partBeingPreviewed;
        this.partToEquipt = partToEquipt;
        if (mode != this.mode)
        {
            switch (mode)
            {
                case InventoryCard.MODES.VIEW_PART_STATS:
                    MASKED_ROBOT_WIDGET.SetActive(false);
                    break;
                case InventoryCard.MODES.PREVIEW_PART:
                    MASKED_ROBOT_WIDGET.transform.Find("MaskedRobotWidgetLabel").gameObject.GetComponent<TextMeshProUGUI>().text = robotWidgetLabel.GetComponent<TMP_InputField>().text;
                    MASKED_ROBOT_WIDGET.transform.Find("MaskedRobotWidgetDurabilityBar").gameObject.GetComponent<RectTransform>().localScale = robotWidgetDurabilityBar.GetComponent<RectTransform>().localScale;
                    MASKED_ROBOT_WIDGET.transform.Find("MaskedRobotWidgetDurabilityBarLabel").gameObject.GetComponent<TextMeshProUGUI>().text = robotWidgetDurabilityBarLabel.GetComponent<TextMeshProUGUI>().text;
                    MASKED_ROBOT_WIDGET.GetComponent<RectTransform>().localScale = Vector3.one;
                    MASKED_ROBOT_WIDGET.transform.position = robotWidget.transform.position;
                    MASKED_ROBOT_WIDGET.SetActive(true);
                    Part robotPart = null;
                    double robotPartRemainingDurability = 0, robotPartDurability = 0;
                    for (int partIndex = 0; partIndex < numberOfWidgets; ++partIndex)
                    {
                        Part part = (partIndex < parts.Count ? parts[partIndex] : attachments[partIndex - parts.Count]);
                        if (part.GetType() == this.partBeingPreviewed.GetType())
                        {
                            robotPart = part;
                            robotPartRemainingDurability = robotPart.getRemainingDurability();
                            robotPartDurability = robotPart.getDurability();
                            break;
                        }
                    }
                    double[] differenceInStats = robotPart != null ? robotPart.compareTo(this.partBeingPreviewed) : (new Blaster("", null, 0, null, Shape.SHAPES.CYLINDER, 0, 0, 0, 0, 0, 0)).compareTo(this.partBeingPreviewed);
                    double tempRemainingDurability = this.remainingDurability - robotPartRemainingDurability + this.partBeingPreviewed.getRemainingDurability();
                    double tempDurability = this.durability - robotPartDurability + this.partBeingPreviewed.getDurability();
                    MASKED_ROBOT_WIDGET.transform.Find("MaskedRobotWidgetDurabilityBar").gameObject.GetComponent<RectTransform>().localScale = new Vector3((float)(tempRemainingDurability / tempDurability), robotWidgetDurabilityBar.GetComponent<RectTransform>().localScale.y, 0);
                    MASKED_ROBOT_WIDGET.transform.Find("MaskedRobotWidgetDurabilityBarLabel").gameObject.GetComponent<TextMeshProUGUI>().text = ConfigurationCard.DURABILITY_BAR_LABEL_PREFIX + StringTools.formatString(tempRemainingDurability) + " / " + StringTools.formatString(tempDurability) + (differenceInStats != null && differenceInStats.Length > 0 ? " (" + applyStatDifferenceFormatting(differenceInStats[0], false) + " / " + applyStatDifferenceFormatting(differenceInStats[1], false) + ")" : "");
                    GameObject maskedRobotWidgetStats = MASKED_ROBOT_WIDGET.transform.Find("MaskedRobotWidgetStats").gameObject;
                    foreach (Transform child in maskedRobotWidgetStats.GetComponent<RectTransform>())
                        GameObject.Destroy(child.gameObject);
                    for (int robotStatStringIndex = 0; robotStatStringIndex < robotStatStringLabels.Count; ++robotStatStringIndex)
                    {
                        robotStatStringLabels[robotStatStringIndex].GetComponent<TextMeshProUGUI>().text = previewRobotStats[robotStatStringIndex];
                        GameObject partStat = GameObject.Instantiate(Resources.Load("Prefabs/PartStat") as GameObject);
                        partStat.GetComponent<TextMeshProUGUI>().text = previewRobotStats[robotStatStringIndex];
                        if (previewRobotStats[robotStatStringIndex].Contains(WEIGHT_STRING) && maxForceExceeded)
                            partStat.GetComponent<TextMeshProUGUI>().color = BAD_COLOR;
                        partStat.transform.SetParent(maskedRobotWidgetStats.GetComponent<RectTransform>());
                        partStat.transform.localPosition = new Vector3(partStat.transform.localPosition.x, partStat.transform.localPosition.y, 0);
                    }
                    break;
                case InventoryCard.MODES.EQUIPT_PART:
                    MASKED_ROBOT_WIDGET.SetActive(false);
                    for (int robotPartIndex = 0; robotPartIndex < parts.Count + attachments.Count; ++robotPartIndex)
                    {
                        if (robotPartIndex < parts.Count && parts[robotPartIndex].GetType() == partToEquipt.GetType())
                        {
                            parts[robotPartIndex] = partToEquipt;
                            break;
                        }
                        else if (robotPartIndex >= parts.Count && attachments[robotPartIndex - parts.Count].GetType() == partToEquipt.GetType())
                        {
                            attachments[robotPartIndex - parts.Count] = (Attachment)partToEquipt;
                            break;
                        }
                        else if (robotPartIndex - parts.Count == attachments.Count - 1)
                            attachments.Add((Attachment)partToEquipt);
                    }
                    initialize();
                    break;
                default:
                    break;
            }
            this.mode = mode;
        }
    }

    public InventoryCard.MODES getMode()
    {
        return mode;
    }

    private string applyStatDifferenceFormatting(double statDifference, bool reverse)
    {
        string formattedStatDifference = "";
        if (!reverse)
            formattedStatDifference = (statDifference > 0 ? STAT_BETTER_COLOR_PREFIX + "+" + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : (statDifference < 0 ? STAT_WORSE_COLOR_PREFIX + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : StringTools.formatString(statDifference)));
        else formattedStatDifference = (statDifference > 0 ? STAT_WORSE_COLOR_PREFIX + "+" + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : (statDifference < 0 ? STAT_BETTER_COLOR_PREFIX + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : StringTools.formatString(statDifference)));
        return formattedStatDifference;
    }

    public void setForStoreCompare(bool comparingWithStore)
    {
        if (comparingWithStore)
        {
            GameObject.Find("BuildHubCanvas").transform.Find("Workshop").gameObject.SetActive(true);
            SCROLL_VIEW.SetActive(false);
            GameObject.Find("InventoryTabs").SetActive(false);
            GameObject.Find("InventoryFilterWidget").SetActive(false);
            GameObject.Find("InventoryCard").SetActive(false);
            if (GameObject.Find("WorkshopPartInfo") != null)
                GameObject.Find("WorkshopPartInfo").SetActive(false);
            GameObject.Find("BuildHubCanvas").transform.Find("Workshop").Find("WorkshopMask").gameObject.SetActive(false);
            if (GameObject.Find("MaskedInventoryPartButton") != null)
                GameObject.Find("MaskedInventoryPartButton").SetActive(false);
            MASKED_ROBOT_WIDGET.SetActive(true);
            MASKED_ROBOT_WIDGET.transform.SetParent(GameObject.Find("BuildHubCanvas").transform, true);
        }
        else
        {
            MASKED_ROBOT_WIDGET.transform.SetParent(GameObject.Find("BuildHubCanvas").transform.Find("Workshop"), true);
            MASKED_ROBOT_WIDGET.SetActive(false);
            GameObject.Find("BuildHubCanvas").transform.Find("Workshop").Find("WorkshopPartInfo").gameObject.SetActive(true);
            GameObject.Find("BuildHubCanvas").transform.Find("Workshop").Find("WorkshopMask").gameObject.SetActive(false);
            GameObject.Find("BuildHubCanvas").transform.Find("Workshop").Find("MaskedInventoryPartButton").gameObject.SetActive(false);
            SCROLL_VIEW.SetActive(true);
            GameObject.Find("BuildHubCanvas").transform.Find("Workshop").Find("InventoryTabs").gameObject.SetActive(true);
            GameObject.Find("BuildHubCanvas").transform.Find("Workshop").Find("InventoryFilterWidget").gameObject.SetActive(true);
            GameObject.Find("BuildHubCanvas").transform.Find("Workshop").Find("InventoryCard").gameObject.SetActive(true);
            GameObject.Find("BuildHubCanvas").transform.Find("Workshop").gameObject.SetActive(false);
        }
    }

    public void destroyAllGeneratedObjects()
    {
        foreach (GameObject statStringLabel in robotStatStringLabels)
            GameObject.Destroy(statStringLabel);
        foreach (GameObject configurationPartWidget in configurationPartWidgets)
            GameObject.Destroy(configurationPartWidget);
        foreach (GameObject widgetButton in widgetButtons)
            GameObject.Destroy(widgetButton);
        foreach (GameObject widgetIcon in widgetIcons)
            GameObject.Destroy(widgetIcon);
        foreach (GameObject widgetLabel in widgetLabels)
            GameObject.Destroy(widgetLabel);
        foreach (GameObject widgetRepairButton in widgetRepairButtons)
            GameObject.Destroy(widgetRepairButton);
        foreach (GameObject widgetRepairButtonLabel in widgetRepairButtonLabels)
            GameObject.Destroy(widgetRepairButtonLabel);
        foreach (GameObject repairDurabilityBar in repairDurabilityBars)
            GameObject.Destroy(repairDurabilityBar);
        foreach (GameObject widgetDurabilityBar in widgetDurabilityBars)
            GameObject.Destroy(widgetDurabilityBar);
        foreach (GameObject widgetDurabilityBarLabel in widgetDurabilityBarLabels)
            GameObject.Destroy(widgetDurabilityBarLabel);
        foreach (GameObject widgetStat in widgetStatsList)
            GameObject.Destroy(widgetStat);
        foreach (List<GameObject> widgetStatsLabels in widgetStatsLabelList)
            foreach (GameObject widgetStatLabel in widgetStatsLabels)
                GameObject.Destroy(widgetStatLabel);
    }
}