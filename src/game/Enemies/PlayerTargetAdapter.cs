using TerraForge.Core;
using TerraForge.Game;

namespace TerraForge.Game.Enemies
{
    public sealed class PlayerTargetAdapter : IZombieTarget
    {
        private readonly PlayerController _player;

        public PlayerTargetAdapter(PlayerController player)
        {
            _player = player;
        }

        public string Name => "Jogador";
        public Vector3 Position => _player.Position;
        public bool IsAlive => _player.Stats.IsAlive;

        public void ApplyDamage(float damage)
        {
            _player.Stats.AddHealth(-damage);
        }
    }
}
