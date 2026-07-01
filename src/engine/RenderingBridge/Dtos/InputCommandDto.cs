namespace TerraForge.Engine.RenderingBridge.Dtos
{
    public sealed class InputCommandDto
    {
        public string Action { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public long Timestamp { get; set; }
    }
}
