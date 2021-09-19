using System.Collections.Generic;
using UnityEngine;

public class MyRobots
{
    private List<Robot> myRobots;
    private List<Part> humanRobotParts;
    private long credits;
    private readonly MyRobotsCard MY_ROBOTS_CARD;
    private MyRobotsCard.MODES mode;
    private Color colorScheme;

    public MyRobots(Robot[] myRobots, Part[] humanRobotParts, long credits, Color colorScheme)
    {
        this.myRobots = new List<Robot>();
        this.myRobots.AddRange(myRobots);
        this.humanRobotParts = new List<Part>();
        this.humanRobotParts.AddRange(humanRobotParts);
        this.credits = credits;
        this.colorScheme = colorScheme;
        MY_ROBOTS_CARD = new MyRobotsCard(this.myRobots.ToArray(), this.humanRobotParts.ToArray(), this.credits, this.colorScheme);
        MY_ROBOTS_CARD.enable();
    }

    public void updateGUI()
    {
        
    }

    public void update(Color colorScheme)
    {
        this.colorScheme = colorScheme;
        MY_ROBOTS_CARD.update(myRobots.ToArray(), humanRobotParts.ToArray(), credits, this.colorScheme);
    }

    public void updateMyRobots(List<Robot> myRobots)
    {
        this.myRobots = myRobots;
        MY_ROBOTS_CARD.update(this.myRobots.ToArray(), humanRobotParts.ToArray(), credits, colorScheme);
    }

    public void updateCredits(long credits)
    {
        this.credits = credits;
        MY_ROBOTS_CARD.update(myRobots.ToArray(), humanRobotParts.ToArray(), credits, colorScheme);
        MY_ROBOTS_CARD.updateCredits();
    }

    public void updateHumanParts(List<Part> humanRobotParts)
    {
        this.humanRobotParts = humanRobotParts;
        MY_ROBOTS_CARD.update(myRobots.ToArray(), this.humanRobotParts.ToArray(), credits, colorScheme);
    }

    public Robot getRobotBeingPreviewed()
    {
        return MY_ROBOTS_CARD.getRobotBeingPreviewed();
    }

    public Robot getRobotBeingPicked()
    {
        return MY_ROBOTS_CARD.getRobotBeingPicked();
    }

    public Robot getNewRobot()
    {
        return MY_ROBOTS_CARD.getNewRobot();
    }

    public List<Robot> getRobotsToRemove()
    {
        return MY_ROBOTS_CARD.getRobotsToRemove();
    }

    public void clearPickedAndNewRobotAndRobotsToRemove()
    {
        MY_ROBOTS_CARD.clearPickedAndNewRobotAndRobotsToRemove();
    }

    public void destroyAllGeneratedObjects()
    {
        MY_ROBOTS_CARD.destroyAllGeneratedObjects();
    }
}