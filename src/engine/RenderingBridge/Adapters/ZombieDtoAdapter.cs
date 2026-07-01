using System;
using System.Runtime.CompilerServices;
using TerraForge.Engine.RenderingBridge.Dtos;
using TerraForge.Game.Enemies;

namespace TerraForge.Engine.RenderingBridge.Adapters
{
    public sealed class ZombieDtoAdapter
    {
        public ZombieStateDto ToDto(ZombieEntity zombie)
        {
            return new ZombieStateDto
            {
                Id = GenerateEntityId(zombie),
                Type = zombie.Type.ToString(),
                Position = new PositionDto
                {
                    X = zombie.Position.X,
                    Y = zombie.Position.Y,
                    Z = zombie.Position.Z
                },
                State = zombie.State.ToString(),
                Health = zombie.Health,
                TargetPosition = new PositionDto
                {
                    X = zombie.LastKnownTargetPosition.X,
                    Y = zombie.LastKnownTargetPosition.Y,
                    Z = zombie.LastKnownTargetPosition.Z
                }
            };
        }

        private static string GenerateEntityId(ZombieEntity zombie)
        {
            return $"zombie-{RuntimeHelpers.GetHashCode(zombie)}";
        }
    }
}