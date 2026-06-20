using TerraForge.Core;

namespace TerraForge.Game
{
    public sealed class GameCamera
    {
        private readonly Vector3 _offset;

        public Vector3 Position { get; private set; }
        public Vector3 Target { get; private set; }

        public GameCamera(Vector3 offset)
        {
            _offset = offset;
            Position = Vector3.Zero;
            Target = Vector3.Zero;
        }

        public void Follow(Vector3 playerPosition)
        {
            Target = playerPosition;
            Position = new Vector3(playerPosition.X + _offset.X, playerPosition.Y + _offset.Y, playerPosition.Z + _offset.Z);
        }
    }
}
