using Microsoft.ML;
using Microsoft.ML.Data;
using MinerSocietyMod014.BotSystem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using MinerSocietyMod014.NeuralNetwork;

public class MinerBotNPC : ModNPC
{
    private MinerPickaxe pickaxe;
    private bool isMining = false;
    private RewardSystem rewardSystem;
    private int miningRange = 10;
    private Item[] botInventory;
    private const int InventorySize = 10;
    private ToggleLearningModeCommand learningCommand;

    private int timeOfDay;
    private const int Morning = 0;
    private const int Night = 1;
    private bool isReturningToBase = false;

    // Adicionando componentes da rede neural com ML.NET
    private MLContext mlContext;
    private ITransformer trainedModel;
    private PredictionEngine<ExplorationData, ExplorationPrediction> predictionEngine;

    // Lista para armazenar os dados de exploração
    private List<ExplorationData> explorationDataList = new List<ExplorationData>();

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 24;
    }

    public override string Texture => "Terraria/Images/NPC_38";

    public override void SetDefaults()
    {
        NPC.width = 18;
        NPC.height = 40;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.friendly = true;
        NPC.noGravity = false;
        NPC.noTileCollide = false;

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;

        NPC.aiStyle = -1;

        pickaxe = new MinerPickaxe();
        learningCommand = new ToggleLearningModeCommand();

        if (rewardSystem == null)
            rewardSystem = new RewardSystem();

        botInventory = new Item[InventorySize];
        for (int i = 0; i < botInventory.Length; i++)
        {
            botInventory[i] = new Item();
        }

        // Inicializando o ML.NET
        mlContext = new MLContext();
        TrainModel(); // Treina o modelo ao iniciar
        predictionEngine = mlContext.Model.CreatePredictionEngine<ExplorationData, ExplorationPrediction>(trainedModel);

        timeOfDay = Morning;
    }

    // AI do NPC, chamada a cada frame
    public override void AI()
    {
        ExecuteDailyRoutine();
    }

    private void ExecuteDailyRoutine()
    {
        if (timeOfDay == Morning && !isReturningToBase)
        {
            if (isMining)
            {
                FindAndMineOre();
            }
            else
            {
                ExploreAndLearn();
            }

            if (IsInventoryFull())
            {
                StoreItemsInChest();
            }
        }
        else if (timeOfDay == Night || isReturningToBase)
        {
            ReturnToBase();
        }
    }

    private void FindAndMineOre()
    {
        MiningData input = new MiningData
        {
            BlockType = GetSurroundingBlockType(),
            DistanceToBlock = GetDistanceToTargetBlock()
        };

        // Coleta dados de mineração e obstáculos
        CollectExplorationData();

        // Usa o modelo treinado para prever se deve ou não minerar
        bool shouldMine = predictionEngine.Predict(new ExplorationData
        {
            DistanceToObstacle = input.DistanceToBlock,
            JumpRequired = IsJumpRequired()
        }).ShouldMine;

        if (shouldMine)
        {
            pickaxe.Mine();
            rewardSystem.AddReward(10);
        }
        else
        {
            DigStaircaseTunnel();
        }
    }

    private void DigStaircaseTunnel()
    {
        NPC.velocity.X = (Main.rand.NextBool() ? -1 : 1) * 1.0f;
        NPC.velocity.Y = 1.0f;
        BreakObstacleInPath();
        pickaxe.Mine();
        Console.WriteLine("Bot cavando túnel em degraus...");
    }

    private void BreakObstacleInPath()
    {
        int npcX = (int)(NPC.position.X / 16);
        int npcY = (int)(NPC.position.Y / 16);

        Tile tileInFront = Main.tile[npcX + Math.Sign(NPC.velocity.X), npcY];
        if (tileInFront != null && Main.tileSolid[tileInFront.TileType])
        {
            WorldGen.KillTile(npcX + Math.Sign(NPC.velocity.X), npcY);
            Console.WriteLine("Bot quebrou um obstáculo.");
        }
    }

    // Lógica de exploração e aprendizado usando ML.NET
    private void ExploreAndLearn()
    {
        NPC.velocity.X = (Main.rand.NextBool() ? -1 : 1) * 1.5f;
        NPC.velocity.Y = 1.0f;

        Console.WriteLine("Explorando o ambiente e coletando dados.");

        // Coleta dados de exploração
        CollectExplorationData();

        TrainOnExplorationData();
    }

    private void TrainOnExplorationData()
    {
        // Usa os dados coletados para ajustar o modelo
        var dataView = mlContext.Data.LoadFromEnumerable(explorationDataList);
        var pipeline = mlContext.Transforms.Concatenate("Features", "DistanceToObstacle", "JumpRequired")
                                           .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

        trainedModel = pipeline.Fit(dataView); // Ajusta o modelo com base na exploração
        Console.WriteLine("Treinando o modelo com base nos dados de exploração.");
    }

    // Novo método para coletar dados de exploração/mineração
    private void CollectExplorationData()
    {
        var newExplorationData = new ExplorationData
        {
            DistanceToObstacle = GetDistanceToObstacle(),
            JumpRequired = IsJumpRequired()
        };

        explorationDataList.Add(newExplorationData);

        // Treina o modelo sempre que novos dados são coletados
        TrainModel();
    }

    private void TrainModel()
    {
        // Use dados coletados durante a exploração e mineração
        if (explorationDataList.Count > 0)
        {
            var dataView = mlContext.Data.LoadFromEnumerable(explorationDataList);

            // Cria o pipeline de treinamento
            var pipeline = mlContext.Transforms.Concatenate("Features", "DistanceToObstacle", "JumpRequired")
                            .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

            // Treina o modelo com base nos dados reais
            trainedModel = pipeline.Fit(dataView);

            Console.WriteLine("Modelo treinado com dados reais.");
        }
        else
        {
            Console.WriteLine("Ainda não há dados suficientes para treinar o modelo.");
        }
    }

    private void ReturnToBase()
    {
        float directionToBaseX = Main.spawnTileX * 16 - NPC.position.X;
        float directionToBaseY = Main.spawnTileY * 16 - NPC.position.Y;

        NPC.velocity.X = Math.Sign(directionToBaseX) * 1.5f;
        NPC.velocity.Y = Math.Sign(directionToBaseY) * 1.5f;

        if (IsObstacleAhead())
        {
            NPC.velocity.Y = -5.0f;
        }

        AdjustNeuralNetworkForObstacle();

        if (AtBase())
        {
            isReturningToBase = false;
            timeOfDay = Morning;
            Console.WriteLine("Bot chegou à base.");
        }
    }

    private bool IsObstacleAhead()
    {
        int npcX = (int)(NPC.position.X / 16);
        int npcY = (int)(NPC.position.Y / 16);
        Tile tileInFront = Main.tile[npcX + Math.Sign(NPC.velocity.X), npcY];
        return tileInFront != null && !Main.tileSolid[tileInFront.TileType];
    }

    private void AdjustNeuralNetworkForObstacle()
    {
        ExplorationData data = new ExplorationData
        {
            DistanceToObstacle = GetDistanceToObstacle(),
            JumpRequired = IsJumpRequired()
        };
        TrainOnExplorationData(); // Ajusta o modelo com base nos obstáculos
    }

    private float GetDistanceToObstacle()
    {
        return 5.0f;
    }

    private bool IsJumpRequired()
    {
        return true;
    }

    private bool AtBase()
    {
        return NPC.position.X <= Main.spawnTileX * 16;
    }

    private void StoreItemsInChest()
    {
        int chestIndex = WorldGen.PlaceChest((int)NPC.position.X, (int)NPC.position.Y);
        if (chestIndex >= 0)
        {
            Chest chest = Main.chest[chestIndex];
            for (int i = 0; i < botInventory.Length; i++)
            {
                if (botInventory[i] != null && !botInventory[i].IsAir)
                {
                    chest.item[i] = botInventory[i];
                    botInventory[i] = new Item();
                }
            }
        }
    }

    private bool IsInventoryFull()
    {
        foreach (var item in botInventory)
        {
            if (item == null || item.IsAir)
            {
                return false;
            }
        }
        return true;
    }

    private int GetSurroundingBlockType()
    {
        int npcX = (int)(NPC.position.X / 16);
        int npcY = (int)(NPC.position.Y / 16);

        for (int x = npcX - miningRange; x <= npcX + miningRange; x++)
        {
            for (int y = npcY - miningRange; y <= npcY + miningRange; y++)
            {
                Tile tile = Main.tile[x, y];
                if (tile != null && IsOre(tile.TileType))
                {
                    return tile.TileType;
                }
            }
        }
        return 0;
    }

    private float GetDistanceToTargetBlock()
    {
        int npcX = (int)(NPC.position.X / 16);
        int npcY = (int)(NPC.position.Y / 16);

        for (int x = npcX - miningRange; x <= npcX + miningRange; x++)
        {
            for (int y = npcY - miningRange; y <= npcY + miningRange; y++)
            {
                Tile tile = Main.tile[x, y];
                if (tile != null && IsOre(tile.TileType))
                {
                    float distanceX = (x - npcX) * (x - npcX);
                    float distanceY = (y - npcY) * (y - npcY);
                    return (float)Math.Sqrt(distanceX + distanceY);
                }
            }
        }

        return float.MaxValue;
    }

    private bool IsOre(int tileType)
    {
        return tileType == TileID.Copper || tileType == TileID.Iron || tileType == TileID.Gold;
    }
}

// Classe ExplorationData para treinar o modelo
public class ExplorationData
{
    public float DistanceToObstacle { get; set; }
    public bool JumpRequired { get; set; }
}

// Classe de previsão de mineração
public class ExplorationPrediction
{
    [ColumnName("PredictedLabel")]
    public bool ShouldMine { get; set; }
}
