namespace TerraForge.Engine.RenderingBridge.Dtos
{
    public sealed class PlayerStateDto
    {
        public string Id { get; set; } = string.Empty;
        public PositionDto Position { get; set; } = null!;
        public VectorDto Velocity { get; set; } = null!;
        public string State { get; set; } = string.Empty;
        public bool IsGrounded { get; set; }
        public float Health { get; set; }
    }

    public sealed class PositionDto
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public sealed class VectorDto
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
