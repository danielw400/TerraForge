using TerraForge.Core;

namespace TerraForge.Game.Enemies
{
    public sealed class ZombiePerception
    {
        public bool HasPlayerInSight { get; set; }
        public Vector3 PlayerPosition { get; set; }
        public Vector3 DirectionToPlayer { get; set; }
        public float DistanceToPlayer { get; set; }

        public bool HasBaseTarget { get; set; }
        public BaseTarget BaseTarget { get; set; }

        public bool HasLoudSound { get; set; }
        public Vector3 SoundPosition { get; set; }
        public float SoundIntensity { get; set; }

        public bool HasBrightLight { get; set; }
        public Vector3 LightPosition { get; set; }
        public float LightIntensity { get; set; }
    }
}
