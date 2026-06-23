using TerraForge.Engine.RenderingBridge.Dtos;
using TerraForge.Game;

namespace TerraForge.Engine.RenderingBridge.Adapters
{
    /// <summary>
    /// Converte o estado do jogador em um DTO para renderização no frontend.
    /// </summary>
    /// <example>
    /// var playerDto = new PlayerDtoAdapter().ToDto(gameLoop.Player);
    /// </example>
    public sealed class PlayerDtoAdapter
    {
        public PlayerStateDto ToDto(PlayerController player)
        {
            return new PlayerStateDto
            {
                Id = "player",
                Position = new PositionDto
                {
                    X = player.Position.X,
                    Y = player.Position.Y,
                    Z = player.Position.Z
                },
                Velocity = new VectorDto
                {
                    X = player.Velocity.X,
                    Y = player.Velocity.Y,
                    Z = player.Velocity.Z
                },
                State = player.CurrentState.ToString(),
                IsGrounded = player.IsGrounded,
                Health = player.Stats.Health
            };
        }
    }
}