using System;
using System.Collections.Generic;
using System.Linq;
using TerraForge.Core;

namespace TerraForge.Game.Enemies
{
    public sealed class ZombieManager
    {
        private readonly List<ZombieEntity> _zombies = new List<ZombieEntity>();
        private readonly List<SoundEvent> _soundEvents = new List<SoundEvent>();
        private readonly List<LightSource> _lightSources = new List<LightSource>();
        private readonly List<BaseTarget> _baseTargets = new List<BaseTarget>();
        private readonly ZombiePerceptionSystem _perceptionSystem = new ZombiePerceptionSystem();
        private readonly IZombieTarget _playerTarget;
        private readonly IZombieNavigation _navigation;

        public IReadOnlyList<ZombieEntity> Zombies => _zombies;
        public IReadOnlyList<SoundEvent> ActiveSounds => _soundEvents;
        public IReadOnlyList<LightSource> ActiveLights => _lightSources;
        public IReadOnlyList<BaseTarget> BaseTargets => _baseTargets;

        public ZombieManager(IZombieTarget playerTarget, IZombieNavigation navigation)
        {
            _playerTarget = playerTarget ?? throw new ArgumentNullException(nameof(playerTarget));
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        }

        public void AddZombie(ZombieEntity zombie)
        {
            if (zombie == null) throw new ArgumentNullException(nameof(zombie));
            _zombies.Add(zombie);
        }

        public void AddBaseTarget(BaseTarget target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            _baseTargets.Add(target);
        }

        public void AddSound(Vector3 position, float intensity, string source, float lifetimeSeconds = 4f)
        {
            _soundEvents.Add(new SoundEvent(position, intensity, source, lifetimeSeconds));
        }

        public void AddLight(Vector3 position, float intensity, float radius, float lifetimeSeconds = 4f)
        {
            _lightSources.Add(new LightSource(position, intensity, radius, lifetimeSeconds));
        }

        public void Update(float deltaTime, VoxelWorldAdapter worldAdapter)
        {
            _soundEvents.RemoveAll(sound => sound.IsExpired);
            _lightSources.RemoveAll(light => light.IsExpired);

            foreach (var sound in _soundEvents)
            {
                sound.Update(deltaTime);
            }

            foreach (var light in _lightSources)
            {
                light.Update(deltaTime);
            }

            foreach (var zombie in _zombies.ToArray())
            {
                if (!zombie.IsAlive) continue;

                var perception = _perceptionSystem.GetPerception(zombie, _playerTarget, _baseTargets, _soundEvents, _lightSources, worldAdapter);
                zombie.Update(deltaTime, perception, _navigation);
            }
        }

        public void SpawnZombie(ZombieType zombieType, Vector3 position)
        {
            var zombie = new ZombieEntity(zombieType, position);
            _zombies.Add(zombie);
        }

        public void ClearZombies()
        {
            _zombies.Clear();
        }
    }
}
