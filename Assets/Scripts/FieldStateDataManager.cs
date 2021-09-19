using System.Collections.Generic;
using TinyJson;

public class FieldStateDataManager : StateDataManager
{
    public FieldState stateDataToState(FieldStateData stateData, List<Part> parts)
    {
        FieldState state = new FieldState();
        state.explorationRate = stateData.explorationRate;
        state.damageReceived = stateData.damageReceived;
        state.damageDealt = stateData.damageDealt;
        state.killed = stateData.killed;
        state.lowDurability = stateData.lowDurability;
        state.touchingWall = stateData.touchingWall;
        state.enemiesKilled = stateData.enemiesKilled;
        state.enemiesLowDurability = stateData.enemiesLowDurability;
        state.canSeeEnemies = stateData.canSeeEnemies;
        state.touchingObstacles = stateData.touchingObstacles;
        state.enemiesCanHit = stateData.enemiesCanHit;
        state.attachmentsCharging = stateData.attachmentsCharging;
        state.attachmentsCooling = stateData.attachmentsCooling;
        state.numberOfEnemies = stateData.numberOfEnemies;
        state.timeSinceLastAction = stateData.timeSinceLastAction;
        List<Attachment> attachments = new List<Attachment>();
        foreach (PlayerPartData partData in stateData.attachments)
        {
            Part part = parts.Find(p => p.getID() == partData.id).clone(true);
            part.damage(part.getDurability() - partData.remainingDurability);
            attachments.Add((Attachment)part);
        }
        state.attachments = attachments.ToArray();
        state.enemiesPosition = stateData.enemiesPosition;
        state.enemiesTouchingObstacles = stateData.enemiesTouchingObstacles;
        state.actions = actionsDataToActions(stateData.actions, attachments);
        return state;
    }

    private List<Action> actionsDataToActions(ActionData[] actions, List<Attachment> parts)
    {
        List<Action> actionList = new List<Action>();
        foreach (FieldActionData actionData in actions)
        {
            FieldAction action = new FieldAction();
            action.moveUp = actionData.moveUp;
            action.moveRight = actionData.moveRight;
            action.moveDown = actionData.moveDown;
            action.moveLeft = actionData.moveLeft;
            action.rotateClockwise = actionData.rotateClockwise;
            action.rotateCounterClockwise = actionData.rotateCounterClockwise;
            action.lookUp = actionData.lookUp;
            action.lookDown = actionData.lookDown;
            action.chargeAttachment = actionData.chargeAttachment;
            action.useAttachment = actionData.useAttachment;
            action.pickAttachment = parts.Find(p => p.getID() == actionData.pickAttachment.id);
            action.qValue = actionData.qValue;
            actionList.Add(action);
        }
        return actionList;
    }

    protected override ActionData[] actionsToActionsData(List<Action> actions)
    {
        List<ActionData> actionDataList = new List<ActionData>();
        foreach (FieldAction action in actions)
        {
            FieldActionData actionData = new FieldActionData();
            actionData.moveUp = action.moveUp;
            actionData.moveRight = action.moveRight;
            actionData.moveDown = action.moveDown;
            actionData.moveLeft = action.moveLeft;
            actionData.rotateClockwise = action.rotateClockwise;
            actionData.rotateCounterClockwise = action.rotateCounterClockwise;
            actionData.lookUp = action.lookUp;
            actionData.lookDown = action.lookDown;
            actionData.chargeAttachment = action.chargeAttachment;
            actionData.useAttachment = action.useAttachment;
            actionData.pickAttachment = new PlayerPartData();
            actionData.pickAttachment.id = action.pickAttachment.getID();
            actionData.pickAttachment.remainingDurability = action.pickAttachment.getRemainingDurability();
            actionData.qValue = action.qValue;
            actionDataList.Add(actionData);
        }
        return actionDataList.ToArray();
    }

    public FieldStateData stateToStateData(FieldState state)
    {
        FieldStateData stateData = new FieldStateData();
        stateData.explorationRate = state.explorationRate;
        stateData.damageReceived = state.damageReceived;
        stateData.damageDealt = state.damageDealt;
        stateData.killed = state.killed;
        stateData.lowDurability = state.lowDurability;
        stateData.touchingWall = state.touchingWall;
        stateData.enemiesKilled = state.enemiesKilled;
        stateData.enemiesLowDurability = state.enemiesLowDurability;
        stateData.canSeeEnemies = state.canSeeEnemies;
        stateData.touchingObstacles = state.touchingObstacles;
        stateData.enemiesCanHit = state.enemiesCanHit;
        stateData.attachmentsCharging = state.attachmentsCharging;
        stateData.attachmentsCooling = state.attachmentsCooling;
        stateData.numberOfEnemies = state.numberOfEnemies;
        stateData.timeSinceLastAction = state.timeSinceLastAction;
        List<PlayerPartData> attachmentsData = new List<PlayerPartData>();
        foreach (Attachment attachment in state.attachments)
        {
            PlayerPartData partData = new PlayerPartData();
            partData.id = attachment.getID();
            partData.remainingDurability = attachment.getRemainingDurability();
            attachmentsData.Add(partData);
        }
        stateData.attachments = attachmentsData.ToArray();
        stateData.enemiesPosition = state.enemiesPosition;
        stateData.enemiesTouchingObstacles = state.enemiesTouchingObstacles;
        stateData.actions = actionsToActionsData(state.actions);
        return stateData;
    }

    public List<FieldStateData> getStateDataFromJSON(string stateJSON)
    {
        string stateString = stateJSON.Substring(1, stateJSON.Length - 2);
        stateString = stateString.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
        if (stateString[0] == '[')
            stateString = stateString.Substring(1);
        if (stateString[stateString.Length - 1] == ']')
            stateString = stateString.Substring(0, stateString.Length - 1);
        stateString = "{" + stateString;
        return stateString.FromJson<List<FieldStateData>>();
    }
}