using System;
using System.Collections.Generic;
using TerraForge.Core;

namespace TerraForge.Game.Building
{
    public sealed class BuildingSystem
    {
        private readonly ChunkManager _chunkManager;
        private readonly BlockRegistry _blockRegistry;
        private readonly BlockPlacement _blockPlacement;
        private readonly BlockDamageManager _damageManager;
        private readonly StructuralStability _stability;
        private readonly StructureRepair _structureRepair;

        public BlockPlacement Placement => _blockPlacement;
        public BlockDamageManager Damage => _damageManager;
        public StructuralStability Stability => _stability;
        public StructureRepair Repair => _structureRepair;

        public BuildingSystem(ChunkManager chunkManager, BlockRegistry blockRegistry)
        {
            _chunkManager = chunkManager ?? throw new ArgumentNullException(nameof(chunkManager));
            _blockRegistry = blockRegistry ?? throw new ArgumentNullException(nameof(blockRegistry));
            _stability = new StructuralStability(chunkManager, blockRegistry);
            _blockPlacement = new BlockPlacement(chunkManager, blockRegistry, _stability);
            _damageManager = new BlockDamageManager(chunkManager, blockRegistry);
            _structureRepair = new StructureRepair(_damageManager);
        }

        public void Update(float deltaTime)
        {
        }

        public StructureIntegrity AnalyzeStructure(Vector3Int centerPosition, int radius = 32)
        {
            var unsupportedBlocks = _stability.FindUnsupportedBlocks(centerPosition, radius);
            var integrity = new StructureIntegrity
            {
                TotalBlocks = CountBlocksInRadius(centerPosition, radius),
                UnsupportedBlocks = unsupportedBlocks.Count,
                IntegrityPercentage = 100f - (unsupportedBlocks.Count * 100f / Math.Max(1, CountBlocksInRadius(centerPosition, radius)))
            };

            return integrity;
        }

        public List<Vector3Int> FindStructureBlocks(Vector3Int origin, int maxDistance = 64)
        {
            var blocks = new List<Vector3Int>();
            var visited = new HashSet<Vector3Int>();
            var queue = new Queue<Vector3Int>();

            queue.Enqueue(origin);
            visited.Add(origin);

            var directions = new Vector3Int[]
            {
                new Vector3Int(1, 0, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(0, -1, 0),
                new Vector3Int(0, 0, 1),
                new Vector3Int(0, 0, -1)
            };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var block = _chunkManager.GetBlock(current);

                if (!block.IsEmpty && _blockRegistry.IsSolid(block.BlockId))
                {
                    blocks.Add(current);
                }

                foreach (var direction in directions)
                {
                    var neighbor = new Vector3Int(current.X + direction.X, current.Y + direction.Y, current.Z + direction.Z);
                    if (Math.Abs(neighbor.X - origin.X) > maxDistance) continue;
                    if (Math.Abs(neighbor.Y - origin.Y) > maxDistance) continue;
                    if (Math.Abs(neighbor.Z - origin.Z) > maxDistance) continue;

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return blocks;
        }

        private int CountBlocksInRadius(Vector3Int center, int radius)
        {
            var count = 0;
            for (var x = center.X - radius; x <= center.X + radius; x++)
            {
                for (var y = center.Y - radius; y <= center.Y + radius; y++)
                {
                    for (var z = center.Z - radius; z <= center.Z + radius; z++)
                    {
                        var block = _chunkManager.GetBlock(new Vector3Int(x, y, z));
                        if (!block.IsEmpty && _blockRegistry.IsSolid(block.BlockId)) count++;
                    }
                }
            }

            return count;
        }
    }

    public sealed class StructureIntegrity
    {
        public int TotalBlocks { get; set; }
        public int UnsupportedBlocks { get; set; }
        public float IntegrityPercentage { get; set; }

        public bool IsStable => IntegrityPercentage > 80f;
        public bool HasDamage => UnsupportedBlocks > 0;
    }
}
