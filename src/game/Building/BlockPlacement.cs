using System;
using TerraForge.Core;

namespace TerraForge.Game.Building
{
    public sealed class BlockPlacement
    {
        private readonly ChunkManager _chunkManager;
        private readonly BlockRegistry _blockRegistry;
        private readonly StructuralStability _stability;

        public BlockPlacement(ChunkManager chunkManager, BlockRegistry blockRegistry, StructuralStability stability)
        {
            _chunkManager = chunkManager ?? throw new ArgumentNullException(nameof(chunkManager));
            _blockRegistry = blockRegistry ?? throw new ArgumentNullException(nameof(blockRegistry));
            _stability = stability ?? throw new ArgumentNullException(nameof(stability));
        }

        public bool CanPlaceBlock(Vector3Int position, ushort blockId)
        {
            if (!_blockRegistry.IsSolid(blockId)) return false;

            var block = _chunkManager.GetBlock(position);
            if (!block.IsEmpty) return false;

            if (!_stability.IsSupported(position)) return false;

            return true;
        }

        public bool TryPlaceBlock(Vector3Int position, ushort blockId)
        {
            if (!CanPlaceBlock(position, blockId)) return false;
            _chunkManager.SetBlock(position, new VoxelData(blockId));
            return true;
        }

        public bool CanRemoveBlock(Vector3Int position)
        {
            var block = _chunkManager.GetBlock(position);
            return !block.IsEmpty && _blockRegistry.IsSolid(block.BlockId);
        }

        public bool TryRemoveBlock(Vector3Int position)
        {
            if (!CanRemoveBlock(position)) return false;
            _chunkManager.DestroyBlock(position);
            return true;
        }

        public bool IsValidPlacementPosition(Vector3Int position)
        {
            var block = _chunkManager.GetBlock(position);
            if (!block.IsEmpty) return false;

            var below = new Vector3Int(position.X, position.Y - 1, position.Z);
            var blockBelow = _chunkManager.GetBlock(below);
            return !blockBelow.IsEmpty && _blockRegistry.IsSolid(blockBelow.BlockId);
        }
    }
}
