using UnityEngine;

public class Laser : Projectile
{
    private readonly double DAMAGE;
    private static readonly Point SCALE = new Point(10, 0, 10);
    private static readonly Dimension SIZE = new Dimension(Robot.SCALE / SCALE.x, Robot.SCALE / SCALE.y, Robot.SCALE / SCALE.z);
    private static readonly Effect<Projectile> EFFECT = new Damage<Projectile>(0);
    private static readonly Shape.SHAPES SHAPE = Shape.SHAPES.CYLINDER;
    private static readonly double VELOCITY = 0;
    private static readonly int LIFETIME = 50;

    public Laser(Robot robot, double damage, float hitDistance) : base(robot, VELOCITY, new Dimension(SIZE.width, hitDistance, SIZE.depth), EFFECT, SHAPE, LIFETIME)
    {
        DAMAGE = damage;
        ((Damage<Projectile>)EFFECT).setDamage(DAMAGE);
        base.effect = EFFECT;
        base.GAME_OBJECT.GetComponent<Renderer>().material.color = Color.red;
        base.GAME_OBJECT.GetComponent<Renderer>().material.shader = Shader.Find("UI/Unlit/Detail");
        base.GAME_OBJECT.GetComponent<Rigidbody>().mass = 0;
        base.GAME_OBJECT.GetComponent<Rigidbody>().useGravity = false;
    }

    public override Robot triggerEffect(Robot robot)
    {
        return robot;
    }
}