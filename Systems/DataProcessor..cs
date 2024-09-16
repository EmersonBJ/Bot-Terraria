using MinerSocietyMod014.NeuralNetwork;
using System;

public class DataProcessor
{
    public void ProcessMiningData(MiningData data)
    {
        Console.WriteLine($"Processing mining data: BlockType={data.BlockType}, Distance={data.DistanceToBlock}");
    }

    public void LogData(string message)
    {
        Console.WriteLine($"Log: {message}");
    }
}
