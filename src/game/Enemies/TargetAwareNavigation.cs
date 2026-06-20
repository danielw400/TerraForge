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
        private readonly float _playerPriorityRange;

        public TargetAwareNavigation(IZombieTarget playerTarget, IReadOnlyList<BaseTarget> baseTargets, Action<IZombieTarget, float> damageHandler, float playerPriorityRange = 6f)
        {
            _playerTarget = playerTarget;
            _baseTargets = baseTargets;
            _damageHandler = damageHandler;
            _playerPriorityRange = playerPriorityRange;
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
            // Prefer player if alive and within priority range or closer than any base
            if (_playerTarget != null && _playerTarget.IsAlive)
            {
                var playerDist = Vector3.Magnitude(_playerTarget.Position - targetPosition);
                BaseTarget nearestBase = null;
                float baseDist = float.PositiveInfinity;
                if (_baseTargets != null)
                {
                    nearestBase = _baseTargets.Where(b => b.IsAlive)
                                              .OrderBy(b => Vector3.Magnitude(b.Position - targetPosition))
                                              .FirstOrDefault();
                    if (nearestBase != null) baseDist = Vector3.Magnitude(nearestBase.Position - targetPosition);
                }

                if (playerDist <= _playerPriorityRange || playerDist <= baseDist)
                {
                    _damageHandler(_playerTarget, damage);
                    return;
                }
            }

            // Fallback to nearest base
            if (_baseTargets != null)
            {
                var bestBase = _baseTargets.Where(b => b.IsAlive)
                                           .OrderBy(b => Vector3.Magnitude(b.Position - targetPosition))
                                           .FirstOrDefault();
                if (bestBase != null)
                {
                    _damageHandler(bestBase, damage);
                }
            }
        }
    }
}
