using System.Collections.Generic;
using UnityEngine;

public class Field : Screen
{
    private readonly GameEngine GAME_ENGINE;
    private readonly Robot[] ROBOTS;
    private readonly Obstacle[] OBSTACLES;
    public readonly Dimension FIELD_SIZE;
    public static readonly double FIELD_HEIGHT = 2, WALL_THICKNESS = 2;
    private string fieldColorString;
    private bool settingsOpen;

    public Field(List<Setting> settingList, Robot[] robots, Obstacle[] obstacles, double previousDamageDifference, double previousMaxDamageDifference, double previousTimeElapsed) : base(settingList)
    {
        ROBOTS = robots;
        OBSTACLES = obstacles;
        FIELD_SIZE = getFieldSize(base.settingPairs.field_size);
        base.GAME_OBJECT.GetComponent<Rigidbody>();
        Vector3 fieldFootprint = GAME_OBJECT.GetComponent<BoxCollider>().bounds.size;
        Vector3 rescale = GAME_OBJECT.transform.localScale;
        rescale.x = (float)FIELD_SIZE.width;
        rescale.y = (float)FIELD_HEIGHT;
        rescale.z = (float)FIELD_SIZE.depth;
        GAME_OBJECT.transform.localScale = rescale;
        if (ROBOTS != null && OBSTACLES != null)
            GAME_ENGINE = new GameEngine(this, ROBOTS, OBSTACLES, previousDamageDifference, previousMaxDamageDifference, previousTimeElapsed, base.settingPairs);
        settingsOpen = false;
        base.GAME_OBJECT.GetComponent<MeshRenderer>().material.color = ImageTools.getColorFromString(base.settingPairs.field_color);
        base.colorScheme = ImageTools.getColorFromString(base.settingPairs.color_scheme);
        fieldColorString = base.settingPairs.field_color;
        base.GAME_OBJECT.GetComponent<MeshRenderer>().material.color = ImageTools.getColorFromString(fieldColorString);
    }

    public static Dimension getFieldSize(double fieldSize)
    {
        return new Dimension(Mathf.Sqrt((float)fieldSize), FIELD_HEIGHT, Mathf.Sqrt((float)fieldSize));
    }

    public override void Dispose()
    {
        base.Dispose();
        GAME_ENGINE.destroyAllGeneratedObjects();
    }

    public override void show()
    {
        if (!base.isDisposed() && GAME_ENGINE != null)
        {
            if (base.areSettingsOpen())
            {
                base.colorScheme = ImageTools.getColorFromString(base.settingPairs.color_scheme);
                if (fieldColorString != base.settingPairs.field_color)
                {
                    fieldColorString = base.settingPairs.field_color;
                    base.GAME_OBJECT.GetComponent<MeshRenderer>().material.color = ImageTools.getColorFromString(fieldColorString);
                }
            }
            GAME_ENGINE.update(base.settingPairs);
            if (!settingsOpen && !base.SETTINGS_MENU.isEnabled() && GAME_ENGINE.getOpenSettings())
            {
                base.openSettings();
                settingsOpen = true;
            }
            else if (settingsOpen && !base.SETTINGS_MENU.isEnabled())
            {
                GAME_ENGINE.closeSettings();
                settingsOpen = false;
            }
            base.show();
        }
    }

    public override void onGUI()
    {
        if (!base.isDisposed() && GAME_ENGINE != null)
        {
            GAME_ENGINE.updateFieldPeripheral();
            base.onGUI();
        }
    }

    public GameEngine getGameEngine()
    {
        return GAME_ENGINE;
    }

    public bool getGoToBuildHub()
    {
        return GAME_ENGINE.getGoToBuildHub();
    }

    public Robot getHumanRobot()
    {
        return GAME_ENGINE.getHumanRobot();
    }
}