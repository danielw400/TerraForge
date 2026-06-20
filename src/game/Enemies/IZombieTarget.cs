using TerraForge.Core;

namespace TerraForge.Game.Enemies
{
    public interface IZombieTarget
    {
        string Name { get; }
        Vector3 Position { get; }
        bool IsAlive { get; }
        void ApplyDamage(float damage);
    }
}
