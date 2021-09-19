using System.Collections.Generic;
using TinyJson;

public class BuildHubStateDataManager : StateDataManager
{
    public BuildHubState stateDataToState(BuildHubStateData stateData)
    {
        BuildHubState state = new BuildHubState();
        state.explorationRate = stateData.explorationRate;
        state.effectOnObstacle = stateData.effectOnObstacle;
        state.canClimbObstacle = stateData.canClimbObstacle;
        state.canMoveOnObstacle = stateData.canMoveOnObstacle;
        state.durable = stateData.durable;
        state.invisible = stateData.invisible;
        state.fast = stateData.fast;
        state.hasEffect = stateData.hasEffect;
        state.heavyDamage = stateData.heavyDamage;
        state.quickCooling = stateData.quickCooling;
        state.quickCharging = stateData.quickCharging;
        state.actions = actionsDataToActions(stateData.actions);
        return state;
    }

    private List<Action> actionsDataToActions(ActionData[] actions)
    {
        List<Action> actionList = new List<Action>();
        foreach (BuildHubActionData actionData in actions)
        {
            BuildHubAction action = new BuildHubAction();
            action.equipt = actionData.equipt;
            action.replace = actionData.replace;
            action.qValue = actionData.qValue;
            actionList.Add(action);
        }
        return actionList;
    }

    protected override ActionData[] actionsToActionsData(List<Action> actions)
    {
        List<ActionData> actionDataList = new List<ActionData>();
        foreach (BuildHubAction action in actions)
        {
            BuildHubActionData actionData = new BuildHubActionData();
            actionData.equipt = action.equipt;
            actionData.replace = action.replace;
            actionData.qValue = action.qValue;
            actionDataList.Add(actionData);
        }
        return actionDataList.ToArray();
    }

    public BuildHubStateData stateToStateData(BuildHubState state)
    {
        BuildHubStateData stateData = new BuildHubStateData();
        stateData.explorationRate = state.explorationRate;
        stateData.effectOnObstacle = state.effectOnObstacle;
        stateData.canClimbObstacle = state.canClimbObstacle;
        stateData.canMoveOnObstacle = state.canMoveOnObstacle;
        stateData.durable = state.durable;
        stateData.invisible = state.invisible;
        stateData.fast = state.fast;
        stateData.hasEffect = state.hasEffect;
        stateData.heavyDamage = state.heavyDamage;
        stateData.quickCooling = state.quickCooling;
        stateData.quickCharging = state.quickCharging;
        stateData.actions = actionsToActionsData(state.actions);
        return stateData;
    }

    public List<BuildHubStateData> getStateDataFromJSON(string stateJSON)
    {
        string stateString = stateJSON.Substring(1, stateJSON.Length - 2);
        stateString = stateString.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
        if (stateString[0] == '[')
            stateString = stateString.Substring(1);
        if (stateString[stateString.Length - 1] == ']')
            stateString = stateString.Substring(0, stateString.Length - 1);
        stateString = "{" + stateString;
        return stateString.FromJson<List<BuildHubStateData>>();
    }
}