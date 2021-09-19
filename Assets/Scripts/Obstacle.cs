using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class Obstacle : Influencer
{
    private readonly int SLOPE_ANGLE;
    private readonly double FRICTION;
    protected Effect<Obstacle> effect;
    private readonly double STATIC_FRICTION_CONST = .05;
    private readonly int SHAPE_STEP = ObstacleGenerator.LIM_VALS[(int)ObstacleGenerator.LIMS.MAX_SIZE]; //smaller = higher res, a denominator must be less than or equal to MAX_SIZE
    private readonly int SLOPE_SIZE = ObstacleGenerator.LIM_VALS[(int)ObstacleGenerator.LIMS.MAX_SIZE] / 3;
    private readonly Dimension PYRAMID_SIZE = new Dimension(ObstacleGenerator.LIM_VALS[(int)ObstacleGenerator.LIMS.MAX_SIZE], ObstacleGenerator.LIM_VALS[(int)ObstacleGenerator.LIMS.MAX_HEIGHT] / 3, ObstacleGenerator.LIM_VALS[(int)ObstacleGenerator.LIMS.MAX_SIZE]);

    public Obstacle(int slopeAngle, double friction, Dimension size, Effect<Obstacle> effect, Shape.SHAPES shape) : base(size, shape)
    {
        SLOPE_ANGLE = slopeAngle;
        FRICTION = friction;
        GameObject.Destroy(base.GAME_OBJECT.GetComponent<Collider>());
        base.GAME_OBJECT.AddComponent<MeshCollider>().convex = true;
        base.GAME_OBJECT.AddComponent<ObstacleListener>();
        base.GAME_OBJECT.GetComponent<Collider>().material = new PhysicMaterial();
        base.GAME_OBJECT.GetComponent<Collider>().material.dynamicFriction = (float)FRICTION;
        base.GAME_OBJECT.GetComponent<Collider>().material.staticFriction = (float)STATIC_FRICTION_CONST;
        if (base.shape == Shape.SHAPES.PYRAMID)
        {
            GameObject.Destroy(base.GAME_OBJECT.GetComponent<MeshCollider>());
            GameObject.Destroy(base.GAME_OBJECT.GetComponent<ObstacleListener>());
            this.size = PYRAMID_SIZE;
            createSlope();
            SLOPE_ANGLE = SLOPE_SIZE;
        }
        this.effect = effect;
        if (this.effect != null)
            this.effect.bindTo(this);
        setConstraints();
    }

    private void createSlope()
    {
        Vector3 obstacleSize = base.GAME_OBJECT.GetComponent<Renderer>().bounds.size;
        Vector3 rescale = base.GAME_OBJECT.transform.localScale;
        rescale.x = Math.Abs((float)size.width / 2 - (float)SLOPE_SIZE) * rescale.x / obstacleSize.x;
        while (rescale.x >= 1)
            rescale.x /= 10;
        rescale.z = Math.Abs((float)size.depth / 2 - (float)SLOPE_SIZE) * rescale.z / obstacleSize.z / 1000;
        while (rescale.z >= 1)
            rescale.z /= 10;
        base.GAME_OBJECT.transform.localScale = rescale;
        GameObject previousGameObject = base.GAME_OBJECT;
        Dimension gameObjectSize = new Dimension(size.width - SLOPE_SIZE, size.height, size.depth - SLOPE_SIZE);
        for (int height = (int)size.height - SHAPE_STEP; height > 0; height -= SHAPE_STEP)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.transform.SetParent(base.GAME_OBJECT.transform, false);
            gameObject.AddComponent<Rigidbody>().position = Vector3.zero;
            gameObject.AddComponent<MeshCollider>().convex = true;
            gameObject.GetComponent<Collider>().isTrigger = false;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            gameObject.GetComponent<Collider>().material = new PhysicMaterial();
            gameObject.GetComponent<Collider>().material.dynamicFriction = (float)FRICTION;
            gameObject.GetComponent<Collider>().material.staticFriction = (float)STATIC_FRICTION_CONST;
            obstacleSize = gameObject.GetComponent<Renderer>().bounds.size;
            rescale = gameObject.transform.localScale;
            gameObjectSize = new Dimension(gameObjectSize.width + SHAPE_STEP, gameObjectSize.height - SHAPE_STEP, gameObjectSize.depth + SHAPE_STEP);
            rescale.x = Math.Abs((float)gameObjectSize.width * rescale.x / base.GAME_OBJECT.GetComponent<Renderer>().bounds.size.x);
            rescale.y = Math.Abs((float)gameObjectSize.height * rescale.y / base.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y);
            rescale.z = Math.Abs((float)gameObjectSize.depth * rescale.z / base.GAME_OBJECT.GetComponent<Renderer>().bounds.size.z);
            gameObject.transform.localScale = rescale;
            gameObject.AddComponent<ObstacleListener>();
            previousGameObject = gameObject;
        }
    }

    public Effect<Obstacle> getEffect()
    {
        return effect;
    }

    public int getSlopeAngle()
    {
        return SLOPE_ANGLE;
    }

    public double getFriction()
    {
        return FRICTION;
    }

    public void setConstraints()
    {
        GAME_OBJECT.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
    }

    public override List<Collider> getCollidersTouching()
    {
        List<Collider> collidersTouching = new List<Collider>();
        ObstacleListener obstacleListener = null;
        if (GAME_OBJECT != null && GAME_OBJECT.TryGetComponent<ObstacleListener>(out obstacleListener))
            collidersTouching.AddRange(obstacleListener.getCollidersTouching());
        return collidersTouching;
    }
}