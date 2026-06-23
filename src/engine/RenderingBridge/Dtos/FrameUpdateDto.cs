using System.Collections.Generic;

namespace TerraForge.Engine.RenderingBridge.Dtos
{
    public sealed class FrameUpdateDto
    {
        public List<ChunkDto> Chunks { get; set; } = new();
        public List<ChunkUnloadDto> ChunkUnloads { get; set; } = new();
        public PlayerStateDto? Player { get; set; }
        public List<ZombieStateDto> Zombies { get; set; } = new();
        public CameraStateDto? Camera { get; set; }
    }
}
