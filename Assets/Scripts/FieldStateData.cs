public class FieldStateData
{
    public double explorationRate;
    public bool damageReceived, damageDealt, killed, lowDurability, touchingWall;
    public bool[] enemiesKilled, enemiesLowDurability, canSeeEnemies, touchingObstacles, enemiesCanHit, attachmentsCharging, attachmentsCooling;
    public int numberOfEnemies, timeSinceLastAction;
    public PlayerPartData[] attachments;
    public Point[] enemiesPosition;
    public bool[][] enemiesTouchingObstacles;
    public ActionData[] actions;
}