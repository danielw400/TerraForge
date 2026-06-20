using TerraForge.Core;

namespace TerraForge.Game.Enemies
{
    public sealed class LightSource
    {
        public Vector3 Position { get; }
        public float Intensity { get; }
        public float Radius { get; }
        public float LifetimeSeconds { get; }
        public float AgeSeconds { get; private set; }

        public LightSource(Vector3 position, float intensity, float radius, float lifetimeSeconds = 4f)
        {
            Position = position;
            Intensity = intensity;
            Radius = radius;
            LifetimeSeconds = lifetimeSeconds;
            AgeSeconds = 0f;
        }

        public void Update(float deltaTime)
        {
            AgeSeconds += deltaTime;
        }

        public bool IsExpired => AgeSeconds >= LifetimeSeconds;

        public float GetEffectiveIntensity(Vector3 observerPosition)
        {
            var distance = Vector3.Magnitude(observerPosition - Position);
            if (distance > Radius) return 0f;
            return Intensity / (1f + distance * distance * 0.12f);
        }
    }
}
