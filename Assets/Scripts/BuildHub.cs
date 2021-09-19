using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class BuildHub : Screen
{
    private MyRobots myRobots;
    private Workshop workshop;
    private Store store;
    private ObstacleList obstacleList;
    private readonly List<ObstacleData> OBSTACLES_DATA;
    private List<Robot> myRobotsList;
    private List<Part> humanRobotParts, previewRobotParts;
    private readonly Part[] STORE_PARTS;
    private long credits;
    public enum MODES { MY_ROBOTS, WORKSHOP, STORE };
    private MODES mode;
    private readonly float PREVIEW_ROTATE_DEGREES_PER_FRAME = 0.5f, PREVIEW_ROBOT_VERTICAL_ROTATION_WEIGHT = 0.1f, MIN_VERTICAL = -45.0f, MAX_VERTICAL = 45.0f;
    private Robot previewRobot, currentRobot;
    private Color ACTIVE_TAB_COLOR = new Color(0, 0, 0, 0.2f), INACTIVE_TAB_COLOR = new Color(0, 0, 0, 0.39215686274f);
    private new Color colorScheme;
    private readonly List<GameObject> TABS;
    private readonly GameObject MY_ROBOTS_BACK_BUTTON, OBSTACLES_INFO_BUTTON, SETTINGS_BUTTON, TRAINING_BUTTON, CREDITS_BUTTON, CREDITS_WIDGET, BACKGROUND, PLATFORM;
    private readonly List<Mesh> MESHES;
    private bool goToField, enablePreviewRobotAnimation, enableCreditsSpentAnimation, trainingMode;
    private string backgroundGraphicString, platformGraphicString;

    public BuildHub(List<Setting> settingList, List<ObstacleData> obstaclesData, List<Robot> myRobots, List<Part> humanRobotParts, Part[] storeParts, long credits, MODES mode) : base(settingList)
    {
        myRobotsList = new List<Robot>();
        if (myRobots != null && myRobots.Count > 0)
            myRobotsList.AddRange(myRobots);
        this.humanRobotParts = humanRobotParts;
        currentRobot = (this.myRobotsList.Count > 0 ? this.myRobotsList[0] : null);
        OBSTACLES_DATA = obstaclesData;
        STORE_PARTS = storeParts;
        this.credits = credits;
        this.mode = mode;
        goToField = false;
        trainingMode = false;
        MESHES = new List<Mesh>();
        MESHES.Add(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube));
        MESHES.Add(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Sphere));
        MESHES.Add(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cylinder));
        MESHES.Add(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube));
        foreach (Part part in this.humanRobotParts)
            if (!(part is Attachment) && part.GAME_OBJECT != null)
                part.toggleGameObject(false);
        foreach (Part part in STORE_PARTS)
            if (part.GAME_OBJECT != null)
                part.toggleGameObject(false);
        if (currentRobot != null)
            createPreviewRobot();
        this.myRobots = new MyRobots(myRobotsList.ToArray(), this.humanRobotParts.ToArray(), this.credits, base.colorScheme);
        workshop = new Workshop(MESHES, previewRobot, (currentRobot != null ? currentRobot.getName() : ""), this.humanRobotParts, (currentRobot != null ? currentRobot.getParts() : null), this.credits, base.colorScheme, enableCreditsSpentAnimation);
        store = new Store(MESHES, previewRobot, this.STORE_PARTS, this.humanRobotParts, (currentRobot != null ? currentRobot.getParts() : null), this.credits, workshop.getConfigurationCard(), base.colorScheme, enableCreditsSpentAnimation);
        obstacleList = new ObstacleList(OBSTACLES_DATA);
        TABS = new List<GameObject>();
        foreach (Transform child in GameObject.Find("BuildHubTabs").transform)
            TABS.Add(child.gameObject);
        MY_ROBOTS_BACK_BUTTON = GameObject.Find("MyRobotsBackButton");
        OBSTACLES_INFO_BUTTON = GameObject.Find("ObstaclesInfoButton");
        SETTINGS_BUTTON = GameObject.Find("SettingsButton");
        TRAINING_BUTTON = GameObject.Find("TrainingButton");
        CREDITS_BUTTON = GameObject.Find("CreditsButton");
        CREDITS_WIDGET = GameObject.Find("CreditsWidget");
        CREDITS_WIDGET.SetActive(false);
        base.colorScheme = ImageTools.getColorFromString(base.settingPairs.color_scheme);
        enableCreditsSpentAnimation = base.settingPairs.credits_spent_animation;
        SETTINGS_BUTTON.GetComponent<UnityEngine.UI.Image>().color = base.colorScheme;
        TRAINING_BUTTON.GetComponent<UnityEngine.UI.Image>().color = base.colorScheme;
        colorScheme = SETTINGS_BUTTON.GetComponent<UnityEngine.UI.Image>().color;
        BACKGROUND = GameObject.Find("Background");
        backgroundGraphicString = settingPairs.build_hub_background_graphic;
        BACKGROUND.GetComponent<UnityEngine.UI.Image>().sprite = ImageTools.getSpriteFromString(backgroundGraphicString);
        PLATFORM = GameObject.Find("Platform");
        platformGraphicString = settingPairs.build_hub_platform_graphic;
        PLATFORM.GetComponent<Renderer>().material.mainTexture = new Image(platformGraphicString).getTexture();
        enablePreviewRobotAnimation = settingPairs.preview_robot_animation;
        updateTabs();
        if (this.mode == MODES.MY_ROBOTS)
        {
            MY_ROBOTS_BACK_BUTTON.SetActive(false);
            foreach (GameObject tab in TABS)
                tab.SetActive(false);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        previewRobot.getHead().destroyGameObject();
        previewRobot.getBody().destroyGameObject();
        previewRobot.getMobility().destroyGameObject();
        GameObject.Destroy(previewRobot.GAME_OBJECT);
        myRobots.destroyAllGeneratedObjects();
        workshop.destroyAllGeneratedObjects();
        store.destroyAllGeneratedObjects();
    }

    private void createPreviewRobot()
    {
        previewRobotParts = new List<Part>();
        for (int partIndex = 0; partIndex < currentRobot.getParts().Length; ++partIndex)
        {
            if (currentRobot.getParts()[partIndex].GAME_OBJECT != null)
                previewRobotParts.Add(currentRobot.getParts()[partIndex].clone(false));
            else
            {
                Part currentPart = currentRobot.getParts()[partIndex];
                Part previewRobotPart = null;
                if (currentPart is Head)
                    previewRobotPart = new Head(currentPart.getID(), currentPart.getImage(), currentPart.getWeight(), currentPart.getShape(), currentPart.getDurability(), currentPart.getRemainingDurability());
                else if (currentPart is Body)
                    previewRobotPart = new Body(currentPart.getID(), currentPart.getImage(), currentPart.getWeight(), currentPart.getShape(), currentPart.getDurability(), currentPart.getRemainingDurability(), ((Body)currentPart).getMaxAttachments());
                else if (currentPart is Mobility)
                    previewRobotPart = new Mobility(currentPart.getID(), currentPart.getImage(), currentPart.getWeight(), currentPart.getShape(), currentPart.getDurability(), currentPart.getRemainingDurability(), ((Mobility)currentPart).getClimbAngle(), ((Mobility)currentPart).getMaxSpeed(), ((Mobility)currentPart).getMaxForce());
                if (!(currentPart is Attachment))
                    previewRobotParts.Add(previewRobotPart);
            }
            if (partIndex < previewRobotParts.Count)
                previewRobotParts[partIndex].toggleGameObject(true);
        }
        previewRobot = new Robot("", false, true, previewRobotParts.ToArray());
        previewRobot.GAME_OBJECT.GetComponent<Rigidbody>().useGravity = false;
        previewRobot.setPosition(new Point(0, -previewRobot.getBody().GAME_OBJECT.transform.position.y, 500));
        previewRobot.GAME_OBJECT.transform.localScale = new Vector3(100, 100, 100);
        previewRobot.GAME_OBJECT.transform.localEulerAngles = new Vector3(0, 180, 0);
    }

    private void updateTabs()
    {
        if (!obstacleList.isEnabled() && !base.areSettingsOpen())
        {
            if (!OBSTACLES_INFO_BUTTON.activeInHierarchy && !SETTINGS_BUTTON.activeInHierarchy)
            {
                if (mode != MODES.MY_ROBOTS)
                {
                    MY_ROBOTS_BACK_BUTTON.SetActive(true);
                    foreach (GameObject tab in TABS)
                        tab.SetActive(true);
                }
                previewRobot.GAME_OBJECT.SetActive(true);
                OBSTACLES_INFO_BUTTON.SetActive(true);
                SETTINGS_BUTTON.SetActive(true);
                TRAINING_BUTTON.SetActive(true);
            }
            if (mode == MODES.MY_ROBOTS && (myRobots.getRobotBeingPicked() != null || myRobots.getNewRobot() != null || myRobots.getRobotsToRemove().Count > 0))
            {
                if (myRobots.getRobotsToRemove().Count > 0)
                {
                    List<Robot> robotsToRemove = myRobots.getRobotsToRemove();
                    foreach (Robot robot in robotsToRemove)
                        myRobotsList.Remove(robot);
                    if (myRobotsList.Count == 0)
                        myRobots = new MyRobots(myRobotsList.ToArray(), humanRobotParts.ToArray(), credits, base.colorScheme);
                    myRobots.clearPickedAndNewRobotAndRobotsToRemove();
                }
                else
                {
                    currentRobot = (myRobots.getRobotBeingPicked() != null ? myRobots.getRobotBeingPicked() : myRobots.getNewRobot());
                    if (myRobots.getNewRobot() != null)
                        myRobotsList.Add(currentRobot);
                    if (previewRobot == null)
                        createPreviewRobot();
                    myRobots.clearPickedAndNewRobotAndRobotsToRemove();
                    workshop.destroyAllGeneratedObjects();
                    store.destroyAllGeneratedObjects();
                    GameObject.Find("BuildHubCanvas").transform.Find("Workshop").gameObject.SetActive(true);
                    GameObject.Find("BuildHubCanvas").transform.Find("Store").gameObject.SetActive(true);
                    workshop = new Workshop(MESHES, previewRobot, currentRobot.getName(), this.humanRobotParts, currentRobot.getParts(), this.credits, base.colorScheme, enableCreditsSpentAnimation);
                    store = new Store(MESHES, previewRobot, this.STORE_PARTS, this.humanRobotParts, currentRobot.getParts(), this.credits, workshop.getConfigurationCard(), base.colorScheme, enableCreditsSpentAnimation);
                    mode = MODES.WORKSHOP;
                    MY_ROBOTS_BACK_BUTTON.SetActive(true);
                    foreach (GameObject tab in TABS)
                        tab.SetActive(true);
                }
            }
            if (MY_ROBOTS_BACK_BUTTON.GetComponent<ButtonListener>().isClicked())
            {
                MY_ROBOTS_BACK_BUTTON.GetComponent<ButtonListener>().resetClick();
                updateCurrentRobotInList();
                mode = MODES.MY_ROBOTS;
                MY_ROBOTS_BACK_BUTTON.SetActive(false);
                foreach (GameObject tab in TABS)
                    tab.SetActive(false);
                myRobots.destroyAllGeneratedObjects();
                GameObject.Find("BuildHubCanvas").transform.Find("MyRobots").gameObject.SetActive(true);
                myRobots = new MyRobots(myRobotsList.ToArray(), humanRobotParts.ToArray(), credits, base.colorScheme);
            }
            foreach (GameObject tab in TABS)
            {
                if (tab.GetComponent<UnityEngine.UI.Image>().color == INACTIVE_TAB_COLOR && (tab.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text.ToLower().Contains(mode.ToString().ToLower()) || tab.GetComponent<ButtonListener>().isClicked()))
                {
                    tab.GetComponent<UnityEngine.UI.Image>().color = ACTIVE_TAB_COLOR;
                    mode = (MODES)Enum.Parse(typeof(MODES), tab.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text.ToUpper(), true);
                    tab.GetComponent<ButtonListener>().resetClick();
                }
                else if (!tab.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>().text.ToLower().Contains(mode.ToString().ToLower()))
                    tab.GetComponent<UnityEngine.UI.Image>().color = INACTIVE_TAB_COLOR;
                tab.GetComponent<ButtonListener>().resetClick();
            }
            GameObject.Find("BuildHubCanvas").transform.Find("MyRobots").gameObject.SetActive(mode == MODES.MY_ROBOTS);
            GameObject.Find("BuildHubCanvas").transform.Find("Workshop").gameObject.SetActive(mode == MODES.WORKSHOP);
            GameObject.Find("BuildHubCanvas").transform.Find("Store").gameObject.SetActive(mode == MODES.STORE);
            if (OBSTACLES_INFO_BUTTON.GetComponent<ButtonListener>().isClicked())
            {
                OBSTACLES_INFO_BUTTON.GetComponent<ButtonListener>().resetClick();
                MY_ROBOTS_BACK_BUTTON.SetActive(false);
                foreach (GameObject tab in TABS)
                    tab.SetActive(false);
                previewRobot.GAME_OBJECT.SetActive(false);
                GameObject.Find("BuildHubCanvas").transform.Find("Workshop").gameObject.SetActive(false);
                GameObject.Find("BuildHubCanvas").transform.Find("Store").gameObject.SetActive(false);
                OBSTACLES_INFO_BUTTON.SetActive(false);
                SETTINGS_BUTTON.SetActive(false);
                TRAINING_BUTTON.SetActive(false);
                obstacleList.enable();
            }
            if (SETTINGS_BUTTON.GetComponent<ButtonListener>().isClicked())
            {
                SETTINGS_BUTTON.GetComponent<ButtonListener>().resetClick();
                MY_ROBOTS_BACK_BUTTON.SetActive(false);
                foreach (GameObject tab in TABS)
                    tab.SetActive(false);
                previewRobot.GAME_OBJECT.SetActive(false);
                GameObject.Find("BuildHubCanvas").transform.Find("Workshop").gameObject.SetActive(false);
                GameObject.Find("BuildHubCanvas").transform.Find("Store").gameObject.SetActive(false);
                OBSTACLES_INFO_BUTTON.SetActive(false);
                SETTINGS_BUTTON.SetActive(false);
                base.openSettings();
            }
            if (TRAINING_BUTTON.GetComponent<ButtonListener>().isClicked())
            {
                TRAINING_BUTTON.GetComponent<ButtonListener>().resetClick();
                workshop.setGoToField(true);
                goToField = true;
                trainingMode = true;
            }
            if (CREDITS_BUTTON.GetComponent<ButtonListener>().isClicked())
            {
                CREDITS_BUTTON.GetComponent<ButtonListener>().resetClick();
                CREDITS_WIDGET.SetActive(!CREDITS_WIDGET.activeInHierarchy);
            }
        }
    }

    private void updateCurrentRobotInList()
    {
        if (currentRobot != null)
        {
            workshop.checkConfigurationCardRobotName();
            for (int robotIndex = 0; robotIndex < myRobotsList.Count; ++robotIndex)
            {
                if (myRobotsList[robotIndex] == currentRobot)
                {
                    currentRobot = new Robot(workshop.getRobotName(), true, true, workshop.getRobotParts().ToArray());
                    myRobotsList[robotIndex] = currentRobot;
                    break;
                }
                if (robotIndex == myRobotsList.Count - 1)
                    currentRobot.setName(workshop.getRobotName());
            }
            if (currentRobot.getName() != workshop.getRobotName())
                currentRobot.setName(workshop.getRobotName());
        }
    }

    public override void show()
    {
        if (!base.isDisposed())
        {
            if (base.areSettingsOpen())
            {
                if (backgroundGraphicString != base.settingPairs.build_hub_background_graphic)
                {
                    backgroundGraphicString = base.settingPairs.build_hub_background_graphic;
                    BACKGROUND.GetComponent<UnityEngine.UI.Image>().sprite = ImageTools.getSpriteFromString(backgroundGraphicString);
                }
                if (platformGraphicString != base.settingPairs.build_hub_platform_graphic)
                {
                    platformGraphicString = base.settingPairs.build_hub_platform_graphic;
                    PLATFORM.GetComponent<Renderer>().material.mainTexture = new Image(platformGraphicString).getTexture();
                }
                if (colorScheme != base.colorScheme)
                {
                    colorScheme = ImageTools.getColorFromString(base.settingPairs.color_scheme);
                    SETTINGS_BUTTON.GetComponent<UnityEngine.UI.Image>().color = base.colorScheme;
                    TRAINING_BUTTON.GetComponent<UnityEngine.UI.Image>().color = base.colorScheme;
                }
                enableCreditsSpentAnimation = base.settingPairs.credits_spent_animation;
                enablePreviewRobotAnimation = base.settingPairs.preview_robot_animation;
            }
            if (obstacleList.isEnabled())
                obstacleList.update();
            MODES oldMode = mode;
            updateTabs();
            switch (mode)
            {
                case MODES.MY_ROBOTS:
                    myRobots.update(base.colorScheme);
                    break;
                case MODES.WORKSHOP:
                    workshop.updateSettings(base.colorScheme, enableCreditsSpentAnimation);
                    workshop.update();
                    break;
                case MODES.STORE:
                    if (oldMode != mode)
                        store.goToDefaultTab();
                    store.updateSettings(base.colorScheme, enableCreditsSpentAnimation);
                    store.update();
                    break;
                default:
                    break;
            }
            if (currentRobot != null)
            {
                if (mode == MODES.WORKSHOP && (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2) || Input.GetKey(KeyCode.Tab) || Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)))
                    updateCurrentRobotInList();
                Part[] robotParts = workshop.getRobotParts().ToArray();
                Part[] currentRobotParts = currentRobot.getParts();
                bool partsChanged = false;
                if (robotParts.Length != currentRobotParts.Length)
                    partsChanged = true;
                else
                {
                    for (int partIndex = 0; partIndex < robotParts.Length; ++partIndex)
                    {
                        if (robotParts[partIndex] != currentRobotParts[partIndex])
                        {
                            partsChanged = true;
                            break;
                        }
                    }
                }
                if (partsChanged)
                {
                    humanRobotParts = workshop.getHumanParts();
                    myRobots.updateHumanParts(humanRobotParts);
                    workshop.updateHumanParts(humanRobotParts);
                    store.updateHumanParts(humanRobotParts);
                    for (int robotIndex = 0; robotIndex < myRobotsList.Count; ++robotIndex)
                    {
                        if (currentRobot == myRobotsList[robotIndex])
                        {
                            currentRobot = workshop.getRobot();
                            myRobotsList[robotIndex] = currentRobot;
                            myRobots.updateMyRobots(myRobotsList);
                            List<Part> robotPartList = new List<Part>();
                            robotPartList.AddRange(robotParts);
                            workshop.updateRobotParts(robotPartList);
                            store.updateRobotParts(robotPartList);
                            break;
                        }
                    }
                }
                Part partBought = store.getPartBought();
                humanRobotParts = workshop.getHumanParts();
                if (partBought != null)
                {
                    humanRobotParts.Add(partBought);
                    workshop.updateHumanParts(humanRobotParts);
                    store.updateHumanParts(humanRobotParts);
                }
                credits = (workshop.getUpdatedCredits() < store.getUpdatedCredits()) ? workshop.getUpdatedCredits() : store.getUpdatedCredits();
                myRobots.updateCredits(credits);
                workshop.updateCredits(credits);
                store.updateCredits(credits);
            }
            if (mode == MODES.MY_ROBOTS)
            {
                Robot robotBeingPreviewed = myRobots.getRobotBeingPreviewed();
                if (robotBeingPreviewed != null)
                {
                    if (robotBeingPreviewed.getHead().getShape() == Shape.SHAPES.HEMISPHERE && !previewRobot.getHead().GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                        previewRobot.getHead().GAME_OBJECT.transform.localPosition = new Vector3(previewRobot.getHead().GAME_OBJECT.transform.localPosition.x, previewRobot.getHead().GAME_OBJECT.transform.localPosition.y - .5f, previewRobot.getHead().GAME_OBJECT.transform.localPosition.z);
                    else if (robotBeingPreviewed.getHead().getShape() != Shape.SHAPES.HEMISPHERE && previewRobot.getHead().GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                        previewRobot.getHead().GAME_OBJECT.transform.localPosition = new Vector3(previewRobot.getHead().GAME_OBJECT.transform.localPosition.x, previewRobot.getHead().GAME_OBJECT.transform.localPosition.y + .5f, previewRobot.getHead().GAME_OBJECT.transform.localPosition.z);
                    previewRobot.getHead().changeTextureAndShape(robotBeingPreviewed.getHead().getImage().getTexture(), MESHES[(int)robotBeingPreviewed.getHead().getShape()], robotBeingPreviewed.getHead().getShape());
                    if (robotBeingPreviewed.getBody().getShape() == Shape.SHAPES.HEMISPHERE && !previewRobot.getBody().GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                        previewRobot.getBody().GAME_OBJECT.transform.localPosition = new Vector3(previewRobot.getBody().GAME_OBJECT.transform.localPosition.x, previewRobot.getBody().GAME_OBJECT.transform.localPosition.y - .5f, previewRobot.getBody().GAME_OBJECT.transform.localPosition.z);
                    else if (robotBeingPreviewed.getBody().getShape() != Shape.SHAPES.HEMISPHERE && previewRobot.getBody().GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                        previewRobot.getBody().GAME_OBJECT.transform.localPosition = new Vector3(previewRobot.getBody().GAME_OBJECT.transform.localPosition.x, previewRobot.getBody().GAME_OBJECT.transform.localPosition.y + .5f, previewRobot.getBody().GAME_OBJECT.transform.localPosition.z);
                    previewRobot.getBody().changeTextureAndShape(robotBeingPreviewed.getBody().getImage().getTexture(), MESHES[(int)robotBeingPreviewed.getBody().getShape()], robotBeingPreviewed.getBody().getShape());
                    if (robotBeingPreviewed.getMobility().getShape() == Shape.SHAPES.HEMISPHERE && !previewRobot.getMobility().GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                        previewRobot.getMobility().GAME_OBJECT.transform.localPosition = new Vector3(previewRobot.getMobility().GAME_OBJECT.transform.localPosition.x, previewRobot.getMobility().GAME_OBJECT.transform.localPosition.y - .5f, previewRobot.getMobility().GAME_OBJECT.transform.localPosition.z);
                    else if (robotBeingPreviewed.getMobility().getShape() != Shape.SHAPES.HEMISPHERE && previewRobot.getMobility().GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                        previewRobot.getMobility().GAME_OBJECT.transform.localPosition = new Vector3(previewRobot.getMobility().GAME_OBJECT.transform.localPosition.x, previewRobot.getMobility().GAME_OBJECT.transform.localPosition.y + .5f, previewRobot.getMobility().GAME_OBJECT.transform.localPosition.z);
                    previewRobot.getMobility().changeTextureAndShape(robotBeingPreviewed.getMobility().getImage().getTexture(), MESHES[(int)robotBeingPreviewed.getMobility().getShape()], robotBeingPreviewed.getMobility().getShape());
                }
            }
            goToField = workshop.getGoToField();
            if (previewRobot != null)
                animatePreviewRobot();
            base.show();
        }
    }

    public override void onGUI()
    {

    }

    public MyRobots getMyRobots()
    {
        return myRobots;
    }

    public Workshop getWorkshop()
    {
        return workshop;
    }

    public Store getStore()
    {
        return store;
    }

    public long getUpdatedCredits()
    {
        return credits;
    }

    private void animatePreviewRobot()
    {
        if (enablePreviewRobotAnimation)
        {
            previewRobot.GAME_OBJECT.transform.Rotate(new Vector3(0, PREVIEW_ROTATE_DEGREES_PER_FRAME, 0));
            if (previewRobot.getHead().GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
            {
                Vector3 mousePosition = Input.mousePosition;
                float rotationX = (UnityEngine.Screen.height / 2 - mousePosition.y) * PREVIEW_ROBOT_VERTICAL_ROTATION_WEIGHT;
                rotationX = Mathf.Clamp(rotationX, MIN_VERTICAL, MAX_VERTICAL);
                previewRobot.getHead().GAME_OBJECT.transform.localEulerAngles = new Vector3(rotationX, 0, 0);
            }
            else previewRobot.getHead().GAME_OBJECT.transform.localEulerAngles = Vector3.zero;
        }
        else
        {
            previewRobot.GAME_OBJECT.transform.localEulerAngles = new Vector3(0, 180, 0);
            previewRobot.getHead().GAME_OBJECT.transform.localEulerAngles = Vector3.zero;
        }
    }

    public bool getGoToField()
    {
        return goToField;
    }

    public bool getTrainingMode()
    {
        return trainingMode;
    }

    public List<Part> getHumanRobotParts()
    {
        return humanRobotParts;
    }

    public List<Robot> getMyRobotsList()
    {
        return myRobotsList;
    }

    public Robot getCurrentRobot()
    {
        return currentRobot;
    }
}