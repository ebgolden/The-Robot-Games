using System.Collections.Generic;
using TinyJson;

public class PlayerDataManager
{
    public PlayerData dataToPlayerData(long experience, long credits, double previousRoundDamageDifference, double previousRoundMaxDamageDifference, double previousRoundTimeElapsed, List<Part> humanRobotParts, List<Robot> myRobots, List<ObstacleData> obstaclesData)
    {
        PlayerData playerData = new PlayerData();
        List<PlayerPartData> playerPartsData = new List<PlayerPartData>();
        foreach (Part part in humanRobotParts)
        {
            PlayerPartData playerPartData = new PlayerPartData();
            playerPartData.id = part.getID();
            playerPartData.remainingDurability = part.getRemainingDurability();
            playerPartsData.Add(playerPartData);
        }
        List<RobotData> robotsData = new List<RobotData>();
        foreach (Robot robot in myRobots)
        {
            RobotData robotData = new RobotData();
            robotData.name = robot.getName();
            robotData.human = robot.isHuman();
            List<int> partIndices = new List<int>();
            Part[] parts = robot.getParts();
            for (int partIndex = 0; partIndex < parts.Length; ++partIndex)
                partIndices.Add(playerPartsData.FindIndex(p => (p.id == parts[partIndex].getID())));
            robotData.partIndices = partIndices.ToArray();
            robotsData.Add(robotData);
        }
        playerData.experience = experience;
        playerData.credits = credits;
        playerData.previousRoundDamageDifference = previousRoundDamageDifference;
        playerData.previousRoundMaxDamageDifference = previousRoundMaxDamageDifference;
        playerData.previousRoundTimeElapsed = previousRoundTimeElapsed;
        playerData.humanRobotParts = playerPartsData.ToArray();
        playerData.myRobots = robotsData.ToArray();
        playerData.obstacles = obstaclesData.ToArray();
        return playerData;
    }

    public PlayerData getPlayerDataFromJSON(string playerDataJSON)
    {
        string playerDataString = playerDataJSON.Substring(1, playerDataJSON.Length - 2);
        playerDataString = playerDataString.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
        if (playerDataString[0] == '[')
            playerDataString = playerDataString.Substring(1);
        if (playerDataString[playerDataString.Length - 1] == ']')
            playerDataString = playerDataString.Substring(0, playerDataString.Length - 1);
        playerDataString = "{" + playerDataString;
        return playerDataString.FromJson<PlayerData>();
    }
}