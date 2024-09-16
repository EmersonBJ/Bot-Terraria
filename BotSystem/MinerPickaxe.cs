using System;

namespace MinerSocietyMod014.BotSystem;
public class MinerPickaxe
{
    public string PickaxeName { get; set; } = "MinerPickaxe";
    public int Strength { get; set; } = 5; // Exemplo de força da picareta

    public void MineBlock()
    {
        // Lógica para minerar um bloco
        Console.WriteLine($"{PickaxeName} está minerando um bloco com força {Strength}");
    }
    public void Mine()
    {
        // Lógica para minerar um bloco
        Console.WriteLine($"{PickaxeName} está minerando um bloco com força {Strength}");
    }
}
