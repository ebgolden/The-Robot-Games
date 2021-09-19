using System.Collections.Generic;

public abstract class StateDataManager
{
    protected abstract ActionData[] actionsToActionsData(List<Action> actions);
}