using System;
using TerraForge.Core;

namespace TerraForge.Game.Enemies
{
    public sealed class SimpleZombieNavigation : IZombieNavigation
    {
        private readonly float _wanderRadius;
        private readonly Random _random = new Random();

        public SimpleZombieNavigation(float wanderRadius = 8f)
        {
            _wanderRadius = wanderRadius;
        }

        public Vector3 ApplyNavigation(Vector3 position, Vector3 proposedVelocity)
        {
            return proposedVelocity;
        }

        public Vector3 GetWanderTarget(Vector3 position)
        {
            var angle = (float)(_random.NextDouble() * Math.PI * 2);
            var distance = 1.5f + (float)_random.NextDouble() * _wanderRadius;
            return new Vector3(position.X + MathF.Cos(angle) * distance, position.Y, position.Z + MathF.Sin(angle) * distance);
        }

        public Vector3 GetSearchTarget(Vector3 position, Vector3 lastKnownPosition)
        {
            var direction = lastKnownPosition - position;
            var distance = Vector3.Magnitude(direction);
            if (distance < 0.5f)
            {
                var randomOffsetX = (float)(_random.NextDouble() * 2.0 - 1.0) * 4f;
                var randomOffsetZ = (float)(_random.NextDouble() * 2.0 - 1.0) * 4f;
                return new Vector3(position.X + randomOffsetX, position.Y, position.Z + randomOffsetZ);
            }

            return lastKnownPosition;
        }

        public void OnAttack(ZombieEntity zombie, Vector3 targetPosition, float damage)
        {
        }
    }
}
