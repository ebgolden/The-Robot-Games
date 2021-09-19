using System.Collections.Generic;

public abstract class Attachment : Part
{
    protected readonly Effect<Attachment> EFFECT;
    protected readonly int MIN_CHARGE_TIME, MAX_CHARGE_TIME, MAX_USAGE_TIME, MIN_COOLING_TIME;
    protected readonly double MAX_DAMAGE;
    protected readonly bool WEAPON, PASSIVE, INVISIBLE;
    protected long chargeStartTime, usageStartTime, coolingStartTime;
    protected double coolingTimeNeeded;
    private bool inUse;
    private readonly string[] STAT_PREFIXES = { "Effect: ", "Min charge time: ", "Max charge time: ", "Max usage time: ", "Min cooling time: ", "Weapon: ", "Passive: ", "Invisibility: ", "Max damage: " }, STAT_SUFFIXES = { "", " sec", " sec", " sec", " sec", "", "", "", "" };
    private readonly string DEFAULT_STAT_STRING = "None";

    public Attachment(string id, Image image, double weight, Effect<Attachment> effect, Shape.SHAPES shape, double durability, double remainingDurability, int minChargeTime, int maxChargeTime, int maxUsageTime, int minCoolingTime, double maxDamage, bool weapon, bool passive, bool invisible): base(id, image, weight, shape, durability, remainingDurability)
    {
        EFFECT = effect;
        MIN_CHARGE_TIME = minChargeTime;
        MAX_CHARGE_TIME = maxChargeTime;
        MAX_USAGE_TIME = maxUsageTime;
        MIN_COOLING_TIME = minCoolingTime;
        MAX_DAMAGE = maxDamage;
        WEAPON = weapon;
        PASSIVE = passive;
        INVISIBLE = invisible;
        chargeStartTime = 0;
        usageStartTime = 0;
        coolingStartTime = 0;
        coolingTimeNeeded = 0;
        inUse = false;
        base.destroyGameObject();
    }

    public Attachment(bool isPreview, string id, Image image, double weight, Effect<Attachment> effect, Shape.SHAPES shape, double durability, double remainingDurability, int minChargeTime, int maxChargeTime, int maxUsageTime, int minCoolingTime, double maxDamage, bool weapon, bool passive, bool invisible) : base(isPreview, id, image, image.getTexture(), weight, shape, durability, remainingDurability)
    {
        EFFECT = effect;
        MIN_CHARGE_TIME = minChargeTime;
        MAX_CHARGE_TIME = maxChargeTime;
        MAX_USAGE_TIME = maxUsageTime;
        MIN_COOLING_TIME = minCoolingTime;
        MAX_DAMAGE = maxDamage;
        WEAPON = weapon;
        PASSIVE = passive;
        INVISIBLE = invisible;
        chargeStartTime = 0;
        usageStartTime = 0;
        coolingStartTime = 0;
        coolingTimeNeeded = 0;
        inUse = false;
        if (!isPreview)
            base.destroyGameObject();
    }

    public int getMinChargeTime()
    {
        return MIN_CHARGE_TIME;
    }

    public int getMaxChargeTime()
    {
        return MAX_CHARGE_TIME;
    }

    public int getMaxUsageTime()
    {
        return MAX_USAGE_TIME;
    }

    public int getMinCoolingTime()
    {
        return MIN_COOLING_TIME;
    }

    public double getMaxDamage()
    {
        return MAX_DAMAGE;
    }

    public bool isWeapon()
    {
        return WEAPON;
    }

    public bool isPassive()
    {
        return PASSIVE;
    }

    public bool isInvisible()
    {
        return INVISIBLE;
    }

    public long getElapsedChargeTime()
    {
        long elapsedChargeTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - chargeStartTime;
        if (chargeStartTime > 0)
            return (elapsedChargeTime <= MAX_CHARGE_TIME ? elapsedChargeTime : MAX_CHARGE_TIME);
        return 0;
    }

    public long getElapsedUsageTime()
    {
        if (usageStartTime > 0)
            return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - usageStartTime;
        return 0;
    }

    public long getElapsedCoolingTime()
    {
        if (coolingStartTime > 0)
            return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - coolingStartTime;
        return 0;
    }

    public bool isCooled()
    {
        if (coolingStartTime > 0)
        {
            bool isCooled = getElapsedCoolingTime() >= coolingTimeNeeded;
            if (isCooled)
            {
                coolingStartTime = 0;
                coolingTimeNeeded = 0;
                chargeStartTime = 0;
                usageStartTime = 0;
            }
            return isCooled;
        }
        return true;
    }

    private void startCooling()
    {
        inUse = false;
        long elapsedUsageTime = getElapsedUsageTime();
        long elapsedChargeTime = getElapsedChargeTime();
        usageStartTime = 0;
        chargeStartTime = 0;
        coolingTimeNeeded = MIN_COOLING_TIME + (elapsedChargeTime > 0 ? (double)elapsedChargeTime / (double)MAX_CHARGE_TIME : (double)elapsedUsageTime / (double)MAX_USAGE_TIME) * MIN_COOLING_TIME;
        coolingStartTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public bool canFire()
    {
        long elapsedChargeTime = getElapsedChargeTime();
        if (isCooled())
        {
            if (elapsedChargeTime >= MIN_CHARGE_TIME)
                return true;
            chargeStartTime = 0;
            return false;
        }
        else if (coolingStartTime == 0)
            startCooling();
        return false;
    }

    public void charge()
    {
        if (chargeStartTime == 0)
            chargeStartTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public abstract Projectile fire(Robot robot, float hitDistance);

    protected double calculateDamage()
    {
        long elapsedChargeTime = getElapsedChargeTime();
        if (elapsedChargeTime > MAX_CHARGE_TIME)
            return MAX_DAMAGE;
        return MAX_DAMAGE * (double)elapsedChargeTime / (double)MAX_CHARGE_TIME;
    }

    protected void stopFiring()
    {
        startCooling();
    }

    public bool canUse()
    {
        if (isCooled() && getElapsedUsageTime() <= MAX_USAGE_TIME)
            return true;
        else if (coolingStartTime == 0)
            startCooling();
        else if (double.IsNaN(coolingTimeNeeded))
        {
            long elapsedUsageTime = getElapsedUsageTime();
            long elapsedChargeTime = getElapsedChargeTime();
            coolingTimeNeeded = MIN_COOLING_TIME + (elapsedChargeTime > 0 ? (double)elapsedChargeTime / (double)MAX_CHARGE_TIME : (double)elapsedUsageTime / (double)MAX_USAGE_TIME) * MIN_COOLING_TIME;
            coolingStartTime = 0;
            return true;
        }
        return false;
    }

    public bool isCharging()
    {
        return chargeStartTime != 0;
    }

    public bool isCharged()
    {
        return getElapsedChargeTime() >= MAX_CHARGE_TIME;
    }

    public abstract Robot use(Robot robot);

    public void stopUsing()
    {
        startCooling();
    }

    public void resetCharging()
    {
        chargeStartTime = 0;
    }

    public bool isInUse()
    {
        return inUse;
    }

    public Effect<Attachment> getEffect()
    {
        return EFFECT;
    }

    public void adjustTime(long timeOffset)
    {
        if (chargeStartTime > 0)
            chargeStartTime += timeOffset;
        if (usageStartTime > 0)
            usageStartTime += timeOffset;
        if (coolingStartTime > 0)
            coolingStartTime += timeOffset;
    }

    public override string[] getStatStrings()
    {
        List<string> partStatStrings = new List<string>();
        partStatStrings.AddRange(base.getStatStrings());
        partStatStrings.Add(EFFECT == null ? DEFAULT_STAT_STRING : EFFECT.GetType().ToString());
        partStatStrings.Add(StringTools.formatString((double)MIN_CHARGE_TIME / 1000));
        partStatStrings.Add(StringTools.formatString((double)MAX_CHARGE_TIME / 1000));
        partStatStrings.Add(StringTools.formatString((double)MAX_USAGE_TIME / 1000));
        partStatStrings.Add(StringTools.formatString((double)MIN_COOLING_TIME / 1000));
        partStatStrings.Add(WEAPON.ToString());
        partStatStrings.Add(PASSIVE.ToString());
        partStatStrings.Add(INVISIBLE.ToString());
        partStatStrings.Add(MAX_DAMAGE == 0 ? DEFAULT_STAT_STRING : StringTools.formatString(MAX_DAMAGE));
        for (int partStatStringIndex = 0; partStatStringIndex < STAT_PREFIXES.Length; ++partStatStringIndex)
            partStatStrings[partStatStringIndex + base.getStatStrings().Length] = STAT_PREFIXES[partStatStringIndex] + partStatStrings[partStatStringIndex + base.getStatStrings().Length] + STAT_SUFFIXES[partStatStringIndex];
        return partStatStrings.ToArray();
    }

    public override double[] compareTo(Part otherPart)
    {
        if (otherPart is Attachment)
        {
            List<double> differenceInStats = new List<double>();
            Attachment comparablePart = (Attachment)otherPart;
            differenceInStats.AddRange(base.compareTo(otherPart));
            differenceInStats.Add(comparablePart.getMinChargeTime() - MIN_CHARGE_TIME);
            differenceInStats.Add(comparablePart.getMaxChargeTime() - MAX_CHARGE_TIME);
            differenceInStats.Add(comparablePart.getMaxUsageTime() - MAX_USAGE_TIME);
            differenceInStats.Add(comparablePart.getMinCoolingTime() - MIN_COOLING_TIME);
            differenceInStats.Add(comparablePart.getMaxDamage() - MAX_DAMAGE);
            return differenceInStats.ToArray();
        }
        else return base.compareTo(otherPart);
    }

    public override double[] getStats()
    {
        List<double> stats = new List<double>();
        stats.AddRange(base.getStats());
        stats.Add(EFFECT != null ? 1 : 0);
        stats.Add(MIN_CHARGE_TIME);
        stats.Add(MAX_CHARGE_TIME);
        stats.Add(MAX_USAGE_TIME);
        stats.Add(MIN_COOLING_TIME);
        stats.Add(WEAPON ? 1 : 0);
        stats.Add(PASSIVE ? 1 : 0);
        stats.Add(INVISIBLE ? 1 : 0);
        stats.Add(MAX_DAMAGE);
        return stats.ToArray();
    }

    public abstract override Part clone(bool isPreview);

    public override bool equals(Part part)
    {
        Attachment attachment = (Attachment)part;
        bool effectsAreEqual = (EFFECT == null) == (attachment.getEffect() == null);
        if (!effectsAreEqual)
            return false;
        if (EFFECT != null)
            effectsAreEqual = EFFECT.GetType() == attachment.getEffect().GetType();
        return base.equals(part) && GetType() == attachment.GetType() && effectsAreEqual && MIN_CHARGE_TIME == attachment.getMinChargeTime() && MAX_CHARGE_TIME == attachment.getMaxChargeTime() && MAX_USAGE_TIME == attachment.getMaxUsageTime() && MIN_COOLING_TIME == attachment.getMinCoolingTime() && MAX_DAMAGE == attachment.getMaxDamage() && WEAPON == attachment.isWeapon() && PASSIVE == attachment.isPassive() && INVISIBLE == attachment.isInvisible();
    }
}