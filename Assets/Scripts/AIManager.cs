using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;

public class AIManager
{
    private readonly string AGENT_PREFIX = "AI Agent ";
    private readonly double DISCOUNT_FACTOR = .9, LEARNING_RATE = .9, INVERSE_SENSITIVITY = .33;
    private readonly int NUMBER_OF_AGENTS, NUMBER_OF_ENEMIES, NUMBER_OF_OBSTACLES, STUCK_MAX_TURN_COUNT = 10;
    private readonly AIAgent[] AI_AGENTS;
    private readonly Robot[] HUMAN_ROBOTS;
    private readonly Part[] PARTS, BASE_PARTS;
    private readonly Obstacle[] OBSTACLES;
    private readonly Dimension FIELD_SIZE;
    private readonly BuildHubState[] PRIOR_BUILD_HUB_STATES;
    private readonly FieldState[] PRIOR_FIELD_STATES;
    private readonly BuildHubAction[] BUILD_HUB_ACTIONS_TO_PERFORM;
    private readonly FieldAction[] FIELD_ACTIONS_TO_PERFORM;
    private readonly Camera[] ROBOT_CAMERAS;
    private Thread actionThread;
    private readonly double[] PRIOR_DISTANCE_FROM_ENEMY, PRIOR_DURABILITY_REMAINING, PRIOR_DAMAGE_DEALT;
    private readonly int[] STUCK_IN_STATE_FOR_TURN_COUNT;
    private readonly System.Random RANDOM;
    private readonly Point[] AGENT_POSITIONS;
    private readonly Point[][] AGENT_ENEMY_POSITIONS;
    private bool EFFECT_ON_OBSTACLE;
    private int STEEPEST_OBSTACLE_ANGLE;
    private double HIGHEST_FRICTION;
    private readonly double BUILD_HUB_EXPLORATION_RATE_ACTION_WEIGHT = .33;
    private readonly List<BuildHubState> BUILD_HUB_STATES;
    private readonly PerformanceMetricCalculator PERFORMANCE_METRIC_CALCULATOR;
    private long CREDITS;
    private bool actionsUpdated;

    public AIManager(int numberOfAgents, int numberOfEnemies, Robot[] humanRobots, Part[] parts, Part[] baseParts, Obstacle[] obstacles, Dimension fieldSize, List<BuildHubState> buildHubStates, List<FieldState> fieldStates, long experience)
    {
        NUMBER_OF_AGENTS = numberOfAgents;
        NUMBER_OF_ENEMIES = numberOfEnemies;
        HUMAN_ROBOTS = humanRobots;
        PARTS = parts;
        BASE_PARTS = baseParts;
        OBSTACLES = obstacles;
        NUMBER_OF_OBSTACLES = OBSTACLES.Length;
        FIELD_SIZE = fieldSize;
        CREDITS = experience;
        PERFORMANCE_METRIC_CALCULATOR = new PerformanceMetricCalculator();
        AI_AGENTS = new AIAgent[NUMBER_OF_AGENTS];
        PRIOR_BUILD_HUB_STATES = new BuildHubState[NUMBER_OF_AGENTS];
        PRIOR_FIELD_STATES = new FieldState[NUMBER_OF_AGENTS];
        BUILD_HUB_ACTIONS_TO_PERFORM = new BuildHubAction[NUMBER_OF_AGENTS];
        FIELD_ACTIONS_TO_PERFORM = new FieldAction[NUMBER_OF_AGENTS];
        ROBOT_CAMERAS = new Camera[NUMBER_OF_AGENTS];
        PRIOR_DISTANCE_FROM_ENEMY = new double[NUMBER_OF_ENEMIES];
        PRIOR_DURABILITY_REMAINING = new double[NUMBER_OF_AGENTS];
        PRIOR_DAMAGE_DEALT = new double[NUMBER_OF_AGENTS];
        STUCK_IN_STATE_FOR_TURN_COUNT = new int[NUMBER_OF_AGENTS];
        AGENT_POSITIONS = new Point[NUMBER_OF_AGENTS];
        AGENT_ENEMY_POSITIONS = new Point[NUMBER_OF_AGENTS][];
        for (int agentIndex = 0; agentIndex < NUMBER_OF_AGENTS; ++agentIndex)
            AGENT_ENEMY_POSITIONS[agentIndex] = new Point[NUMBER_OF_ENEMIES];
        RANDOM = new System.Random();
        EFFECT_ON_OBSTACLE = false;
        STEEPEST_OBSTACLE_ANGLE = 0;
        HIGHEST_FRICTION = 0;
        foreach (Obstacle obstacle in OBSTACLES)
        {
            if (obstacle.getEffect() != default)
                EFFECT_ON_OBSTACLE = true;
            if (obstacle.getSlopeAngle() > STEEPEST_OBSTACLE_ANGLE)
                STEEPEST_OBSTACLE_ANGLE = obstacle.getSlopeAngle();
            if (obstacle.getFriction() > HIGHEST_FRICTION)
                HIGHEST_FRICTION = obstacle.getFriction();
        }
        BUILD_HUB_STATES = buildHubStates;
        if (BUILD_HUB_STATES == default)
            BUILD_HUB_STATES = new List<BuildHubState>();
        startAgents(fieldStates);
        actionsUpdated = false;
        actionThread = new Thread(updateActions);
        actionThread.Start();
    }

    private void startAgents(List<FieldState> fieldStates)
    {
        for (int agentIndex = 0; agentIndex < NUMBER_OF_AGENTS; ++agentIndex)
            AI_AGENTS[agentIndex] = new AIAgent(generateRobot(agentIndex + 1), BUILD_HUB_STATES, fieldStates, DISCOUNT_FACTOR, LEARNING_RATE/*, PROB_OF_EXPLOITATION*/, INVERSE_SENSITIVITY);
        for (int agentIndex = 0; agentIndex < NUMBER_OF_AGENTS; ++agentIndex)
        {
            List<Robot> enemies = new List<Robot>();
            List<AIAgent> aiAgents = new List<AIAgent>();
            aiAgents.AddRange(AI_AGENTS);
            aiAgents.RemoveAt(agentIndex);
            enemies.AddRange(aiAgents.ConvertAll(new Converter<AIAgent, Robot>(agentToRobot)));
            if (HUMAN_ROBOTS != null && HUMAN_ROBOTS.Length > 0)
                enemies.AddRange(HUMAN_ROBOTS);
            PRIOR_FIELD_STATES[agentIndex] = initializeState(null, null, AI_AGENTS[agentIndex].getBot(), enemies.ToArray(), agentIndex);
        }
    }

    private static Robot agentToRobot(AIAgent agent)
    {
        return agent.getBot();
    }

    private Camera findCameraForRobot(Robot robot)
    {
        foreach (Transform transform in robot.getHead().GAME_OBJECT.transform)
        {
            GameObject gameObject = transform.gameObject;
            if (gameObject.name.Contains("Camera"))
                return gameObject.GetComponent<Camera>() as Camera;
        }
        return Camera.main;
    }

    private FieldState initializeState(FieldState priorState, FieldAction priorAction, Robot bot, Robot[] enemies, int agentIndex)
    {
        FieldState state = new FieldState();
        state.enemiesKilled = new bool[NUMBER_OF_ENEMIES];
        state.enemiesPosition = new Point[NUMBER_OF_ENEMIES];
        state.enemiesCanHit = new bool[NUMBER_OF_ENEMIES];
        int numberOfAttachments = 0;
        Camera camera = ROBOT_CAMERAS[agentIndex];
        if (camera == default)
        {
            camera = findCameraForRobot(bot);
            ROBOT_CAMERAS[agentIndex] = camera;
        }
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        Collider collider = null;
        AGENT_POSITIONS[agentIndex] = new Point(bot.GAME_OBJECT.transform.position);
        if (priorState == null)
        {
            state.killed = bot.getRemainingDurability() <= 0;
            state.enemiesTouchingObstacles = new bool[NUMBER_OF_ENEMIES][];
            if (enemies.Length > 0)
            {
                for (int enemyIndex = 0; enemyIndex < NUMBER_OF_ENEMIES; ++enemyIndex)
                {
                    state.enemiesKilled[enemyIndex] = enemies[enemyIndex].getRemainingDurability() <= 0;
                    AGENT_ENEMY_POSITIONS[agentIndex][enemyIndex] = new Point(enemies[enemyIndex].GAME_OBJECT.transform.position);
                    state.enemiesPosition[enemyIndex] = generateLocationRelativeToCrosshair(bot, enemies[enemyIndex], camera, AGENT_ENEMY_POSITIONS[agentIndex][enemyIndex]);
                    state.enemiesTouchingObstacles[enemyIndex] = new bool[NUMBER_OF_OBSTACLES];
                }
            }
            state.timeSinceLastAction = 0;
            Attachment[] attachments = bot.getAttachments().FindAll(attachment => !attachment.isPassive()).ToArray();
            state.attachments = attachments;
            state.enemiesLowDurability = new bool[NUMBER_OF_ENEMIES];
            state.canSeeEnemies = new bool[NUMBER_OF_ENEMIES];
            state.touchingObstacles = new bool[NUMBER_OF_OBSTACLES];
            state.numberOfEnemies = NUMBER_OF_ENEMIES;
            numberOfAttachments = attachments.Length;
            state.attachmentsCharging = new bool[numberOfAttachments];
            state.attachmentsCooling = new bool[numberOfAttachments];
        }
        else
        {
            state.damageReceived = (PRIOR_DURABILITY_REMAINING[agentIndex] - bot.getRemainingDurability()) > 0 ? true : false;
            PRIOR_DURABILITY_REMAINING[agentIndex] = bot.getRemainingDurability();
            state.damageDealt = (bot.getDamageDealt() - PRIOR_DAMAGE_DEALT[agentIndex]) != 0 ? true : false;
            PRIOR_DAMAGE_DEALT[agentIndex] = bot.getDamageDealt();
            state.killed = (bot.getRemainingDurability() <= 0) != priorState.killed;
            state.enemiesTouchingObstacles = priorState.enemiesTouchingObstacles;
            if (enemies.Length > 0)
            {
                for (int enemyIndex = 0; enemyIndex < NUMBER_OF_ENEMIES; ++enemyIndex)
                {
                    state.enemiesKilled[enemyIndex] = (enemies[enemyIndex].getRemainingDurability() <= 0) != priorState.enemiesKilled[enemyIndex];
                    AGENT_ENEMY_POSITIONS[agentIndex][enemyIndex] = new Point(enemies[enemyIndex].GAME_OBJECT.transform.position);
                    state.enemiesPosition[enemyIndex] = generateLocationRelativeToCrosshair(bot, enemies[enemyIndex], camera, AGENT_ENEMY_POSITIONS[agentIndex][enemyIndex]);
                }
            }
            if (priorAction == null)
                state.timeSinceLastAction = 0;
            else if (priorAction.pickAttachment == null
                && !priorAction.moveUp
                && !priorAction.moveRight
                && !priorAction.moveDown
                && !priorAction.moveLeft
                && !priorAction.rotateClockwise
                && !priorAction.rotateCounterClockwise
                && !priorAction.lookUp
                && !priorAction.lookDown
                && !priorAction.chargeAttachment
                && !priorAction.useAttachment)
                ++state.timeSinceLastAction;
            else state.timeSinceLastAction = 0;
            state.attachments = priorState.attachments;
            state.enemiesLowDurability = priorState.enemiesLowDurability;
            state.canSeeEnemies = priorState.canSeeEnemies;
            state.touchingObstacles = priorState.touchingObstacles;
            state.numberOfEnemies = NUMBER_OF_ENEMIES;
            numberOfAttachments = state.attachments.Length;
            state.attachmentsCharging = priorState.attachmentsCharging;
            state.attachmentsCooling = priorState.attachmentsCooling;
        }
        state.lowDurability = bot.getRemainingDurability() / bot.getDurability() <= .3f;
        Point botPosition = new Point(bot.GAME_OBJECT.transform.position);
        Dimension botSize = bot.getSize();
        state.touchingWall = botPosition.x >= (FIELD_SIZE.width / 2 - botSize.width) || botPosition.x <= (-FIELD_SIZE.width / 2 + botSize.width)
                            || botPosition.z >= (FIELD_SIZE.depth / 2 - botSize.depth) || botPosition.z <= (-FIELD_SIZE.depth / 2 + botSize.width);
        if (enemies.Length > 0)
        {
            for (int enemyIndex = 0; enemyIndex < NUMBER_OF_ENEMIES; ++enemyIndex)
            {
                state.enemiesLowDurability[enemyIndex] = enemies[enemyIndex].getRemainingDurability() <= .3f;
                state.canSeeEnemies[enemyIndex] = enemies[enemyIndex].isVisible();
                if (state.canSeeEnemies[enemyIndex])
                {
                    float distanceToRobot = 0f;
                    float hitDistance = default;
                    distanceToRobot = Vector3.Distance(camera.transform.position, AGENT_ENEMY_POSITIONS[agentIndex][enemyIndex].toVector3());
                    hitDistance = getDistanceToObstruction(camera);
                    collider = enemies[enemyIndex].GAME_OBJECT.GetComponent<Collider>();
                    state.canSeeEnemies[enemyIndex] = hitDistance != Mathf.Infinity && enemies[enemyIndex].isVisible() && GeometryUtility.TestPlanesAABB(planes, collider.bounds) && distanceToRobot == hitDistance;
                }
                state.enemiesCanHit[enemyIndex] = canHit(enemies[enemyIndex], bot, camera, AGENT_POSITIONS[agentIndex]);
                for (int obstacleIndex = 0; obstacleIndex < NUMBER_OF_OBSTACLES; ++obstacleIndex)
                {
                    List<Collider> collidersTouchingObstacle = OBSTACLES[obstacleIndex].getCollidersTouching();
                    Collider robotCollider = enemies[enemyIndex].GAME_OBJECT.GetComponent<Collider>();
                    Collider robotHeadCollider = enemies[enemyIndex].getHead().GAME_OBJECT.GetComponent<Collider>();
                    Collider robotBodyCollider = enemies[enemyIndex].getBody().GAME_OBJECT.GetComponent<Collider>();
                    Collider robotMobilityCollider = enemies[enemyIndex].getMobility().GAME_OBJECT.GetComponent<Collider>();
                    state.enemiesTouchingObstacles[enemyIndex][obstacleIndex] = collidersTouchingObstacle.Contains(robotCollider) || collidersTouchingObstacle.Contains(robotHeadCollider) || collidersTouchingObstacle.Contains(robotBodyCollider) || collidersTouchingObstacle.Contains(robotMobilityCollider);
                }
            }
        }
        for (int obstacleIndex = 0; obstacleIndex < NUMBER_OF_OBSTACLES; ++obstacleIndex)
        {
            List<Collider> collidersTouchingObstacle = OBSTACLES[obstacleIndex].getCollidersTouching();
            Collider robotCollider = bot.GAME_OBJECT.GetComponent<Collider>();
            Collider robotHeadCollider = bot.getHead().GAME_OBJECT.GetComponent<Collider>();
            Collider robotBodyCollider = bot.getBody().GAME_OBJECT.GetComponent<Collider>();
            Collider robotMobilityCollider = bot.getMobility().GAME_OBJECT.GetComponent<Collider>();
            state.touchingObstacles[obstacleIndex] = collidersTouchingObstacle.Contains(robotCollider) || collidersTouchingObstacle.Contains(robotHeadCollider) || collidersTouchingObstacle.Contains(robotBodyCollider) || collidersTouchingObstacle.Contains(robotMobilityCollider);
        }
        Attachment[] attachmentsInState = state.attachments;
        numberOfAttachments = attachmentsInState.Length;
        for (int attachmentIndex = 0; attachmentIndex < numberOfAttachments; ++attachmentIndex)
        {
            state.attachmentsCharging[attachmentIndex] = attachmentsInState[attachmentIndex].getElapsedChargeTime() < attachmentsInState[attachmentIndex].getMaxChargeTime();
            state.attachmentsCooling[attachmentIndex] = !attachmentsInState[attachmentIndex].isCooled();
        }
        return state;
    }

    private bool canHit(Robot sourceRobot, Robot targetRobot, Camera camera, Point targetPosition)
    {
        if (!targetRobot.isVisible())
            return false;
        Point locationRelativeToCrosshair = generateLocationRelativeToCrosshair(sourceRobot, targetRobot, camera, targetPosition);
        return locationRelativeToCrosshair.x == 0 && locationRelativeToCrosshair.y == 0 && locationRelativeToCrosshair.z == 0;
    }

    private Point generateLocationRelativeToCrosshair(Robot robot, Robot enemy, Camera camera, Point enemyPosition)
    {
        Vector3 cameraVectorPosition = camera.transform.position, cameraVectorAngle = camera.transform.eulerAngles;
        cameraVectorAngle.y -= 90;
        int rotationCount = Math.Abs((int)(cameraVectorAngle.y / 360));
        double cameraAngle = cameraVectorAngle.y >= 0 ? 1 : -1;
        cameraAngle *= Math.Abs(cameraVectorAngle.y) - (rotationCount * 360);
        if (cameraAngle < 0)
            cameraAngle += 360;
        cameraAngle = 360 - cameraAngle;
        Point location = new Point(), cameraPosition = new Point(cameraVectorPosition), enemyDistanceFromCameraPolar = new Point();
        double xDistance = enemyPosition.x - cameraPosition.x;
        double zDistance = enemyPosition.z - cameraPosition.z;
        double mag = Math.Sqrt(xDistance * xDistance + zDistance * zDistance);
        double ang = Math.Atan2(zDistance, xDistance) * 180 / Math.PI;
        if (ang < 0)
            ang += 360;
        ang -= cameraAngle;
        if (ang < 0)
            ang += 360;
        double hDistance = mag * Math.Cos(ang * Math.PI / 180);
        double vDistance = mag * Math.Sin(ang * Math.PI / 180);
        enemyDistanceFromCameraPolar.x = mag;
        enemyDistanceFromCameraPolar.y = ang;
        double lookUpAngle = -camera.transform.eulerAngles.x;
        if (lookUpAngle < 0)
            lookUpAngle += 360;
        double projectedHeight = enemyDistanceFromCameraPolar.x * Math.Tan(lookUpAngle * Math.PI / 180) + cameraPosition.y;
        double projectedAngle = Math.Atan(robot.getSize().width / 2 / enemyDistanceFromCameraPolar.x) * 180 / Math.PI;
        if (projectedHeight < FIELD_SIZE.height / 2)
            location.x = 1;
        else if (projectedHeight > cameraPosition.y)
            location.x = -1;
        else location.x = 0;
        if (enemyDistanceFromCameraPolar.y < (360 - projectedAngle) && enemyDistanceFromCameraPolar.y > 180)
            location.y = 1;
        else if (enemyDistanceFromCameraPolar.y > projectedAngle && enemyDistanceFromCameraPolar.y <= 180)
            location.y = -1;
        else location.y = 0;
        return location;
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

    private Robot generateRobot(int agentNumber)
    {
        List<Part> parts = new List<Part>();
        int fastestCooling = int.MaxValue, fastestCharging = int.MaxValue, maxAttachments = 0;
        double highestClimbAngle = 0, durability = 0, weight = 0, highestSpeed = 0, highestDamage = 0, maxForce = 0, credits = CREDITS;
        bool invisible = false, hasEffect = false;
        BuildHubState priorBuildHubState = new BuildHubState();
        BuildHubAction priorBuildHubAction = new BuildHubAction();
        foreach (Part part in PARTS)
        {
            BuildHubState buildHubState = new BuildHubState();
            buildHubState.effectOnObstacle = EFFECT_ON_OBSTACLE;
            buildHubState.canClimbObstacle = highestClimbAngle >= STEEPEST_OBSTACLE_ANGLE;
            buildHubState.canMoveOnObstacle = maxForce >= weight && highestSpeed >= HIGHEST_FRICTION * weight;
            buildHubState.durable = durability > 0 && durability >= highestDamage * 2;
            buildHubState.invisible = invisible;
            buildHubState.fast = maxForce >= weight && highestSpeed > 0 && highestSpeed >= weight * 2;
            buildHubState.hasEffect = hasEffect;
            buildHubState.heavyDamage = highestDamage > 0 && highestDamage >= durability / 4;
            buildHubState.quickCooling = fastestCooling <= 1000;
            buildHubState.quickCharging = fastestCharging <= 1000;
            if ((BUILD_HUB_STATES.Count == 0) || !BUILD_HUB_STATES.Exists(s => s.Compare(buildHubState) == 0))
            {
                List<Action> buildHubActionsForState = default;
                if ((BUILD_HUB_STATES.Count == 0) || (RANDOM.NextDouble() < priorBuildHubState.explorationRate))
                    buildHubActionsForState = generateBuildHubActions();
                else buildHubActionsForState = getClosestBuildHubState(buildHubState).getActions();
                buildHubState.setActions(buildHubActionsForState);
                BUILD_HUB_STATES.Add(buildHubState);
            }
            else buildHubState = BUILD_HUB_STATES.Find(s => s.Compare(buildHubState) == 0);
            if ((buildHubState.explorationRate > 0) && (RANDOM.NextDouble() < buildHubState.explorationRate))
            {
                buildHubState.indexOfSelectedAction = RANDOM.Next(buildHubState.getActions().Count);
                priorBuildHubAction = (BuildHubAction)buildHubState.getActions()[buildHubState.indexOfSelectedAction];
            }
            else
            {
                BuildHubAction optimalAction = (BuildHubAction)buildHubState.getActions()[0];
                for (int actionIndex = 1; actionIndex < buildHubState.getActions().Count; ++actionIndex)
                {
                    Action action = buildHubState.getActions()[actionIndex];
                    if (action.qValue > optimalAction.qValue)
                    {
                        optimalAction = (BuildHubAction)action;
                        buildHubState.indexOfSelectedAction = actionIndex;
                    }
                }
                priorBuildHubAction = optimalAction;
            }
            if (priorBuildHubAction.equipt)
            {
                int costNewPart = PERFORMANCE_METRIC_CALCULATOR.calculateCost(part);
                if (!parts.Exists(p => p.GetType() == part.GetType()))
                {
                    if ((!(part is Attachment) || ((part is Attachment) && (maxAttachments > 0))) && ((weight + part.getWeight()) < maxForce) && (credits >= costNewPart))
                    {
                        credits -= costNewPart;
                        --maxAttachments;
                        weight += part.getWeight();
                        durability += part.getDurability();
                        if (part is Body && (maxAttachments < ((Body)part).getMaxAttachments()))
                            maxAttachments = ((Body)part).getMaxAttachments();
                        if (part is Mobility)
                        {
                            highestClimbAngle = ((Mobility)part).getClimbAngle();
                            highestSpeed = ((Mobility)part).getMaxSpeed();
                            maxForce = ((Mobility)part).getMaxForce();
                        }
                        parts.Add(part);
                        if (part is Attachment)
                        {
                            if (fastestCooling > ((Attachment)part).getMinCoolingTime())
                                fastestCooling = ((Attachment)part).getMinCoolingTime();
                            if (fastestCharging > ((Attachment)part).getMinChargeTime())
                                fastestCharging = ((Attachment)part).getMinChargeTime();
                            if (highestDamage < ((Attachment)part).getMaxDamage())
                                highestDamage = ((Attachment)part).getMaxDamage();
                            if (!invisible && ((Attachment)part).isInvisible())
                                invisible = true;
                            if (!hasEffect && ((Attachment)part).getEffect() != default)
                                hasEffect = true;
                        }
                    }
                    else buildHubState.indexOfSelectedAction += 2;
                }
                else
                {
                    int partIndex = parts.FindIndex(p => p.GetType() == part.GetType());
                    int costOldPart = PERFORMANCE_METRIC_CALCULATOR.calculateCost(parts[partIndex]);
                    if ((!(part is Attachment) || ((part is Attachment) && (maxAttachments > 0))) && ((weight + part.getWeight()) < maxForce) && ((credits + costOldPart) >= costNewPart))
                    {
                        credits += costOldPart;
                        credits -= costNewPart;
                        --maxAttachments;
                        weight -= parts[partIndex].getWeight();
                        weight += part.getWeight();
                        durability -= parts[partIndex].getDurability();
                        durability += part.getDurability();
                        if (part is Body && (maxAttachments < ((Body)part).getMaxAttachments()))
                            maxAttachments = ((Body)part).getMaxAttachments();
                        if (part is Mobility)
                        {
                            highestClimbAngle = ((Mobility)part).getClimbAngle();
                            highestSpeed = ((Mobility)part).getMaxSpeed();
                            maxForce = ((Mobility)part).getMaxForce();
                        }
                        parts[partIndex] = part;
                        if (part is Attachment)
                        {
                            fastestCooling = int.MaxValue;
                            fastestCharging = int.MaxValue;
                            highestDamage = 0;
                            invisible = false;
                            hasEffect = false;
                            foreach (Part currentPart in parts)
                            {
                                if (currentPart is Attachment)
                                {
                                    if (fastestCooling > ((Attachment)currentPart).getMinCoolingTime())
                                        fastestCooling = ((Attachment)part).getMinCoolingTime();
                                    if (fastestCharging > ((Attachment)currentPart).getMinChargeTime())
                                        fastestCharging = ((Attachment)currentPart).getMinChargeTime();
                                    if (highestDamage < ((Attachment)currentPart).getMaxDamage())
                                        highestDamage = ((Attachment)currentPart).getMaxDamage();
                                    if (!invisible && ((Attachment)currentPart).isInvisible())
                                        invisible = true;
                                    if (!hasEffect && ((Attachment)currentPart).getEffect() != default)
                                        hasEffect = true;
                                }
                            }
                        }
                        buildHubState.indexOfSelectedAction += 1;
                    }
                    else buildHubState.indexOfSelectedAction += 2;
                }

            }
            else if (priorBuildHubAction.replace)
            {
                int costNewPart = PERFORMANCE_METRIC_CALCULATOR.calculateCost(part);
                if (!parts.Exists(p => p.GetType() == part.GetType()))
                {
                    if ((!(part is Attachment) || ((part is Attachment) && (maxAttachments > 0))) && ((weight + part.getWeight()) < maxForce) && (credits >= costNewPart))
                    {
                        credits -= costNewPart;
                        --maxAttachments;
                        weight += part.getWeight();
                        durability += part.getDurability();
                        if (part is Body && (maxAttachments < ((Body)part).getMaxAttachments()))
                            maxAttachments = ((Body)part).getMaxAttachments();
                        if (part is Mobility)
                        {
                            highestClimbAngle = ((Mobility)part).getClimbAngle();
                            highestSpeed = ((Mobility)part).getMaxSpeed();
                            maxForce = ((Mobility)part).getMaxForce();
                        }
                        parts.Add(part);
                        if (part is Attachment)
                        {
                            if (fastestCooling > ((Attachment)part).getMinCoolingTime())
                                fastestCooling = ((Attachment)part).getMinCoolingTime();
                            if (fastestCharging > ((Attachment)part).getMinChargeTime())
                                fastestCharging = ((Attachment)part).getMinChargeTime();
                            if (highestDamage < ((Attachment)part).getMaxDamage())
                                highestDamage = ((Attachment)part).getMaxDamage();
                            if (!invisible && ((Attachment)part).isInvisible())
                                invisible = true;
                            if (!hasEffect && ((Attachment)part).getEffect() != default)
                                hasEffect = true;
                        }
                        buildHubState.indexOfSelectedAction -= 1;
                    }
                    else buildHubState.indexOfSelectedAction += 1;
                }
                else
                {
                    int partIndex = parts.FindIndex(p => p.GetType() == part.GetType());
                    int costOldPart = PERFORMANCE_METRIC_CALCULATOR.calculateCost(parts[partIndex]);
                    if ((!(part is Attachment) || ((part is Attachment) && (maxAttachments > 0))) && ((weight + part.getWeight()) < maxForce) && ((credits + costOldPart) >= costNewPart))
                    {
                        credits += costOldPart;
                        credits -= costNewPart;
                        --maxAttachments;
                        weight -= parts[partIndex].getWeight();
                        weight += part.getWeight();
                        durability -= parts[partIndex].getDurability();
                        durability += part.getDurability();
                        if (part is Body && (maxAttachments < ((Body)part).getMaxAttachments()))
                            maxAttachments = ((Body)part).getMaxAttachments();
                        if (part is Mobility)
                        {
                            highestClimbAngle = ((Mobility)part).getClimbAngle();
                            highestSpeed = ((Mobility)part).getMaxSpeed();
                            maxForce = ((Mobility)part).getMaxForce();
                        }
                        parts[partIndex] = part;
                        if (part is Attachment)
                        {
                            fastestCooling = int.MaxValue;
                            fastestCharging = int.MaxValue;
                            highestDamage = 0;
                            invisible = false;
                            hasEffect = false;
                            foreach (Part currentPart in parts)
                            {
                                if (currentPart is Attachment)
                                {
                                    if (fastestCooling > ((Attachment)currentPart).getMinCoolingTime())
                                        fastestCooling = ((Attachment)part).getMinCoolingTime();
                                    if (fastestCharging > ((Attachment)currentPart).getMinChargeTime())
                                        fastestCharging = ((Attachment)currentPart).getMinChargeTime();
                                    if (highestDamage < ((Attachment)currentPart).getMaxDamage())
                                        highestDamage = ((Attachment)currentPart).getMaxDamage();
                                    if (!invisible && ((Attachment)currentPart).isInvisible())
                                        invisible = true;
                                    if (!hasEffect && ((Attachment)currentPart).getEffect() != default)
                                        hasEffect = true;
                                }
                            }
                        }
                    }
                    else buildHubState.indexOfSelectedAction += 1;
                }
            }
            priorBuildHubState = buildHubState;
        }
        List<Part> baseParts = new List<Part>();
        baseParts.AddRange(BASE_PARTS);
        if (!parts.Exists(p => p is Head))
            parts.Add(baseParts.Find(p => p is Head));
        if (!parts.Exists(p => p is Body))
            parts.Add(baseParts.Find(p => p is Body));
        if (!parts.Exists(p => p is Mobility))
            parts.Add(baseParts.Find(p => p is Mobility));
        if (!parts.Exists(p => (p is Attachment) && ((Attachment)p).isWeapon()))
            parts.Add(baseParts.Find(p => p is Blaster));
        for (int partIndex = 0; partIndex < parts.Count; ++partIndex)
            parts[partIndex] = parts[partIndex].clone(false);
        return new Robot(AGENT_PREFIX + agentNumber, false, parts.ToArray());
    }

    private List<Action> generateBuildHubActions()
    {
        List<Action> actions = new List<Action>();
        BuildHubAction action1 = new BuildHubAction();
        BuildHubAction action2 = new BuildHubAction();
        BuildHubAction action3 = new BuildHubAction();
        action1.equipt = true;
        action2.replace = true;
        actions.Add(action1);
        actions.Add(action2);
        actions.Add(action3);
        return actions;
    }

    private BuildHubState getClosestBuildHubState(BuildHubState state)
    {
        BuildHubState closestState = BUILD_HUB_STATES[0];
        double closestRank = state.Compare(closestState);
        foreach (BuildHubState buildHubState in BUILD_HUB_STATES)
        {
            double rank = state.Compare(buildHubState);
            if (rank < closestRank)
            {
                closestState = buildHubState;
                closestRank = rank;
            }
        }
        return closestState;
    }

    public void update()
    {
        for (int agentIndex = 0; agentIndex < NUMBER_OF_AGENTS; ++agentIndex)
        {
            AIAgent agent = AI_AGENTS[agentIndex];
            List<Robot> enemies = new List<Robot>();
            List<AIAgent> aiAgents = new List<AIAgent>();
            aiAgents.AddRange(AI_AGENTS);
            aiAgents.RemoveAt(agentIndex);
            enemies.AddRange(aiAgents.ConvertAll(new Converter<AIAgent, Robot>(agentToRobot)));
            if (HUMAN_ROBOTS != null && HUMAN_ROBOTS.Length > 0)
                enemies.AddRange(HUMAN_ROBOTS);
            FieldState state = initializeState(PRIOR_FIELD_STATES[agentIndex], FIELD_ACTIONS_TO_PERFORM[agentIndex], agent.getBot(), enemies.ToArray(), agentIndex);
            double reward = 0;
            reward -= state.damageReceived ? 2 : 0;
            reward += state.damageDealt ? 2 : 0;
            for (int enemyIndex = 0; enemyIndex < state.numberOfEnemies; ++enemyIndex)
            {
                if (state.enemiesKilled != null)
                    reward += state.enemiesKilled[enemyIndex] ? 10 : 0;
                if (state.enemiesPosition != null && state.enemiesPosition[enemyIndex].x == 0 && state.enemiesPosition[enemyIndex].y == 0 && state.enemiesPosition[enemyIndex].z == 0)
                    reward += 50;
                if (state.enemiesCanHit != null)
                    reward -= state.enemiesCanHit[enemyIndex] ? 1 : 0;
                double distanceFromEnemy = Vector3.Distance(AGENT_POSITIONS[agentIndex].toVector3(), AGENT_ENEMY_POSITIONS[agentIndex][enemyIndex].toVector3());
                reward += 1000 * (PRIOR_DISTANCE_FROM_ENEMY[enemyIndex] - distanceFromEnemy);
                PRIOR_DISTANCE_FROM_ENEMY[enemyIndex] = distanceFromEnemy;
                if (FIELD_ACTIONS_TO_PERFORM[agentIndex] != null)
                {
                    if (PRIOR_FIELD_STATES[agentIndex].enemiesPosition[enemyIndex].x == 1 && FIELD_ACTIONS_TO_PERFORM[agentIndex].rotateClockwise)
                        reward += 2;
                    else if (PRIOR_FIELD_STATES[agentIndex].enemiesPosition[enemyIndex].x == 1 && FIELD_ACTIONS_TO_PERFORM[agentIndex].rotateCounterClockwise)
                        reward -= 2;
                    else if (PRIOR_FIELD_STATES[agentIndex].enemiesPosition[enemyIndex].x == -1 && FIELD_ACTIONS_TO_PERFORM[agentIndex].rotateCounterClockwise)
                        reward += 2;
                    else if (PRIOR_FIELD_STATES[agentIndex].enemiesPosition[enemyIndex].x == -1 && FIELD_ACTIONS_TO_PERFORM[agentIndex].rotateClockwise)
                        reward -= 2;
                    else if (PRIOR_FIELD_STATES[agentIndex].enemiesPosition[enemyIndex].x == 0)
                    {
                        if (!FIELD_ACTIONS_TO_PERFORM[agentIndex].rotateClockwise && !FIELD_ACTIONS_TO_PERFORM[agentIndex].rotateCounterClockwise)
                            reward += 20;
                        else reward += 10;
                    }
                    if (PRIOR_FIELD_STATES[agentIndex].enemiesPosition[enemyIndex].y == 1 && FIELD_ACTIONS_TO_PERFORM[agentIndex].lookUp)
                        reward += 2;
                    else if (PRIOR_FIELD_STATES[agentIndex].enemiesPosition[enemyIndex].y == 1 && FIELD_ACTIONS_TO_PERFORM[agentIndex].lookDown)
                        reward -= 2;
                    else if (PRIOR_FIELD_STATES[agentIndex].enemiesPosition[enemyIndex].y == -1 && FIELD_ACTIONS_TO_PERFORM[agentIndex].lookDown)
                        reward += 2;
                    else if (PRIOR_FIELD_STATES[agentIndex].enemiesPosition[enemyIndex].y == -1 && FIELD_ACTIONS_TO_PERFORM[agentIndex].lookUp)
                        reward -= 2;
                    else if (PRIOR_FIELD_STATES[agentIndex].enemiesPosition[enemyIndex].y == 0)
                    {
                        if (!FIELD_ACTIONS_TO_PERFORM[agentIndex].lookUp && !FIELD_ACTIONS_TO_PERFORM[agentIndex].lookDown)
                            reward += 20;
                        else reward += 10;
                    }
                }
            }
            reward -= state.killed ? 20 : 0;
            reward -= state.touchingWall ? 1 : 0;
            if (PRIOR_FIELD_STATES[agentIndex].Compare(state) == 0)
                ++STUCK_IN_STATE_FOR_TURN_COUNT[agentIndex];
            else STUCK_IN_STATE_FOR_TURN_COUNT[agentIndex] = 0;
            if (STUCK_IN_STATE_FOR_TURN_COUNT[agentIndex] > STUCK_MAX_TURN_COUNT)
                state.timeSinceLastAction += STUCK_MAX_TURN_COUNT * RANDOM.Next(STUCK_MAX_TURN_COUNT * STUCK_MAX_TURN_COUNT);
            reward -= state.timeSinceLastAction > 0 ? 3 : 0;
            agent.applyRewardForActionInState(reward);
            PRIOR_FIELD_STATES[agentIndex] = state;
        }
        actionsUpdated = true;
    }

    private void updateActions()
    {
        while (true)
        {
            if (actionsUpdated)
            {
                actionsUpdated = false;
                for (int agentIndex = 0; agentIndex < NUMBER_OF_AGENTS; ++agentIndex)
                    FIELD_ACTIONS_TO_PERFORM[agentIndex] = AI_AGENTS[agentIndex].getActionForState(PRIOR_FIELD_STATES[agentIndex]);
            }
        }
    }

    public void applyBuildHubRewards()
    {
        foreach (AIAgent agent in AI_AGENTS)
        {
            bool won = agent.getBot().getRemainingDurability() > 0;
            List<BuildHubState> buildHubStates = agent.getBuildHubStates();
            for (int stateIndex = 0; stateIndex < buildHubStates.Count; ++stateIndex)
            {
                int selectedActionIndex = buildHubStates[stateIndex].indexOfSelectedAction;
                if (selectedActionIndex >= 0)
                {
                    double reward = 0;
                    if (won)
                        reward = 20;
                    else reward = -20;
                    double priorQValue = buildHubStates[stateIndex].actions[selectedActionIndex].qValue;
                    BuildHubAction optimalAction = (BuildHubAction)buildHubStates[stateIndex].actions[0];
                    foreach (Action action in buildHubStates[stateIndex].actions)
                        if (action.qValue > optimalAction.qValue)
                            optimalAction = (BuildHubAction)action;
                    buildHubStates[stateIndex].actions[selectedActionIndex].qValue += LEARNING_RATE * (reward + DISCOUNT_FACTOR * optimalAction.qValue - priorQValue);
                    double growth = Math.Pow(Math.E, -Math.Abs(buildHubStates[stateIndex].actions[selectedActionIndex].qValue - priorQValue) / INVERSE_SENSITIVITY);
                    buildHubStates[stateIndex].explorationRate = BUILD_HUB_EXPLORATION_RATE_ACTION_WEIGHT * (1.0 - growth) / (1.0 + growth) + (1.0 - BUILD_HUB_EXPLORATION_RATE_ACTION_WEIGHT) * buildHubStates[stateIndex].explorationRate;
                    if (buildHubStates[stateIndex].explorationRate > 1)
                        buildHubStates[stateIndex].explorationRate = 1;
                    if (buildHubStates[stateIndex].explorationRate < 0)
                        buildHubStates[stateIndex].explorationRate = 0;
                }
            }
        }
    }

    public FieldAction[] getFieldActionsToPerform()
    {
        return FIELD_ACTIONS_TO_PERFORM;
    }

    public Robot[] getAgentRobots()
    {
        List<AIAgent> aiAgents = new List<AIAgent>();
        aiAgents.AddRange(AI_AGENTS);
        return aiAgents.ConvertAll(new System.Converter<AIAgent, Robot>(agentToRobot)).ToArray();
    }

    public AIAgent[] getAgents()
    {
        return AI_AGENTS;
    }
}