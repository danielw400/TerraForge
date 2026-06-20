using System;
using System.Collections.Generic;
using System.Linq;
using TerraForge.Core;

namespace TerraForge.Game.Enemies
{
    public sealed class TargetAwareNavigation : IZombieNavigation
    {
        private readonly IZombieTarget _playerTarget;
        private readonly IReadOnlyList<BaseTarget> _baseTargets;
        private readonly Action<IZombieTarget, float> _damageHandler;

        public TargetAwareNavigation(IZombieTarget playerTarget, IReadOnlyList<BaseTarget> baseTargets, Action<IZombieTarget, float> damageHandler)
        {
            _playerTarget = playerTarget;
            _baseTargets = baseTargets;
            _damageHandler = damageHandler;
        }

        public Vector3 ApplyNavigation(Vector3 position, Vector3 proposedVelocity)
        {
            return proposedVelocity;
        }

        public Vector3 GetWanderTarget(Vector3 position)
        {
            var random = new Random();
            var angle = (float)(random.NextDouble() * Math.PI * 2);
            var distance = 1.5f + (float)random.NextDouble() * 6f;
            return new Vector3(position.X + MathF.Cos(angle) * distance, position.Y, position.Z + MathF.Sin(angle) * distance);
        }

        public Vector3 GetSearchTarget(Vector3 position, Vector3 lastKnownPosition)
        {
            var offsetAngle = (float)(new Random().NextDouble() * Math.PI * 2);
            var randomOffsetX = MathF.Cos(offsetAngle) * 3.5f;
            var randomOffsetZ = MathF.Sin(offsetAngle) * 3.5f;
            return new Vector3(lastKnownPosition.X + randomOffsetX, position.Y, lastKnownPosition.Z + randomOffsetZ);
        }

        public void OnAttack(ZombieEntity zombie, Vector3 targetPosition, float damage)
        {
            var candidateTargets = new List<IZombieTarget>();
            if (_playerTarget != null && _playerTarget.IsAlive)
            {
                candidateTargets.Add(_playerTarget);
            }

            if (_baseTargets != null)
            {
                candidateTargets.AddRange(_baseTargets.Where(b => b.IsAlive));
            }

            var bestTarget = candidateTargets
                .OrderBy(t => Vector3.Magnitude(t.Position - targetPosition))
                .FirstOrDefault();

            if (bestTarget != null)
            {
                _damageHandler(bestTarget, damage);
            }
        }
    }
}
