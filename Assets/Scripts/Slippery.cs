public class Slippery<T> : Effect<T>
{
    private readonly double MULTIPLIER;

    public Slippery(double multiplier)
    {
        MULTIPLIER = multiplier;
    }

    public override Robot applyTo(Robot robot, bool effects)
    {
        if (effects)
        {
            Point force = robot.getForce();
            force.x *= MULTIPLIER;
            force.z *= MULTIPLIER;
            robot.setForce(force, false);
        }
        return robot;
    }
}