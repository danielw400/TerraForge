using TerraForge.Core;

namespace TerraForge.Game.Enemies
{
    public sealed class BaseTarget : IZombieTarget
    {
        public string Name { get; }
        public Vector3 Position { get; }
        public float Integrity { get; private set; }

        public BaseTarget(string name, Vector3 position, float integrity = 100f)
        {
            Name = name;
            Position = position;
            Integrity = integrity;
        }

        public bool IsAlive => Integrity > 0f;

        public bool IsDestroyed => Integrity <= 0f;

        public void ApplyDamage(float amount)
        {
            Integrity -= amount;
            if (Integrity < 0f) Integrity = 0f;
        }
    }
}
