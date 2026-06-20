using TerraForge.Core;

namespace TerraForge.Game.Enemies
{
    public sealed class SoundEvent
    {
        public Vector3 Position { get; }
        public float Intensity { get; }
        public string Source { get; }
        public float LifetimeSeconds { get; }
        public float AgeSeconds { get; private set; }

        public SoundEvent(Vector3 position, float intensity, string source, float lifetimeSeconds = 4f)
        {
            Position = position;
            Intensity = intensity;
            Source = source;
            LifetimeSeconds = lifetimeSeconds;
            AgeSeconds = 0f;
        }

        public void Update(float deltaTime)
        {
            AgeSeconds += deltaTime;
        }

        public bool IsExpired => AgeSeconds >= LifetimeSeconds;
    }
