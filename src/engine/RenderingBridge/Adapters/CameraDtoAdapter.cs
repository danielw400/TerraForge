using TerraForge.Engine.RenderingBridge.Dtos;
using TerraForge.Game;

namespace TerraForge.Engine.RenderingBridge.Adapters
{
    /// <summary>
    /// Converte o estado de câmera do jogo em um DTO para renderização.
    /// </summary>
    /// <example>
    /// var cameraDto = new CameraDtoAdapter().ToDto(gameLoop.Camera);
    /// </example>
    public sealed class CameraDtoAdapter
    {
        public CameraStateDto ToDto(GameCamera camera)
        {
            return new CameraStateDto
            {
                Position = new PositionDto
                {
                    X = camera.Position.X,
                    Y = camera.Position.Y,
                    Z = camera.Position.Z
                },
                Target = new PositionDto
                {
                    X = camera.Target.X,
                    Y = camera.Target.Y,
                    Z = camera.Target.Z
                }
            };
        }
    }
}