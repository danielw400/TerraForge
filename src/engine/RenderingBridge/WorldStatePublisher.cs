using System;
using System.Collections.Generic;
using System.Linq;
using TerraForge.Core;
using TerraForge.Engine.RenderingBridge.Adapters;
using TerraForge.Engine.RenderingBridge.Dtos;
using TerraForge.Game;
using TerraForge.Game.Enemies;

namespace TerraForge.Engine.RenderingBridge
{
    public sealed class WorldStatePublisher
    {
        private readonly ChunkChangeTracker _chunkChangeTracker;
        private readonly ChunkDtoAdapter _chunkAdapter;
        private readonly PlayerDtoAdapter _playerAdapter;
        private readonly ZombieDtoAdapter _zombieAdapter;
        private readonly CameraDtoAdapter _cameraAdapter;
        private readonly GameLoop _gameLoop;

        private PlayerStateDto? _lastPublishedPlayer;
        private IReadOnlyList<ZombieStateDto> _lastPublishedZombies = Array.Empty<ZombieStateDto>();
        private CameraStateDto? _lastPublishedCamera;

        public WorldStatePublisher(
            GameLoop gameLoop,
            ChunkChangeTracker chunkChangeTracker,
            ChunkDtoAdapter chunkAdapter,
            PlayerDtoAdapter playerAdapter,
            ZombieDtoAdapter zombieAdapter,
            CameraDtoAdapter cameraAdapter)
        {
            _gameLoop = gameLoop ?? throw new ArgumentNullException(nameof(gameLoop));
            _chunkChangeTracker = chunkChangeTracker ?? throw new ArgumentNullException(nameof(chunkChangeTracker));
            _chunkAdapter = chunkAdapter ?? throw new ArgumentNullException(nameof(chunkAdapter));
            _playerAdapter = playerAdapter ?? throw new ArgumentNullException(nameof(playerAdapter));
            _zombieAdapter = zombieAdapter ?? throw new ArgumentNullException(nameof(zombieAdapter));
            _cameraAdapter = cameraAdapter ?? throw new ArgumentNullException(nameof(cameraAdapter));
        }

        public FrameUpdateDto PublishInitialSnapshot()
        {
            _lastPublishedPlayer = null;
            _lastPublishedZombies = Array.Empty<ZombieStateDto>();
            _lastPublishedCamera = null;

            return PublishFrameState();
        }

        public FrameUpdateDto PublishFrameState()
        {
            var frame = new FrameUpdateDto();

            var chunkUpdates = _chunkChangeTracker.GetUpdatedChunks();
            frame.Chunks.AddRange(chunkUpdates.Select(_chunkAdapter.ToDto));
            frame.ChunkUnloads.AddRange(_chunkChangeTracker.GetRemovedChunks().Select(coord => new ChunkUnloadDto
            {
                Coord = _chunkAdapter.ToDtoCoord(coord)
            }));

            var currentPlayer = _playerAdapter.ToDto(_gameLoop.Player);
            var currentZombies = _gameLoop.ZombieManager.Zombies.Select(_zombieAdapter.ToDto).ToList();
            var currentCamera = _cameraAdapter.ToDto(_gameLoop.Camera);

            if (_lastPublishedPlayer == null || !ArePlayerStatesEqual(_lastPublishedPlayer, currentPlayer))
            {
                frame.Player = currentPlayer;
            }

            if (_lastPublishedZombies.Count != currentZombies.Count || !AreZombieStatesEqual(_lastPublishedZombies, currentZombies))
            {
                frame.Zombies.AddRange(currentZombies);
            }

            if (_lastPublishedCamera == null || !AreCameraStatesEqual(_lastPublishedCamera, currentCamera))
            {
                frame.Camera = currentCamera;
            }

            _lastPublishedPlayer = currentPlayer;
            _lastPublishedZombies = currentZombies;
            _lastPublishedCamera = currentCamera;

            return frame;
        }

        private static bool ArePlayerStatesEqual(PlayerStateDto left, PlayerStateDto right)
        {
            return left.Id == right.Id &&
                   left.State == right.State &&
                   left.IsGrounded == right.IsGrounded &&
                   left.Health == right.Health &&
                   left.Position.X == right.Position.X &&
                   left.Position.Y == right.Position.Y &&
                   left.Position.Z == right.Position.Z &&
                   left.Velocity.X == right.Velocity.X &&
                   left.Velocity.Y == right.Velocity.Y &&
                   left.Velocity.Z == right.Velocity.Z;
        }

        private static bool AreZombieStatesEqual(IReadOnlyList<ZombieStateDto> previous, IReadOnlyList<ZombieStateDto> current)
        {
            if (previous.Count != current.Count)
            {
                return false;
            }

            for (var index = 0; index < previous.Count; index++)
            {
                if (!AreZombieStatesEqual(previous[index], current[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AreZombieStatesEqual(ZombieStateDto left, ZombieStateDto right)
        {
            return left.Id == right.Id &&
                   left.Type == right.Type &&
                   left.State == right.State &&
                   left.Health == right.Health &&
                   left.Position.X == right.Position.X &&
                   left.Position.Y == right.Position.Y &&
                   left.Position.Z == right.Position.Z &&
                   (left.TargetPosition == null && right.TargetPosition == null ||
                    left.TargetPosition != null && right.TargetPosition != null &&
                    left.TargetPosition.X == right.TargetPosition.X &&
                    left.TargetPosition.Y == right.TargetPosition.Y &&
                    left.TargetPosition.Z == right.TargetPosition.Z);
        }

        private static bool AreCameraStatesEqual(CameraStateDto left, CameraStateDto right)
        {
            return left.Position.X == right.Position.X &&
                   left.Position.Y == right.Position.Y &&
                   left.Position.Z == right.Position.Z &&
                   left.Target.X == right.Target.X &&
                   left.Target.Y == right.Target.Y &&
                   left.Target.Z == right.Target.Z;
        }
    }
}
