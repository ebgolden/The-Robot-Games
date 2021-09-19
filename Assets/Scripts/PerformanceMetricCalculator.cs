using UnityEngine;

public class PerformanceMetricCalculator
{
    private readonly float MAX_PERFORMANCE = 100, IMPROVEMENT_INCENTIVE_PERCENTAGE = .5f;
    private readonly int MIN_PERFORMANCE_METRIC = 1;

    public long calculatePerformanceMetric(double damageDifference, double maxDamageDifference, double timeElapsed, double previousDamageDifference, double previousMaxDamageDifference, double previousTimeElapsed)
    {
        double timeElapsedInSeconds = timeElapsed / 1000;
        if (timeElapsedInSeconds < 1)
            timeElapsedInSeconds = 1;
        double previousTimeElapsedInSeconds = previousTimeElapsed / 1000;
        if (previousTimeElapsedInSeconds < 1)
            previousTimeElapsedInSeconds = 1;
        float performancePercentage = (float)(damageDifference / timeElapsedInSeconds) / (float)maxDamageDifference;
        float previousPerformancePercentage = 0;
        if (previousMaxDamageDifference <= 0)
            previousPerformancePercentage = 1;
        else previousPerformancePercentage = (float)(previousDamageDifference / previousTimeElapsedInSeconds) / (float)previousMaxDamageDifference;
        int performanceMetric = Mathf.RoundToInt(performancePercentage * MAX_PERFORMANCE + (performancePercentage / previousPerformancePercentage) * IMPROVEMENT_INCENTIVE_PERCENTAGE * MAX_PERFORMANCE);
        return (performanceMetric >= MIN_PERFORMANCE_METRIC ? performanceMetric : MIN_PERFORMANCE_METRIC);
    }

    public int calculateCost(Part part)
    {
        double cost = (part.getDurability() > 0 ? part.getDurability() : 1) / ((part.getWeight() / GameEngine.GRAVITY) > 0 ? (part.getWeight() / GameEngine.GRAVITY) : 1);
        if (part is Body)
            cost *= (((Body)part).getMaxAttachments() > 0 ? ((Body)part).getMaxAttachments() : 1);
        else if (part is Mobility)
            cost *= (((Mobility)part).getClimbAngle() > 0 ? ((Mobility)part).getClimbAngle() : 1) * ((Mobility)part).getMaxSpeed() * ((Mobility)part).getMaxForce();
        return Mathf.RoundToInt((float)cost);
    }

    public int calculateCost(Attachment part)
    {
        double cost = (part.getDurability() > 0 ? part.getDurability() : 1) / ((part.getWeight() / GameEngine.GRAVITY) > 0 ? (part.getWeight() / GameEngine.GRAVITY) : 1);
        cost *= (part.getMaxUsageTime() > 0 ? part.getMaxUsageTime() / 1000 : 1) * (part.getMaxDamage() > 0 ? part.getMaxDamage() : 1) / ((part.getMinChargeTime() > 0 ? part.getMinChargeTime() / 1000 : 1) * (part.getMaxChargeTime() > 0 ? part.getMaxChargeTime() / 1000 : 1) * (part.getMinCoolingTime() > 0 ? part.getMinCoolingTime() / 1000 : 1));
        return Mathf.RoundToInt((float)cost);
    }

    public int calculateCostToRepair(int cost, double durability, double remainingDurability)
    {
        double damageToRepair = durability - remainingDurability;
        double costToRepair = damageToRepair / durability * (double)cost;
        return Mathf.RoundToInt((float)costToRepair);
    }
}