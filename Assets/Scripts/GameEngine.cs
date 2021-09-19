using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class GameEngine
{
    public enum ACTIONS { UP_PRESS, RIGHT_PRESS, DOWN_PRESS, LEFT_PRESS, UP_RELEASE, RIGHT_RELEASE, DOWN_RELEASE, LEFT_RELEASE, CHARGE_WEAPON, FIRE_WEAPON, SWITCH_ATTACHMENT_NEXT, SWITCH_ATTACHMENT_PREVIOUS, USE_ATTACHMENT, STOP_USING_ATTACHMENT, MENU_TOGGLE_ON, MENU_TOGGLE_OFF, TURN_CLOCKWISE, TURN_COUNTER_CLOCKWISE, LOOK_UP, LOOK_DOWN }
    private KeyCode up = KeyCode.W, right = KeyCode.D, down = KeyCode.S, left = KeyCode.A, charge_weapon = KeyCode.Space, switch_attachment_next = KeyCode.Tab, switch_attachment_previous = KeyCode.LeftShift, use_attachment = KeyCode.Return, menu_toggle = KeyCode.Escape;
    private bool effects = true, directional_movement = true;
    private int input_sensitivity = 1;
    private enum AXIS { X_AXIS, Z_AXIS };
    private enum DIRECTION { POSITIVE, NEGATIVE };
    public static readonly double GRAVITY = 9.81;
    private readonly double FORCE_MULTIPLIER = 1;
    private static readonly int SCALE_MULTIPLIER = 6;
    private readonly float POSITION_LIMIT;
    private static readonly Point START_POSITION_OFFSET = new Point(SCALE_MULTIPLIER * Robot.SCALE, 0, SCALE_MULTIPLIER * Robot.SCALE);
    private readonly Dimension FIELD_SIZE;
    private readonly Point HUMAN_START_POINT;
    private readonly Point FIRST_ROBOT_START_POINT;
    private readonly Point ROBOT_START_POINT_OFFSET = new Point(Robot.SCALE * 1.5, 0, 0);
    private readonly Point START_ROTATE_POINT = new Point(0, 180, 0);
    private readonly bool ROTATE_HUMAN_ON_START = true;
    private readonly Field FIELD;
    private readonly Robot HUMAN;
    private readonly List<Robot> ROBOTS;
    private readonly float[] ROBOT_VERTICAL_LOOK_ANGLES;
    private readonly Obstacle[] OBSTACLES;
    private readonly GamePlayMenu GAME_PLAY_MENU;
    private readonly List<Camera> ROBOT_CAMERAS;
    private readonly float SENS_HORIZONTAL = 10.0f, SENS_VERTICAL = 10.0f, MIN_VERTICAL = -45.0f, MAX_VERTICAL = 45.0f;
    private float rotationX, rotationY;
    private List<Projectile> projectiles;
    private List<Attachment>[] attachmentsUsedByRobots;
    private bool chargeWeapon = false, attachmentToggle = false;
    private readonly FieldPeripheral FIELD_PERIPHERAL;
    private bool gamePlay = true, menuToggle = false, openSettings = false, goToBuildHub = false, switchingAttachment = false;
    private long startTime, endTime, pauseTime, timeWithoutMovingRobotHead;
    private List<Projectile> projectileGarbageBin;
    private readonly string CANVAS_NAME = "FieldDynamicCanvas", CAMERA_NAME_PREFIX = "Default Camera", SUMMARY_CARD_NAME = "SummaryCard", SUMMARY_DETAILS_NAME = "SummaryDetails", STATUS_NAME = "Status", DAMAGE_RECEIVED_NAME = "DamageReceived", DAMAGE_DEALT_NAME = "DamageDealt", ROBOTS_DEFEATED_NAME = "RobotsDefeated", TIME_ELAPSED_NAME = "TimeElapsed", CREDITS_EARNED_NAME = "CreditsEarned", BACK_TO_WORKSHOP_NAME = "BackToWorkshop", WON_TEXT = "<color=#07D20C>Won", LOST_TEXT = "<color=#FF0000>Lost";
    private readonly int UI_LAYER = 5, MAX_TIME_BEFORE_ROBOT_HEAD_STOPS_MOVING = 300;
    private readonly GameObject SUMMARY_CARD, STATUS, DAMAGE_RECEIVED, DAMAGE_DEALT, ROBOTS_DEFEATED, TIME_ELAPSED, CREDITS_EARNED, BACK_TO_WORKSHOP, ATTACHMENT_SOUND, WEAPON_SOUND, ROBOT_SOUND;
    private readonly double STARTING_DURABILITY, PREVIOUS_DAMAGE_DIFFERENCE, PREVIOUS_MAX_DAMAGE_DIFFERENCE, PREVIOUS_TIME_ELAPSED;
    private readonly PerformanceMetricCalculator PERFORMANCE_METRIC_CALCULATOR;
    private int scrollCount, numberOfActiveAttachments;
    private SettingPairs settingPairs;
    private Color colorScheme;

    public GameEngine(Field field, Robot[] robots, Obstacle[] obstacles, double previousDamageDifference, double previousMaxDamageDifference, double previousTimeElapsed, SettingPairs settingPairs)
    {
        FIELD = field;
        OBSTACLES = obstacles;
        setSettingPairs(settingPairs);
        colorScheme = ImageTools.getColorFromString(this.settingPairs.color_scheme);
        GAME_PLAY_MENU = new GamePlayMenu(this.settingPairs);
        ATTACHMENT_SOUND = GameObject.Find("FieldAttachmentSound");
        ATTACHMENT_SOUND.GetComponent<AudioSource>().volume = (float)(this.settingPairs.master_volume / 100 * this.settingPairs.attachment_sound_volume / 100);
        WEAPON_SOUND = GameObject.Find("FieldWeaponSound");
        WEAPON_SOUND.GetComponent<AudioSource>().volume = (float)(this.settingPairs.master_volume / 100 * this.settingPairs.weapon_used_volume / 100);
        ROBOT_SOUND = GameObject.Find("FieldRobotSound");
        ROBOT_SOUND.GetComponent<AudioSource>().volume = (float)(this.settingPairs.master_volume / 100 * this.settingPairs.robot_sound_volume / 100);
        FIELD_SIZE = Field.getFieldSize(this.settingPairs.field_size);
        POSITION_LIMIT = (float)FIELD_SIZE.width / 2 - (float)Robot.SCALE;
        HUMAN_START_POINT = new Point(FIELD_SIZE.width / 2 - START_POSITION_OFFSET.x, FIELD_SIZE.height / 2 + 1, FIELD_SIZE.depth / 2 - START_POSITION_OFFSET.z);
        FIRST_ROBOT_START_POINT = new Point(-FIELD_SIZE.width / 2 + START_POSITION_OFFSET.x, FIELD_SIZE.height / 2 + 1, -FIELD_SIZE.depth / 2 + START_POSITION_OFFSET.z);
        rotationX = 0;
        rotationY = 0;
        PERFORMANCE_METRIC_CALCULATOR = new PerformanceMetricCalculator();
        PREVIOUS_DAMAGE_DIFFERENCE = previousDamageDifference;
        PREVIOUS_MAX_DAMAGE_DIFFERENCE = previousMaxDamageDifference;
        PREVIOUS_TIME_ELAPSED = previousTimeElapsed;
        attachmentsUsedByRobots = new List<Attachment>[robots.Length];
        for (int attachmentsIndex = 0; attachmentsIndex < robots.Length; ++attachmentsIndex)
            attachmentsUsedByRobots[attachmentsIndex] = new List<Attachment>();
        Robot human = null;
        List<Robot> nonHumanRobots = new List<Robot>();
        foreach (Robot robot in robots)
        {
            if (!robot.isHuman())
                nonHumanRobots.Add(robot);
            else if (human == null && robot.isHuman())
                human = robot;
        }
        HUMAN = human;
        if (HUMAN != null)
            STARTING_DURABILITY = HUMAN.getRemainingDurability();
        else STARTING_DURABILITY = 0;
        ROBOTS = new List<Robot>();
        ROBOTS.AddRange(nonHumanRobots);
        Robot firstRobot = (HUMAN != null) ? HUMAN : ROBOTS[0];
        firstRobot.setPosition(HUMAN_START_POINT);
        List<Attachment> listOfFirstRobotAttachments = firstRobot.getAttachments();
        numberOfActiveAttachments = listOfFirstRobotAttachments.FindAll(attachment => !attachment.isPassive()).Count;
        if (ROTATE_HUMAN_ON_START)
            firstRobot.GAME_OBJECT.transform.localEulerAngles = START_ROTATE_POINT.toVector3();
        ROBOT_CAMERAS = new List<Camera>();
        int cameraIndex = 0;
        foreach (Robot robot in robots)
        {
            Camera camera = GameObject.Find(CAMERA_NAME_PREFIX + (cameraIndex + 1).ToString()).GetComponent<Camera>() as Camera;
            camera.transform.SetParent(robot.getHead().GAME_OBJECT.transform);
            camera.transform.localPosition = Vector3.zero;
            camera.transform.localEulerAngles = Vector3.zero;
            ROBOT_CAMERAS.Add(camera);
            ++cameraIndex;
        }
        Part[] firstRobotParts = firstRobot.getParts();
        foreach (Part part in firstRobotParts)
        {
            if (!(part is Attachment))
            {
                part.toggleGameObject(true);
                part.GAME_OBJECT.GetComponent<Renderer>().enabled = false;
            }
        }
        firstRobot.getHead().GAME_OBJECT.GetComponent<Renderer>().enabled = true;
        Robot[] otherRobots = (HUMAN != null) ? ROBOTS.ToArray() : new Robot[ROBOTS.Count - 1];
        if (HUMAN == null)
            for (int otherRobotIndex = 1; otherRobotIndex < ROBOTS.Count; ++otherRobotIndex)
                otherRobots[otherRobotIndex - 1] = ROBOTS[otherRobotIndex];
        FIELD_PERIPHERAL = new FieldPeripheral(HUMAN, ROBOTS.ToArray(), this.settingPairs);
        ROBOT_VERTICAL_LOOK_ANGLES = new float[ROBOTS.Count];
        if (HUMAN == null)
            ROBOT_VERTICAL_LOOK_ANGLES[0] = 0;
        int robotIndex = 0;
        foreach (Robot robot in otherRobots)
        {
            if (HUMAN == null)
                ROBOT_VERTICAL_LOOK_ANGLES[robotIndex + 1] = 0;
            else ROBOT_VERTICAL_LOOK_ANGLES[robotIndex] = 0;
            robot.setPosition(new Point(FIRST_ROBOT_START_POINT.x + ROBOT_START_POINT_OFFSET.x * robotIndex, FIRST_ROBOT_START_POINT.y + ROBOT_START_POINT_OFFSET.y * robotIndex, FIRST_ROBOT_START_POINT.z + ROBOT_START_POINT_OFFSET.z * robotIndex++));
            if (!ROTATE_HUMAN_ON_START)
                robot.GAME_OBJECT.transform.localEulerAngles = START_ROTATE_POINT.toVector3();
        }
        projectiles = new List<Projectile>();
        projectileGarbageBin = new List<Projectile>();
        startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        endTime = 0;
        pauseTime = 0;
        timeWithoutMovingRobotHead = 0;
        scrollCount = 0;
        SUMMARY_CARD = GameObject.Find(SUMMARY_CARD_NAME);
        GameObject summaryDetails = GameObject.Find(SUMMARY_DETAILS_NAME);
        STATUS = summaryDetails.transform.Find(STATUS_NAME).gameObject;
        DAMAGE_RECEIVED = summaryDetails.transform.Find(DAMAGE_RECEIVED_NAME).gameObject;
        DAMAGE_DEALT = summaryDetails.transform.Find(DAMAGE_DEALT_NAME).gameObject;
        ROBOTS_DEFEATED = summaryDetails.transform.Find(ROBOTS_DEFEATED_NAME).gameObject;
        TIME_ELAPSED = summaryDetails.transform.Find(TIME_ELAPSED_NAME).gameObject;
        CREDITS_EARNED = summaryDetails.transform.Find(CREDITS_EARNED_NAME).gameObject;
        BACK_TO_WORKSHOP = GameObject.Find(BACK_TO_WORKSHOP_NAME);
        BACK_TO_WORKSHOP.GetComponent<UnityEngine.UI.Image>().color = colorScheme;
        SUMMARY_CARD.transform.SetAsLastSibling();
        SUMMARY_CARD.SetActive(false);
        updateSettings();
    }

    public void setSettingPairs(SettingPairs settingPairs)
    {
        this.settingPairs = settingPairs;
    }

    private void updateSettings()
    {
        if (effects != settingPairs.effects)
            effects = settingPairs.effects;
        if (input_sensitivity != Mathf.RoundToInt((float)settingPairs.input_sensitivity))
            input_sensitivity = Mathf.RoundToInt((float)settingPairs.input_sensitivity);
        if (directional_movement != settingPairs.directional_movement)
            directional_movement = settingPairs.directional_movement;
        if (settingPairs.up != default && up.ToString() != settingPairs.up)
            up = (KeyCode)Enum.Parse(typeof(KeyCode), settingPairs.up, true);
        if (settingPairs.right != default && right.ToString() != settingPairs.right)
            right = (KeyCode)Enum.Parse(typeof(KeyCode), settingPairs.right, true);
        if (settingPairs.down != default && down.ToString() != settingPairs.down)
            down = (KeyCode)Enum.Parse(typeof(KeyCode), settingPairs.down, true);
        if (settingPairs.left != default && left.ToString() != settingPairs.left)
            left = (KeyCode)Enum.Parse(typeof(KeyCode), settingPairs.left, true);
        if (settingPairs.charge_weapon != default && charge_weapon.ToString() != settingPairs.charge_weapon)
            charge_weapon = (KeyCode)Enum.Parse(typeof(KeyCode), settingPairs.charge_weapon, true);
        if (settingPairs.switch_attachment_next != default && switch_attachment_next.ToString() != settingPairs.switch_attachment_next)
            switch_attachment_next = (KeyCode)Enum.Parse(typeof(KeyCode), settingPairs.switch_attachment_next, true);
        if (settingPairs.switch_attachment_previous != default && switch_attachment_previous.ToString() != settingPairs.switch_attachment_previous)
            switch_attachment_previous = (KeyCode)Enum.Parse(typeof(KeyCode), settingPairs.switch_attachment_previous, true);
        if (settingPairs.use_attachment != default && use_attachment.ToString() != settingPairs.use_attachment)
            use_attachment = (KeyCode)Enum.Parse(typeof(KeyCode), settingPairs.use_attachment, true);
        if (settingPairs.menu_toggle != default && menu_toggle.ToString() != settingPairs.menu_toggle)
            menu_toggle = (KeyCode)Enum.Parse(typeof(KeyCode), settingPairs.menu_toggle, true);
        if (settingPairs.color_scheme != default && ImageTools.getColorFromString(settingPairs.color_scheme) != colorScheme)
            colorScheme = ImageTools.getColorFromString(settingPairs.color_scheme);
    }

    private void keyPressed()
    {
        if (HUMAN != null)
        {
            if (Input.GetKey(up))
                triggerAction(HUMAN, ACTIONS.UP_PRESS);
            if (Input.GetKey(right))
                triggerAction(HUMAN, ACTIONS.RIGHT_PRESS);
            if (Input.GetKey(down))
                triggerAction(HUMAN, ACTIONS.DOWN_PRESS);
            if (Input.GetKey(left))
                triggerAction(HUMAN, ACTIONS.LEFT_PRESS);
            if (Input.GetKey(charge_weapon))
                triggerAction(HUMAN, ACTIONS.CHARGE_WEAPON);
            if (!switchingAttachment)
            {
                switchingAttachment = true;
                if (Input.GetKey(switch_attachment_next) && Input.GetKey(switch_attachment_previous))
                    triggerAction(HUMAN, ACTIONS.SWITCH_ATTACHMENT_PREVIOUS);
                else if (Input.GetKey(switch_attachment_next))
                    triggerAction(HUMAN, ACTIONS.SWITCH_ATTACHMENT_NEXT);
            }
            if (Input.GetKey(use_attachment) && !HUMAN.getAttachments().Selected().isWeapon() && !HUMAN.getAttachments().Selected().isPassive())
            {
                if (!HUMAN.getAttachments().Selected().isInUse() && !attachmentToggle)
                    triggerAction(HUMAN, ACTIONS.USE_ATTACHMENT);
                else if (HUMAN.getAttachments().Selected().isInUse() && attachmentToggle)
                    triggerAction(HUMAN, ACTIONS.STOP_USING_ATTACHMENT);
            }
        }
        if (Input.GetKey(menu_toggle))
        {
            if (gamePlay && !menuToggle)
                triggerAction(HUMAN, ACTIONS.MENU_TOGGLE_ON);
            else if (!gamePlay && menuToggle)
                triggerAction(HUMAN, ACTIONS.MENU_TOGGLE_OFF);
        }
    }

    private void keyReleased()
    {
        if (HUMAN != null)
        {
            if (chargeWeapon && !Input.GetMouseButton(0) && !Input.GetKey(charge_weapon))
                triggerAction(HUMAN, ACTIONS.FIRE_WEAPON);
            if (!Input.GetKey(up))
                triggerAction(HUMAN, ACTIONS.UP_RELEASE);
            if (!Input.GetKey(right))
                triggerAction(HUMAN, ACTIONS.RIGHT_RELEASE);
            if (!Input.GetKey(down))
                triggerAction(HUMAN, ACTIONS.DOWN_RELEASE);
            if (!Input.GetKey(left))
                triggerAction(HUMAN, ACTIONS.LEFT_RELEASE);
            if (!Input.GetKey(switch_attachment_next))
                switchingAttachment = false;
            if (gamePlay && !Input.GetKey(use_attachment) && !HUMAN.getAttachments().Selected().isWeapon() && !HUMAN.getAttachments().Selected().isPassive())
                attachmentToggle = HUMAN.getAttachments().Selected().isInUse();
        }
        if (!Input.GetKey(menu_toggle))
            menuToggle = !gamePlay;
    }

    private void mousePressed()
    {
        if (Input.GetMouseButton(0))
            triggerAction(HUMAN, ACTIONS.CHARGE_WEAPON);
    }

    private void mouseReleased()
    {
        if (chargeWeapon && !Input.GetMouseButton(0) && !Input.GetKey(charge_weapon))
            triggerAction(HUMAN, ACTIONS.FIRE_WEAPON);
    }

    private void mouseMoved()
    {
        HUMAN.GAME_OBJECT.transform.Rotate(0, Input.GetAxis("Mouse X") * SENS_HORIZONTAL, 0);
        rotationX -= Input.GetAxis("Mouse Y") * SENS_VERTICAL;
        rotationX = Mathf.Clamp(rotationX, MIN_VERTICAL, MAX_VERTICAL);
        rotationY = HUMAN.getHead().GAME_OBJECT.transform.localEulerAngles.y;
        if (rotationX > MIN_VERTICAL && rotationX < MAX_VERTICAL && Input.GetAxis("Mouse Y") != 0)
        {
            if (!ROBOT_SOUND.GetComponent<AudioSource>().isPlaying)
            {
                ROBOT_SOUND.GetComponent<AudioSource>().clip = Resources.Load("Sounds/Robot Head 1 Loop") as AudioClip;
                ROBOT_SOUND.GetComponent<AudioSource>().loop = true;
                ROBOT_SOUND.GetComponent<AudioSource>().Play();
            }
        }
        else if (ROBOT_SOUND.GetComponent<AudioSource>().isPlaying && !ROBOT_SOUND.GetComponent<AudioSource>().clip.name.Contains("End") && (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timeWithoutMovingRobotHead /*- pauseTime*/ > MAX_TIME_BEFORE_ROBOT_HEAD_STOPS_MOVING))
        {
            ROBOT_SOUND.GetComponent<AudioSource>().clip = Resources.Load("Sounds/Robot Head 1 End") as AudioClip;
            ROBOT_SOUND.GetComponent<AudioSource>().loop = false;
            ROBOT_SOUND.GetComponent<AudioSource>().Play();
        }
        HUMAN.getHead().GAME_OBJECT.transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);
    }

    private void mouseWheelMoved()
    {
        scrollCount += (int)Input.mouseScrollDelta.y;
        int sensitivity = 100 - input_sensitivity;
        if (sensitivity == 0)
            sensitivity = 1;
        if (Math.Abs(scrollCount) >= sensitivity)
        {
            ACTIONS action = (scrollCount > 0) ? ACTIONS.SWITCH_ATTACHMENT_NEXT : ACTIONS.SWITCH_ATTACHMENT_PREVIOUS;
            triggerAction(HUMAN, action);
            scrollCount = 0;
        }
    }

    private double calculateForce(Robot robot, DIRECTION direction)
    {
        double force = 0;
        force = (direction == DIRECTION.POSITIVE) ? 1 : -1;
        if (robot.getMaxForce() > robot.getWeight())
            return force * (robot.getMaxForce() - robot.getWeight());
        else return 0;
    }

    private bool isObstacleTouchingRobot(Robot robot, Obstacle obstacle)
    {
        List<Collider> collidersTouchingObstacle = obstacle.getCollidersTouching();
        Collider robotCollider = robot.GAME_OBJECT.GetComponent<Collider>();
        Collider robotHeadCollider = robot.getHead().GAME_OBJECT.GetComponent<Collider>();
        Collider robotBodyCollider = robot.getBody().GAME_OBJECT.GetComponent<Collider>();
        Collider robotMobilityCollider = robot.getMobility().GAME_OBJECT.GetComponent<Collider>();
        return collidersTouchingObstacle.Contains(robotCollider) || collidersTouchingObstacle.Contains(robotHeadCollider) || collidersTouchingObstacle.Contains(robotBodyCollider) || collidersTouchingObstacle.Contains(robotMobilityCollider);
    }

    private bool isProjectileTouchingRobot(Robot robot, Projectile projectile)
    {
        List<Collider> collidersTouchingProjectile = projectile.getCollidersTouching();
        Collider robotCollider = robot.GAME_OBJECT.GetComponent<Collider>();
        Collider robotHeadCollider = robot.getHead().GAME_OBJECT.GetComponent<Collider>();
        Collider robotBodyCollider = robot.getBody().GAME_OBJECT.GetComponent<Collider>();
        Collider robotMobilityCollider = robot.getMobility().GAME_OBJECT.GetComponent<Collider>();
        return collidersTouchingProjectile.Contains(robotCollider) || collidersTouchingProjectile.Contains(robotHeadCollider) || collidersTouchingProjectile.Contains(robotBodyCollider) || collidersTouchingProjectile.Contains(robotMobilityCollider);
    }

    public List<Obstacle> obstaclesNearRobot(Robot robot)
    {
        List<Obstacle> obstacles = new List<Obstacle>();
        foreach (Obstacle obstacle in OBSTACLES)
            if (isObstacleTouchingRobot(robot, obstacle))
                obstacles.Add(obstacle);
        if (obstacles == null || obstacles.Count == 0)
            robot.setConstraints();
        return obstacles;
    }

    private void moveRobot(Robot robot, Point force)
    {
        bool directional_movement = this.directional_movement/* && robot.isHuman()*/;
        Point position = robot.getPosition();
        Point velocity = robot.getVelocity();
        double mass = robot.getWeight() / GRAVITY;
        Point acceleration = new Point(force.x / mass, force.y / mass, force.z / mass);
        Vector3 nextPosition = new Point(position.x + velocity.x  + .5 * acceleration.x, position.y + velocity.y + .5 * acceleration.y, position.z + velocity.z + .5 * acceleration.z).toVector3();
        nextPosition = new Vector3(Mathf.Clamp(nextPosition.x, -POSITION_LIMIT, POSITION_LIMIT), Mathf.Clamp(nextPosition.y, -POSITION_LIMIT, POSITION_LIMIT), Mathf.Clamp(nextPosition.z, -POSITION_LIMIT, POSITION_LIMIT));
        Point adjustedForce = new Point(2 * mass * (nextPosition.x - position.x - velocity.x), 2 * mass * (nextPosition.y - position.y - velocity.y), 2 * mass * (nextPosition.z - position.z - velocity.z));
        robot.setForce(adjustedForce, directional_movement);
        List<Obstacle> obstacles = obstaclesNearRobot(robot);
        if (obstacles != null && obstacles.Count > 0)
        {
            foreach (Obstacle obstacle in obstacles)
            {
                Effect<Obstacle> effect = obstacle.getEffect();
                if (effect != null)
                    robot = effect.applyTo(robot, effects);
            }
        }
        else
        {
            Vector3 rotation = robot.GAME_OBJECT.transform.eulerAngles;
            rotation.x = 0;
            rotation.z = 0;
            robot.GAME_OBJECT.transform.eulerAngles = rotation;
        }
        if (Math.Abs((float)robot.getVelocity().x) > robot.getMaxSpeed())
        {
            float absForceX = Math.Abs((float)robot.getVelocity().x);
            if (robot.getVelocity().x > 0)
                robot.setForce(new Point(-absForceX, 0, 0), directional_movement);
            else robot.setForce(new Point(absForceX, 0, 0), directional_movement);
        }
        if (Math.Abs((float)robot.getVelocity().z) > robot.getMaxSpeed())
        {
            float absForceZ = Math.Abs((float)robot.getVelocity().z);
            if (robot.getVelocity().z > 0)
                robot.setForce(new Point(0, 0, -absForceZ), directional_movement);
            else robot.setForce(new Point(0, 0, absForceZ), directional_movement);
        }
    }

    private void recordPauseTime()
    {
        pauseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private void adjustTimesByElapsedPauseTime()
    {
        long elapsedPauseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - pauseTime;
        pauseTime = 0;
        startTime += elapsedPauseTime;
        Attachment[] attachments = null;
        if (HUMAN != null)
        {
            attachments = HUMAN.getAttachments().ToArray();
            foreach (Attachment attachment in attachments)
                attachment.adjustTime(elapsedPauseTime);
        }
        foreach (Robot robot in ROBOTS)
        {
            attachments = robot.getAttachments().ToArray();
            foreach (Attachment attachment in attachments)
                attachment.adjustTime(elapsedPauseTime);
        }
        foreach (Projectile projectile in projectileGarbageBin)
            projectile.adjustTime(elapsedPauseTime);
    }

    public void triggerAction(Robot robot, FieldAction action)
    {
        if (action != null)
        {
            triggerAction(robot, action.moveUp ? ACTIONS.UP_PRESS : ACTIONS.UP_RELEASE);
            triggerAction(robot, action.moveRight ? ACTIONS.RIGHT_PRESS : ACTIONS.RIGHT_RELEASE);
            triggerAction(robot, action.moveDown ? ACTIONS.DOWN_PRESS : ACTIONS.DOWN_RELEASE);
            triggerAction(robot, action.moveLeft ? ACTIONS.LEFT_PRESS : ACTIONS.LEFT_RELEASE);
            if (action.rotateClockwise)
                triggerAction(robot, ACTIONS.TURN_CLOCKWISE);
            if (action.rotateCounterClockwise)
                triggerAction(robot, ACTIONS.TURN_COUNTER_CLOCKWISE);
            if (action.lookUp)
                triggerAction(robot, ACTIONS.LOOK_UP);
            if (action.lookDown)
                triggerAction(robot, ACTIONS.LOOK_DOWN);
            if (action.chargeAttachment)
                triggerAction(robot, ACTIONS.CHARGE_WEAPON);
            else if (chargeWeapon)
                triggerAction(robot, ACTIONS.FIRE_WEAPON);
            if (!robot.getAttachments().Selected().isWeapon() && !robot.getAttachments().Selected().isPassive())
                triggerAction(robot, action.useAttachment ? ACTIONS.USE_ATTACHMENT : ACTIONS.STOP_USING_ATTACHMENT);
            if (action.pickAttachment != null)
                while (robot.getAttachments().Selected().getID() != action.pickAttachment.getID())
                    triggerAction(robot, ACTIONS.SWITCH_ATTACHMENT_NEXT);
        }
    }

    public void triggerAction(Robot robot, ACTIONS action)
    {
        if ((robot == null) || robot.isHuman())
        {
            switch (action)
            {
                case ACTIONS.MENU_TOGGLE_ON:
                    gamePlay = false;
                    recordPauseTime();
                    ROBOT_SOUND.GetComponent<AudioSource>().Stop();
                    GAME_PLAY_MENU.enable();
                    return;
                case ACTIONS.MENU_TOGGLE_OFF:
                    gamePlay = true;
                    adjustTimesByElapsedPauseTime();
                    GAME_PLAY_MENU.disable();
                    return;
                default:
                    break;
            }
        }
        if (gamePlay)
        {
            double xForce = 0, zForce = 0;
            Vector3 velocity = robot.GAME_OBJECT.GetComponent<Rigidbody>().velocity;
            int robotIndex = 0;
            if (HUMAN == null || robot != HUMAN)
                robotIndex = ROBOTS.IndexOf(robot);
            Attachment attachment = robot.getAttachments().Selected();
            bool directional_movement = this.directional_movement && robot.isHuman();
            switch (action)
            {
                case ACTIONS.UP_PRESS:
                    zForce = calculateForce(robot, DIRECTION.POSITIVE);
                    moveRobot(robot, new Point(robot.getForce().x, 0, zForce * FORCE_MULTIPLIER));
                    break;
                case ACTIONS.RIGHT_PRESS:
                    xForce = calculateForce(robot, DIRECTION.POSITIVE);
                    moveRobot(robot, new Point(xForce * FORCE_MULTIPLIER, 0, robot.getForce().z));
                    break;
                case ACTIONS.DOWN_PRESS:
                    zForce = calculateForce(robot, DIRECTION.NEGATIVE);
                    moveRobot(robot, new Point(robot.getForce().x, 0, zForce * FORCE_MULTIPLIER));
                    break;
                case ACTIONS.LEFT_PRESS:
                    xForce = calculateForce(robot, DIRECTION.NEGATIVE);
                    moveRobot(robot, new Point(xForce * FORCE_MULTIPLIER, 0, robot.getForce().z));
                    break;
                case ACTIONS.UP_RELEASE:
                case ACTIONS.DOWN_RELEASE:
                    robot.setForce(new Point(robot.getForce().x, robot.getForce().y, 0), directional_movement);
                    velocity.z = 0;
                    break;
                case ACTIONS.RIGHT_RELEASE:
                case ACTIONS.LEFT_RELEASE:
                    robot.setForce(new Point(0, robot.getForce().y, robot.getForce().z), directional_movement);
                    velocity.x = 0;
                    break;
                case ACTIONS.CHARGE_WEAPON:
                    chargeWeapon = true;
                    if (robot.getAttachments().Selected().isWeapon())
                    {
                        robot.getAttachments().Selected().charge();
                        if(robot.isHuman() || ((HUMAN == null) && (robotIndex == 0)))
                            FIELD_PERIPHERAL.chargeWeapon();
                    }
                    break;
                case ACTIONS.FIRE_WEAPON:
                    Projectile projectile = null;
                    float hitDistance = 0f;
                    float chargeRatio = 0;
                    if (robot.getAttachments().Selected().isWeapon())
                    {
                        chargeWeapon = false;
                        if (robot.isHuman() || ((HUMAN == null) && (robotIndex == 0)))
                            FIELD_PERIPHERAL.fireWeapon();
                        if (HUMAN == null || robot != HUMAN)
                            robotIndex = ROBOTS.IndexOf(robot);
                        Camera camera = ROBOT_CAMERAS[robotIndex];
                        hitDistance = getDistanceToObstruction(camera);
                        hitDistance = (hitDistance == 0) ? -1f : hitDistance;
                        chargeRatio = (float)(robot.getAttachments().Selected().getElapsedChargeTime() - robot.getAttachments().Selected().getMinChargeTime()) / (float)(robot.getAttachments().Selected().getMaxChargeTime() - robot.getAttachments().Selected().getMinChargeTime());
                        projectile = robot.getAttachments().Selected().fire(robot, hitDistance);
                    }
                    if (projectile != null)
                    {
                        WEAPON_SOUND.GetComponent<AudioSource>().pitch = chargeRatio * -2.5f + 3;
                        WEAPON_SOUND.GetComponent<AudioSource>().Play();
                        projectile.GAME_OBJECT.transform.position = Vector3.zero;
                        projectile.GAME_OBJECT.transform.SetParent(robot.GAME_OBJECT.transform, false);
                        projectile.GAME_OBJECT.transform.position = new Vector3(projectile.GAME_OBJECT.transform.position.x, robot.getHead().GAME_OBJECT.transform.position.y, projectile.GAME_OBJECT.transform.position.z);
                        projectile.GAME_OBJECT.transform.Rotate(new Vector3(90 + robot.getHead().GAME_OBJECT.transform.localEulerAngles.x, 0, 0));
                        projectile.GAME_OBJECT.transform.SetParent(null);
                        Vector3 forward = robot.getHead().GAME_OBJECT.transform.up * ((float)projectile.getSize().height / 2 + (float)Robot.SCALE);
                        projectile.GAME_OBJECT.transform.Translate(forward);
                        projectile.GAME_OBJECT.GetComponent<Rigidbody>().velocity = projectile.GAME_OBJECT.transform.up * (float)projectile.getVelocity();
                        if (projectile.getVelocity() != 0 || hitDistance != -1)
                            projectiles.Add(projectile);
                        projectileGarbageBin.Add(projectile);
                    }
                    break;
                case ACTIONS.SWITCH_ATTACHMENT_NEXT:
                    if (((HUMAN != null && robot == HUMAN) || robot == ROBOTS[0]) && numberOfActiveAttachments > 1)
                        ATTACHMENT_SOUND.GetComponent<AudioSource>().Play();
                    robot.nextAttachment();
                    if (robot.isHuman() || ((HUMAN == null) && (robotIndex == 0)))
                        FIELD_PERIPHERAL.nextAttachment();
                    break;
                case ACTIONS.SWITCH_ATTACHMENT_PREVIOUS:
                    if (((HUMAN != null && robot == HUMAN) || robot == ROBOTS[0]) && numberOfActiveAttachments > 1)
                        ATTACHMENT_SOUND.GetComponent<AudioSource>().Play();
                    robot.previousAttachment();
                    if (robot.isHuman() || ((HUMAN == null) && (robotIndex == 0)))
                        FIELD_PERIPHERAL.previousAttachment();
                    break;
                case ACTIONS.USE_ATTACHMENT:
                    if (!attachment.isWeapon() && !attachment.isPassive() && attachment.getEffect() != null && !attachmentsUsedByRobots[robotIndex].Contains(attachment) && attachment.canUse())
                    {
                        attachmentsUsedByRobots[robotIndex].Add(attachment);
                        robot = attachment.use(robot);
                    }
                    break;
                case ACTIONS.STOP_USING_ATTACHMENT:
                    if (!attachment.isWeapon() && !attachment.isPassive() && attachment.getEffect() != null && attachmentsUsedByRobots[robotIndex].Contains(attachment))
                    {
                        attachment.stopUsing();
                        attachmentsUsedByRobots[robotIndex].Remove(attachment);
                    }
                    break;
                case ACTIONS.TURN_CLOCKWISE:
                    robot.GAME_OBJECT.transform.Rotate(new Vector3(0, 1, 0));
                    break;
                case ACTIONS.TURN_COUNTER_CLOCKWISE:
                    robot.GAME_OBJECT.transform.Rotate(new Vector3(0, -1, 0));
                    break;
                case ACTIONS.LOOK_UP:
                    if (ROBOT_VERTICAL_LOOK_ANGLES[robotIndex] < MAX_VERTICAL)
                    {
                        robot.getHead().GAME_OBJECT.transform.Rotate(new Vector3(1, 0, 0));
                        ++ROBOT_VERTICAL_LOOK_ANGLES[robotIndex];
                    }
                    break;
                case ACTIONS.LOOK_DOWN:
                    if (ROBOT_VERTICAL_LOOK_ANGLES[robotIndex] > MIN_VERTICAL)
                    {
                        robot.getHead().GAME_OBJECT.transform.Rotate(new Vector3(-1, 0, 0));
                        --ROBOT_VERTICAL_LOOK_ANGLES[robotIndex];
                    }
                    break;
                default:
                    break;
            }
            robot.GAME_OBJECT.GetComponent<Rigidbody>().velocity = velocity;
        }
    }

    private void removeProjectile(Projectile projectile)
    {
        GameObject.Destroy(projectile.GAME_OBJECT);
        projectiles.Remove(projectile);
    }

    private void checkProjectileCollisions()
    {
        List<Robot> robots = getAllRobots();
        bool touchingRobot = false;
        for (int projectileIndex = projectiles.Count - 1; projectileIndex >= 0; --projectileIndex)
        {
            Robot currentRobot = null;
            foreach (Robot robot in robots)
            {
                currentRobot = robot;
                touchingRobot = isProjectileTouchingRobot(robot, projectiles[projectileIndex]);
                if (touchingRobot)
                    break;
            }
            List<Collider> collidersTouchingObstacle = projectiles[projectileIndex].GAME_OBJECT.GetComponent<ProjectileListener>().getCollidersTouching();
            if ((projectiles[projectileIndex].GAME_OBJECT.GetComponent<ProjectileListener>().collided || collidersTouchingObstacle.Contains(FIELD.GAME_OBJECT.GetComponent<Collider>())))
            {
                if (touchingRobot)
                {
                    double currentRobotDurability = currentRobot.getRemainingDurability();
                    currentRobot = projectiles[projectileIndex].getEffect().applyTo(currentRobot, effects);
                    currentRobotDurability -= currentRobot.getRemainingDurability();
                    if (HUMAN != null && HUMAN == projectiles[projectileIndex].getRobot())
                        HUMAN.didDamage(currentRobotDurability);
                    else
                        for (int robotIndex = 0; robotIndex < ROBOTS.Count; ++robotIndex)
                            if (ROBOTS[robotIndex] == projectiles[projectileIndex].getRobot())
                                ROBOTS[robotIndex].didDamage(currentRobotDurability);
                }
                removeProjectile(projectiles[projectileIndex]);
            }
        }
    }

    private List<Robot> getAllRobots()
    {
        List<Robot> robots = new List<Robot>();
        if (HUMAN != null)
            robots.Add(HUMAN);
        robots.AddRange(ROBOTS);
        return robots;
    }

    private void clearOutsideGameObjects()
    {
        Dimension fieldSize = FIELD_SIZE;
        GameObject[] allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        int robotCount = (HUMAN != null) ? ROBOTS.Count + 1 : ROBOTS.Count;
        List<GameObject> robotGameObjects = new List<GameObject>();
        if (HUMAN != null)
        {
            robotGameObjects.Add(HUMAN.GAME_OBJECT);
            robotGameObjects.Add(HUMAN.getHead().GAME_OBJECT);
            robotGameObjects.Add(HUMAN.getBody().GAME_OBJECT);
            robotGameObjects.Add(HUMAN.getMobility().GAME_OBJECT);
        }
        foreach (Robot robot in ROBOTS)
        {
            robotGameObjects.Add(robot.GAME_OBJECT);
            robotGameObjects.Add(robot.getHead().GAME_OBJECT);
            robotGameObjects.Add(robot.getBody().GAME_OBJECT);
            robotGameObjects.Add(robot.getMobility().GAME_OBJECT);
        }
        foreach (GameObject gameObject in allGameObjects)
        {
            if (gameObject.activeInHierarchy && !robotGameObjects.Contains(gameObject) && !gameObject.name.Contains(CAMERA_NAME_PREFIX))
            {
                Vector3 gameObjectPosition = gameObject.transform.position;
                if (gameObjectPosition.x > fieldSize.width / 2 || gameObjectPosition.x < -fieldSize.width / 2 || gameObjectPosition.y < -fieldSize.height / 2 || gameObjectPosition.z > fieldSize.depth / 2 || gameObjectPosition.x < -fieldSize.depth / 2)
                {
                    int totalProjectiles = projectiles.Count;
                    foreach (Projectile projectile in projectiles)
                    {
                        if (projectile.GAME_OBJECT.Equals(gameObject))
                        {
                            removeProjectile(projectile);
                            break;
                        }
                    }
                    if (totalProjectiles == projectiles.Count && !FIELD.GAME_OBJECTS.Contains(gameObject) && gameObject.GetComponent<RectTransform>() == null && new List<Obstacle>(OBSTACLES).FindIndex(obstacle => obstacle.GAME_OBJECT == gameObject) == -1)
                        GameObject.Destroy(gameObject);
                }
            }
        }
    }

    private void checkRobotsAreWithinBounds()
    {
        List<Robot> robots = getAllRobots();
        foreach (Robot robot in robots)
        {
            Vector3 robotPosition = robot.GAME_OBJECT.transform.position;
            Vector3 robotVelocity = robot.getVelocity().toVector3();
            if (robotPosition.x > POSITION_LIMIT || robotPosition.x < -POSITION_LIMIT)
            {
                robotPosition.x = (robotPosition.x > 0) ? POSITION_LIMIT : -POSITION_LIMIT;
            }
            if (robotPosition.y > POSITION_LIMIT)
            {
                robotPosition.y = (robotPosition.y > 0) ? POSITION_LIMIT : -POSITION_LIMIT;
            }
            if (robotPosition.z > POSITION_LIMIT || robotPosition.z < -POSITION_LIMIT)
            {
                robotPosition.z = (robotPosition.z > 0) ? POSITION_LIMIT : -POSITION_LIMIT;
            }
            robot.setVelocity(new Point(robotVelocity));
            robot.GAME_OBJECT.transform.position = robotPosition;
        }
    }


    private void checkStatusOfRobots()
    {
        int activeRobots = getActiveRobots();
        if (((HUMAN != null) && (!HUMAN.isActive())) || ((HUMAN == null) && (!ROBOTS[0].isActive())))
        {
            gamePlay = false;
            endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            displaySummaryCard(false, activeRobots);
        }
        else if (((HUMAN != null) && HUMAN.isActive() && (activeRobots == 0)) || ((HUMAN == null) && ROBOTS[0].isActive() && (activeRobots == 1)))
        {
            gamePlay = false;
            endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (HUMAN != null)
                displaySummaryCard(true, activeRobots);
        }
    }

    private int getActiveRobots()
    {
        int activeRobots = 0;
        foreach (Robot robot in ROBOTS)
            if (robot.isActive())
                ++activeRobots;
        return activeRobots;
    }

    private void displaySummaryCard(bool status, int activeRobots)
    {
        SUMMARY_CARD.SetActive(true);
        SUMMARY_CARD.transform.SetAsLastSibling();
        STATUS.GetComponent<TextMeshProUGUI>().text += (status ? WON_TEXT : LOST_TEXT);
        DAMAGE_RECEIVED.GetComponent<TextMeshProUGUI>().text += (STARTING_DURABILITY - HUMAN.getRemainingDurability()).ToString();
        DAMAGE_DEALT.GetComponent<TextMeshProUGUI>().text += HUMAN.getDamageDealt().ToString();
        ROBOTS_DEFEATED.GetComponent<TextMeshProUGUI>().text += (ROBOTS.Count - activeRobots).ToString();
        TIME_ELAPSED.GetComponent<TextMeshProUGUI>().text += (endTime - startTime).ToString() + " ms";
        CREDITS_EARNED.GetComponent<TextMeshProUGUI>().text += PERFORMANCE_METRIC_CALCULATOR.calculatePerformanceMetric(HUMAN.getDamageDealt() - (STARTING_DURABILITY - HUMAN.getRemainingDurability()), HUMAN.getDamageDealt(), endTime - startTime, PREVIOUS_DAMAGE_DIFFERENCE, PREVIOUS_MAX_DAMAGE_DIFFERENCE, PREVIOUS_TIME_ELAPSED).ToString();
    }

    private void checkStatusOfAttachments()
    {
        int robotIndex = 0;
        Robot robot = null;
        List<Robot> allRobots = new List<Robot>();
        if (HUMAN != null)
            allRobots.Add(HUMAN);
        allRobots.AddRange(ROBOTS);
        foreach (List<Attachment> attachments in attachmentsUsedByRobots)
        {
            robot = allRobots[robotIndex++];
            foreach (Attachment attachment in attachments)
            {
                if (attachment.canUse())
                    robot = attachment.use(robot);
                else attachments.Remove(attachment);
            }
        }
    }

    public void updateFieldPeripheral()
    {
        if (gamePlay)
            FIELD_PERIPHERAL.update(settingPairs);
        GAME_PLAY_MENU.update(settingPairs);
    }

    private void updateCursor()
    {
        Cursor.visible = !gamePlay;
        if (gamePlay)
            Cursor.lockState = UnityEngine.CursorLockMode.Locked;
        else Cursor.lockState = UnityEngine.CursorLockMode.None;
    }

    private void checkMenuButtonStates()
    {
        bool[] menuButtonStates = GAME_PLAY_MENU.getButtonStates();
        if (menuButtonStates[(int)GamePlayMenu.BUTTONS.BACK_TO_GAME])
            triggerAction(HUMAN, ACTIONS.MENU_TOGGLE_OFF);
        if (menuButtonStates[(int)GamePlayMenu.BUTTONS.SETTINGS])
            openSettings = true;
        if (menuButtonStates[(int)GamePlayMenu.BUTTONS.QUIT_TO_BUILD_HUB])
        {
            goToBuildHub = true;
            triggerAction(HUMAN, ACTIONS.MENU_TOGGLE_OFF);
            destroyAllGeneratedObjects();
        }
        GAME_PLAY_MENU.resetButtonStates();
    }

    private void verifyObstaclePositions()
    {
        foreach (Obstacle obstacle in OBSTACLES)
        {
            if (obstacle.GAME_OBJECT != null)
            {
                Vector3 obstacleCurrentPosition = obstacle.GAME_OBJECT.transform.position;
                Point obstacleCorrectPosition = obstacle.getPosition();
                Vector3 obstacleCorrectPositionVector = obstacleCorrectPosition.toVector3();
                if (obstacleCurrentPosition.x != obstacleCorrectPositionVector.x || obstacleCurrentPosition.y != obstacleCorrectPositionVector.y || obstacleCurrentPosition.z != obstacleCorrectPositionVector.z)
                    obstacle.GAME_OBJECT.transform.position = obstacleCorrectPositionVector;
            }
        }
    }

    private void checkProjectileGarbageBin()
    {
        for (int projectileIndex = 0; projectileIndex < projectileGarbageBin.Count; ++projectileIndex)
        {
            if (!projectileGarbageBin[projectileIndex].hasLifetimeStarted())
                projectileGarbageBin[projectileIndex].startLifetime();
            if (projectileGarbageBin[projectileIndex].isLifetimeOver())
            {
                if (projectiles.Contains(projectileGarbageBin[projectileIndex]))
                    projectiles.Remove(projectileGarbageBin[projectileIndex]);
                GameObject.Destroy(projectileGarbageBin[projectileIndex].GAME_OBJECT);
                projectileGarbageBin.Remove(projectileGarbageBin[projectileIndex]);
            }
        }
    }

    public void update(SettingPairs settingPairs)
    {
        this.settingPairs = settingPairs;
        verifyObstaclePositions();
        if (BACK_TO_WORKSHOP != null && BACK_TO_WORKSHOP.GetComponent<ButtonListener>().isClicked())
        {
            goToBuildHub = true;
            BACK_TO_WORKSHOP.GetComponent<ButtonListener>().resetClick();
        }
        if (gamePlay)
        {
            if (HUMAN != null)
            {
                mousePressed();
                mouseReleased();
                mouseMoved();
                mouseWheelMoved();
            }
            checkProjectileCollisions();
            checkProjectileGarbageBin();
            clearOutsideGameObjects();
            checkRobotsAreWithinBounds();
            checkStatusOfRobots();
            checkStatusOfAttachments();
        }
        keyPressed();
        keyReleased();
        updateCursor();
        checkMenuButtonStates();
        if (openSettings)
        {
            updateSettings();
            if (BACK_TO_WORKSHOP.GetComponent<UnityEngine.UI.Image>().color != colorScheme)
                BACK_TO_WORKSHOP.GetComponent<UnityEngine.UI.Image>().color = colorScheme;
        }
    }

    public bool isRoundOver()
    {
        if ((HUMAN == null) && (getActiveRobots() == 1))
            return true;
        if ((BACK_TO_WORKSHOP != null) && BACK_TO_WORKSHOP.GetComponent<ButtonListener>().isClicked())
        {
            goToBuildHub = true;
            BACK_TO_WORKSHOP.GetComponent<ButtonListener>().resetClick();
        }
        return endTime > 0 && goToBuildHub;
    }

    public double[] getRoundOverData()
    {
        double damageDealt = 0;
        Robot firstRobot = (HUMAN != null) ? HUMAN : ROBOTS[0];
        Robot[] otherRobots = (HUMAN != null) ? ROBOTS.ToArray() : new Robot[ROBOTS.Count - 1];
        if (HUMAN == null)
            damageDealt = ROBOTS[0].getDamageDealt();
        else damageDealt = HUMAN.getDamageDealt();
        double damageReceived = (HUMAN != null ? STARTING_DURABILITY - HUMAN.getRemainingDurability() : firstRobot.getDurability() - firstRobot.getRemainingDurability());
        return new double[] { damageDealt - damageReceived, damageDealt, endTime - startTime };
    }

    public Robot getBestNonHumanRobot()
    {
        Robot bestRobot = null;
        double roundDamageDifference = int.MinValue;
        foreach (Robot robot in ROBOTS)
        {
            if (!robot.isHuman() && ((robot.getDamageDealt() - (robot.getDurability() - robot.getRemainingDurability())) > roundDamageDifference))
            {
                bestRobot = robot;
                roundDamageDifference = robot.getDamageDealt() - (robot.getDurability() - robot.getRemainingDurability());
            }
        }
        return bestRobot;
    }

    public double getDamageDealt(Robot robot)
    {
        return robot.getDamageDealt();
    }

    public bool canSeeAllOpponents(Robot robot)
    {
        int robotIndex = 0;
        if (HUMAN == null || robot != HUMAN)
            robotIndex = ROBOTS.IndexOf(robot);
        Camera camera = ROBOT_CAMERAS[robotIndex];
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        Collider collider = null;
        float distanceToRobot = 0f;
        float hitDistance = default;
        if (HUMAN != null && robot != HUMAN)
        {
            distanceToRobot = Vector3.Distance(camera.transform.position, HUMAN.GAME_OBJECT.transform.position);
            hitDistance = getDistanceToObstruction(camera);
            collider = HUMAN.GAME_OBJECT.GetComponent<Collider>();
            return hitDistance != Mathf.Infinity && HUMAN.isVisible() && GeometryUtility.TestPlanesAABB(planes, collider.bounds) && distanceToRobot == hitDistance;
        }
        foreach (Robot otherRobot in ROBOTS)
        {
            if (otherRobot != robot)
            {
                distanceToRobot = Vector3.Distance(camera.transform.position, otherRobot.GAME_OBJECT.transform.position);
                hitDistance = getDistanceToObstruction(camera);
                collider = otherRobot.GAME_OBJECT.GetComponent<Collider>();
                return hitDistance != Mathf.Infinity && otherRobot.isVisible() && GeometryUtility.TestPlanesAABB(planes, collider.bounds) && distanceToRobot == hitDistance;
            }
        }
        return true;
    }

    private float getDistanceToObstruction(Camera camera)
    {
        int layerMask = 1 << 2;
        layerMask = ~layerMask;
        RaycastHit hit;
        float hitDistance = 0f;
        if (Physics.Raycast(camera.transform.position + camera.transform.TransformDirection(Vector3.forward) * (float)Robot.SCALE, camera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
            hitDistance = hit.distance;
        return hitDistance;
    }

    public bool isGamePlay()
    {
        return gamePlay;
    }

    public bool getOpenSettings()
    {
        return openSettings;
    }

    public void closeSettings()
    {
        openSettings = false;
    }

    public bool getGoToBuildHub()
    {
        return goToBuildHub;
    }

    public void destroyAllGeneratedObjects()
    {
        GameObject[] allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        for (int gameObjectIndex = 0; gameObjectIndex < allGameObjects.Length; ++gameObjectIndex)
            if (allGameObjects[gameObjectIndex].layer == UI_LAYER && allGameObjects[gameObjectIndex].name != CANVAS_NAME)
                GameObject.Destroy(allGameObjects[gameObjectIndex]);
        if (HUMAN != null)
        {
            GameObject.Destroy(HUMAN.GAME_OBJECT);
            GameObject.Destroy(HUMAN.getHead().GAME_OBJECT);
            GameObject.Destroy(HUMAN.getBody().GAME_OBJECT);
            GameObject.Destroy(HUMAN.getMobility().GAME_OBJECT);
        }
        for (int robotIndex = 0; robotIndex < ROBOTS.Count; ++robotIndex)
        {
            GameObject.Destroy(ROBOTS[robotIndex].GAME_OBJECT);
            GameObject.Destroy(ROBOTS[robotIndex].getHead().GAME_OBJECT);
            GameObject.Destroy(ROBOTS[robotIndex].getBody().GAME_OBJECT);
            GameObject.Destroy(ROBOTS[robotIndex].getMobility().GAME_OBJECT);
        }
        for (int obstacleIndex = 0; obstacleIndex < OBSTACLES.Length; ++obstacleIndex)
            GameObject.Destroy(OBSTACLES[obstacleIndex].GAME_OBJECT);
        for (int projectileIndex = 0; projectileIndex < projectileGarbageBin.Count; ++projectileIndex)
        {
            if (projectiles.Contains(projectileGarbageBin[projectileIndex]))
                projectiles.Remove(projectileGarbageBin[projectileIndex]);
            GameObject.Destroy(projectileGarbageBin[projectileIndex].GAME_OBJECT);
            projectileGarbageBin.Remove(projectileGarbageBin[projectileIndex]);
        }
    }

    public Robot getHumanRobot()
    {
        return HUMAN;
    }
}