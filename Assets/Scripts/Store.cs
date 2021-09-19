using System.Collections.Generic;
using UnityEngine;

public class Store
{
    private readonly List<Part> STORE_PARTS;
    private List<Part> humanParts, robotParts;
    private long credits;
    private Robot previewRobot, robot;
    private readonly StoreCard STORE_CARD;
    private readonly ConfigurationCard CONFIGURATION_CARD;
    private readonly GameObject MASK, STORE_CREDITS_SPENT;
    private readonly Vector3 STORE_CREDITS_SPENT_HOME_POSITION;
    private readonly string STAT_BETTER_COLOR_PREFIX = "<color=#07D20C>", STAT_WORSE_COLOR_PREFIX = "<color=#FFA300>", FORMAT_COLOR_SUFFIX = "</color>";
    private enum PART_INDICES { HEAD, BODY, MOBILITY };
    private readonly int FIRST_STAT_INDEX = 2;
    private StoreCard.MODES mode;
    private readonly PerformanceMetricCalculator PERFORMANCE_METRIC_CALCULATOR;
    private readonly List<Mesh> MESHES;
    private Part partBought, partWithPreviewedTexture;
    private Color colorScheme;
    private bool enableCreditsSpentAnimation;

    public Store(List<Mesh> meshes, Robot previewRobot, Part[] storeParts, List<Part> humanParts, Part[] robotParts, long credits, ConfigurationCard configurationCard, Color colorScheme, bool enableCreditsSpentAnimation)
    {
        MESHES = meshes;
        this.previewRobot = previewRobot;
        this.humanParts = humanParts;
        this.credits = credits;
        this.colorScheme = colorScheme;
        this.enableCreditsSpentAnimation = enableCreditsSpentAnimation;
        GameObject storePanel = GameObject.Find("StorePanel");
        foreach (Transform child in storePanel.transform)
            GameObject.Destroy(child.gameObject);
        MASK = GameObject.Find("Store").transform.Find("StoreMask").gameObject;
        MASK.SetActive(false);
        this.robotParts = new List<Part>();
        if (robotParts != null && robotParts.Length > 0)
        {
            this.robotParts.AddRange(robotParts);
            robot = new Robot("", true, true, this.robotParts.ToArray());
            STORE_PARTS = new List<Part>();
            STORE_PARTS.AddRange(storeParts);
            STORE_CARD = new StoreCard(this.credits, STORE_PARTS.ToArray(), this.humanParts.ToArray(), this.robotParts.ToArray(), colorScheme, enableCreditsSpentAnimation);
            STORE_CARD.enable();
            CONFIGURATION_CARD = configurationCard;
        }
        mode = StoreCard.MODES.VIEW_PART_STATS;
        PERFORMANCE_METRIC_CALCULATOR = new PerformanceMetricCalculator();
        partBought = null;
        partWithPreviewedTexture = null;
    }

    private void checkStoreCardMode()
    {
        Part partBeingPreviewed = null, partBeingBought = null;
        Part[] previewRobotParts = previewRobot.getParts();
        string[] robotStats = null;
        if (STORE_CARD.getMode() != mode)
        {
            mode = STORE_CARD.getMode();
            switch (mode)
            {
                case StoreCard.MODES.VIEW_PART_STATS:
                    MASK.SetActive(false);
                    previewRobot.GAME_OBJECT.transform.localPosition = new Vector3(previewRobot.GAME_OBJECT.transform.localPosition.x, previewRobot.GAME_OBJECT.transform.localPosition.y, 500);
                    previewRobot.GAME_OBJECT.transform.localScale = new Vector3(100, 100, 100);
                    CONFIGURATION_CARD.setForStoreCompare(false);
                    if (partWithPreviewedTexture != null)
                    {
                        if (partWithPreviewedTexture.getShape() == Shape.SHAPES.HEMISPHERE && !partWithPreviewedTexture.GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                            partWithPreviewedTexture.GAME_OBJECT.transform.localPosition = new Vector3(partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.x, partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.y - .5f, partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.z);
                        else if (partWithPreviewedTexture.getShape() != Shape.SHAPES.HEMISPHERE && partWithPreviewedTexture.GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                            partWithPreviewedTexture.GAME_OBJECT.transform.localPosition = new Vector3(partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.x, partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.y + .5f, partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.z);
                        partWithPreviewedTexture.changeTextureAndShape(partWithPreviewedTexture.getImage().getTexture(), MESHES[(int)partWithPreviewedTexture.getShape()], partWithPreviewedTexture.getShape());
                        partWithPreviewedTexture = null;
                    }
                    break;
                case StoreCard.MODES.PREVIEW_PART:
                    partBeingPreviewed = STORE_CARD.getPartBeingPreviewed();
                    previewRobot.GAME_OBJECT.transform.localPosition = new Vector3(previewRobot.GAME_OBJECT.transform.localPosition.x, previewRobot.GAME_OBJECT.transform.localPosition.y, 125);
                    previewRobot.GAME_OBJECT.transform.localScale = new Vector3(25, 25, 25);
                    MASK.SetActive(true);
                    List<Part> parts = new List<Part>();
                    List<Attachment> attachments = new List<Attachment>();
                    if (partBeingPreviewed is Attachment)
                        attachments.Add((Attachment)partBeingPreviewed);
                    for (int robotPartIndex = 0; robotPartIndex < robotParts.Count; ++robotPartIndex)
                    {
                        if (!(robotParts[robotPartIndex] is Attachment))
                        {
                            if (partBeingPreviewed.GetType() == robotParts[robotPartIndex].GetType())
                            {
                                partWithPreviewedTexture = previewRobotParts[robotPartIndex];
                                if (partBeingPreviewed.getShape() == Shape.SHAPES.HEMISPHERE && !previewRobotParts[robotPartIndex].GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                                    previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition = new Vector3(previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition.x, previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition.y - .5f, previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition.z);
                                else if (partBeingPreviewed.getShape() != Shape.SHAPES.HEMISPHERE && previewRobotParts[robotPartIndex].GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                                    previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition = new Vector3(previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition.x, previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition.y + .5f, previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition.z);
                                previewRobotParts[robotPartIndex].changeTextureAndShape(partBeingPreviewed.getImage().getTexture(), MESHES[(int)partBeingPreviewed.getShape()], partBeingPreviewed.getShape());
                            }
                            parts.Add(partBeingPreviewed.GetType() == robotParts[robotPartIndex].GetType() ? partBeingPreviewed : robotParts[robotPartIndex]);
                        }
                        else if (robotParts[robotPartIndex].GetType() != partBeingPreviewed.GetType())
                            attachments.Add((Attachment)robotParts[robotPartIndex]);
                    }
                    Robot tempRobot = new Robot("", true, robot.isHuman(), (Head)parts[(int)PART_INDICES.HEAD], (Body)parts[(int)PART_INDICES.BODY], (Mobility)parts[(int)PART_INDICES.MOBILITY], attachments.ToArray());
                    robotStats = tempRobot.getRobotStatStrings();
                    double[] differenceInStats = robot.compareTo(tempRobot);
                    for (int differenceInStatsIndex = FIRST_STAT_INDEX; differenceInStatsIndex < differenceInStats.Length; ++differenceInStatsIndex)
                        robotStats[differenceInStatsIndex - FIRST_STAT_INDEX] += (differenceInStats[differenceInStatsIndex] != 0 ? " (" + applyStatDifferenceFormatting(differenceInStats[differenceInStatsIndex], robotStats[differenceInStatsIndex - FIRST_STAT_INDEX].Contains("Weight")) + ")" : "");
                    CONFIGURATION_CARD.setForStoreCompare(true);
                    break;
                case StoreCard.MODES.BUY_PART:
                    MASK.SetActive(false);
                    partBeingBought = STORE_CARD.getPartBeingBought();
                    partBought = partBeingBought;
                    if (partWithPreviewedTexture != null)
                    {
                        if (partWithPreviewedTexture.getShape() == Shape.SHAPES.HEMISPHERE && !partWithPreviewedTexture.GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                            partWithPreviewedTexture.GAME_OBJECT.transform.localPosition = new Vector3(partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.x, partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.y - .5f, partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.z);
                        else if (partWithPreviewedTexture.getShape() != Shape.SHAPES.HEMISPHERE && partWithPreviewedTexture.GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                            partWithPreviewedTexture.GAME_OBJECT.transform.localPosition = new Vector3(partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.x, partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.y + .5f, partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.z);
                        partWithPreviewedTexture.changeTextureAndShape(partWithPreviewedTexture.getImage().getTexture(), MESHES[(int)partWithPreviewedTexture.getShape()], partWithPreviewedTexture.getShape());
                        partWithPreviewedTexture = null;
                    }
                    credits -= PERFORMANCE_METRIC_CALCULATOR.calculateCost(partBeingBought);
                    robot = new Robot("", true, robot.isHuman(), robotParts.ToArray());
                    CONFIGURATION_CARD.setForStoreCompare(false);
                    break;
                default:
                    break;
            }
            if (mode != StoreCard.MODES.BUY_PART)
                CONFIGURATION_CARD.setMode((InventoryCard.MODES)(int)mode, partBeingPreviewed, null, robotStats);
        }
    }

    public long getUpdatedCredits()
    {
        return credits;
    }

    public Part getPartBought()
    {
        Part tempPartBought = partBought;
        partBought = null;
        return tempPartBought;
    }

    public void updateGUI()
    {
        
    }

    public void update()
    {
        STORE_CARD.update(credits, humanParts.ToArray(), robotParts.ToArray(), colorScheme, enableCreditsSpentAnimation);
        checkStoreCardMode();
    }

    private string applyStatDifferenceFormatting(double statDifference, bool reverse)
    {
        string formattedStatDifference = "";
        if (!reverse)
            formattedStatDifference = (statDifference > 0 ? STAT_BETTER_COLOR_PREFIX + "+" + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : (statDifference < 0 ? STAT_WORSE_COLOR_PREFIX + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : StringTools.formatString(statDifference)));
        else formattedStatDifference = (statDifference > 0 ? STAT_WORSE_COLOR_PREFIX + "+" + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : (statDifference < 0 ? STAT_BETTER_COLOR_PREFIX + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : StringTools.formatString(statDifference)));
        return formattedStatDifference;
    }

    public void updateSettings(Color colorScheme, bool enableCreditsSpentAnimation)
    {
        this.colorScheme = colorScheme;
        this.enableCreditsSpentAnimation = enableCreditsSpentAnimation;
    }

    public void goToDefaultTab()
    {
        STORE_CARD.goToDefaultTab();
    }

    public void updateHumanParts(List<Part> humanParts)
    {
        this.humanParts = humanParts;
        STORE_CARD.update(credits, this.humanParts.ToArray(), robotParts.ToArray(), colorScheme, enableCreditsSpentAnimation);
    }

    public void updateRobotParts(List<Part> robotParts)
    {
        this.robotParts = robotParts;
        STORE_CARD.update(credits, humanParts.ToArray(), this.robotParts.ToArray(), colorScheme, enableCreditsSpentAnimation);
    }

    public void updateCredits(long credits)
    {
        this.credits = credits;
        STORE_CARD.update(credits, humanParts.ToArray(), robotParts.ToArray(), colorScheme, enableCreditsSpentAnimation);
        STORE_CARD.updateCredits();
    }

    public void destroyAllGeneratedObjects()
    {
        if (robot != null)
        {
            robot.getHead().destroyGameObject();
            robot.getBody().destroyGameObject();
            robot.getMobility().destroyGameObject();
            GameObject.Destroy(robot.GAME_OBJECT);
            STORE_CARD.destroyAllGeneratedObjects();
        }
    }
}