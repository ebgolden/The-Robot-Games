using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;

public class ObstacleGenerator
{
    private readonly int EXPERIENCE, MAX_NUM_OF_OBSTACLES;
    private int numberOfObstacles;
    private readonly Dimension FIELD_SIZE;
    public enum OPERATION_CONSTS { MOD_DENOM,
        LOWEST_CHANCE, HIGHEST_CHANCE,
        ROBOT_SCALE,
        FIELD_MIN_X, FIELD_MAX_X, FIELD_MIN_Z, FIELD_MAX_Z, FIELD_BASE_MULTIPLIER }; //DO NOT CHANGE THESE, used to get specific digits, represent the full percent range, etc
    public static readonly double[] OPERATION_CONSTS_VALS = { 10, 1, 100, Robot.SCALE, -1.0, 1.0, -1.0, 1.0, 1.0 / 2.0};
    public enum LIMS { MIN_MIN_NUM, MAX_MIN_NUM, MIN_MAX_NUM, MAX_MAX_NUM,
        EXTRA_OBSTACLE_CHANCE,
        PROGRESS_DAMPER,
        MIN_HEIGHT, MAX_HEIGHT,
        MIN_SLOPE_ANGLE, MAX_SLOPE_ANGLE,
        MAX_SIZE,
        POS_RES,
        MIN_MULTIPLIER,
        MAX_MULTIPLIER
    };
    public static readonly int[] LIM_VALS = { 1, 6, 2, 10, 5, 50, 1, 2, 5, 90, 10, 25, 2, 100 };
    public enum FRICTION_LIMS { MIN_DYNA_FRICTION, MAX_DYNA_FRICTION };
    public static readonly double[] FRICTION_LIM_VALS = { .05, .99 };
    private readonly System.Random RANDOM;
    private List<Obstacle> obstacles;

    public ObstacleGenerator(long experience, int maxObstacles, Dimension fieldSize)
    {
        EXPERIENCE = (int)experience;
        MAX_NUM_OF_OBSTACLES = maxObstacles;
        FIELD_SIZE = fieldSize;
        RANDOM = new System.Random();
        double fieldBaseMultiplier = OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_BASE_MULTIPLIER];
        OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MIN_X] = OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MIN_X] / Math.Abs((float)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MIN_X]) * fieldBaseMultiplier * FIELD_SIZE.width;
        OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MAX_X] = OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MAX_X] / Math.Abs((float)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MAX_X]) * fieldBaseMultiplier * FIELD_SIZE.width;
        OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MIN_Z] = OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MIN_Z] / Math.Abs((float)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MIN_Z]) * fieldBaseMultiplier * FIELD_SIZE.depth;
        OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MAX_Z] = OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MAX_Z] / Math.Abs((float)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MAX_Z]) * fieldBaseMultiplier * FIELD_SIZE.depth;
        LIM_VALS[(int)LIMS.MIN_HEIGHT] = (int)FIELD_SIZE.height / LIM_VALS[(int)LIMS.MIN_HEIGHT];
        LIM_VALS[(int)LIMS.MAX_HEIGHT] = (int)FIELD_SIZE.width / LIM_VALS[(int)LIMS.MAX_HEIGHT];
        LIM_VALS[(int)LIMS.MAX_SIZE] = (int)FIELD_SIZE.width / LIM_VALS[(int)LIMS.MAX_SIZE];
        LIM_VALS[(int)LIMS.POS_RES] = (int)FIELD_SIZE.width / LIM_VALS[(int)LIMS.POS_RES];
        obstacles = new List<Obstacle>();
    }

    private int generateNumberOfObstacles()
    {
        int progressDamper = LIM_VALS[(int)LIMS.PROGRESS_DAMPER];
        int min = LIM_VALS[(int)LIMS.MIN_MIN_NUM] + EXPERIENCE / progressDamper;
        min += (RANDOM.Next((int)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.LOWEST_CHANCE], (int)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.HIGHEST_CHANCE] + 1) <= LIM_VALS[(int)LIMS.EXTRA_OBSTACLE_CHANCE]) ? 1 : 0;
        if (min > LIM_VALS[(int)LIMS.MAX_MIN_NUM])
            min = LIM_VALS[(int)LIMS.MAX_MIN_NUM];
        if (min > MAX_NUM_OF_OBSTACLES)
            min = MAX_NUM_OF_OBSTACLES;
        int max = LIM_VALS[(int)LIMS.MIN_MAX_NUM] + EXPERIENCE / progressDamper;
        int maxModifier = (EXPERIENCE % (int)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.MOD_DENOM]) / 2;
        max += (EXPERIENCE / max > maxModifier) ? maxModifier : 0;
        max = (max <= min) ? min + 1 : max;
        max += (RANDOM.Next((int)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.LOWEST_CHANCE], (int)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.HIGHEST_CHANCE] + 1) <= LIM_VALS[(int)LIMS.EXTRA_OBSTACLE_CHANCE]) ? 1 : 0;
        if (max > LIM_VALS[(int)LIMS.MAX_MAX_NUM])
            max = LIM_VALS[(int)LIMS.MAX_MAX_NUM];
        if (max > MAX_NUM_OF_OBSTACLES)
            max = MAX_NUM_OF_OBSTACLES;
        return RANDOM.Next(min, max + 1);
    }

    private Obstacle generateObstacle()
    {
        //size
        double width = RANDOM.NextDouble() * ((double)LIM_VALS[(int)LIMS.MAX_SIZE] + 1 - OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE]) + OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE] + OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE];
        double height = RANDOM.Next(LIM_VALS[(int)LIMS.MIN_HEIGHT], LIM_VALS[(int)LIMS.MAX_HEIGHT] + 1);
        double depth = RANDOM.NextDouble() * ((double)LIM_VALS[(int)LIMS.MAX_SIZE] + 1 - OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE]) + OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE] + OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE];
        Dimension size = new Dimension(width, height, depth);

        //slope angle
        double smallestDim = (width < depth) ? width : depth;
        double slopeWidth = smallestDim / 2 - OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE] / 2;
        int minSlope = (int)(Math.Atan(height / slopeWidth) + 1);
        int slopeAngle = RANDOM.Next(minSlope, LIM_VALS[(int)LIMS.MAX_SLOPE_ANGLE] + 1);
        if (slopeAngle < LIM_VALS[(int)LIMS.MIN_SLOPE_ANGLE])
            slopeAngle = LIM_VALS[(int)LIMS.MIN_SLOPE_ANGLE];

        //friction
        double friction = RANDOM.NextDouble() * (FRICTION_LIM_VALS[(int)FRICTION_LIMS.MAX_DYNA_FRICTION] + 1 - FRICTION_LIM_VALS[(int)FRICTION_LIMS.MIN_DYNA_FRICTION]) + FRICTION_LIM_VALS[(int)FRICTION_LIMS.MIN_DYNA_FRICTION];

        //shape
        int numberOfShapes = Enum.GetValues(typeof(Shape.SHAPES)).Length;
        Shape.SHAPES shape = (Shape.SHAPES)Enum.Parse(typeof(Shape.SHAPES), Enum.GetNames(typeof(Shape.SHAPES))[RANDOM.Next(1, numberOfShapes)]);

        //effect
        Effect<Obstacle> effect;
        int effectTypeMaxIndex = EXPERIENCE / LIM_VALS[(int)LIMS.PROGRESS_DAMPER];
        List<Type> effectTypes = TypeTools.getTypesOfBaseType(typeof(Effect<Obstacle>));
        int numberOfEffectTypes = effectTypes.Count;
        if (effectTypeMaxIndex >= numberOfEffectTypes)
            effectTypeMaxIndex = numberOfEffectTypes - 1;
        int effectTypeIndex = RANDOM.Next(effectTypeMaxIndex + 1);
        if (effectTypeIndex == 0)
            effect = null;
        else
        {
            Type effectType = effectTypes[effectTypeIndex].MakeGenericType(typeof(Obstacle));
            ConstructorInfo[] effectConstructorInfo = effectType.GetConstructors();
            ParameterInfo[] effectParameters = effectConstructorInfo[0].GetParameters();
            object[] parameterValues = generateEffectParameterValues(effectParameters);
            effect = (Effect<Obstacle>)effectConstructorInfo[0].Invoke(parameterValues);
        }

        Obstacle obstacle;
        int obstacleTypeMaxIndex = EXPERIENCE / LIM_VALS[(int)LIMS.PROGRESS_DAMPER];
        List<Type> obstacleTypes = TypeTools.getTypesOfBaseType(typeof(Obstacle));
        int numberOfObstacleTypes = obstacleTypes.Count;
        if (obstacleTypeMaxIndex >= numberOfObstacleTypes)
            obstacleTypeMaxIndex = numberOfObstacleTypes - 1;
        int obstacleTypeIndex = RANDOM.Next(1, obstacleTypeMaxIndex + 1);
        Type obstacleType = obstacleTypes[obstacleTypeIndex];
        ConstructorInfo[] obstacleConstructorInfo = obstacleType.GetConstructors();
        obstacle = (Obstacle)obstacleConstructorInfo[0].Invoke(new object[] { slopeAngle, friction, size, effect, shape });

        //position
        obstacle = generatePosition(obstacle);
        obstacle.GAME_OBJECT.GetComponent<Collider>().isTrigger = true;
        return obstacle;
    }

    private object[] generateEffectParameterValues(ParameterInfo[] effectParameters)
    {
        object[] parameterValues = new object[effectParameters.Length];
        for (int parameterIndex = 0; parameterIndex < effectParameters.Length; ++parameterIndex)
        {
            Type parameterType = effectParameters[parameterIndex].ParameterType;
            object value = default;
            if (parameterType == typeof(int))
                value = RANDOM.Next(LIM_VALS[(int)LIMS.MIN_MULTIPLIER], LIM_VALS[(int)LIMS.MAX_MULTIPLIER] + 1);
            else if (parameterType == typeof(double) || parameterType == typeof(float))
                value = RANDOM.NextDouble() * ((double)LIM_VALS[(int)LIMS.MAX_MULTIPLIER] + 1 - LIM_VALS[(int)LIMS.MIN_MULTIPLIER]) + LIM_VALS[(int)LIMS.MIN_MULTIPLIER];
            parameterValues[parameterIndex] = value;
        }
        return parameterValues;
    }

    private Obstacle generatePosition(Obstacle obstacleToTry)
    {
        Dimension size = obstacleToTry.getSize();
        Point position = new Point(RANDOM.Next((int)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MIN_X] + (int)size.width / 2, (int)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MAX_X] - (int)size.width / 2 + 1), 0, RANDOM.Next((int)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MIN_X] + (int)size.width / 2, (int)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MAX_X] - (int)size.width / 2 + 1));
        obstacleToTry.setPosition(position);
        if (obstacles.Count == 0 || obstacles[0] == null || obstacleToTry.GAME_OBJECT.GetComponent<ObstacleListener>().getCollidersTouching() == null || obstacleToTry.GAME_OBJECT.GetComponent<ObstacleListener>().getCollidersTouching().Count == 0)
        {
            obstacleToTry.setSize(new Dimension(obstacleToTry.getSize().width, obstacleToTry.getSize().height, obstacleToTry.getSize().depth));
            return obstacleToTry;
        }
        int originalX = (int)position.x;
        int originalZ = (int)position.z;
        for (int x = originalX; x <= OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MAX_X] - (int)size.width / 2; x += LIM_VALS[(int)LIMS.POS_RES])
        {
            for (int z = originalZ; z <= OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MAX_Z] - (int)size.depth / 2; z += LIM_VALS[(int)LIMS.POS_RES])
            {
                obstacleToTry.setPosition(new Point(x, 0, z));
                if (obstacleToTry.GAME_OBJECT.GetComponent<ObstacleListener>().getCollidersTouching() == null || obstacleToTry.GAME_OBJECT.GetComponent<ObstacleListener>().getCollidersTouching().Count == 0)
                {
                    obstacleToTry.setSize(new Dimension(obstacleToTry.getSize().width, obstacleToTry.getSize().height, obstacleToTry.getSize().depth));
                    return obstacleToTry;
                }
            }
        }
        for (int x = (int)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MIN_X] + (int)size.width / 2; x < originalX; x += LIM_VALS[(int)LIMS.POS_RES])
        {
            for (int z = (int)OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.FIELD_MIN_Z] + (int)size.depth / 2; z < originalZ; z += LIM_VALS[(int)LIMS.POS_RES])
            {
                obstacleToTry.setPosition(new Point(x, 0, z));
                if (obstacleToTry.GAME_OBJECT.GetComponent<ObstacleListener>().getCollidersTouching() == null || obstacleToTry.GAME_OBJECT.GetComponent<ObstacleListener>().getCollidersTouching().Count == 0)
                {
                    obstacleToTry.setSize(new Dimension(obstacleToTry.getSize().width, obstacleToTry.getSize().height, obstacleToTry.getSize().depth));
                    return obstacleToTry;
                }
            }
        }
        obstacleToTry.GAME_OBJECT.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        obstacleToTry.GAME_OBJECT.GetComponent<Rigidbody>().mass = 0;
        obstacleToTry.GAME_OBJECT.transform.localScale = Vector3.zero;
        return null;
    }

    public Obstacle[] getObstacles()
    {
        numberOfObstacles = generateNumberOfObstacles();
        obstacles = new List<Obstacle>(numberOfObstacles);
        for (int obstacleIndex = 0; obstacleIndex < numberOfObstacles; ++obstacleIndex)
            obstacles[obstacleIndex] = generateObstacle();
        return obstacles.ToArray();
    }

    public ObstacleData[] getObstaclesData()
    {
        numberOfObstacles = generateNumberOfObstacles();
        ObstacleData[] obstaclesData = new ObstacleData[numberOfObstacles];
        for (int obstacleIndex = 0; obstacleIndex < numberOfObstacles; ++obstacleIndex)
            obstaclesData[obstacleIndex] = generateObstacleData();
        return obstaclesData;
    }

    public ObstacleData generateObstacleData()
    {
        //size
        double width = RANDOM.NextDouble() * ((double)LIM_VALS[(int)LIMS.MAX_SIZE] + 1 - OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE]) + OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE] + OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE];
        double height = RANDOM.Next(LIM_VALS[(int)LIMS.MIN_HEIGHT], LIM_VALS[(int)LIMS.MAX_HEIGHT] + 1);
        double depth = RANDOM.NextDouble() * ((double)LIM_VALS[(int)LIMS.MAX_SIZE] + 1 - OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE]) + OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE] + OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE];
        Dimension size = new Dimension(width, height, depth);

        //slope angle
        double smallestDim = (width < depth) ? width : depth;
        double slopeWidth = smallestDim / 2 - OPERATION_CONSTS_VALS[(int)OPERATION_CONSTS.ROBOT_SCALE] / 2;
        int minSlope = (int)(Math.Atan(height / slopeWidth) + 1);
        int slopeAngle = RANDOM.Next(minSlope, LIM_VALS[(int)LIMS.MAX_SLOPE_ANGLE] + 1);
        if (slopeAngle < LIM_VALS[(int)LIMS.MIN_SLOPE_ANGLE])
            slopeAngle = LIM_VALS[(int)LIMS.MIN_SLOPE_ANGLE];

        //friction
        double friction = RANDOM.NextDouble() * ((float)FRICTION_LIM_VALS[(int)FRICTION_LIMS.MAX_DYNA_FRICTION] + 1 - FRICTION_LIM_VALS[(int)FRICTION_LIMS.MAX_DYNA_FRICTION]) + FRICTION_LIM_VALS[(int)FRICTION_LIMS.MIN_DYNA_FRICTION];

        //shape
        int numberOfShapes = Enum.GetValues(typeof(Shape.SHAPES)).Length;
        Shape.SHAPES shape = (Shape.SHAPES)Enum.Parse(typeof(Shape.SHAPES), Enum.GetNames(typeof(Shape.SHAPES))[RANDOM.Next(numberOfShapes)]);

        //effect
        Effect<Obstacle> effect;
        int effectTypeMaxIndex = EXPERIENCE / LIM_VALS[(int)LIMS.PROGRESS_DAMPER];
        List<Type> effectTypes = TypeTools.getTypesOfBaseType(typeof(Effect<Obstacle>));
        int numberOfEffectTypes = effectTypes.Count;
        if (effectTypeMaxIndex >= numberOfEffectTypes)
            effectTypeMaxIndex = numberOfEffectTypes - 1;
        int effectTypeIndex = RANDOM.Next(effectTypeMaxIndex + 1);
        if (effectTypeIndex == 0)
            effect = null;
        else
        {
            Type effectType = effectTypes[effectTypeIndex].MakeGenericType(typeof(Obstacle));
            ConstructorInfo[] constructorInfo = effectType.GetConstructors();
            ParameterInfo[] parameters = constructorInfo[0].GetParameters();
            object[] parameterValues = new object[parameters.Length];
            for (int parameterIndex = 0; parameterIndex < parameters.Length; ++parameterIndex)
            {
                Type parameterType = parameters[parameterIndex].ParameterType;
                object value = default;
                if (parameterType == typeof(int))
                    value = RANDOM.Next(LIM_VALS[(int)LIMS.MIN_MULTIPLIER], LIM_VALS[(int)LIMS.MAX_MULTIPLIER] + 1);
                else if (parameterType == typeof(double) || parameterType == typeof(float))
                    value = RANDOM.NextDouble() * ((double)LIM_VALS[(int)LIMS.MAX_MULTIPLIER] + 1 - (double)LIM_VALS[(int)LIMS.MIN_MULTIPLIER]) + LIM_VALS[(int)LIMS.MIN_MULTIPLIER];
                parameterValues[parameterIndex] = value;
            }
            effect = (Effect<Obstacle>)constructorInfo[0].Invoke(parameterValues);
        }

        int obstacleTypeMaxIndex = EXPERIENCE / LIM_VALS[(int)LIMS.PROGRESS_DAMPER];
        List<Type> obstacleTypes = TypeTools.getTypesOfBaseType(typeof(Obstacle));
        int numberOfObstacleTypes = obstacleTypes.Count;
        if (obstacleTypeMaxIndex >= numberOfObstacleTypes)
            obstacleTypeMaxIndex = numberOfObstacleTypes - 1;
        int obstacleTypeIndex = RANDOM.Next(obstacleTypeMaxIndex + 1);
        Type obstacleType = obstacleTypes[obstacleTypeIndex];

        ObstacleData obstacleData = new ObstacleData();
        obstacleData.obstacleType = obstacleType.ToString();
        obstacleData.width = width;
        obstacleData.height = height;
        obstacleData.depth = depth;
        obstacleData.slopeAngle = slopeAngle;
        obstacleData.friction = friction;
        obstacleData.shape = shape.ToString();
        if (effect != null)
            obstacleData.effect = effect.GetType().ToString();
        return obstacleData;
    }

    public Obstacle[] obstaclesDataToObstacles(ObstacleData[] obstaclesData)
    {
        numberOfObstacles = obstaclesData.Length;
        obstacles = new List<Obstacle>(numberOfObstacles);
        for (int obstacleIndex = 0; obstacleIndex < numberOfObstacles; ++obstacleIndex)
            obstacles.Add(generateObstacleFromObstacleData(obstaclesData[obstacleIndex]));
        return obstacles.ToArray();
    }

    private Obstacle generateObstacleFromObstacleData(ObstacleData obstacleData)
    {
        //size
        Dimension size = new Dimension(obstacleData.width, obstacleData.height, obstacleData.depth);
        
        //slope angle
        int slopeAngle = obstacleData.slopeAngle;

        //friction
        double friction = obstacleData.friction;

        //shape
        Shape.SHAPES shape = (Shape.SHAPES)Enum.Parse(typeof(Shape.SHAPES), obstacleData.shape, true);

        //effect
        Effect<Obstacle> effect;
        if (obstacleData.effect == null)
            effect = null;
        else
        {
            Type effectType = Type.GetType(obstacleData.effect);
            ConstructorInfo[] effectConstructorInfo = effectType.GetConstructors();
            ParameterInfo[] effectParameters = effectConstructorInfo[0].GetParameters();
            object[] parameterValues = generateEffectParameterValues(effectParameters);
            effect = (Effect<Obstacle>)effectConstructorInfo[0].Invoke(parameterValues);
        }

        ConstructorInfo[] obstacleConstructorInfo = Type.GetType(obstacleData.obstacleType).GetConstructors();
        Obstacle obstacle = (Obstacle)obstacleConstructorInfo[0].Invoke(new object[] { slopeAngle, friction, size, effect, shape });

        //position
        obstacle = generatePosition(obstacle);
        obstacle.GAME_OBJECT.GetComponent<Collider>().isTrigger = true;
        return obstacle;
    }
}