using System;
using System.Collections.Generic;
using System.Linq;
using TerraForge.Core;

namespace TerraForge.Game.Building
{
    public sealed class BlockDamageManager
    {
        private readonly Dictionary<Vector3Int, BlockHealth> _blockHealthMap = new Dictionary<Vector3Int, BlockHealth>();
        private readonly ChunkManager _chunkManager;
        private readonly BlockRegistry _blockRegistry;

        public BlockDamageManager(ChunkManager chunkManager, BlockRegistry blockRegistry)
        {
            _chunkManager = chunkManager ?? throw new ArgumentNullException(nameof(chunkManager));
            _blockRegistry = blockRegistry ?? throw new ArgumentNullException(nameof(blockRegistry));
        }

        public BlockHealth GetBlockHealth(Vector3Int position)
        {
            if (_blockHealthMap.TryGetValue(position, out var health)) return health;
            var block = _chunkManager.GetBlock(position);
            if (block.IsEmpty) return null;

            var maxHealth = GetMaxHealthForBlock(block.BlockId);
            var newHealth = new BlockHealth(position, maxHealth);
            _blockHealthMap[position] = newHealth;
            return newHealth;
        }

        public void DamageBlock(Vector3Int position, float damage)
        {
            var health = GetBlockHealth(position);
            if (health == null) return;

            health.TakeDamage(damage);
            if (health.IsDestroyed)
            {
                _chunkManager.DestroyBlock(position);
                _blockHealthMap.Remove(position);
            }
        }

        public void RepairBlock(Vector3Int position, float amount)
        {
            var health = GetBlockHealth(position);
            if (health != null)
            {
                health.Repair(amount);
            }
        }

        public void DestroyBlockCompletely(Vector3Int position)
        {
            _chunkManager.DestroyBlock(position);
            _blockHealthMap.Remove(position);
        }

        public float GetBlockHealthPercentage(Vector3Int position)
        {
            var health = GetBlockHealth(position);
            return health?.GetHealthPercentage() ?? 0f;
        }

        public void ClearHealthData()
        {
            _blockHealthMap.Clear();
        }

        private float GetMaxHealthForBlock(ushort blockId)
        {
            return blockId switch
            {
                1 => 100f,
                2 => 50f,
                3 => 50f,
                4 => 75f,
                5 => 40f,
                6 => 30f,
                8 => 60f,
                9 => 60f,
                11 => 60f,
                13 => 80f,
                14 => 40f,
                16 => 70f,
                _ => 50f
            };
        }
    }
}
