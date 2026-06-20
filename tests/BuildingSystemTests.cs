using System;
using System.Collections.Generic;
using TerraForge.Core;
using TerraForge.Game.Building;
using TerraForge.Game.Inventory;

namespace TerraForge.Tests
{
    public sealed class BuildingSystemTests
    {
        private BlockRegistry _blockRegistry;
        private ChunkManager _chunkManager;
        private BuildingSystem _buildingSystem;

        public void Setup()
        {
            _blockRegistry = new BlockRegistry();
            RegisterTestBlocks(_blockRegistry);
            _chunkManager = new ChunkManager(_blockRegistry);
            _buildingSystem = new BuildingSystem(_chunkManager, _blockRegistry);
            InitializeTestWorld();
        }

        private void RegisterTestBlocks(BlockRegistry registry)
        {
            registry.Register(new BlockType(0, "Air", false, true, false));
            registry.Register(new BlockType(1, "Stone", true, false));
            registry.Register(new BlockType(2, "Dirt", true, false));
            registry.Register(new BlockType(4, "Wood", true, false));
        }

        private void InitializeTestWorld()
        {
            var baseY = 10;
            for (var x = 0; x < 16; x++)
            {
                for (var z = 0; z < 16; z++)
                {
                    _chunkManager.SetBlock(new Vector3Int(x, baseY, z), new VoxelData(2));
                }
            }
        }

        public void TestBlockPlacementValidation()
        {
            Console.WriteLine("[TEST] Block Placement Validation");
            var placement = _buildingSystem.Placement;
            var testPos = new Vector3Int(5, 11, 5);
            var stoneId = (ushort)1;

            // Should place on supported ground
            if (!placement.CanPlaceBlock(testPos, stoneId))
            {
                throw new Exception("Block should be placeable on ground");
            }

            if (!placement.TryPlaceBlock(testPos, stoneId))
            {
                throw new Exception("Block placement should succeed");
            }

            var block = _chunkManager.GetBlock(testPos);
            if (block.BlockId != stoneId)
            {
                throw new Exception($"Block ID mismatch: expected {stoneId}, got {block.BlockId}");
            }

            // Should not place on occupied position
            if (placement.CanPlaceBlock(testPos, stoneId))
            {
                throw new Exception("Should not place on occupied position");
            }

            Console.WriteLine("  ✓ Placement validation passed");
        }

        public void TestStructuralSupport()
        {
            Console.WriteLine("[TEST] Structural Support");
            var placement = _buildingSystem.Placement;
            var stability = _buildingSystem.Stability;

            var basePos = new Vector3Int(5, 11, 5);
            var stoneId = (ushort)1;
            placement.TryPlaceBlock(basePos, stoneId);

            if (!stability.IsSupported(basePos))
            {
                throw new Exception("Block on ground should be supported");
            }

            // Unsupported floating block
            var floatingPos = new Vector3Int(5, 20, 5);
            if (stability.IsSupported(floatingPos))
            {
                throw new Exception("Floating block should not be supported");
            }

            Console.WriteLine("  ✓ Structural support validation passed");
        }

        public void TestBlockHealth()
        {
            Console.WriteLine("[TEST] Block Health System");
            var placement = _buildingSystem.Placement;
            var damage = _buildingSystem.Damage;

            var testPos = new Vector3Int(5, 11, 5);
            placement.TryPlaceBlock(testPos, 1);

            var health = damage.GetBlockHealth(testPos);
            if (health == null)
            {
                throw new Exception("Block health should be created");
            }

            var initialHealth = health.CurrentHealth;
            damage.DamageBlock(testPos, 25f);

            var damagedHealth = damage.GetBlockHealth(testPos);
            if (Math.Abs(damagedHealth.CurrentHealth - (initialHealth - 25f)) > 0.01f)
            {
                throw new Exception($"Health should decrease by 25, got {damagedHealth.CurrentHealth}");
            }

            if (damagedHealth.IsDestroyed)
            {
                throw new Exception("Block should not be destroyed yet");
            }

            // Destroy completely
            damage.DamageBlock(testPos, initialHealth);
            if (!damagedHealth.IsDestroyed)
            {
                throw new Exception("Block should be destroyed after taking max damage");
            }

            var destroyed = _chunkManager.GetBlock(testPos);
            if (!destroyed.IsEmpty)
            {
                throw new Exception("Destroyed block should be removed from world");
            }

            Console.WriteLine("  ✓ Block health system passed");
        }

        public void TestBlockRepair()
        {
            Console.WriteLine("[TEST] Block Repair System");
            var placement = _buildingSystem.Placement;
            var damage = _buildingSystem.Damage;
            var repair = _buildingSystem.Repair;

            var testPos = new Vector3Int(5, 11, 5);
            placement.TryPlaceBlock(testPos, 1);
            damage.DamageBlock(testPos, 25f);

            var damagedHealth = damage.GetBlockHealth(testPos);
            var healthBeforeRepair = damagedHealth.CurrentHealth;

            if (!repair.CanRepairBlock(testPos))
            {
                throw new Exception("Damaged block should be repairable");
            }

            repair.TryRepairBlock(testPos, 1, 2);
            var repairedHealth = damage.GetBlockHealth(testPos);

            if (repairedHealth.CurrentHealth <= healthBeforeRepair)
            {
                throw new Exception("Health should increase after repair");
            }

            Console.WriteLine("  ✓ Block repair system passed");
        }

        public void TestStructureIntegrity()
        {
            Console.WriteLine("[TEST] Structure Integrity Analysis");
            var placement = _buildingSystem.Placement;

            var basePos = new Vector3Int(5, 11, 5);
            placement.TryPlaceBlock(basePos, 1);
            placement.TryPlaceBlock(new Vector3Int(6, 11, 5), 1);
            placement.TryPlaceBlock(new Vector3Int(5, 12, 5), 1);

            var integrity = _buildingSystem.AnalyzeStructure(basePos, 16);

            if (integrity.TotalBlocks < 3)
            {
                throw new Exception("Should find at least 3 blocks");
            }

            if (integrity.IntegrityPercentage < 80f)
            {
                throw new Exception("Small stable structure should have high integrity");
            }

            if (!integrity.IsStable)
            {
                throw new Exception("Small structure should be stable");
            }

            Console.WriteLine($"  ✓ Structure integrity analysis passed (Integrity: {integrity.IntegrityPercentage:F1}%)");
        }

        public void TestInventoryIntegration()
        {
            Console.WriteLine("[TEST] Inventory Integration");
            var placement = _buildingSystem.Placement;
            var damage = _buildingSystem.Damage;
            var repair = _buildingSystem.Repair;

            var testPos = new Vector3Int(5, 11, 5);
            placement.TryPlaceBlock(testPos, 1);
            damage.DamageBlock(testPos, 30f);

            var stoneItem = new ItemDefinition(1, "Stone", "Material", ItemCategory.Material, 64);
            var inventory = new InventoryContainer(27);
            inventory.AddItem(new InventoryItem(stoneItem, 10));

            var healthBefore = damage.GetBlockHealth(testPos);
            if (!repair.RepairFromInventory(testPos, inventory, 1))
            {
                throw new Exception("Repair from inventory should succeed");
            }

            var healthAfter = damage.GetBlockHealth(testPos);
            if (healthAfter.CurrentHealth <= healthBefore.CurrentHealth)
            {
                throw new Exception("Health should increase after inventory repair");
            }

            Console.WriteLine("  ✓ Inventory integration passed");
        }

        public void TestFindStructureBlocks()
        {
            Console.WriteLine("[TEST] Find Structure Blocks");
            var placement = _buildingSystem.Placement;

            var basePos = new Vector3Int(8, 11, 8);
            placement.TryPlaceBlock(basePos, 1);
            placement.TryPlaceBlock(new Vector3Int(9, 11, 8), 1);
            placement.TryPlaceBlock(new Vector3Int(8, 12, 8), 1);
            placement.TryPlaceBlock(new Vector3Int(8, 11, 9), 1);

            var blocks = _buildingSystem.FindStructureBlocks(basePos, 8);

            if (blocks.Count < 4)
            {
                throw new Exception($"Should find at least 4 structure blocks, found {blocks.Count}");
            }

            Console.WriteLine($"  ✓ Found {blocks.Count} structure blocks");
        }

        public static void RunAllTests()
        {
            Console.WriteLine("\n=== Building System Unit Tests ===\n");
            var tests = new BuildingSystemTests();

            try
            {
                tests.Setup();
                tests.TestBlockPlacementValidation();
                tests.Setup();
                tests.TestStructuralSupport();
                tests.Setup();
                tests.TestBlockHealth();
                tests.Setup();
                tests.TestBlockRepair();
                tests.Setup();
                tests.TestStructureIntegrity();
                tests.Setup();
                tests.TestInventoryIntegration();
                tests.Setup();
                tests.TestFindStructureBlocks();

                Console.WriteLine("\n✓ All tests passed!\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Test failed: {ex.Message}\n");
                throw;
            }
        }
    }
}
