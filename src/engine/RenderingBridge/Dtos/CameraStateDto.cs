namespace TerraForge.Engine.RenderingBridge.Dtos
{
    public sealed class CameraStateDto
    {
        public PositionDto Position { get; set; } = null!;
        public PositionDto Target { get; set; } = null!;
    }
}
