using MinerSocietyMod014.BotSystem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures; // Para usar EntitySource_SpawnNPC
using System;

namespace MinerSocietyMod014.BotSystem
{
    public class MinerBotSystem
    {
        private MinerPickaxe pickaxe;
        private int miningRange = 10; // Alcance para detectar minérios
        private bool isMining = false;
        private int botID; // Armazena o ID do bot NPC

        // Método para spawnar o bot automaticamente no início do jogo
        public void SpawnBot()
        {
            int spawnX = (int)Main.spawnTileX; // Posição do spawn do jogador
            int spawnY = (int)Main.spawnTileY; // Posição do spawn do jogador

            // Use EntitySource_SpawnNPC para criar o NPC no mundo
            var entitySource = new EntitySource_SpawnNPC();
            botID = NPC.NewNPC(entitySource, spawnX * 16, spawnY * 16, ModContent.NPCType<MinerBotNPC>());
            Main.npc[botID].friendly = true; // Define o NPC como amigável
        }

        // Método para inicializar o processo de mineração do bot
        public void InitializeMining()
        {
            pickaxe = new MinerPickaxe();
            StartMiningProcess();
        }

        // Método melhorado para o processo de mineração
        private void StartMiningProcess()
        {
            if (!isMining)
            {
                // Tentar encontrar minério dentro do alcance
                Tuple<int, int> orePosition = FindNearbyOre();
                if (orePosition != null)
                {
                    MoveTowardsOre(orePosition.Item1, orePosition.Item2);
                    pickaxe.MineBlock();
                }
                else
                {
                    // Não encontrou minério, então começar a escavar aleatoriamente
                    RandomDigging();
                }
            }
        }

        // Método para encontrar minérios próximos dentro do alcance especificado
        private Tuple<int, int> FindNearbyOre()
        {
            for (int x = (int)(Main.LocalPlayer.position.X / 16) - miningRange; x < (int)(Main.LocalPlayer.position.X / 16) + miningRange; x++)
            {
                for (int y = (int)(Main.LocalPlayer.position.Y / 16) - miningRange; y < (int)(Main.LocalPlayer.position.Y / 16) + miningRange; y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile != null && IsOre(tile.TileType))
                    {
                        // Retorna a posição do minério
                        return new Tuple<int, int>(x, y);
                    }
                }
            }

            // Retorna null se não houver minério no alcance
            return null;
        }

        // Verifica se o bloco é um minério conhecido
        private bool IsOre(int tileType)
        {
            return tileType == TileID.Copper || tileType == TileID.Iron || tileType == TileID.Gold;
        }

        // Método para mover o bot em direção ao minério encontrado
        private void MoveTowardsOre(int oreX, int oreY)
        {
            NPC npc = Main.npc[botID]; // Referência do bot NPC
            float directionX = oreX - npc.position.X / 16;
            float directionY = oreY - npc.position.Y / 16;

            npc.velocity.X = directionX * 0.1f; // Ajusta a velocidade para mover na direção do minério
            npc.velocity.Y = directionY * 0.1f;

            isMining = true;

            // Se o bot está suficientemente perto do minério, minerar
            if (Math.Abs(npc.position.X / 16 - oreX) < 1 && Math.Abs(npc.position.Y / 16 - oreY) < 1)
            {
                pickaxe.MineBlock();
                isMining = false;
            }
        }

        // Método para escavação aleatória quando nenhum minério for encontrado
        private void RandomDigging()
        {
            NPC npc = Main.npc[botID];
            npc.velocity.X = (Main.rand.NextBool() ? -1 : 1) * 1.5f; // Movimento aleatório para a esquerda ou direita
            npc.velocity.Y = 1.5f; // Desce um pouco enquanto escava
            pickaxe.MineBlock();
        }
    }
}
