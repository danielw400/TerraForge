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

        public FrameUpdateDto PublishFrameState()
        {
            var frame = new FrameUpdateDto();

            var chunkUpdates = _chunkChangeTracker.GetUpdatedChunks();
            frame.Chunks.AddRange(chunkUpdates.Select(_chunkAdapter.ToDto));
            frame.ChunkUnloads.AddRange(_chunkChangeTracker.GetRemovedChunks().Select(coord => new ChunkUnloadDto
            {
                Coord = _chunkAdapter.ToDtoCoord(coord)
            }));

            frame.Player = _playerAdapter.ToDto(_gameLoop.Player);
            frame.Zombies.AddRange(_gameLoop.ZombieManager.Zombies.Select(_zombieAdapter.ToDto));
            frame.Camera = _cameraAdapter.ToDto(_gameLoop.Camera);

            return frame;
        }
    }
}
