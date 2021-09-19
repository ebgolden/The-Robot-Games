using System.Collections.Generic;
using UnityEngine;

public class Workshop
{
    private string robotName;
    private List<Part> humanParts, robotParts;
    private long credits;
    private Robot previewRobot, robot;
    private readonly ConfigurationCard CONFIGURATION_CARD;
    private readonly InventoryCard INVENTORY_CARD;
    private readonly GameObject MASK, BATTLE_BUTTON;
    private readonly string STAT_BETTER_COLOR_PREFIX = "<color=#07D20C>", STAT_WORSE_COLOR_PREFIX = "<color=#FFA300>", FORMAT_COLOR_SUFFIX = "</color>";
    private enum PART_INDICES { HEAD, BODY, MOBILITY };
    private readonly int FIRST_STAT_INDEX = 2;
    private Part partWithPreviewedTexture;
    private bool goToField;
    private readonly Color INACTIVE_BATTLE_BUTTON_COLOR = new Color(0.48235294117f, 0.48235294117f, 0.48235294117f, 1), ACTIVE_BATTLE_BUTTON_COLOR;
    private readonly List<Mesh> MESHES;
    private Color colorScheme;
    private bool enableCreditsSpentAnimation;

    public Workshop(List<Mesh> meshes, Robot previewRobot, string robotName, List<Part> humanParts, Part[] robotParts, long credits, Color colorScheme, bool enableCreditsSpentAnimation)
    {
        MESHES = meshes;
        this.previewRobot = previewRobot;
        this.robotName = robotName;
        this.humanParts = humanParts;
        this.credits = credits;
        this.colorScheme = colorScheme;
        this.enableCreditsSpentAnimation = enableCreditsSpentAnimation;
        goToField = false;
        GameObject widgetsPanel = GameObject.Find("WidgetsPanel");
        foreach (Transform child in widgetsPanel.transform)
            GameObject.Destroy(child.gameObject);
        GameObject partsPanel = GameObject.Find("PartsPanel");
        foreach (Transform child in partsPanel.transform)
            GameObject.Destroy(child.gameObject);
        MASK = GameObject.Find("Workshop").transform.Find("WorkshopMask").gameObject;
        MASK.SetActive(false);
        BATTLE_BUTTON = GameObject.Find("BattleButton");
        ACTIVE_BATTLE_BUTTON_COLOR = BATTLE_BUTTON.GetComponent<UnityEngine.UI.Image>().color;
        this.robotParts = new List<Part>();
        if (robotParts != null && robotParts.Length > 0)
        {
            this.robotParts.AddRange(robotParts);
            robot = new Robot("", true, true, this.robotParts.ToArray());
            CONFIGURATION_CARD = new ConfigurationCard(robotName, credits, robot.getDurability(), robot.getRemainingDurability(), robot.getRobotStatStrings(), robot.getWeight() > robot.getMaxForce(), this.robotParts.ToArray(), colorScheme, enableCreditsSpentAnimation);
            CONFIGURATION_CARD.enable();
            INVENTORY_CARD = new InventoryCard(this.credits, this.humanParts.ToArray(), this.robotParts.ToArray());
            INVENTORY_CARD.enable();
        }
        partWithPreviewedTexture = null;
    }

    private void checkConfigurationCardRepairButtonStates()
    {
        bool[] repairButtonStates = CONFIGURATION_CARD.getButtonStates();
        for (int buttonIndex = 0; buttonIndex < repairButtonStates.Length; ++buttonIndex)
        {
            if (repairButtonStates[buttonIndex])
            {
                credits -= CONFIGURATION_CARD.getRepairCosts()[buttonIndex];
                robotParts[buttonIndex].repair();
                humanParts[humanParts.FindIndex(part => part.getID() == robotParts[buttonIndex].getID())].repair();
            }
        }
        CONFIGURATION_CARD.resetButtonStates();
    }

    private void checkConfigurationCardRemoveParts()
    {
        bool[] removeParts = CONFIGURATION_CARD.getRemoveParts();
        for (int buttonIndex = 0; buttonIndex < removeParts.Length; ++buttonIndex)
        {
            if (removeParts[buttonIndex] && robotParts[buttonIndex] is Attachment)
            {
                bool canRemovePart = false;
                if (((Attachment)robotParts[buttonIndex]).isWeapon())
                {
                    for (int partIndex = 0; partIndex < robotParts.Count; ++partIndex)
                    {
                        if (robotParts[partIndex] is Attachment && robotParts[partIndex] != robotParts[buttonIndex] && ((Attachment)robotParts[partIndex]).isWeapon())
                        {
                            canRemovePart = true;
                            break;
                        }
                    }
                }
                else canRemovePart = true;
                if (canRemovePart)
                    robotParts.Remove(robotParts[buttonIndex]);
            }
        }
        CONFIGURATION_CARD.resetRemoveParts();
        if (removeParts.Length > robotParts.Count || humanParts.Count != INVENTORY_CARD.getHumanParts().Length)
        {
            CONFIGURATION_CARD.update(robotName, credits, robot.getDurability(), robot.getRemainingDurability(), robot.getRobotStatStrings(), robot.getWeight() > robot.getMaxForce(), robotParts.ToArray(), colorScheme, enableCreditsSpentAnimation);
            INVENTORY_CARD.update(credits, humanParts.ToArray(), robotParts.ToArray());
        }
    }

    public void checkConfigurationCardRobotName()
    {
        if (CONFIGURATION_CARD != null)
        {
            CONFIGURATION_CARD.checkRobotName();
            this.robotName = CONFIGURATION_CARD.getRobotName();
        }
    }

    private void checkInventoryCardMode()
    {
        InventoryCard.MODES mode = INVENTORY_CARD.getMode();
        Part partBeingPreviewed = null, partBeingEquipt = null;
        Part[] previewRobotParts = previewRobot.getParts();
        string[] robotStats = null;
        if (CONFIGURATION_CARD.getMode() != mode)
        {
            switch (mode)
            {
                case InventoryCard.MODES.VIEW_PART_STATS:
                    MASK.SetActive(false);
                    previewRobot.GAME_OBJECT.transform.localPosition = new Vector3(previewRobot.GAME_OBJECT.transform.localPosition.x, previewRobot.GAME_OBJECT.transform.localPosition.y, 500);
                    previewRobot.GAME_OBJECT.transform.localScale = new Vector3(100, 100, 100);
                    CONFIGURATION_CARD.initialize();
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
                case InventoryCard.MODES.PREVIEW_PART:
                    partBeingPreviewed = INVENTORY_CARD.getPartBeingPreviewed();
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
                    break;
                case InventoryCard.MODES.EQUIPT_PART:
                    MASK.SetActive(false);
                    partBeingEquipt = INVENTORY_CARD.getPartBeingEquipt();
                    if (partWithPreviewedTexture != null)
                    {
                        if (partWithPreviewedTexture.getShape() == Shape.SHAPES.HEMISPHERE && !partWithPreviewedTexture.GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                            partWithPreviewedTexture.GAME_OBJECT.transform.localPosition = new Vector3(partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.x, partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.y - .5f, partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.z);
                        else if (partWithPreviewedTexture.getShape() != Shape.SHAPES.HEMISPHERE && partWithPreviewedTexture.GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                            partWithPreviewedTexture.GAME_OBJECT.transform.localPosition = new Vector3(partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.x, partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.y + .5f, partWithPreviewedTexture.GAME_OBJECT.transform.localPosition.z);
                        partWithPreviewedTexture.changeTextureAndShape(partWithPreviewedTexture.getImage().getTexture(), MESHES[(int)partWithPreviewedTexture.getShape()], partWithPreviewedTexture.getShape());
                        partWithPreviewedTexture = null;
                    }
                    for (int robotPartIndex = 0; robotPartIndex < robotParts.Count; ++robotPartIndex)
                    {
                        if (robotParts[robotPartIndex].GetType() == partBeingEquipt.GetType())
                        {
                            robotParts[robotPartIndex] = partBeingEquipt;
                            if (!(partBeingEquipt is Attachment))
                            {
                                if (partBeingEquipt.getShape() == Shape.SHAPES.HEMISPHERE && !previewRobotParts[robotPartIndex].GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                                    previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition = new Vector3(previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition.x, previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition.y - .5f, previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition.z);
                                else if (partBeingEquipt.getShape() != Shape.SHAPES.HEMISPHERE && previewRobotParts[robotPartIndex].GAME_OBJECT.GetComponent<MeshFilter>().mesh.name.Contains("Sphere"))
                                    previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition = new Vector3(previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition.x, previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition.y + .5f, previewRobotParts[robotPartIndex].GAME_OBJECT.transform.localPosition.z);
                                previewRobotParts[robotPartIndex].changeTextureAndShape(partBeingPreviewed.getImage().getTexture(), MESHES[(int)partBeingPreviewed.getShape()], partBeingPreviewed.getShape());
                            }
                            break;
                        }
                        if (robotPartIndex == robotParts.Count - 1)
                            robotParts.Add(partBeingEquipt);
                    }
                    robot = new Robot("", true, robot.isHuman(), robotParts.ToArray());
                    break;
                default:
                    break;
            }
            CONFIGURATION_CARD.setMode(mode, partBeingPreviewed, partBeingEquipt, robotStats);
        }
    }

    public long getUpdatedCredits()
    {
        return credits;
    }

    public void updateGUI()
    {
        
    }

    public void update()
    {
        CONFIGURATION_CARD.update(robotName, credits, robot.getDurability(), robot.getRemainingDurability(), robot.getRobotStatStrings(), robot.getWeight() > robot.getMaxForce(), robotParts.ToArray(), colorScheme, enableCreditsSpentAnimation);
        checkConfigurationCardRepairButtonStates();
        checkConfigurationCardRemoveParts();
        checkConfigurationCardRobotName();
        INVENTORY_CARD.update(credits, humanParts.ToArray(), robotParts.ToArray());
        checkInventoryCardMode();
        if (BATTLE_BUTTON.GetComponent<ButtonListener>().isClicked())
        {
            BATTLE_BUTTON.GetComponent<ButtonListener>().resetClick();
            if (robot.getRemainingDurability() > 0)
                goToField = true;
        }
        BATTLE_BUTTON.GetComponent<UnityEngine.UI.Image>().color = robot.getRemainingDurability() <= 0 ? INACTIVE_BATTLE_BUTTON_COLOR : ACTIVE_BATTLE_BUTTON_COLOR;
    }

    private string applyStatDifferenceFormatting(double statDifference, bool reverse)
    {
        string formattedStatDifference = "";
        if (!reverse)
            formattedStatDifference = (statDifference > 0 ? STAT_BETTER_COLOR_PREFIX + "+" + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : (statDifference < 0 ? STAT_WORSE_COLOR_PREFIX + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : StringTools.formatString(statDifference)));
        else formattedStatDifference = (statDifference > 0 ? STAT_WORSE_COLOR_PREFIX + "+" + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : (statDifference < 0 ? STAT_BETTER_COLOR_PREFIX + StringTools.formatString(statDifference) + FORMAT_COLOR_SUFFIX : StringTools.formatString(statDifference)));
        return formattedStatDifference;
    }

    public ConfigurationCard getConfigurationCard()
    {
        return CONFIGURATION_CARD;
    }

    public void updateSettings(Color colorScheme, bool enableCreditsSpentAnimation)
    {
        this.colorScheme = colorScheme;
        this.enableCreditsSpentAnimation = enableCreditsSpentAnimation;
    }

    public void updateHumanParts(List<Part> humanParts)
    {
        this.humanParts = humanParts;
        CONFIGURATION_CARD.update(robotName, credits, robot.getDurability(), robot.getRemainingDurability(), robot.getRobotStatStrings(), robot.getWeight() > robot.getMaxForce(), robotParts.ToArray(), colorScheme, enableCreditsSpentAnimation);
        INVENTORY_CARD.update(credits, this.humanParts.ToArray(), robotParts.ToArray());
    }

    public void updateRobotParts(List<Part> robotParts)
    {
        this.robotParts = robotParts;
        CONFIGURATION_CARD.update(robotName, credits, robot.getDurability(), robot.getRemainingDurability(), robot.getRobotStatStrings(), robot.getWeight() > robot.getMaxForce(), this.robotParts.ToArray(), colorScheme, enableCreditsSpentAnimation);
        INVENTORY_CARD.update(credits, humanParts.ToArray(), this.robotParts.ToArray());
    }

    public void updateCredits(long credits)
    {
        this.credits = credits;
        if (robotParts != null && robotParts.Count > 0)
        {
            CONFIGURATION_CARD.update(robotName, credits, robot.getDurability(), robot.getRemainingDurability(), robot.getRobotStatStrings(), robot.getWeight() > robot.getMaxForce(), robotParts.ToArray(), colorScheme, enableCreditsSpentAnimation);
            CONFIGURATION_CARD.updateCredits();
            INVENTORY_CARD.update(credits, humanParts.ToArray(), robotParts.ToArray());
        }
    }

    public string getRobotName()
    {
        return CONFIGURATION_CARD.getRobotName();
    }

    public void setGoToField(bool goToField)
    {
        this.goToField = goToField;
    }

    public bool getGoToField()
    {
        return goToField;
    }

    public List<Part> getRobotParts()
    {
        return robotParts;
    }

    public List<Part> getHumanParts()
    {
        return humanParts;
    }

    public Robot getRobot()
    {
        return robot;
    }

    public void destroyAllGeneratedObjects()
    {
        if (robot != null)
        {
            robot.getHead().destroyGameObject();
            robot.getBody().destroyGameObject();
            robot.getMobility().destroyGameObject();
            GameObject.Destroy(robot.GAME_OBJECT);
            CONFIGURATION_CARD.destroyAllGeneratedObjects();
            INVENTORY_CARD.destroyAllGeneratedObjects();
        }
    }
}