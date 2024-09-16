using System;

public class EmotionSystem
{
    public enum BotEmotion
    {
        Happy,
        Neutral,
        Sad
    }

    private BotEmotion currentEmotion = BotEmotion.Neutral;

    public void UpdateEmotionBasedOnMining(int minedBlocks)
    {
        if (minedBlocks > 10)
        {
            currentEmotion = BotEmotion.Happy;
        }
        else if (minedBlocks < 5)
        {
            currentEmotion = BotEmotion.Sad;
        }
        else
        {
            currentEmotion = BotEmotion.Neutral;
        }

        Console.WriteLine($"Current emotion: {currentEmotion}");
    }

    public BotEmotion GetCurrentEmotion()
    {
        return currentEmotion;
    }
}
