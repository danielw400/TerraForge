using System.Collections.Generic;
using System.Linq;
using TerraForge.Core;

namespace TerraForge.Game.Enemies
{
    public sealed class ZombiePerceptionSystem
    {
        public ZombiePerception GetPerception(ZombieEntity zombie, IZombieTarget playerTarget, IReadOnlyList<BaseTarget> baseTargets, IReadOnlyList<SoundEvent> soundEvents, IReadOnlyList<LightSource> lightSources, VoxelWorldAdapter worldAdapter)
        {
            var perception = new ZombiePerception();
            perception.DistanceToPlayer = float.PositiveInfinity;

            if (playerTarget != null && playerTarget.IsAlive)
            {
                var playerPosition = playerTarget.Position;
                var directionToPlayer = playerPosition - zombie.Position;
                perception.DistanceToPlayer = Vector3.Magnitude(directionToPlayer);
                perception.DirectionToPlayer = Vector3.Normalize(directionToPlayer);
                perception.PlayerPosition = playerPosition;

                if (perception.DistanceToPlayer <= zombie.AwarenessRadius && HasLineOfSight(zombie.Position, playerPosition, worldAdapter))
                {
                    perception.HasPlayerInSight = true;
                }
            }

            if (soundEvents != null)
            {
                foreach (var sound in soundEvents)
                {
                    var distance = Vector3.Magnitude(sound.Position - zombie.Position);
                    if (distance > sound.Intensity * 2f) continue;

                    var intensity = sound.Intensity / (1f + distance * distance * 0.15f);
                    if (intensity > 0.15f)
                    {
                        perception.HasLoudSound = true;
                        perception.SoundPosition = sound.Position;
                        perception.SoundIntensity = intensity;
                        break;
                    }
                }
            }

            if (lightSources != null)
            {
                foreach (var light in lightSources)
                {
                    var intensity = light.GetEffectiveIntensity(zombie.Position);
                    if (intensity <= 0f) continue;

                    perception.HasBrightLight = true;
                    perception.LightPosition = light.Position;
                    perception.LightIntensity = intensity;
                    break;
                }
            }

            if (baseTargets != null && baseTargets.Count > 0)
            {
                var nearestBase = baseTargets
                    .Where(b => b.IsAlive)
                    .OrderBy(b => Vector3.Magnitude(b.Position - zombie.Position))
                    .FirstOrDefault();

                if (nearestBase != null)
                {
                    perception.HasBaseTarget = true;
                    perception.BaseTarget = nearestBase;
                }
            }

            return perception;
        }

        private static bool HasLineOfSight(Vector3 origin, Vector3 destination, VoxelWorldAdapter worldAdapter)
        {
            var direction = destination - origin;
            var distance = Vector3.Magnitude(direction);
            if (distance < 0.001f)
            {
                return true;
            }

            return worldAdapter.RaycastBlock(origin, direction, distance) == null;
        }
    }
}
