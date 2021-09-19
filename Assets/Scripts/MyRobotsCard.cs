using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyRobotsCard : PeripheralElement
{
    private List<Robot> myRobots, robotsToRemove;
    private List<Part> humanRobotParts;
    private List<int> selectedRobotIndicesToRemove;
    private long credits;
    private Robot robotBeingPreviewed, robotBeingPicked, newRobot;
    private readonly string MY_ROBOTS_CARD_NAME = "MyRobotsCard", MY_ROBOTS_CONTAINER_NAME = "MyRobotsContainer", MY_ROBOTS_PANEL_NAME = "MyRobotsPanel", MY_ROBOTS_SCROLLBAR_NAME = "MyRobotsScrollbar", MY_ROBOTS_INFO_NAME = "MyRobotsInfo", MY_ROBOTS_NAME = "MyRobotsName", MY_ROBOTS_STATS_NAME = "MyRobotsStats", DEFAULT_SORT_STRING = "Sort", WEIGHT_NAME = "Weight", REMAINING_DURABILITY_NAME = "Remaining durability", DURABILITY_NAME = "Durability", DURABILITY_BAR_LABEL_PREFIX = "Durability: ", REMOVE_ROBOTS_TEXT = "Remove Robots", REMOVE_SELECTED_ROBOTS_TEXT = "Remove Selected";
    private readonly GameObject MY_ROBOTS_SORT, SCROLL_VIEW, SCROLLBAR_OBJECT, NEW_ROBOT_BUTTON, REMOVE_ROBOTS_BUTTON, CANCEL_REMOVE_ROBOTS_BUTTON, MY_ROBOTS_CREDIT;
    private GameObject myRobotsInfo, myRobotsName, myRobotsStats, robotWidgetDurabilityBar, robotWidgetDurabilityBarLabel;
    private List<GameObject> myRobotsButtons, myRobotsStatsList;
    private readonly int DEFAULT_SORT = 0;
    public enum MODES { VIEW_MY_ROBOTS_STATS, PICK_ROBOT, NEW_ROBOT, REMOVE_ROBOTS };
    private MODES mode;
    private readonly ScrollRect SCROLL_RECT;
    private readonly Scrollbar SCROLLBAR;
    private readonly RectTransform MY_ROBOTS_CONTAINER, MY_ROBOTS_PANEL;
    private int currentSort;
    private readonly Color REMOVE_ROBOTS_BUTTON_COLOR = Color.red;
    private Color colorScheme;

    public MyRobotsCard(Robot[] myRobots, Part[] humanRobotParts, long credits, Color colorScheme)
    {
        this.myRobots = new List<Robot>();
        this.myRobots.AddRange(myRobots);
        this.humanRobotParts = new List<Part>();
        this.humanRobotParts.AddRange(humanRobotParts);
        this.credits = credits;
        this.colorScheme = colorScheme;
        robotBeingPreviewed = null;
        robotBeingPicked = null;
        robotsToRemove = new List<Robot>();
        selectedRobotIndicesToRemove = new List<int>();
        newRobot = null;
        mode = MODES.VIEW_MY_ROBOTS_STATS;
        MY_ROBOTS_SORT = GameObject.Find("MyRobotsSort");
        currentSort = DEFAULT_SORT;
        SCROLL_VIEW = GameObject.Find(MY_ROBOTS_CARD_NAME);
        GameObject storeContainerGameObject = GameObject.Find(MY_ROBOTS_CONTAINER_NAME);
        MY_ROBOTS_CONTAINER = storeContainerGameObject.GetComponent<RectTransform>();
        SCROLL_RECT = SCROLL_VIEW.GetComponent<ScrollRect>();
        MY_ROBOTS_PANEL = GameObject.Find(MY_ROBOTS_PANEL_NAME).GetComponent<RectTransform>();
        SCROLLBAR_OBJECT = GameObject.Find(MY_ROBOTS_CARD_NAME).transform.Find(MY_ROBOTS_SCROLLBAR_NAME).gameObject;
        SCROLLBAR = SCROLLBAR_OBJECT.GetComponent<Scrollbar>();
        SCROLLBAR_OBJECT.SetActive(false);
        SCROLL_RECT.vertical = false;
        myRobotsButtons = new List<GameObject>();
        myRobotsInfo = GameObject.Find("MyRobots").transform.Find(MY_ROBOTS_INFO_NAME).gameObject;
        myRobotsName = myRobotsInfo.transform.Find(MY_ROBOTS_NAME).gameObject;
        myRobotsStats = myRobotsInfo.transform.Find(MY_ROBOTS_STATS_NAME).gameObject;
        robotWidgetDurabilityBar = myRobotsInfo.transform.transform.Find("DurabilityBar").gameObject;
        robotWidgetDurabilityBarLabel = myRobotsInfo.transform.transform.Find("DurabilityBarLabel").gameObject;
        myRobotsInfo.SetActive(false);
        NEW_ROBOT_BUTTON = GameObject.Find("NewRobot");
        NEW_ROBOT_BUTTON.GetComponent<UnityEngine.UI.Image>().color = this.colorScheme;
        REMOVE_ROBOTS_BUTTON = GameObject.Find("MyRobots").transform.Find("RemoveRobots").gameObject;
        CANCEL_REMOVE_ROBOTS_BUTTON = GameObject.Find("MyRobots").transform.Find("CancelRemoveRobots").gameObject;
        if (myRobots.Length == 0)
            REMOVE_ROBOTS_BUTTON.SetActive(false);
        CANCEL_REMOVE_ROBOTS_BUTTON.SetActive(false);
        MY_ROBOTS_CREDIT = GameObject.Find("MyRobotsCredit");
        MY_ROBOTS_CREDIT.GetComponent<TextMeshProUGUI>().color = this.colorScheme;
        updateCredits();
        myRobotsStatsList = new List<GameObject>();
        foreach (Robot robot in this.myRobots)
        {
            GameObject robotButton = GameObject.Instantiate(Resources.Load("Prefabs/RobotButton") as GameObject);
            Texture2D robotIcon = robot.getHead().getIcon();
            if (robotIcon != null)
                robotButton.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(robotIcon, new Rect(0, 0, robotIcon.width, robotIcon.height), new Vector2(0.5f, 0.5f), 100);
            robotButton.GetComponent<RectTransform>().localScale = Vector3.one;
            robotButton.transform.localPosition = new Vector3(robotButton.transform.localPosition.x, robotButton.transform.localPosition.y, 0);
            robotButton.transform.Find("NameLabel").GetComponent<TextMeshProUGUI>().text = robot.getName();
            robotButton.transform.SetParent(MY_ROBOTS_PANEL);
            robotButton.transform.localPosition = new Vector3(robotButton.transform.localPosition.x, robotButton.transform.localPosition.y, 0);
            robotButton.transform.Find("SelectionIndicator").gameObject.SetActive(false);
            myRobotsButtons.Add(robotButton);
        }
        if (this.myRobots.Count > 0)
            updateSorting();
    }

    public void updateCredits()
    {
        string storeCreditText = MY_ROBOTS_CREDIT.GetComponent<TextMeshProUGUI>().text;
        MY_ROBOTS_CREDIT.GetComponent<TextMeshProUGUI>().text = storeCreditText.Substring(0, storeCreditText.LastIndexOf(" ") + 1) + credits.ToString();
    }

    protected override void calculatePoints()
    {

    }

    public override void update()
    {

    }

    public void update(Robot[] myRobots, Part[] humanRobotParts, long credits, Color colorScheme)
    {
        this.credits = credits;
        if (this.colorScheme != colorScheme)
        {
            this.colorScheme = colorScheme;
            NEW_ROBOT_BUTTON.GetComponent<UnityEngine.UI.Image>().color = this.colorScheme;
            MY_ROBOTS_CREDIT.GetComponent<TextMeshProUGUI>().color = this.colorScheme;
        }
        if (this.myRobots.Count != myRobots.Length)
        {
            this.myRobots.Clear();
            this.myRobots.AddRange(myRobots);
        }
        if (this.humanRobotParts.Count != humanRobotParts.Length)
        {
            this.humanRobotParts.Clear();
            this.humanRobotParts.AddRange(humanRobotParts);
        }
        if (base.enabled)
        {
            if ((mode != MODES.NEW_ROBOT && (myRobots == null || myRobots.Length == 0)) || (NEW_ROBOT_BUTTON.activeSelf && NEW_ROBOT_BUTTON.GetComponent<ButtonListener>().isClicked()))
            {
                NEW_ROBOT_BUTTON.GetComponent<ButtonListener>().resetClick();
                mode = MODES.NEW_ROBOT;
                Part[] parts = null;
                if (myRobots != null && myRobots.Length > 0)
                    parts = myRobots[0].getParts();
                else
                {
                    List<Part> defaultParts = new List<Part>();
                    defaultParts.Add(findFirstPartOfType(this.humanRobotParts, typeof(Head)));
                    defaultParts.Add(findFirstPartOfType(this.humanRobotParts, typeof(Body)));
                    defaultParts.Add(findFirstPartOfType(this.humanRobotParts, typeof(Mobility)));
                    defaultParts.Add(findFirstPartOfType(this.humanRobotParts, typeof(Blaster)));
                    parts = defaultParts.ToArray();
                }
                List<Part> newParts = new List<Part>();
                foreach (Part part in parts)
                    newParts.Add(part.clone(true));
                newRobot = new Robot("", true, true, newParts.ToArray());
                robotBeingPreviewed = null;
            }
            else if (CANCEL_REMOVE_ROBOTS_BUTTON.GetComponent<ButtonListener>().isClicked())
            {
                mode = MODES.VIEW_MY_ROBOTS_STATS;
                CANCEL_REMOVE_ROBOTS_BUTTON.GetComponent<ButtonListener>().resetClick();
                REMOVE_ROBOTS_BUTTON.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text = REMOVE_ROBOTS_TEXT;
                CANCEL_REMOVE_ROBOTS_BUTTON.SetActive(false);
                robotsToRemove = new List<Robot>();
                selectedRobotIndicesToRemove = new List<int>();
                foreach (GameObject robotButton in myRobotsButtons)
                    robotButton.transform.Find("SelectionIndicator").gameObject.SetActive(false);
            }
            else if (REMOVE_ROBOTS_BUTTON.GetComponent<ButtonListener>().isClicked())
            {
                REMOVE_ROBOTS_BUTTON.GetComponent<ButtonListener>().resetClick();
                switch (mode)
                {
                    case MODES.REMOVE_ROBOTS:
                        if (selectedRobotIndicesToRemove.Count > 0)
                        {
                            mode = MODES.VIEW_MY_ROBOTS_STATS;
                            REMOVE_ROBOTS_BUTTON.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text = REMOVE_ROBOTS_TEXT;
                            CANCEL_REMOVE_ROBOTS_BUTTON.SetActive(false);
                            robotsToRemove = new List<Robot>();
                            foreach (int robotIndex in selectedRobotIndicesToRemove)
                            {
                                robotsToRemove.Add(this.myRobots[robotIndex]);
                                this.myRobots.RemoveAt(robotIndex);
                                GameObject.Destroy(myRobotsButtons[robotIndex]);
                                myRobotsButtons.RemoveAt(robotIndex);
                            }
                            selectedRobotIndicesToRemove = new List<int>();
                        }
                        break;
                    default:
                        mode = MODES.REMOVE_ROBOTS;
                        REMOVE_ROBOTS_BUTTON.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text = REMOVE_SELECTED_ROBOTS_TEXT;
                        REMOVE_ROBOTS_BUTTON.GetComponent<UnityEngine.UI.Image>().color = CANCEL_REMOVE_ROBOTS_BUTTON.GetComponent<UnityEngine.UI.Image>().color;
                        CANCEL_REMOVE_ROBOTS_BUTTON.SetActive(true);
                        robotsToRemove = new List<Robot>();
                        selectedRobotIndicesToRemove = new List<int>();
                        break;
                }
            }
            if ((MY_ROBOTS_PANEL.offsetMax.y - MY_ROBOTS_PANEL.offsetMin.y) > (MY_ROBOTS_CONTAINER.offsetMax.y - MY_ROBOTS_CONTAINER.offsetMin.y))
            {
                SCROLLBAR_OBJECT.SetActive(true);
                SCROLL_RECT.vertical = true;
            }
            else
            {
                SCROLLBAR_OBJECT.SetActive(false);
                SCROLL_RECT.vertical = false;
            }
            for (int myRobotsButtonIndex = 0; myRobotsButtonIndex < myRobotsButtons.Count; ++myRobotsButtonIndex)
            {
                ButtonListener buttonListener = myRobotsButtons[myRobotsButtonIndex].GetComponent<ButtonListener>();
                if (buttonListener.isClicked())
                {
                    buttonListener.resetClick();
                    switch (mode)
                    {
                        case MODES.VIEW_MY_ROBOTS_STATS:
                            mode = MODES.PICK_ROBOT;
                            myRobotsInfo.SetActive(false);
                            robotBeingPicked = myRobots[myRobotsButtonIndex];
                            break;
                        case MODES.REMOVE_ROBOTS:
                            if (!selectedRobotIndicesToRemove.Contains(myRobotsButtonIndex))
                            {
                                selectedRobotIndicesToRemove.Add(myRobotsButtonIndex);
                                myRobotsButtons[myRobotsButtonIndex].transform.Find("SelectionIndicator").gameObject.SetActive(true);
                                REMOVE_ROBOTS_BUTTON.GetComponent<UnityEngine.UI.Image>().color = REMOVE_ROBOTS_BUTTON_COLOR;
                            }
                            else
                            {
                                selectedRobotIndicesToRemove.Remove(myRobotsButtonIndex);
                                myRobotsButtons[myRobotsButtonIndex].transform.Find("SelectionIndicator").gameObject.SetActive(false);
                                if (selectedRobotIndicesToRemove.Count == 0)
                                    REMOVE_ROBOTS_BUTTON.GetComponent<UnityEngine.UI.Image>().color = CANCEL_REMOVE_ROBOTS_BUTTON.GetComponent<UnityEngine.UI.Image>().color;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                }
                if (buttonListener.isMouseOver() && mode == MODES.VIEW_MY_ROBOTS_STATS)
                {
                    robotBeingPreviewed = myRobots[myRobotsButtonIndex];
                    Vector2 position;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(GameObject.Find("BuildHubCanvas").GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out position);
                    position.x += myRobotsInfo.GetComponent<RectTransform>().sizeDelta.x / 2;
                    myRobotsInfo.transform.localPosition = position;
                    myRobotsName.GetComponent<TextMeshProUGUI>().text = myRobots[myRobotsButtonIndex].getName();
                    foreach (GameObject partStat in myRobotsStatsList)
                        GameObject.Destroy(partStat);
                    myRobotsStatsList.Clear();
                    robotWidgetDurabilityBar.GetComponent<RectTransform>().localScale = new Vector3((float)(myRobots[myRobotsButtonIndex].getRemainingDurability() / myRobots[myRobotsButtonIndex].getDurability()), robotWidgetDurabilityBar.GetComponent<RectTransform>().localScale.y, 0);
                    robotWidgetDurabilityBarLabel.GetComponent<TextMeshProUGUI>().text = DURABILITY_BAR_LABEL_PREFIX + StringTools.formatString(myRobots[myRobotsButtonIndex].getRemainingDurability()) + " / " + StringTools.formatString(myRobots[myRobotsButtonIndex].getDurability());
                    string[] myRobotsStatStrings = myRobots[myRobotsButtonIndex].getRobotStatStrings();
                    for (int myRobotsStatIndex = 0; myRobotsStatIndex < myRobotsStatStrings.Length; ++myRobotsStatIndex)
                    {
                        GameObject myRobotsStat = GameObject.Instantiate(Resources.Load("Prefabs/PartStat") as GameObject);
                        myRobotsStat.GetComponent<TextMeshProUGUI>().text = myRobotsStatStrings[myRobotsStatIndex];
                        myRobotsStat.transform.SetParent(myRobotsStats.GetComponent<RectTransform>());
                        myRobotsStat.transform.localPosition = new Vector3(myRobotsStat.transform.localPosition.x, myRobotsStat.transform.localPosition.y, 0);
                        myRobotsStatsList.Add(myRobotsStat);
                    }
                    myRobotsInfo.SetActive(true);
                    break;
                }
                else myRobotsInfo.SetActive(false);
            }
            if (currentSort != MY_ROBOTS_SORT.GetComponent<TMP_Dropdown>().value)
            {
                currentSort = MY_ROBOTS_SORT.GetComponent<TMP_Dropdown>().value;
                applySorting();
            }
        }
    }

    private Part findFirstPartOfType(List<Part> partList, Type partType)
    {
        foreach (Part part in partList)
            if (part.GetType() == partType)
                return part;
        return null;
    }

    private void updateSorting()
    {
        string[] partStatStrings = myRobots[0].getRobotStatStrings();
        MY_ROBOTS_SORT.GetComponent<TMP_Dropdown>().ClearOptions();
        List<TMP_Dropdown.OptionData> sortOptions = new List<TMP_Dropdown.OptionData>();
        sortOptions.Add(new TMP_Dropdown.OptionData(DEFAULT_SORT_STRING));
        sortOptions.Add(new TMP_Dropdown.OptionData(DURABILITY_NAME));
        sortOptions.Add(new TMP_Dropdown.OptionData(REMAINING_DURABILITY_NAME));
        foreach (string statString in partStatStrings)
            sortOptions.Add(new TMP_Dropdown.OptionData(statString.Substring(0, statString.IndexOf(":"))));
        MY_ROBOTS_SORT.GetComponent<TMP_Dropdown>().AddOptions(sortOptions);
        applySorting();
    }

    private void applySorting()
    {
        List<GameObject> myRobotsButtonsToSort = new List<GameObject>();
        GameObject[] sortedRobotButtons = null;
        List<Robot> myRobotsToSort = new List<Robot>();
        for (int myRobotsButtonIndex = 0; myRobotsButtonIndex < myRobotsButtons.Count; ++myRobotsButtonIndex)
        {
            if (myRobotsButtons[myRobotsButtonIndex].transform.parent != null)
            {
                myRobotsButtonsToSort.Add(myRobotsButtons[myRobotsButtonIndex]);
                myRobotsButtons[myRobotsButtonIndex].transform.SetParent(null);
                myRobotsToSort.Add(myRobots[myRobotsButtonIndex]);
            }
        }
        sortedRobotButtons = new GameObject[myRobotsButtonsToSort.Count];
        if (currentSort > 0)
        {
            myRobotsToSort.Sort((x, y) => Mathf.RoundToInt((float)y.compareTo(x)[currentSort]));
            if (!(MY_ROBOTS_SORT.GetComponent<TMP_Dropdown>().captionText.text.Contains(WEIGHT_NAME)))
                myRobotsToSort.Reverse();
        }
        foreach (GameObject robotButton in myRobotsButtonsToSort)
            sortedRobotButtons[myRobotsToSort.IndexOf(myRobots[myRobotsButtons.IndexOf(robotButton)])] = robotButton;
        foreach (GameObject robotButton in sortedRobotButtons)
            robotButton.transform.SetParent(MY_ROBOTS_PANEL);
    }

    public MODES getMode()
    {
        return mode;
    }

    public Robot getRobotBeingPreviewed()
    {
        return robotBeingPreviewed;
    }

    public Robot getRobotBeingPicked()
    {
        return robotBeingPicked;
    }

    public Robot getNewRobot()
    {
        return newRobot;
    }

    public List<Robot> getRobotsToRemove()
    {
        return robotsToRemove;
    }

    public void clearPickedAndNewRobotAndRobotsToRemove()
    {
        robotBeingPicked = null;
        newRobot = null;
        robotsToRemove = new List<Robot>();
        selectedRobotIndicesToRemove = new List<int>();
    }

    public void destroyAllGeneratedObjects()
    {
        foreach (GameObject robotButton in myRobotsButtons)
            GameObject.Destroy(robotButton);
        foreach (GameObject myRobotsStat in myRobotsStatsList)
            GameObject.Destroy(myRobotsStat);
    }
}