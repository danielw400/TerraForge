using System.Collections.Generic;

namespace TerraForge.Core
{
    public sealed class BlockRegistry
    {
        private readonly Dictionary<ushort, BlockType> _blockTypes = new Dictionary<ushort, BlockType>();

        public BlockRegistry()
        {
            RegisterDefaultBlocks();
        }

        public void Register(BlockType blockType)
        {
            if (!_blockTypes.ContainsKey(blockType.Id))
            {
                _blockTypes.Add(blockType.Id, blockType);
            }
        }

        public BlockType Get(ushort blockId)
        {
            if (_blockTypes.TryGetValue(blockId, out var blockType))
            {
                return blockType;
            }

            return _blockTypes[0];
        }

        public bool IsSolid(ushort blockId)
        {
            return Get(blockId).IsSolid;
        }

        private void RegisterDefaultBlocks()
        {
            Register(new BlockType(0, "Air", false, true, false));
            Register(new BlockType(1, "Stone", true, false));
            Register(new BlockType(2, "Dirt", true, false));
            Register(new BlockType(3, "Grass", true, false));
            Register(new BlockType(4, "Wood", true, false));
            Register(new BlockType(5, "Glass", true, true));
            Register(new BlockType(6, "Sand", true, false));
            Register(new BlockType(7, "SwampWater", false, true, false));
            Register(new BlockType(8, "InfectedGrass", true, false));
            Register(new BlockType(9, "InfectedSoil", true, false));
            Register(new BlockType(10, "Water", false, true, false));
            Register(new BlockType(11, "Mud", true, false));
            Register(new BlockType(12, "Leaves", false, true));
            Register(new BlockType(13, "WoodLog", true, false));
            Register(new BlockType(14, "Cactus", true, false));
            Register(new BlockType(15, "LilyPad", false, true, false));
            Register(new BlockType(16, "DeadLog", true, false));
            Register(new BlockType(17, "Reed", false, true, false));
            Register(new BlockType(18, "Mushroom", false, true, false));
        }
    }
}
