using System;
using System.Collections.Generic;
using TerraForge.Core;
using TerraForge.Game.Enemies;

namespace TerraForge.Game
{
    // Minimal game loop integration to host the ZombieManager.
    // This class is intentionally lightweight and modular so the engine can call Start() and Update().
    public sealed class GameLoop
    {
        private readonly VoxelWorldAdapter _worldAdapter;
        private readonly PlayerController _player;
        private readonly PlayerTargetAdapter _playerTarget;
        private readonly List<BaseTarget> _baseTargets = new List<BaseTarget>();
        private readonly ZombieManager _zombieManager;
        private readonly IZombieNavigation _navigation;

        public GameLoop(VoxelWorldAdapter worldAdapter, PlayerController player)
        {
            _worldAdapter = worldAdapter ?? throw new ArgumentNullException(nameof(worldAdapter));
            _player = player ?? throw new ArgumentNullException(nameof(player));

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

            // Spawn a few zombies near the player/world
            _zombieManager.SpawnZombie(ZombieType.InfectadoComum, _player.Position + new Vector3(5f, 0f, 0f));
            _zombieManager.SpawnZombie(ZombieType.Corredor, _player.Position + new Vector3(-6f, 0f, 2f));
        }

        // Called every frame by the engine's update loop
        public void Update(float deltaTime)
        {
            // Update perception-driven systems
            _zombieManager.Update(deltaTime, _worldAdapter);
        }

        // Expose manager for debugging or editor hooks
        public ZombieManager ZombieManager => _zombieManager;
    }
}
