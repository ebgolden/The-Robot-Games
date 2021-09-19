public class BuildHubAction : Action
{
    public bool equipt = false, replace = false;

    public override bool equals(Action action)
    {
        BuildHubAction buildHubAction = (BuildHubAction)action;
        return equipt == buildHubAction.equipt
            && replace == buildHubAction.replace;
    }
}