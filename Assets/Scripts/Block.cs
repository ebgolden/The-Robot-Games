public class Block : Obstacle
{
    public Block(int slopeAngle, double friction, Dimension size, Effect<Obstacle> effect, Shape.SHAPES shape) : base(slopeAngle, friction, size, effect, shape)
    {
        
    }

    public override Robot triggerEffect(Robot robot)
    {
        return robot;
    }
}