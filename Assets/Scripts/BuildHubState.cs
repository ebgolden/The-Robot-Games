public class BuildHubState : State
{
    public bool effectOnObstacle = false, canClimbObstacle = false, canMoveOnObstacle = false, durable = false, invisible = false, fast = false, hasEffect = false, heavyDamage = false, quickCooling = false, quickCharging = false;
    public int indexOfSelectedAction = -1;

    public override double Compare(State state)
    {
        BuildHubState buildHubState = (BuildHubState)state;
        double rating = 0;
        rating += effectOnObstacle == buildHubState.effectOnObstacle ? 0 : 1;
        rating += canClimbObstacle == buildHubState.canClimbObstacle ? 0 : 1;
        rating += canMoveOnObstacle == buildHubState.canMoveOnObstacle ? 0 : 1;
        rating += durable == buildHubState.durable ? 0 : 1;
        rating += invisible == buildHubState.invisible ? 0 : 1;
        rating += fast == buildHubState.fast ? 0 : 1;
        rating += hasEffect == buildHubState.hasEffect ? 0 : 1;
        rating += heavyDamage == buildHubState.heavyDamage ? 0 : 1;
        rating += quickCooling == buildHubState.quickCooling ? 0 : 1;
        rating += quickCharging == buildHubState.quickCharging ? 0 : 1;
        rating /= 100.0;
        return rating;
    }
}