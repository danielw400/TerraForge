namespace TerraForge.Engine.RenderingBridge.Dtos
{
    public sealed class BlockTypeMetadataDto
    {
        public ushort Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool IsTransparent { get; set; }
        public bool IsFullCube { get; set; }
    }
}
