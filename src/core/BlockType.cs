namespace TerraForge.Core
{
    public sealed class BlockType
    {
        public ushort Id { get; }
        public string Name { get; }
        public bool IsSolid { get; }
        public bool IsTransparent { get; }
        public bool IsFullCube { get; }

        public BlockType(ushort id, string name, bool isSolid, bool isTransparent, bool isFullCube = true)
        {
            Id = id;
            Name = name;
            IsSolid = isSolid;
            IsTransparent = isTransparent;
            IsFullCube = isFullCube;
        }
    }
}
