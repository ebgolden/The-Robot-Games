public class Blaster : Attachment
{
    private static readonly new bool WEAPON = true, PASSIVE = false, INVISIBLE = false;

    public Blaster(string id, Image image, double weight, Effect<Robot> effect, Shape.SHAPES shape, double durability, double remainingDurability, int minChargeTime, int maxChargeTime, int minCoolingTime, double maxDamage) : base(id, image, weight, null, shape, durability, remainingDurability, minChargeTime, maxChargeTime, 0, minCoolingTime, maxDamage, WEAPON, PASSIVE, INVISIBLE)
    {

    }

    public Blaster(bool isPreview, string id, Image image, double weight, Effect<Robot> effect, Shape.SHAPES shape, double durability, double remainingDurability, int minChargeTime, int maxChargeTime, int minCoolingTime, double maxDamage) : base(isPreview, id, image, weight, null, shape, durability, remainingDurability, minChargeTime, maxChargeTime, 0, minCoolingTime, maxDamage, WEAPON, PASSIVE, INVISIBLE)
    {

    }

    public override Robot use(Robot robot)
    {
        return robot;
    }

    public override Projectile fire(Robot robot, float hitDistance)
    {
        Projectile projectile = null;
        if (base.canFire())
        {
            projectile = new Laser(robot, base.calculateDamage(), hitDistance);
            base.stopFiring();
        }
        return projectile;
    }

    public override Part clone(bool isPreview)
    {
        return new Blaster(isPreview, base.ID, base.getImage(), base.getWeight(), null, base.getShape(), base.getDurability(), base.getRemainingDurability(), base.getMinChargeTime(), base.getMaxChargeTime(), base.getMinCoolingTime(), base.getMaxDamage());
    }
}