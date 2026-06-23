using System;

namespace TerraForge.Engine.RenderingBridge.Dtos
{
    public sealed class ChunkDto
    {
        public ChunkCoordDto Coord { get; set; } = null!;
        public ushort[] Blocks { get; set; } = Array.Empty<ushort>();
        public int Version { get; set; }
        public bool IsDirty { get; set; }
    }

    public sealed class ChunkCoordDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }
}
