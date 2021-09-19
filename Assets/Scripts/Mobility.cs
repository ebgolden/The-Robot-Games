using System.Collections.Generic;
using UnityEngine;

public class Mobility : Part
{
    private readonly double CLIMB_ANGLE;
    private readonly double MAX_SPEED;
    private readonly double MAX_FORCE;
    private readonly Point RESCALE = new Point(1, .25, 1);
    private readonly string[] STAT_PREFIXES = { "Climb angle: ", "Max speed: ", "Max capacity: " }, STAT_SUFFIXES = { " deg", " m/s", " N" };

    public Mobility(string id, Image image, double weight, Shape.SHAPES shape, double durability, double remainingDurability, double climbAngle, double maxSpeed, double maxForce) : base(id, image, weight, shape, durability, remainingDurability)
    {
        CLIMB_ANGLE = climbAngle;
        MAX_SPEED = maxSpeed;
        MAX_FORCE = maxForce;
        Vector3 rescale = RESCALE.toVector3();
        base.GAME_OBJECT.transform.localScale = rescale;
    }

    public Mobility(bool isPreview, string id, Image image, Texture2D icon, double weight, Shape.SHAPES shape, double durability, double remainingDurability, double climbAngle, double maxSpeed, double maxForce) : base(isPreview, id, image, icon, weight, shape, durability, remainingDurability)
    {
        CLIMB_ANGLE = climbAngle;
        MAX_SPEED = maxSpeed;
        MAX_FORCE = maxForce;
        if (!isPreview)
        {
            Vector3 rescale = RESCALE.toVector3();
            base.GAME_OBJECT.transform.localScale = rescale;
        }
    }

    public double getClimbAngle()
    {
        return CLIMB_ANGLE;
    }

    public double getMaxSpeed()
    {
        return MAX_SPEED;
    }

    public double getMaxForce()
    {
        return MAX_FORCE;
    }

    public override string[] getStatStrings()
    {
        List<string> partStatStrings = new List<string>();
        partStatStrings.AddRange(base.getStatStrings());
        partStatStrings.Add(StringTools.formatString(CLIMB_ANGLE));
        partStatStrings.Add(StringTools.formatString(MAX_SPEED));
        partStatStrings.Add(StringTools.formatString(MAX_FORCE));
        for (int partStatStringIndex = 0; partStatStringIndex < STAT_PREFIXES.Length; ++partStatStringIndex)
            partStatStrings[partStatStringIndex + base.getStatStrings().Length] = STAT_PREFIXES[partStatStringIndex] + partStatStrings[partStatStringIndex + base.getStatStrings().Length] + STAT_SUFFIXES[partStatStringIndex];
        return partStatStrings.ToArray();
    }

    public override double[] compareTo(Part otherPart)
    {
        if (otherPart.GetType() == this.GetType())
        {
            List<double> differenceInStats = new List<double>();
            Mobility comparablePart = (Mobility)otherPart;
            differenceInStats.AddRange(base.compareTo(otherPart));
            differenceInStats.Add(comparablePart.getClimbAngle() - CLIMB_ANGLE);
            differenceInStats.Add(comparablePart.getMaxSpeed() - MAX_SPEED);
            differenceInStats.Add(comparablePart.getMaxForce() - MAX_FORCE);
            return differenceInStats.ToArray();
        }
        else return base.compareTo(otherPart);
    }

    public override double[] getStats()
    {
        List<double> stats = new List<double>();
        stats.AddRange(base.getStats());
        stats.Add(CLIMB_ANGLE);
        stats.Add(MAX_SPEED);
        stats.Add(MAX_FORCE);
        return stats.ToArray();
    }

    public override Part clone(bool isPreview)
    {
        return new Mobility(isPreview, base.ID, base.getImage(), base.getIcon(), base.getWeight(), base.getShape(), base.getDurability(), base.getRemainingDurability(), CLIMB_ANGLE, MAX_SPEED, MAX_FORCE);
    }

    public override bool equals(Part part)
    {
        Mobility mobility = (Mobility)part;
        return base.equals(part) && CLIMB_ANGLE == mobility.getClimbAngle() && MAX_SPEED == mobility.getMaxSpeed() && MAX_FORCE == mobility.getMaxForce();
    }
}