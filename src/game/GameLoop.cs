using System;
using System.Collections.Generic;
using TerraForge.Core;
using TerraForge.Game.Enemies;
using TerraForge.Input;

namespace TerraForge.Game
{
    // Minimal game loop integration to host the ZombieManager and player update flow.
    public sealed class GameLoop
    {
        private readonly VoxelWorldAdapter _worldAdapter;
        private readonly PlayerController _player;
        private readonly InputManager _inputManager;
        private readonly GameCamera _camera;
        private readonly PlayerTargetAdapter _playerTarget;
        private readonly List<BaseTarget> _baseTargets = new List<BaseTarget>();
        private readonly ZombieManager _zombieManager;
        private readonly IZombieNavigation _navigation;

        public GameLoop(VoxelWorldAdapter worldAdapter, PlayerController player, InputManager inputManager)
        {
            _worldAdapter = worldAdapter ?? throw new ArgumentNullException(nameof(worldAdapter));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            _camera = new GameCamera(new Vector3(0f, 2.0f, -5.0f));

            _playerTarget = new PlayerTargetAdapter(_player);
            _navigation = new TargetAwareNavigation(_playerTarget, _baseTargets, (target, dmg) => target.ApplyDamage(dmg));
            _zombieManager = new ZombieManager(_playerTarget, _navigation);
        }

        // Call once at initialization to register bases / spawn initial zombies.
        public void Start()
        {
            // Example base - real game should create and register bases through game editors or managers
            var baseAlpha = new BaseTarget("Base Alpha", new Vector3(12f, 12f, 12f), 250f);
            _baseTargets.Add(baseAlpha);
            _zombieManager.AddBaseTarget(baseAlpha);

            // Ensure nearby world data is updated before the first player physics update.
            _worldAdapter.UpdateChunksForPosition(_player.Position);

            // Spawn a few zombies near the player/world
            _zombieManager.SpawnZombie(ZombieType.InfectadoComum, _player.Position + new Vector3(5f, 0f, 0f));
            _zombieManager.SpawnZombie(ZombieType.Corredor, _player.Position + new Vector3(-6f, 0f, 2f));

            _camera.Follow(_player.Position);
        }

        // Called every frame by the engine's update loop
        public void Update(float deltaTime)
        {
            _inputManager.Update(deltaTime);
            var playerInput = _inputManager.ToPlayerInput();

            _player.Update(
                deltaTime,
                playerInput,
                _worldAdapter.GetSweepCollisionFunction(),
                _worldAdapter.GetIsClimbableFunction(),
                _worldAdapter.GetIsWaterFunction(),
                _worldAdapter.GetGroundInfoFunction());

            _worldAdapter.UpdateChunksForPosition(_player.Position);
            _camera.Follow(_player.Position);
            _zombieManager.Update(deltaTime, _worldAdapter);
        }

        public PlayerController Player => _player;
        public GameCamera Camera => _camera;
        public ZombieManager ZombieManager => _zombieManager;
    }
}
