using System.Collections.Generic;
using System.Linq;
using System;

public class FieldState : State
{
    public bool damageReceived = false, damageDealt = false, killed = false, lowDurability = false, touchingWall = false;
    public bool[] enemiesKilled, enemiesLowDurability, canSeeEnemies, touchingObstacles, enemiesCanHit, attachmentsCharging, attachmentsCooling;
    public int numberOfEnemies = 0, timeSinceLastAction = 0;
    public Attachment[] attachments;
    public Point[] enemiesPosition;
    public bool[][] enemiesTouchingObstacles;

    public override double Compare(State state)
    {
        FieldState fieldState = (FieldState)state;
        double rating = 0;
        rating += damageReceived == fieldState.damageReceived ? 0 : 1;
        rating += damageDealt == fieldState.damageDealt ? 0 : 1;
        rating += killed == fieldState.killed ? 0 : 1;
        rating += lowDurability == fieldState.lowDurability ? 0 : 1;
        rating += touchingWall == fieldState.touchingWall ? 0 : 1;
        for (int enemyIndex = 0; enemyIndex < numberOfEnemies; ++enemyIndex)
        {
            rating += enemiesKilled[enemyIndex] == fieldState.enemiesKilled[enemyIndex] ? 0 : 1;
            rating += enemiesLowDurability[enemyIndex] == fieldState.enemiesLowDurability[enemyIndex] ? 0 : 1;
            rating += enemiesKilled[enemyIndex] == fieldState.enemiesKilled[enemyIndex] ? 0 : 1;
            rating += enemiesPosition[enemyIndex].x == fieldState.enemiesPosition[enemyIndex].x ? 0 : 1;
            rating += enemiesPosition[enemyIndex].y == fieldState.enemiesPosition[enemyIndex].y ? 0 : 1;
            rating += enemiesPosition[enemyIndex].z == fieldState.enemiesPosition[enemyIndex].z ? 0 : 1;
            rating += enemiesCanHit[enemyIndex] == fieldState.enemiesCanHit[enemyIndex] ? 0 : 1;
            for (int obstacleIndex = 0; obstacleIndex < enemiesTouchingObstacles[0].Length; ++obstacleIndex)
                rating += enemiesTouchingObstacles[enemyIndex][obstacleIndex] == fieldState.enemiesTouchingObstacles[enemyIndex][obstacleIndex] ? 0 : 1;
        }
        if (enemiesTouchingObstacles != null && enemiesTouchingObstacles[0] != null)
            for (int obstacleIndex = 0; obstacleIndex < enemiesTouchingObstacles[0].Length; ++obstacleIndex)
                rating += touchingObstacles[obstacleIndex] == fieldState.touchingObstacles[obstacleIndex] ? 0 : 1;
        rating += Math.Abs((numberOfEnemies - fieldState.numberOfEnemies) / numberOfEnemies);
        if (timeSinceLastAction > 0)
            rating += Math.Abs((timeSinceLastAction - fieldState.timeSinceLastAction) / timeSinceLastAction);
        else if (fieldState.timeSinceLastAction > 0)
            rating += Math.Abs((fieldState.timeSinceLastAction - timeSinceLastAction) / fieldState.timeSinceLastAction);
        if (attachments != null)
        {
            for (int attachmentIndex = 0; attachmentIndex < attachments.Length; ++attachmentIndex)
            {
                List<double> attachmentDiffList = new List<double>();
                attachmentDiffList.AddRange(attachments[attachmentIndex].compareTo(fieldState.attachments[attachmentIndex]));
                rating += Math.Abs(Enumerable.Average(attachmentDiffList) / 100);
                rating += attachmentsCharging[attachmentIndex] == fieldState.attachmentsCharging[attachmentIndex] ? 0 : 1;
                rating += attachmentsCooling[attachmentIndex] == fieldState.attachmentsCooling[attachmentIndex] ? 0 : 1;
            }
        }
        rating /= 100.0;
        return rating;
    }
}