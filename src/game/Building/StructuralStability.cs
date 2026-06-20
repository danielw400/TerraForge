using System;
using System.Collections.Generic;
using TerraForge.Core;

namespace TerraForge.Game.Building
{
    public sealed class StructuralStability
    {
        private readonly ChunkManager _chunkManager;
        private readonly BlockRegistry _blockRegistry;
        private readonly HashSet<Vector3Int> _supportedBlocks = new HashSet<Vector3Int>();
        private readonly HashSet<Vector3Int> _unsupportedBlocks = new HashSet<Vector3Int>();

        public StructuralStability(ChunkManager chunkManager, BlockRegistry blockRegistry)
        {
            _chunkManager = chunkManager ?? throw new ArgumentNullException(nameof(chunkManager));
            _blockRegistry = blockRegistry ?? throw new ArgumentNullException(nameof(blockRegistry));
        }

        public bool IsSupported(Vector3Int position)
        {
            return CheckSupport(position);
        }

        public List<Vector3Int> FindUnsupportedBlocks(Vector3Int position, int maxRadius = 32)
        {
            _supportedBlocks.Clear();
            _unsupportedBlocks.Clear();

            BreadthFirstSearch(position, maxRadius);
            return new List<Vector3Int>(_unsupportedBlocks);
        }

        private bool CheckSupport(Vector3Int position)
        {
            var belowPos = new Vector3Int(position.X, position.Y - 1, position.Z);
            var blockBelow = _chunkManager.GetBlock(belowPos);
            if (!blockBelow.IsEmpty && _blockRegistry.IsSolid(blockBelow.BlockId)) return true;

            var adjacentOffsets = new (int x, int y, int z)[]
            {
                (1, 0, 0), (-1, 0, 0),
                (0, 0, 1), (0, 0, -1)
            };

            var supportCount = 0;
            foreach (var offset in adjacentOffsets)
            {
                var adjacent = new Vector3Int(position.X + offset.x, position.Y + offset.y, position.Z + offset.z);
                var adjacentBlock = _chunkManager.GetBlock(adjacent);
                if (!adjacentBlock.IsEmpty && _blockRegistry.IsSolid(adjacentBlock.BlockId)) supportCount++;
            }

            return supportCount >= 2;
        }

        private void BreadthFirstSearch(Vector3Int startPosition, int maxRadius)
        {
            var queue = new Queue<Vector3Int>();
            var visited = new HashSet<Vector3Int> { startPosition };
            queue.Enqueue(startPosition);

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
                if (block.IsEmpty || !_blockRegistry.IsSolid(block.BlockId)) continue;

                if (CheckSupport(current))
                {
                    _supportedBlocks.Add(current);
                }
                else
                {
                    _unsupportedBlocks.Add(current);
                }

                foreach (var direction in directions)
                {
                    var neighbor = new Vector3Int(current.X + direction.X, current.Y + direction.Y, current.Z + direction.Z);
                    if (Math.Abs(neighbor.X - startPosition.X) > maxRadius) continue;
                    if (Math.Abs(neighbor.Y - startPosition.Y) > maxRadius) continue;
                    if (Math.Abs(neighbor.Z - startPosition.Z) > maxRadius) continue;

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }
}
