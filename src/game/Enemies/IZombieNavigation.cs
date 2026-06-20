using TerraForge.Core;

namespace TerraForge.Game.Enemies
{
    public interface IZombieNavigation
    {
        Vector3 ApplyNavigation(Vector3 position, Vector3 proposedVelocity);
        Vector3 GetWanderTarget(Vector3 position);
        Vector3 GetSearchTarget(Vector3 position, Vector3 lastKnownPosition);
        void OnAttack(ZombieEntity zombie, Vector3 targetPosition, float damage);
    }
}
