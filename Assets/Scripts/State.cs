using System.Collections.Generic;
using System.Linq;

public abstract class State
{
    public double explorationRate = 1;
    public List<Action> actions;

    public State()
    {
        actions = new List<Action>();
    }

    public void setActions(List<Action> actions)
    {
        this.actions = actions;
    }

    public List<Action> getActions()
    {
        actions.OrderByDescending(action => action.qValue);
        return actions;
    }

    public abstract double Compare(State state);
}