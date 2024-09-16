using System;

public class RewardSystem
{
    private int rewardPoints = 0;

    public void AddReward(int points)
    {
        rewardPoints += points;
        Console.WriteLine($"Reward added: {points} points. Total: {rewardPoints} points.");
    }

    public int GetTotalScore()
    {
        return rewardPoints;
    }
}
