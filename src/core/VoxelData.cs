namespace TerraForge.Core
{
    public readonly struct VoxelData
    {
        public ushort BlockId { get; }
        public byte Meta { get; }

        public static VoxelData Empty => new VoxelData(0, 0);

        public VoxelData(ushort blockId, byte meta = 0)
        {
            BlockId = blockId;
            Meta = meta;
        }

        public bool IsEmpty => BlockId == 0;
    }
}
