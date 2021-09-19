public class Damage<T> : Effect<T>
{
    private double damage;

    public Damage(double damage)
    {
        this.damage = damage;
    }

    public void setDamage(double damage)
    {
        this.damage = damage;
    }

    public override Robot applyTo(Robot robot, bool effects)
    {
        if (!(!effects && typeof(T) == typeof(Obstacle)))
            robot.damage(damage);
        return robot;
    }
}