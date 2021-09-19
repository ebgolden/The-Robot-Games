using System.Collections.Generic;
using System;

public class QLearning
{
    public enum SUB_STATES { WEAPON_COOLED, WEAPON_COOLING,
                            ATTACHMENT_COOLED, ATTACHMENT_COOLING,
                            WEAPON_CHARGING, WEAPON_CHARGED, WEAPON_READY,
                            UNDER_ATTACK, NO_ATTACK,
                            HITTING_OBSTACLE, NO_OBSTACLE,
                            CAN_MOVE, CANNOT_MOVE,
                            OPPONENTS_VISIBLE, OPPONENTS_INVISIBLE };
    private static string[] states = States.STATE_STRINGS;
    private readonly int NUMBER_OF_STATES = states.Length;
    private readonly int NUMBER_OF_ACTIONS = Enum.GetNames(typeof(GameEngine.ACTIONS)).Length;
    private readonly double EPSILON = 0.3; //explore percentage
    private readonly double ALPHA = 0.9; //learning rate
    private readonly double GAMMA = 0.85; //discount factor
    private readonly Random RANDOM;
    private List<KeyValuePair<string, List<double>>> qTable;
    private string priorState;
    private int priorAction;

    public QLearning()
    {
        RANDOM = new Random();
        priorState = states[0];
        priorAction = 0;
        qTable = new List<KeyValuePair<string, List<double>>>();
        for (int stateIndex = 0; stateIndex < NUMBER_OF_STATES; ++stateIndex)
        {
            KeyValuePair<string, List<double>> stateActionPair = new KeyValuePair<string, List<double>>(states[stateIndex], new List<double>());
            for (int actionIndex = 0; actionIndex < NUMBER_OF_ACTIONS; ++actionIndex)
                stateActionPair.Value.Add(0);
            qTable.Add(stateActionPair);
        }
    }

    public QLearning(List<KeyValuePair<string, List<double>>> qTable)
    {
        priorState = states[0];
        priorAction = 0;
        this.qTable = qTable;
    }

    public GameEngine.ACTIONS generateAction(double reward, string state)
    {
        int action = 0;
        int indexOfState = getIndexOfState(state);
        int maxQindex = getMaxQ(qTable[indexOfState].Value);
        double maxQForNewState = qTable[indexOfState].Value[maxQindex];
        if (reward != 0)
            qTable[getIndexOfState(priorState)].Value[priorAction] += ALPHA * (reward + GAMMA * maxQForNewState - qTable[getIndexOfState(priorState)].Value[priorAction]);
        if (RANDOM.NextDouble() < EPSILON) //explore
            action = RANDOM.Next(NUMBER_OF_ACTIONS);
        else //exploit
            action = getMaxQ(qTable[indexOfState].Value);
        priorState = state;
        priorAction = action;
        return (GameEngine.ACTIONS)action;
    }

    private int getMaxQ(List<double> actionList)
    {
        int indexOfMax = 0, qIndex = 0;
        foreach (double q in actionList)
        {
            if (q > actionList[indexOfMax])
                indexOfMax = qIndex;
            ++qIndex;
        }
        return indexOfMax;
    }

    private int getIndexOfState(string state)
    {
        int qTableIndex = 0;
        foreach (KeyValuePair<string, List<double>> keyValuePair in qTable)
        {
            if (keyValuePair.Key.Equals(state))
                break;
            else ++qTableIndex;
        }
        if (qTableIndex == qTable.Count)
            qTableIndex = -1;
        return qTableIndex;
    }

    public List<KeyValuePair<string, List<double>>> getQTable()
    {
        return qTable;
    }
}