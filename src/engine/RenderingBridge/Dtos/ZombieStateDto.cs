namespace TerraForge.Engine.RenderingBridge.Dtos
{
    public sealed class ZombieStateDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public PositionDto Position { get; set; } = null!;
        public string State { get; set; } = string.Empty;
        public float Health { get; set; }
        public PositionDto? TargetPosition { get; set; }
    }
}
