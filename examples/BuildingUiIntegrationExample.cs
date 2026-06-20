using System;
using System.Collections.Generic;
using TerraForge.Core;
using TerraForge.Game.Building;
using TerraForge.Game.Inventory;

namespace TerraForge.Examples
{
    public sealed class BuildingUiIntegrationExample
    {
        public static void Main()
        {
            Console.WriteLine("=== TerraForge Building UI Integration Example ===\n");

            var blockRegistry = new BlockRegistry();
            RegisterBlocks(blockRegistry);

            var chunkManager = new ChunkManager(blockRegistry);
            InitializeWorld(chunkManager, blockRegistry);

            var buildingSystem = new BuildingSystem(chunkManager, blockRegistry);
            var uiManager = new BuildingUiSimulator(buildingSystem);

            Console.WriteLine("Simulando construção com preview e HUD:\n");

            // Simular sequência de construção
            SimulateBuildingSequence(buildingSystem, uiManager);

            Console.WriteLine("\n=== Exemplo concluído ===\n");
        }

        private static void RegisterBlocks(BlockRegistry registry)
        {
            registry.Register(new BlockType(0, "Air", false, true, false));
            registry.Register(new BlockType(1, "Stone", true, false));
            registry.Register(new BlockType(2, "Dirt", true, false));
            registry.Register(new BlockType(3, "Grass", true, false));
            registry.Register(new BlockType(4, "Wood", true, false));
        }

        private static void InitializeWorld(ChunkManager chunkManager, BlockRegistry blockRegistry)
        {
            for (var x = 0; x < 16; x++)
            {
                for (var z = 0; z < 16; z++)
                {
                    chunkManager.SetBlock(new Vector3Int(x, 10, z), new VoxelData(2));
                }
            }
        }

        private static void SimulateBuildingSequence(BuildingSystem buildingSystem, BuildingUiSimulator ui)
        {
            // 1. Construindo uma pequena casa (4x4x3)
            Console.WriteLine("[ CONSTRUÇÃO 1 ] Casa 4x4x3 em pedra:");
            BuildHouse(buildingSystem, ui, new Vector3Int(4, 11, 4));

            // 2. Construindo uma torre (1x1x6)
            Console.WriteLine("\n[ CONSTRUÇÃO 2 ] Torre 1x1x6 em madeira:");
            BuildTower(buildingSystem, ui, new Vector3Int(12, 11, 12));

            // 3. Danificando estrutura
            Console.WriteLine("\n[ COMBATE ] Danificando blocos:");
            DamageStructure(buildingSystem, ui);

            // 4. Reparando estrutura
            Console.WriteLine("\n[ REPARO ] Reparando com materiais:");
            RepairStructure(buildingSystem, ui);

            // 5. Análise de estabilidade
            Console.WriteLine("\n[ ANÁLISE ] Integridade estrutural:");
            AnalyzeStructures(buildingSystem);
        }

        private static void BuildHouse(BuildingSystem buildingSystem, BuildingUiSimulator ui, Vector3Int baseCorner)
        {
            var placement = buildingSystem.Placement;

            // Paredes (altura 3)
            for (var height = 0; height < 3; height++)
            {
                for (var x = 0; x < 4; x++)
                {
                    for (var z = 0; z < 4; z++)
                    {
                        // Apenas paredes (perímetro)
                        if (x == 0 || x == 3 || z == 0 || z == 3)
                        {
                            var pos = baseCorner + new Vector3Int(x, height, z);
                            if (placement.TryPlaceBlock(pos, 1)) // Stone
                            {
                                ui.ShowPlacementFeedback(pos, "Stone", true);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("  ✓ Casa construída com 48 blocos de pedra");
        }

        private static void BuildTower(BuildingSystem buildingSystem, BuildingUiSimulator ui, Vector3Int base)
        {
            var placement = buildingSystem.Placement;

            for (var height = 0; height < 6; height++)
            {
                var pos = base + new Vector3Int(0, height, 0);
                if (placement.TryPlaceBlock(pos, 4)) // Wood
                {
                    ui.ShowPlacementFeedback(pos, "Wood", true);
                }
            }

            Console.WriteLine("  ✓ Torre construída com 6 blocos de madeira");
        }

        private static void DamageStructure(BuildingSystem buildingSystem, BuildingUiSimulator ui)
        {
            var damage = buildingSystem.Damage;

            var houseBase = new Vector3Int(4, 11, 4);
            var towerBase = new Vector3Int(12, 11, 12);

            // Danificar casa
            damage.DamageBlock(houseBase, 30f);
            var houseHealth = damage.GetBlockHealth(houseBase);
            ui.ShowDamageFeedback(houseBase, 30f, houseHealth.GetHealthPercentage());

            // Danificar torre
            damage.DamageBlock(towerBase, 50f);
            var towerHealth = damage.GetBlockHealth(towerBase);
            ui.ShowDamageFeedback(towerBase, 50f, towerHealth.GetHealthPercentage());
        }

        private static void RepairStructure(BuildingSystem buildingSystem, BuildingUiSimulator ui)
        {
            var repair = buildingSystem.Repair;
            var damage = buildingSystem.Damage;
            var houseBase = new Vector3Int(4, 11, 4);

            var stoneItem = new ItemDefinition(1, "Stone", "Material", ItemCategory.Material, 64);
            var inventory = new InventoryContainer(27);
            inventory.AddItem(new InventoryItem(stoneItem, 20));

            var beforeHealth = damage.GetBlockHealth(houseBase);
            Console.WriteLine($"  Saúde antes: {beforeHealth.CurrentHealth:F0}/{beforeHealth.MaxHealth:F0}");

            if (repair.RepairFromInventory(houseBase, inventory, 1))
            {
                var afterHealth = damage.GetBlockHealth(houseBase);
                ui.ShowRepairFeedback(houseBase, beforeHealth.CurrentHealth, afterHealth.CurrentHealth);
            }
        }

        private static void AnalyzeStructures(BuildingSystem buildingSystem)
        {
            var houseBase = new Vector3Int(4, 11, 4);
            var towerBase = new Vector3Int(12, 11, 12);

            var houseIntegrity = buildingSystem.AnalyzeStructure(houseBase, 32);
            Console.WriteLine($"  Casa:");
            Console.WriteLine($"    Total: {houseIntegrity.TotalBlocks} blocos");
            Console.WriteLine($"    Integridade: {houseIntegrity.IntegrityPercentage:F1}%");
            Console.WriteLine($"    Status: {(houseIntegrity.IsStable ? "✓ Estável" : "✗ Instável")}");

            var towerIntegrity = buildingSystem.AnalyzeStructure(towerBase, 32);
            Console.WriteLine($"  Torre:");
            Console.WriteLine($"    Total: {towerIntegrity.TotalBlocks} blocos");
            Console.WriteLine($"    Integridade: {towerIntegrity.IntegrityPercentage:F1}%");
            Console.WriteLine($"    Status: {(towerIntegrity.IsStable ? "✓ Estável" : "✗ Instável")}");
        }
    }

    public sealed class BuildingUiSimulator
    {
        private readonly BuildingSystem _buildingSystem;
        private int _messageCount = 0;

        public BuildingUiSimulator(BuildingSystem buildingSystem)
        {
            _buildingSystem = buildingSystem;
        }

        public void ShowPlacementFeedback(Vector3Int position, string blockName, bool success)
        {
            _messageCount++;
            var status = success ? "✓" : "✗";
            var color = success ? "\x1b[32m" : "\x1b[31m";
            var reset = "\x1b[0m";
            Console.WriteLine($"{color}{status}{reset} [{position.X},{position.Y},{position.Z}] {blockName} colocado");
        }

        public void ShowDamageFeedback(Vector3Int position, float damage, float healthPercent)
        {
            var healthBar = GetHealthBar(healthPercent);
            Console.WriteLine($"  💥 [{position.X},{position.Y},{position.Z}] -{damage:F0} HP {healthBar} {healthPercent:P0}");
        }

        public void ShowRepairFeedback(Vector3Int position, float before, float after)
        {
            var healing = after - before;
            Console.WriteLine($"  ✓ [{position.X},{position.Y},{position.Z}] Reparado +{healing:F0} HP");
            Console.WriteLine($"    Antes:  {GetHealthBar(before / 100f)} {before:F0}/100");
            Console.WriteLine($"    Depois: {GetHealthBar(after / 100f)} {after:F0}/100");
        }

        private string GetHealthBar(float percentage)
        {
            var barLength = 10;
            var filled = (int)(barLength * Math.Clamp(percentage, 0, 1));
            var empty = barLength - filled;
            var color = percentage > 0.5f ? "\x1b[32m" : percentage > 0.25f ? "\x1b[33m" : "\x1b[31m";
            var reset = "\x1b[0m";
            return $"{color}[{'█'.ToString().PadRight(filled, '█')}{'░'.ToString().PadRight(empty, '░')}]{reset}";
        }
    }
}
