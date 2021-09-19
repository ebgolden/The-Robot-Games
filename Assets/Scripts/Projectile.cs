using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : Influencer
{
    private readonly Robot ROBOT;
    private readonly double VELOCITY;
    protected Effect<Projectile> effect;
    private readonly int LIFETIME;
    private long startTime;

    public Projectile(Robot robot, double velocity, Dimension size, Effect<Projectile> effect, Shape.SHAPES shape, int lifetime) : base(size, shape)
    {
        ROBOT = robot;
        base.GAME_OBJECT.transform.position = new Vector3(0, (float)Field.FIELD_HEIGHT / 2, 0);
        base.GAME_OBJECT.AddComponent<ProjectileListener>();
        if (base.shape == Shape.SHAPES.PYRAMID)
        {
            base.shape = Shape.SHAPES.CYLINDER;
            base.size = size;
            Vector3 obstacleSize = base.GAME_OBJECT.GetComponent<Renderer>().bounds.size;
            Vector3 rescale = base.GAME_OBJECT.transform.localScale;
            rescale.x = (float)size.width * rescale.x / obstacleSize.x;
            rescale.y = (float)size.height * rescale.y / obstacleSize.y;
            rescale.z = (float)size.depth * rescale.z / obstacleSize.z;
            base.GAME_OBJECT.transform.localScale = rescale;
        }
        this.effect = effect;
        if (this.effect != null)
            this.effect.bindTo(this);
        VELOCITY = velocity;
        LIFETIME = lifetime;
        startTime = 0;
    }

    public Effect<Projectile> getEffect()
    {
        return effect;
    }

    public override List<Collider> getCollidersTouching()
    {
        List<Collider> collidersTouching = new List<Collider>();
        ProjectileListener projectileListener = null;
        if (base.GAME_OBJECT.TryGetComponent<ProjectileListener>(out projectileListener))
            collidersTouching.AddRange(projectileListener.getCollidersTouching());
        return collidersTouching;
    }

    public double getVelocity()
    {
        return VELOCITY;
    }

    public Robot getRobot()
    {
        return ROBOT;
    }

    public bool hasLifetimeStarted()
    {
        return startTime != 0;
    }

    public void startLifetime()
    {
        startTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public bool isLifetimeOver()
    {
        return LIFETIME <= (System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime);
    }

    public void adjustTime(long timeOffset)
    {
        if (startTime > 0)
            startTime += timeOffset;
    }
}