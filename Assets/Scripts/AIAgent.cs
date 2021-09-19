using System.Collections.Generic;
using System.Linq;
using System;

public class AIAgent
{
    private readonly int MAX_ACTIONS, NUMBER_OF_NON_PASSIVE_ATTACHMENTS;
    private readonly Robot BOT;
    private readonly List<BuildHubState> BUILD_HUB_STATES;
    private readonly List<FieldState> FIELD_STATES;
    private double discountFactor, learningRate, inverseSensitivity;
    private readonly double EXPLORATION_RATE_ACTION_WEIGHT;
    private FieldState priorState;
    private FieldAction priorAction;
    private readonly List<FieldAction> BASE_ACTIONS;
    private readonly Random RANDOM;
    private long priorTime = 0;

    public AIAgent(Robot robot, List<BuildHubState> buildHubStates, List<FieldState> fieldStates, double discountFactor, double learningRate, double inverseSensitivity)
    {
        BOT = robot;
        BUILD_HUB_STATES = buildHubStates;
        FIELD_STATES = fieldStates;
        if (FIELD_STATES == default)
            FIELD_STATES = new List<FieldState>();
        this.discountFactor = discountFactor;
        this.learningRate = learningRate;
        this.inverseSensitivity = inverseSensitivity;
        priorState = null;
        priorAction = null;
        RANDOM = new Random();
        NUMBER_OF_NON_PASSIVE_ATTACHMENTS = BOT.getAttachments().FindAll(attachment => !attachment.isPassive()).Count();
        MAX_ACTIONS = (int)Math.Round(Math.Pow(2, typeof(FieldAction).GetFields().Length - 2)) * (NUMBER_OF_NON_PASSIVE_ATTACHMENTS + 1);
        EXPLORATION_RATE_ACTION_WEIGHT = 1.0 / MAX_ACTIONS;
        BASE_ACTIONS = generateActions();
    }

    private List<FieldAction> generateActions()
    {
        List<FieldAction> actions = new List<FieldAction>();
        for (int moveUp = 0; moveUp <= 1; ++moveUp)
        {
            for (int moveRight = 0; moveRight <= 1; ++moveRight)
            {
                for (int moveDown = 0; moveDown <= 1; ++moveDown)
                {
                    for (int moveLeft = 0; moveLeft <= 1; ++moveLeft)
                    {
                        for (int rotateClockwise = 0; rotateClockwise <= 1; ++rotateClockwise)
                        {
                            for (int rotateCounterClockwise = 0; rotateCounterClockwise <= 1; ++rotateCounterClockwise)
                            {
                                for (int lookUp = 0; lookUp <= 1; ++lookUp)
                                {
                                    for (int lookDown = 0; lookDown <= 1; ++lookDown)
                                    {
                                        for (int chargeAttachment = 0; chargeAttachment <= 1; ++chargeAttachment)
                                        {
                                            for (int useAttachment = 0; useAttachment <= 1; ++useAttachment)
                                            {
                                                for (int pickAttachment = 0; pickAttachment < NUMBER_OF_NON_PASSIVE_ATTACHMENTS; ++pickAttachment)
                                                {
                                                    FieldAction action = new FieldAction();
                                                    action.moveUp = moveUp == 1;
                                                    action.moveRight = moveRight == 1;
                                                    action.moveDown = moveDown == 1;
                                                    action.moveLeft = moveLeft == 1;
                                                    action.rotateClockwise = rotateClockwise == 1;
                                                    action.rotateCounterClockwise = rotateCounterClockwise == 1;
                                                    action.lookUp = lookUp == 1;
                                                    action.lookDown = lookDown == 1;
                                                    action.chargeAttachment = chargeAttachment == 1;
                                                    action.useAttachment = useAttachment == 1;
                                                    action.pickAttachment = BOT.getAttachments()[pickAttachment];
                                                    actions.Add(action);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return actions;
    }

    public Robot getBot()
    {
        return BOT;
    }

    public void applyRewardForActionInState(double reward)
    {
        if (reward != 0 && priorState != null && priorAction != null && priorState.getActions() != null && priorState.getActions().Count() > 0)
        {
            List<Action> actions = priorState.getActions();
            double priorQValue = priorAction.qValue;
            FieldAction optimalAction = (FieldAction)actions[0];
            foreach (Action action in actions)
                if (action.qValue > optimalAction.qValue)
                    optimalAction = (FieldAction)action;
            priorAction.qValue += learningRate * (reward + discountFactor * optimalAction.qValue - priorAction.qValue);
            double growth = Math.Pow(Math.E, -Math.Abs(priorAction.qValue - priorQValue) / inverseSensitivity);
            priorState.explorationRate = EXPLORATION_RATE_ACTION_WEIGHT * (1.0 - growth) / (1.0 + growth) + (1.0 - EXPLORATION_RATE_ACTION_WEIGHT) * priorState.explorationRate;
            if (priorState.explorationRate > 1)
                priorState.explorationRate = 1;
            if (priorState.explorationRate < 0)
                priorState.explorationRate = 0;
        }
    }

    public FieldAction getActionForState(FieldState state)
    {
        if (priorTime == 0)
            priorTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        priorTime = currentTime;
        if (state != null)
            priorState = state;
        else state = priorState;
        if ((FIELD_STATES.Count() == 0) || !FIELD_STATES.Exists(s => s.Compare(state) == 0))
        {
            List<Action> actionsForState = null;
            if (FIELD_STATES.Count == 0 || RANDOM.NextDouble() < priorState.explorationRate)
                actionsForState = generateActionsForState();
            else actionsForState = getClosestState(state).getActions();
            addStateActionPair(state, actionsForState);
        }
        else priorState = FIELD_STATES.Find(s => s.Compare(state) == 0);
        List<Action> actions = default;
        if (FIELD_STATES.Count > 0)
            actions = priorState.getActions();
        if ((actions == default) || (FIELD_STATES.Count == 0) || ((priorState.explorationRate > 0) && (RANDOM.NextDouble() < priorState.explorationRate)))
        {
            int actionIndex = RANDOM.Next(MAX_ACTIONS);
            if (actions != default && FIELD_STATES.Count > 0 && actionIndex < actions.Count())
                priorAction = (FieldAction)actions[actionIndex];
        }
        else
        {
            FieldAction optimalAction = (FieldAction)actions[0];
            foreach (Action action in actions)
                if (action.qValue > optimalAction.qValue)
                    optimalAction = (FieldAction)action;
            priorAction = optimalAction;
        }
        return priorAction;
    }

    private void addStateActionPair(FieldState state, List<Action> actions)
    {
        state.setActions(actions);
        FIELD_STATES.Add(state);
    }

    private FieldState getClosestState(FieldState state)
    {
        FieldState closestState = null;
        int closestStateIndex = 0;
        double closestStateRating = 0;
        for (int stateIndex = 0; stateIndex < FIELD_STATES.Count(); ++stateIndex)
        {
            double stateRating = FIELD_STATES[stateIndex].Compare(state);
            if (stateRating > closestStateRating)
            {
                closestStateRating = stateRating;
                closestStateIndex = stateIndex;
            }
        }
        closestState = FIELD_STATES[closestStateIndex];
        return closestState;
    }

    private List<Action> generateActionsForState()
    {
        FieldAction[] actions = new FieldAction[BASE_ACTIONS.Count];
        BASE_ACTIONS.CopyTo(actions);
        List<Action> actionsForState = new List<Action>();
        actionsForState.AddRange(actions);
        return actionsForState;
    }
    
    private FieldAction generateActionForState(List<FieldAction> actionsInState)
    {
        FieldAction action = new FieldAction();
        action.moveUp = RANDOM.NextDouble() > .5f;
        action.moveRight = RANDOM.NextDouble() > .5f;
        action.moveDown = RANDOM.NextDouble() > .5f;
        action.moveLeft = RANDOM.NextDouble() > .5f;
        action.rotateClockwise = RANDOM.NextDouble() > .5f;
        action.rotateCounterClockwise = RANDOM.NextDouble() > .5f;
        action.lookUp = RANDOM.NextDouble() > .5f;
        action.lookDown = RANDOM.NextDouble() > .5f;
        action.chargeAttachment = RANDOM.NextDouble() > .5f;
        action.useAttachment = RANDOM.NextDouble() > .5f;
        action.pickAttachment = null;
        int attachmentIndex = RANDOM.Next(NUMBER_OF_NON_PASSIVE_ATTACHMENTS);
        if (attachmentIndex < NUMBER_OF_NON_PASSIVE_ATTACHMENTS)
            action.pickAttachment = BOT.getAttachments()[attachmentIndex];
        action.qValue = 0;
        if (actionsInState.Exists(a => a.equals(action)))
        {
            bool newAction = false;
            for (int moveUp = 0; moveUp <= 1; ++moveUp)
            {
                for (int moveRight = 0; moveRight <= 1; ++moveRight)
                {
                    for (int moveDown = 0; moveDown <= 1; ++moveDown)
                    {
                        for (int moveLeft = 0; moveLeft <= 1; ++moveLeft)
                        {
                            for (int rotateClockwise = 0; rotateClockwise <= 1; ++rotateClockwise)
                            {
                                for (int rotateCounterClockwise = 0; rotateCounterClockwise <= 1; ++rotateCounterClockwise)
                                {
                                    for (int lookUp = 0; lookUp <= 1; ++lookUp)
                                    {
                                        for (int lookDown = 0; lookDown <= 1; ++lookDown)
                                        {
                                            for (int chargeAttachment = 0; chargeAttachment <= 1; ++chargeAttachment)
                                            {
                                                for (int useAttachment = 0; useAttachment <= 1; ++useAttachment)
                                                {
                                                    for (int pickAttachment = 0; pickAttachment < NUMBER_OF_NON_PASSIVE_ATTACHMENTS; ++pickAttachment)
                                                    {
                                                        action.moveUp = moveUp == 1;
                                                        action.moveRight = moveRight == 1;
                                                        action.moveDown = moveDown == 1;
                                                        action.moveLeft = moveLeft == 1;
                                                        action.rotateClockwise = rotateClockwise == 1;
                                                        action.rotateCounterClockwise = rotateCounterClockwise == 1;
                                                        action.lookUp = lookUp == 1;
                                                        action.lookDown = lookDown == 1;
                                                        action.chargeAttachment = chargeAttachment == 1;
                                                        action.useAttachment = useAttachment == 1;
                                                        action.pickAttachment = BOT.getAttachments()[pickAttachment];
                                                        if (actionsInState.Exists(a => a.equals(action)))
                                                            continue;
                                                        else
                                                        {
                                                            newAction = true;
                                                            break;
                                                        }
                                                    }
                                                    if (newAction)
                                                        break;
                                                }
                                                if (newAction)
                                                    break;
                                            }
                                            if (newAction)
                                                break;
                                        }
                                        if (newAction)
                                            break;
                                    }
                                    if (newAction)
                                        break;
                                }
                                if (newAction)
                                    break;
                            }
                            if (newAction)
                                break;
                        }
                        if (newAction)
                            break;
                    }
                    if (newAction)
                        break;
                }
                if (newAction)
                    break;
            }
        }
        return action;
    }

    public List<BuildHubState> getBuildHubStates()
    {
        return BUILD_HUB_STATES;
    }

    public List<FieldState> getFieldStates()
    {
        return FIELD_STATES;
    }
}