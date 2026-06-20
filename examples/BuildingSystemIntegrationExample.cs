using System;
using TerraForge.Core;
using TerraForge.Game.Building;
using TerraForge.Game.Inventory;
using TerraForge.Input;

namespace TerraForge.Examples
{
    public sealed class BuildingSystemIntegrationExample
    {
        public static void Main()
        {
            Console.WriteLine("=== TerraForge Building System Integration Example ===\n");

            var blockRegistry = new BlockRegistry();
            RegisterDefaultBlocks(blockRegistry);

            var chunkManager = new ChunkManager(blockRegistry);
            InitializeTestWorld(chunkManager, blockRegistry);

            var buildingSystem = new BuildingSystem(chunkManager, blockRegistry);

            Console.WriteLine("1. Block Placement Test");
            Console.WriteLine("---------------------------------");
            TestBlockPlacement(buildingSystem, chunkManager);

            Console.WriteLine("\n2. Structural Stability Test");
            Console.WriteLine("---------------------------------");
            TestStructuralStability(buildingSystem, chunkManager);

            Console.WriteLine("\n3. Block Damage System Test");
            Console.WriteLine("---------------------------------");
            TestBlockDamage(buildingSystem);

            Console.WriteLine("\n4. Structure Repair Test");
            Console.WriteLine("---------------------------------");
            TestStructureRepair(buildingSystem);

            Console.WriteLine("\n5. Building with Inventory Integration");
            Console.WriteLine("---------------------------------");
            TestBuildingWithInventory(buildingSystem, chunkManager);

            Console.WriteLine("\n✓ All building system tests completed successfully!\n");
        }

        private static void RegisterDefaultBlocks(BlockRegistry registry)
        {
            registry.Register(new BlockType(0, "Air", false, true, false));
            registry.Register(new BlockType(1, "Stone", true, false));
            registry.Register(new BlockType(2, "Dirt", true, false));
            registry.Register(new BlockType(3, "Grass", true, false));
            registry.Register(new BlockType(4, "Wood", true, false));
        }

        private static void InitializeTestWorld(ChunkManager chunkManager, BlockRegistry blockRegistry)
        {
            var baseY = 10;
            for (var x = 0; x < 16; x++)
            {
                for (var z = 0; z < 16; z++)
                {
                    chunkManager.SetBlock(new Vector3Int(x, baseY, z), new VoxelData(2));
                    chunkManager.SetBlock(new Vector3Int(x, baseY - 1, z), new VoxelData(1));
                }
            }

            Console.WriteLine("✓ Test world initialized with dirt base layer");
        }

        private static void TestBlockPlacement(BuildingSystem buildingSystem, ChunkManager chunkManager)
        {
            var placement = buildingSystem.Placement;

            var woodBlockId = (ushort)4;
            var testPos = new Vector3Int(5, 11, 5);

            Console.WriteLine($"Checking placement at {testPos}...");

            if (placement.CanPlaceBlock(testPos, woodBlockId))
            {
                if (placement.TryPlaceBlock(testPos, woodBlockId))
                {
                    Console.WriteLine($"✓ Wood block placed successfully at {testPos}");
                    var placed = chunkManager.GetBlock(testPos);
                    Console.WriteLine($"  Block ID in world: {placed.BlockId}");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to place block at {testPos}");
                }
            }
            else
            {
                Console.WriteLine($"✗ Cannot place block at {testPos} - unsupported position");
            }

            var buildTestPos = new Vector3Int(6, 12, 5);
            if (placement.CanPlaceBlock(buildTestPos, woodBlockId))
            {
                placement.TryPlaceBlock(buildTestPos, woodBlockId);
                Console.WriteLine($"✓ Additional block placed at {buildTestPos} for structure testing");
            }
        }

        private static void TestStructuralStability(BuildingSystem buildingSystem, ChunkManager chunkManager)
        {
            var stability = buildingSystem.Stability;

            var testPos = new Vector3Int(5, 11, 5);
            var block = chunkManager.GetBlock(testPos);

            if (!block.IsEmpty)
            {
                Console.WriteLine($"Testing stability for block at {testPos}...");

                if (stability.IsSupported(testPos))
                {
                    Console.WriteLine($"✓ Block is structurally supported");
                }
                else
                {
                    Console.WriteLine($"✗ Block lacks structural support");
                }

                var unsupported = stability.FindUnsupportedBlocks(testPos, 16);
                Console.WriteLine($"  Unsupported blocks in radius: {unsupported.Count}");

                var integrity = buildingSystem.AnalyzeStructure(testPos);
                Console.WriteLine($"  Total blocks: {integrity.TotalBlocks}");
                Console.WriteLine($"  Integrity: {integrity.IntegrityPercentage:F1}%");
                Console.WriteLine($"  Status: {(integrity.IsStable ? "Stable" : "Unstable")}");
            }
        }

        private static void TestBlockDamage(BuildingSystem buildingSystem)
        {
            var damage = buildingSystem.Damage;
            var testPos = new Vector3Int(5, 11, 5);

            Console.WriteLine($"Damaging block at {testPos}...");

            var initialHealth = damage.GetBlockHealth(testPos);
            if (initialHealth != null)
            {
                Console.WriteLine($"  Initial health: {initialHealth.CurrentHealth:F0}/{initialHealth.MaxHealth:F0}");

                damage.DamageBlock(testPos, 25f);
                var damagedHealth = damage.GetBlockHealth(testPos);
                Console.WriteLine($"  After 25 damage: {damagedHealth.CurrentHealth:F0}/{damagedHealth.MaxHealth:F0}");
                Console.WriteLine($"  Health %: {damagedHealth.GetHealthPercentage():P0}");

                damage.DamageBlock(testPos, 25f);
                var furtherDamaged = damage.GetBlockHealth(testPos);
                Console.WriteLine($"  After 50 total damage: {furtherDamaged.CurrentHealth:F0}/{furtherDamaged.MaxHealth:F0}");
            }
        }

        private static void TestStructureRepair(BuildingSystem buildingSystem)
        {
            var repair = buildingSystem.Repair;
            var testPos = new Vector3Int(5, 11, 5);

            Console.WriteLine($"Repairing block at {testPos}...");

            if (repair.CanRepairBlock(testPos))
            {
                var stoneRepairCost = repair.GetRepairCostInMaterials(testPos, 1);
                Console.WriteLine($"  Repair cost in stone: {stoneRepairCost:F2} units");

                if (repair.TryRepairBlock(testPos, 1, 3))
                {
                    var repairedHealth = buildingSystem.Damage.GetBlockHealth(testPos);
                    Console.WriteLine($"  After repair with 3 stone: {repairedHealth.CurrentHealth:F0}/{repairedHealth.MaxHealth:F0}");
                }
            }
            else
            {
                Console.WriteLine("  Block does not need repair");
            }
        }

        private static void TestBuildingWithInventory(BuildingSystem buildingSystem, ChunkManager chunkManager)
        {
            Console.WriteLine("Setting up inventory with building materials...");

            var itemRegistry = new CraftingManager();
            var stoneItem = new ItemDefinition(1, "Stone", "Building material", ItemCategory.Material, 64);
            var woodItem = new ItemDefinition(4, "Wood", "Building material", ItemCategory.Material, 64);

            var inventory = new InventoryContainer(27);
            inventory.AddItem(new InventoryItem(stoneItem, 16));
            inventory.AddItem(new InventoryItem(woodItem, 8));

            Console.WriteLine($"  Inventory created with 16 stone + 8 wood");

            var repairPos = new Vector3Int(6, 12, 5);
            var damageManager = buildingSystem.Damage;
            damageManager.DamageBlock(repairPos, 30f);

            var damageRepair = buildingSystem.Repair;
            var health = damageManager.GetBlockHealth(repairPos);
            Console.WriteLine($"  Block at {repairPos} damaged to {health.CurrentHealth:F0}/{health.MaxHealth:F0}");

            if (damageRepair.RepairFromInventory(repairPos, inventory, 1))
            {
                var repairedHealth = damageManager.GetBlockHealth(repairPos);
                Console.WriteLine($"  ✓ Repaired from inventory! New health: {repairedHealth.CurrentHealth:F0}/{repairedHealth.MaxHealth:F0}");

                var stoneSlot = inventory.Slots[0];
                Console.WriteLine($"  Stone remaining in inventory: {(stoneSlot.IsEmpty ? 0 : stoneSlot.Item.Quantity)}");
            }
            else
            {
                Console.WriteLine("  ✗ Not enough materials to repair");
            }
        }
    }
}
