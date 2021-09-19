using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TinyJson;

public class RobotGame : MonoBehaviour
{
    private string directory = "";
    private readonly string PARTS_DIRECTORY = "/parts", PLAYER_DATA_DIRECTORY = "/player", MODEL_DIRECTORY = "/models";
    private Screen currentScreen = null, screenChangingFrom = null;
    private long experience = 0, credits = 20000;
    private Robot humanRobot = null;
    private double currentHumanRobotHealth = 0;
    private List<Robot> myRobots = new List<Robot>(), currentMyRobots = new List<Robot>();
    private List<PartData> partsData = new List<PartData>();
    private PlayerData playerData = new PlayerData();
    private List<Part> humanRobotParts = new List<Part>(), parts = new List<Part>(), currentHumanRobotParts = new List<Part>();
    private List<ObstacleData> obstaclesData = new List<ObstacleData>();
    private List<string> partFiles = new List<string>();
    private double previousRoundDamageDifference = 0;
    private double previousRoundMaxDamageDifference = 0;
    private double previousRoundTimeElapsed = 0;
    private readonly string /*Q_TABLE_FILE_NAME = "qTable.json", */SPLASH_SCREEN_SCENE_NAME = "SplashScreen";
    //private List<KeyValuePair<string, List<double>>> qTable = null;
    public enum TESTS { HUMAN_V_VOID_ROBOT, AI_AGENT_TRAINING, FIELD, BUILD_HUB };
    private TESTS currentTest;
    private enum ROUND_OVER_DATA_TYPES { ROUND_DAMAGE_DIFFERENCE, ROUND_MAX_DAMAGE_DIFFERENCE, ROUND_TIME_ELAPSED };
    private readonly PerformanceMetricCalculator PERFORMANCE_METRIC_CALCULATOR = new PerformanceMetricCalculator();
    private static readonly int NUMBER_OF_AGENTS = 2;
    private AIManager aiManager = null;
    //private readonly QLearning[] ENGINEER_AGENTS = new QLearning[NUMBER_OF_AGENTS];
    //private readonly QLearning[] FIELD_AGENTS = new QLearning[NUMBER_OF_AGENTS];
    private readonly Robot[] FIELD_AGENT_ROBOTS = new Robot[NUMBER_OF_AGENTS];
    //private readonly double[] FIELD_AGENT_ROBOTS_PREVIOUS_DURABILITY = new double[NUMBER_OF_AGENTS], FIELD_AGENT_ROBOTS_PREVIOUS_DAMAGE_DEALT = new double[NUMBER_OF_AGENTS];
    private readonly double POSITION_THRESHOLD = Robot.SCALE + Field.WALL_THICKNESS;
    private readonly System.Random RANDOM = new System.Random();
    private CancellationTokenSource cancellationTokenSource;
    private AsyncOperation splashScreenLoader = default;
    private Type screenTypeToChangeTo = default;
    private Dimension fieldSize = new Dimension();
    private string currentSettings = "";
    private List<Setting> settingList = new List<Setting>();
    private List<string> currentSettingValueList = new List<string>();
    private SettingsManager settingsManager = new SettingsManager();
    private PartManager partManager = new PartManager();
    private PlayerDataManager playerDataManager = new PlayerDataManager();
    private BuildHubStateDataManager buildHubStateDataManager = new BuildHubStateDataManager();
    private FieldStateDataManager fieldStateDataManager = new FieldStateDataManager();
    private List<BuildHubStateData> buildHubStatesData = default;
    private List<FieldStateData> fieldStatesData = default;
    private FileManager fileManager = null;
    private bool playerDataHasUpdated = false, stateDataHasUpdated = false, running = false, gameOver = false, trainingMode = false;
    private Thread saveDataThread = null;

    public void startRobotGame()
    {
        //setScreen(new Field());
    }

    public void startRobotGame(TESTS test)
    {
        currentTest = test;
        switch (test)
        {
            case TESTS.FIELD:
                prepareScene(typeof(Field));
                break;
            case TESTS.BUILD_HUB:
                prepareScene(typeof(BuildHub));
                break;
        }
    }

    private void prepareScene(Type type)
    {
        string sceneName = type.Name;
        if (cancellationTokenSource == null)
        {
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                loadScene(cancellationTokenSource.Token, sceneName);
            }
            catch
            {}
            finally
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }
        }
        else
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = null;
        }
    }

    private void loadScene(CancellationToken token, string sceneName)
    {
        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);
            if (sceneName != scene.name)
            {
                foreach (GameObject gameObject in scene.GetRootGameObjects())
                {
                    RobotGame robotGame = gameObject.GetComponent<RobotGame>();
                    if (robotGame != null)
                    {
                        experience = robotGame.getExperience();
                        credits = robotGame.getCredits();
                        humanRobot = robotGame.getHumanRobot();
                        humanRobotParts = robotGame.getHumanRobotParts();
                        obstaclesData = robotGame.getObstaclesData();
                        currentSettings = robotGame.getCurrentSettings();
                        previousRoundDamageDifference = robotGame.getPreviousRoundDamageDifference();
                        previousRoundMaxDamageDifference = robotGame.getPreviousRoundMaxDamageDifference();
                        previousRoundTimeElapsed = robotGame.getPreviousRoundTimeElapsed();
                        buildHubStatesData = robotGame.getBuildHubStatesData();
                        fieldStatesData = robotGame.getFieldStatesData();
                        trainingMode = robotGame.getTrainingMode();
                        if (trainingMode)
                            currentTest = TESTS.AI_AGENT_TRAINING;
                        foreach (Part part in humanRobotParts)
                            currentHumanRobotParts.Add(part.clone(true));
                        foreach (Robot robot in myRobots)
                            currentMyRobots.Add(robot.clone(true));
                        foreach (Setting setting in settingList)
                            currentSettingValueList.Add(setting.currentValue);
                    }
                    SplashScreen splashScreen = gameObject.GetComponent<SplashScreen>();
                    if (splashScreen != null)
                    {
                        List<GameObject> gameObjectsInOldScene = new List<GameObject>(scene.GetRootGameObjects());
                        GameObject splashScreenCanvas = gameObjectsInOldScene.Find(g => g.name == "SplashScreenCanvas");
                        SceneManager.MoveGameObjectToScene(splashScreenCanvas, SceneManager.GetActiveScene());
                        GameObject loadingSplashScreen = splashScreenCanvas.transform.Find("LoadingSplashScreen").gameObject;
                        List<GameObject> gameObjectsInCurrentScene = new List<GameObject>(FindObjectsOfType<GameObject>());
                        GameObject currentSplashScreenCanvas = gameObjectsInCurrentScene.Find(g => g.name.Contains("SplashScreenCanvas"));
                        loadingSplashScreen.transform.SetParent(currentSplashScreenCanvas.transform);
                        splashScreen.transform.SetParent(GameObject.Find("Directional Light").transform);
                    }
                }
                SceneManager.UnloadSceneAsync(scene);
            }
        }
        splashScreenLoader = SceneManager.LoadSceneAsync(SPLASH_SCREEN_SCENE_NAME, LoadSceneMode.Additive);
        splashScreenLoader.allowSceneActivation = false;
        initializeScene(sceneName);
    }

    private void switchScene(Type type)
    {
        screenTypeToChangeTo = type;
        splashScreenLoader.allowSceneActivation = true;
    }

    private async void initializeScene(string sceneName) //test stuff inside! will need to be rewritten for actual game
    {
        running = true;
        saveDataThread = new Thread(saveData);
        saveDataThread.Start();
        if (currentSettings == "")
            currentSettings = SettingsManager.DEFAULT_SETTINGS;
        SettingPairs settingPairs = settingsManager.getSettingPairs(currentSettings);
        settingList = settingsManager.getSettingList(currentSettings);
        fieldSize = Field.getFieldSize(settingPairs.field_size);
        foreach (PartData partData in partsData)
        {
            Part part = partManager.partDataToPart(partData);
            if (part.GAME_OBJECT != null)
                part.toggleGameObject(false);
            parts.Add(part);
        }
        if (currentTest == TESTS.BUILD_HUB)
        {
            IconGenerator iconGenerator = new IconGenerator();
            Camera camera = null;
            foreach (Camera currCam in Camera.allCameras)
            {
                if (currCam.name == "IconCamera")
                {
                    camera = currCam;
                    camera.forceIntoRenderTexture = true;
                    break;
                }
            }
            iconGenerator.camera = camera;
            Camera.main.targetDisplay = 1;
            foreach (Part part in parts)
            {
                if (part.GAME_OBJECT == null)
                    continue;
                part.toggleGameObject(true);
                part.GAME_OBJECT.transform.Rotate(new Vector3(9, 120, -15));
                iconGenerator.gameObjectOfIcon = part.GAME_OBJECT;
                while (part.getIcon() == null)
                {
                    if (part.GAME_OBJECT.GetComponent<Renderer>().isVisible)
                    {
                        iconGenerator.initialize();
                        if (iconGenerator.getIcon() != null)
                            part.setIcon(iconGenerator.getIcon());
                    }
                    if (part.getIcon() == null)
                        await Task.Delay(25);
                }
                part.toggleGameObject(false);
                part.destroyGameObject();
            }
            Camera.main.targetDisplay = 0;
        }
        else
        {
            foreach (Part part in parts)
                if (part.GAME_OBJECT != null)
                    part.destroyGameObject();
        }
        experience = playerData.experience;
        credits = playerData.credits;
        previousRoundDamageDifference = playerData.previousRoundDamageDifference;
        previousRoundMaxDamageDifference = playerData.previousRoundMaxDamageDifference;
        previousRoundTimeElapsed = playerData.previousRoundTimeElapsed;
        ObstacleGenerator obstacleGenerator = new ObstacleGenerator(experience, settingPairs.max_obstacles, fieldSize);
        humanRobotParts = new List<Part>();
        obstaclesData = new List<ObstacleData>();
        List<int> badIndices = new List<int>();
        PerformanceMetricCalculator performanceMetricCalculator = new PerformanceMetricCalculator();
        Head cheapestHead = null;
        Body cheapestBody = null;
        Mobility cheapestMobility = null;
        Blaster cheapestBlaster = null;
        foreach (Part part in parts)
        {
            if (part is Head && (cheapestHead == null || performanceMetricCalculator.calculateCost(part) < performanceMetricCalculator.calculateCost(cheapestHead)))
                cheapestHead = (Head)part;
            else if (part is Body && (cheapestBody == null || performanceMetricCalculator.calculateCost(part) < performanceMetricCalculator.calculateCost(cheapestBody)))
                cheapestBody = (Body)part;
            else if (part is Mobility && (cheapestMobility == null || performanceMetricCalculator.calculateCost(part) < performanceMetricCalculator.calculateCost(cheapestMobility)))
                cheapestMobility = (Mobility)part;
            else if (part is Blaster && (cheapestBlaster == null || performanceMetricCalculator.calculateCost(part) < performanceMetricCalculator.calculateCost(cheapestBlaster)))
                cheapestBlaster = (Blaster)part;
        }
        if (playerData.humanRobotParts != default)
        {
            for (int partIndex = 0; partIndex < playerData.humanRobotParts.Length; ++partIndex)
            {
                PlayerPartData playerPartData = playerData.humanRobotParts[partIndex];
                if (parts.Exists(p => p.getID() == playerPartData.id))
                {
                    Part part = parts.Find(p => p.getID() == playerPartData.id);
                    part.damage(part.getDurability() - playerPartData.remainingDurability);
                    if (part is Head)
                        humanRobotParts.Add((Head)part.clone(true));
                    else if (part is Body)
                        humanRobotParts.Add((Body)part.clone(true));
                    else if (part is Mobility)
                        humanRobotParts.Add((Mobility)part.clone(true));
                    else if (part is Attachment)
                        humanRobotParts.Add((Attachment)part.clone(true));
                }
                else
                {
                    humanRobotParts.Add(null);
                    badIndices.Add(partIndex);
                }
            }
        }
        else humanRobotParts.AddRange(new Part[] { cheapestHead, cheapestBody, cheapestMobility, cheapestBlaster });
        myRobots = new List<Robot>();
        if (playerData.myRobots != default)
        {
            foreach (RobotData robotData in playerData.myRobots)
            {
                List<Part> robotPartList = new List<Part>();
                foreach (int partIndex in robotData.partIndices)
                    if (!badIndices.Contains(partIndex))
                        robotPartList.Add(humanRobotParts[partIndex]);
                try
                {
                    Robot robot = new Robot(robotData.name, true, robotData.human, robotPartList.ToArray());
                    myRobots.Add(robot);
                }
                catch {}
            }
        }
        foreach (int index in badIndices)
            humanRobotParts.RemoveAt(index);
        if (!humanRobotParts.Exists(part => part is Head))
            humanRobotParts.Add(cheapestHead);
        if (!humanRobotParts.Exists(part => part is Body))
            humanRobotParts.Add(cheapestBody);
        if (!humanRobotParts.Exists(part => part is Mobility))
            humanRobotParts.Add(cheapestMobility);
        if (!humanRobotParts.Exists(part => part is Attachment && ((Attachment)part).isWeapon()))
            humanRobotParts.Add(cheapestBlaster);
        if (humanRobotParts.Contains(null))
            throw new Exception("There are missing part files. There neeeds to be at least one part file each of types Head, Body, Mobility, and a weapon Attachment.");
        if (playerData.obstacles != default)
            obstaclesData.AddRange(playerData.obstacles);
        else obstaclesData.AddRange(obstacleGenerator.getObstaclesData());
        if (currentSettingValueList.Count == 0)
            foreach (Setting setting in settingList)
                currentSettingValueList.Add(setting.currentValue);
        Head head1 = null;
        Head head2 = null;
        Body body1 = null;
        Body body2 = null;
        Mobility mobility = null;
        Attachment attachment1 = null;
        Attachment attachment2 = null;
        Attachment attachment3 = null;
        foreach (Part part in parts)
        {
            if (part is Head)
            {
                if (head1 == null)
                {
                    head1 = (Head)part.clone(true);
                    head1.damage(2);
                }
                else if (head2 == null)
                {
                    head2 = (Head)part.clone(true);
                    head2.damage(3);
                }
            }
            else if (part is Body)
            {
                if (body1 == null)
                {
                    body1 = (Body)part.clone(true);
                    body1.damage(3);
                }
                else if (body2 == null)
                {
                    body2 = (Body)part.clone(true);
                    body2.damage(4);
                }
            }
            else if (part is Mobility)
            {
                if (mobility == null)
                {
                    mobility = (Mobility)part.clone(true);
                    mobility.damage(1);
                }
            }
            else if (part is Attachment)
            {
                if (attachment1 == null)
                {
                    attachment1 = (Attachment)part.clone(true);
                    attachment1.damage(1);
                }
                else if (attachment2 == null)
                {
                    attachment2 = (Attachment)part.clone(true);
                    attachment2.damage(1);
                }
                else if (attachment3 == null)
                {
                    attachment3 = (Attachment)part.clone(true);
                    attachment3.damage(3);
                }
            }
        }
        List<Robot> robots = new List<Robot>();
        int numberOfNonHumanRobots = 0;
        int numberOfHumanRobots = 0;
        Obstacle[] obstacles = null;
        List<BuildHubState> buildHubStates = default;
        if (buildHubStatesData != default && buildHubStatesData.Count > 0)
        {
            buildHubStates = new List<BuildHubState>();
            foreach (BuildHubStateData buildHubStateData in buildHubStatesData)
                buildHubStates.Add(buildHubStateDataManager.stateDataToState(buildHubStateData));
        }
        List<FieldState> fieldStates = default;
        if (fieldStatesData != default && fieldStatesData.Count > 0)
        {
            fieldStates = new List<FieldState>();
            foreach (FieldStateData fieldStateData in fieldStatesData)
                fieldStates.Add(fieldStateDataManager.stateDataToState(fieldStateData, parts));
        }
        BuildHub.MODES mode;
        switch (currentTest)
        {
            case TESTS.FIELD:
            case TESTS.HUMAN_V_VOID_ROBOT:
                List<Part> humanParts = new List<Part>();
                foreach (Part part in humanRobot.getParts())
                {
                    Part clonePart = part.clone(false);
                    if (clonePart is Head)
                        clonePart = (Head)clonePart;
                    else if (clonePart is Body)
                        clonePart = (Body)clonePart;
                    else if (clonePart is Mobility)
                        clonePart = (Mobility)clonePart;
                    else if (clonePart is Attachment)
                        clonePart = (Attachment)clonePart;
                    humanParts.Add(clonePart);
                }
                humanRobot = new Robot(humanRobot.getName(), humanRobot.isHuman(), humanParts.ToArray());
                currentHumanRobotHealth = humanRobot.getRemainingDurability();
                Attachment[] nonHumanAttachments = { (Attachment)attachment1.clone(true) };
                numberOfNonHumanRobots = RANDOM.Next(1, settingPairs.max_ai_robots + 1);
                numberOfHumanRobots = 1;
                obstacles = obstacleGenerator.obstaclesDataToObstacles(obstaclesData.ToArray());
                aiManager = new AIManager(numberOfNonHumanRobots, numberOfNonHumanRobots - 1 + numberOfHumanRobots, new Robot[] { humanRobot }, parts.ToArray(), new Part[] { cheapestHead, cheapestBody, cheapestMobility, cheapestBlaster }, obstacles, fieldSize, buildHubStates, fieldStates, experience);
                robots.Add(humanRobot);
                robots.AddRange(aiManager.getAgentRobots());
                setScreen(new Field(settingList, robots.ToArray(), obstacles, previousRoundDamageDifference, previousRoundMaxDamageDifference, previousRoundTimeElapsed));
                break;
            case TESTS.AI_AGENT_TRAINING:
                numberOfNonHumanRobots = NUMBER_OF_AGENTS;
                numberOfHumanRobots = 0;
                obstacles = obstacleGenerator.obstaclesDataToObstacles(obstaclesData.ToArray());
                aiManager = new AIManager(numberOfNonHumanRobots, numberOfNonHumanRobots - 1 + numberOfHumanRobots, null, parts.ToArray(), new Part[] { cheapestHead, cheapestBody, cheapestMobility, cheapestBlaster }, obstacles, fieldSize, buildHubStates, fieldStates, experience);
                Robot[] agentRobots = aiManager.getAgentRobots();
                for (int agentIndex = 0; agentIndex < NUMBER_OF_AGENTS; ++agentIndex)
                    FIELD_AGENT_ROBOTS[agentIndex] = agentRobots[agentIndex];
                robots.AddRange(FIELD_AGENT_ROBOTS);
                setScreen(new Field(settingList, robots.ToArray(), obstacles, previousRoundDamageDifference, previousRoundMaxDamageDifference, previousRoundTimeElapsed));
                break;
            case TESTS.BUILD_HUB:
                mode = BuildHub.MODES.MY_ROBOTS;
                setScreen(new BuildHub(settingList, obstaclesData, myRobots, humanRobotParts, parts.ToArray(), credits, mode));
                break;
        }
    }

    public void setScreen(Screen screen)
    {
        currentScreen = screen;
        currentScreen.show();
        settingList = currentScreen.getSettings();
    }

    void Start()
    {
        directory = Directory.GetCurrentDirectory();
        fileManager = new FileManager(directory);
        string[] files = Directory.GetFiles(directory);
        string settingsFile = "";
        foreach (string file in files)
        {
            if (file.Contains(FileManager.FILE_SUFFIX))
            {
                if (file.Contains("settings"))
                {
                    settingsFile = file;
                    break;
                }
            }
        }
        currentSettings = fileManager.readFromFile(settingsFile);
        if (currentSettings.Length <= 1)
            currentSettings = SettingsManager.DEFAULT_SETTINGS;
        if (!Directory.Exists(directory + PARTS_DIRECTORY))
            Directory.CreateDirectory(directory + PARTS_DIRECTORY);
        files = Directory.GetFiles(directory + PARTS_DIRECTORY);
        partFiles = new List<string>();
        foreach (string file in files)
            if (file.Contains(FileManager.FILE_SUFFIX))
                partFiles.Add(file);
        partsData = new List<PartData>();
        foreach (string partFile in partFiles)
        {
            string partDataString = fileManager.readFromFile(partFile);
            if (partDataString.Length > 1)
                partsData.Add(partManager.getPartDataFromJSON(partDataString));
        }
        if (!Directory.Exists(directory + PLAYER_DATA_DIRECTORY))
            Directory.CreateDirectory(directory + PLAYER_DATA_DIRECTORY);
        files = Directory.GetFiles(directory + PLAYER_DATA_DIRECTORY);
        string playerDataFile = "";
        foreach (string file in files)
        {
            if (file.Contains(FileManager.FILE_SUFFIX))
            {
                playerDataFile = file;
                break;
            }
        }
        playerData = new PlayerData();
        string playerDataString = fileManager.readFromFile(playerDataFile);
        if (playerDataString.Length > 1)
            playerData = playerDataManager.getPlayerDataFromJSON(playerDataString);
        if (!Directory.Exists(directory + MODEL_DIRECTORY))
            Directory.CreateDirectory(directory + MODEL_DIRECTORY);
        files = Directory.GetFiles(directory + MODEL_DIRECTORY);
        List<string> modelFiles = new List<string>();
        foreach (string file in files)
            if (file.Contains(FileManager.FILE_SUFFIX))
                modelFiles.Add(file);
        buildHubStatesData = new List<BuildHubStateData>();
        fieldStatesData = new List<FieldStateData>();
        foreach (string modelFile in modelFiles)
        {
            string stateDataString = fileManager.readFromFile(modelFile);
            if (stateDataString.Length > 1)
            {
                if (modelFile.Contains("build_hub"))
                    buildHubStatesData = buildHubStateDataManager.getStateDataFromJSON(stateDataString);
                else if (modelFile.Contains("field"))
                    fieldStatesData = fieldStateDataManager.getStateDataFromJSON(stateDataString);
            }
        }
        string sceneName = SceneManager.GetActiveScene().name;
        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);
            if (scene != SceneManager.GetActiveScene())
                sceneName = scene.name;
        }
        for (int sceneNameCharacterIndex = sceneName.Length - 1; sceneNameCharacterIndex >= 1; --sceneNameCharacterIndex)
            if (Char.IsUpper(sceneName[sceneNameCharacterIndex]))
                sceneName = sceneName.Insert(sceneNameCharacterIndex, "_");
        startRobotGame((TESTS)Enum.Parse(typeof(TESTS), sceneName.ToUpper(), true));
    }

    void FixedUpdate()
    {
        if (currentScreen != null) //test stuff inside! will need to be rewritten for actual game
        {
            currentScreen.show();
            if (currentScreen is Field)
            {
                bool goToBuildHub = false;
                GameEngine gameEngine = null;
                Robot humanRobotOnField = null;
                if (!gameOver)
                {
                    gameEngine = ((Field)currentScreen).getGameEngine();
                    humanRobotOnField = ((Field)currentScreen).getHumanRobot();
                }
                if (humanRobotOnField != null && humanRobotOnField.getRemainingDurability() != currentHumanRobotHealth)
                {
                    humanRobot = humanRobotOnField;
                    currentHumanRobotHealth = humanRobotOnField.getRemainingDurability();
                    foreach (Part part in humanRobotOnField.getParts())
                    {
                        int partIndex = humanRobotParts.FindIndex(p => p.getID() == part.getID());
                        humanRobotParts[partIndex].damage(humanRobotParts[partIndex].getRemainingDurability() - part.getRemainingDurability());
                    }
                    updatePlayerData(credits, humanRobotParts, myRobots, obstaclesData);
                }
                if (gameEngine != null && gameEngine.isRoundOver())
                {
                    gameOver = true;
                    goToBuildHub = ((Field)currentScreen).getGoToBuildHub();
                    double[] roundOverData = gameEngine.getRoundOverData();
                    double roundDamageDifference = roundOverData[(int)ROUND_OVER_DATA_TYPES.ROUND_DAMAGE_DIFFERENCE];
                    double roundMaxDamageDifference = roundOverData[(int)ROUND_OVER_DATA_TYPES.ROUND_MAX_DAMAGE_DIFFERENCE];
                    double roundTimeElapsed = roundOverData[(int)ROUND_OVER_DATA_TYPES.ROUND_TIME_ELAPSED];
                    long performanceMetric = PERFORMANCE_METRIC_CALCULATOR.calculatePerformanceMetric(roundDamageDifference, roundMaxDamageDifference, roundTimeElapsed, previousRoundDamageDifference, previousRoundMaxDamageDifference, previousRoundTimeElapsed);
                    if (performanceMetric > 0)
                    {
                        experience += performanceMetric;
                        credits += performanceMetric;
                    }
                    ObstacleGenerator obstacleGenerator = new ObstacleGenerator(experience, settingsManager.getSettingPairs(currentSettings).max_obstacles, fieldSize);
                    obstaclesData.Clear();
                    obstaclesData.AddRange(obstacleGenerator.getObstaclesData());
                    updatePlayerData(experience, credits, humanRobotParts, myRobots, obstaclesData, roundDamageDifference, roundMaxDamageDifference, roundTimeElapsed);
                    Robot bestAIAgentBot = gameEngine.getBestNonHumanRobot();
                    if (bestAIAgentBot != null && !bestAIAgentBot.isHuman())
                    {
                        List<AIAgent> agents = new List<AIAgent>();
                        agents.AddRange(aiManager.getAgents());
                        aiManager.applyBuildHubRewards();
                        AIAgent bestAIAgent = agents.Find(agent => agent.getBot().getName() == bestAIAgentBot.getName());
                        List<BuildHubState> buildHubStates = bestAIAgent.getBuildHubStates();
                        List<FieldState> fieldStates = bestAIAgent.getFieldStates();
                        List<BuildHubStateData> buildHubStateDataList = new List<BuildHubStateData>();
                        List<FieldStateData> fieldStateDataList = new List<FieldStateData>();
                        foreach (BuildHubState buildHubState in buildHubStates)
                            buildHubStateDataList.Add(buildHubStateDataManager.stateToStateData(buildHubState));
                        buildHubStatesData = buildHubStateDataList;
                        foreach (FieldState fieldState in fieldStates)
                            fieldStateDataList.Add(fieldStateDataManager.stateToStateData(fieldState));
                        fieldStatesData = fieldStateDataList;
                        stateDataHasUpdated = true;
                    }
                    gameEngine.destroyAllGeneratedObjects();
                    gameEngine = null;
                }
                else if (gameEngine != null && gameEngine.isGamePlay())
                {
                    aiManager.update();
                    FieldAction[] agentActions = aiManager.getFieldActionsToPerform();
                    AIAgent[] aiAgents = aiManager.getAgents();
                    for (int agentIndex = 0; agentIndex < agentActions.Length; ++agentIndex)
                        gameEngine.triggerAction(aiAgents[agentIndex].getBot(), agentActions[agentIndex]);
                }
                if (currentTest == TESTS.AI_AGENT_TRAINING && gameEngine == null)
                {
                    screenChangingFrom = currentScreen;
                    currentScreen = null;
                    switchScene(typeof(Field));
                    //enable below when done with single-round testing of AI Agent Training
                    //currentScreen = null;
                    //switchScene(typeof(Field));
                }
                else if (goToBuildHub || (currentScreen != null && gameEngine != null && ((Field)currentScreen).getGoToBuildHub()))
                {
                    trainingMode = false;
                    screenChangingFrom = currentScreen;
                    currentScreen = null;
                    switchScene(typeof(BuildHub));
                }
            }
            else if (currentScreen is BuildHub)
            {
                humanRobot = ((BuildHub)currentScreen).getCurrentRobot();
                updatePlayerData(((BuildHub)currentScreen).getUpdatedCredits(), ((BuildHub)currentScreen).getHumanRobotParts(), ((BuildHub)currentScreen).getMyRobotsList(), obstaclesData);
                if (((BuildHub)currentScreen).getGoToField())
                {
                    trainingMode = ((BuildHub)currentScreen).getTrainingMode();
                    screenChangingFrom = currentScreen;
                    currentScreen = null;
                    switchScene(typeof(Field));
                }
            }
            if (currentScreen != null && currentScreen.areSettingsOpen())
                updateSettings(currentScreen.getSettings());
        }
    }

    private void updatePlayerData(long experience, long credits, List<Part> humanRobotParts, List<Robot> myRobots, List<ObstacleData> obstaclesData, double roundDamageDifference, double roundMaxDamageDifference, double roundTimeElapsed)
    {
        this.experience = experience;
        this.credits = credits;
        this.humanRobotParts = humanRobotParts;
        this.myRobots = myRobots;
        this.obstaclesData = obstaclesData;
        previousRoundDamageDifference = roundDamageDifference;
        previousRoundMaxDamageDifference = roundMaxDamageDifference;
        previousRoundTimeElapsed = roundTimeElapsed;
        playerDataHasUpdated = true;
    }

    private void updatePlayerData(long credits, List<Part> humanRobotParts, List<Robot> myRobots, List<ObstacleData> obstaclesData)
    {
        bool creditsOrPartsOrRobotsChanged = false;
        if (this.credits != credits || currentHumanRobotParts.Count != humanRobotParts.Count || currentMyRobots.Count != myRobots.Count)
            creditsOrPartsOrRobotsChanged = true;
        else
        {
            for (int partIndex = 0; partIndex < this.humanRobotParts.Count; ++partIndex)
            {
                if (this.humanRobotParts[partIndex] != humanRobotParts[partIndex] || !humanRobotParts[partIndex].equals(currentHumanRobotParts[partIndex]))
                {
                    creditsOrPartsOrRobotsChanged = true;
                    break;
                }
            }
            if (!creditsOrPartsOrRobotsChanged)
            {
                for (int robotIndex = 0; robotIndex < this.myRobots.Count; ++robotIndex)
                {
                    if (this.myRobots[robotIndex] != myRobots[robotIndex] || !myRobots[robotIndex].equals(currentMyRobots[robotIndex]))
                    {
                        creditsOrPartsOrRobotsChanged = true;
                        break;
                    }
                }
            }
        }
        if (creditsOrPartsOrRobotsChanged)
            updatePlayerData(experience, credits, humanRobotParts, myRobots, obstaclesData, previousRoundDamageDifference, previousRoundMaxDamageDifference, previousRoundTimeElapsed);
    }

    private void saveData()
    {
        while (running)
        {
            if (playerDataHasUpdated)
            {
                playerDataHasUpdated = false;
                currentHumanRobotParts.Clear();
                foreach (Part part in this.humanRobotParts)
                    currentHumanRobotParts.Add(part.clone(true));
                currentMyRobots.Clear();
                foreach (Robot robot in this.myRobots)
                    currentMyRobots.Add(robot.clone(true));
                playerData = playerDataManager.dataToPlayerData(this.experience, this.credits, previousRoundDamageDifference, previousRoundMaxDamageDifference, previousRoundTimeElapsed, this.humanRobotParts, this.myRobots, this.obstaclesData);
                string data = playerData.ToJson();
                List<string> playerDataFiles = new List<string>();
                string[] files = Directory.GetFiles(directory + PLAYER_DATA_DIRECTORY);
                foreach (string file in files)
                    if (file.Contains(FileManager.FILE_SUFFIX))
                        playerDataFiles.Add(file);
                fileManager.writeToFile(playerData, data, directory + PLAYER_DATA_DIRECTORY);
                foreach (string file in playerDataFiles)
                    File.Delete(file);
            }
            if (stateDataHasUpdated)
            {
                stateDataHasUpdated = false;
                fileManager.writeToFile(buildHubStatesData, buildHubStatesData.ToJson(), directory + MODEL_DIRECTORY);
                fileManager.writeToFile(fieldStatesData, fieldStatesData.ToJson(), directory + MODEL_DIRECTORY);
            }
        }
    }

    private void updateSettings(List<Setting> settingList)
    {
        bool settingsChanged = false;
        if (currentSettingValueList.Count != settingList.Count)
            settingsChanged = true;
        else
        {
            for (int settingIndex = 0; settingIndex < settingList.Count; ++settingIndex)
            {
                if (currentSettingValueList[settingIndex] != settingList[settingIndex].currentValue)
                {
                    settingsChanged = true;
                    break;
                }
            }
        }
        if (settingsChanged)
        {
            this.settingList = settingList;
            currentSettingValueList.Clear();
            foreach (Setting setting in this.settingList)
                currentSettingValueList.Add(setting.currentValue);
            string settingsString = settingsManager.getSettingsJSON(settingList);
            if (currentSettings != settingsString)
            {
                currentSettings = settingsString;
                fileManager.writeToFile(settingList, currentSettings);
            }
        }
    }

    void OnGUI()
    {
        if (currentScreen != null)
            currentScreen.onGUI();
    }

    private void OnDestroy()
    {
        if (saveDataThread != null)
        saveDataThread.Abort();
        running = false;
    }

    private void OnApplicationQuit()
    {
        if (saveDataThread != null)
            saveDataThread.Abort();
        running = false;
    }

    public long getExperience()
    {
        return experience;
    }

    public long getCredits()
    {
        return credits;
    }

    public Robot getHumanRobot()
    {
        return humanRobot;
    }

    public List<Part> getHumanRobotParts()
    {
        return humanRobotParts;
    }

    public List<ObstacleData> getObstaclesData()
    {
        return obstaclesData;
    }

    public string getCurrentSettings()
    {
        return currentSettings;
    }

    public double getPreviousRoundDamageDifference()
    {
        return previousRoundDamageDifference;
    }

    public double getPreviousRoundMaxDamageDifference()
    {
        return previousRoundMaxDamageDifference;
    }

    public double getPreviousRoundTimeElapsed()
    {
        return previousRoundTimeElapsed;
    }

    public List<BuildHubStateData> getBuildHubStatesData()
    {
        return buildHubStatesData;
    }

    public List<FieldStateData> getFieldStatesData()
    {
        return fieldStatesData;
    }

    public Screen getScreen()
    {
        return screenChangingFrom;
    }

    public Type getScreenTypeToChangeTo()
    {
        return screenTypeToChangeTo;
    }

    public bool getTrainingMode()
    {
        return trainingMode;
    }
}