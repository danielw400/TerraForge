namespace TerraForge.Game
{
    public sealed class PlayerInput
    {
        public Vector3 Move { get; set; }
        public float Vertical { get; set; }
        public bool IsRunning { get; set; }
        public bool IsCrouching { get; set; }
        public bool Jump { get; set; }
        public bool IsClimbing { get; set; }
        public bool IsSwimming { get; set; }
    }
}
