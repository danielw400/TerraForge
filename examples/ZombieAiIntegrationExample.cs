using System;
using System.Collections.Generic;
using TerraForge.Core;
using TerraForge.Game;
using TerraForge.Game.Enemies;

namespace TerraForge.Examples
{
    public sealed class ZombieAiIntegrationExample
    {
        public static void Main()
        {
            Console.WriteLine("=== TerraForge Zombie AI Integration Example ===\n");

            var blockRegistry = new BlockRegistry();
            var chunkManager = new ChunkManager(blockRegistry);
            var worldAdapter = new VoxelWorldAdapter(chunkManager, new Vector3(0.45f, 0.9f, 0.45f));

            var playerStats = new PlayerStats();
            var player = new PlayerController(playerStats, new Vector3(8f, 12f, 8f));
            var playerTarget = new PlayerTargetAdapter(player);

            var baseTarget = new BaseTarget("Base Alpha", new Vector3(12f, 12f, 12f), 250f);
            var baseTargets = new List<BaseTarget> { baseTarget };

            var navigation = new TargetAwareNavigation(playerTarget, baseTargets, (target, damage) => target.ApplyDamage(damage));
            var zombieManager = new ZombieManager(playerTarget, navigation);
            zombieManager.AddBaseTarget(baseTarget);

            zombieManager.SpawnZombie(ZombieType.InfectadoComum, new Vector3(2f, 12f, 2f));
            zombieManager.SpawnZombie(ZombieType.Corredor, new Vector3(4f, 12f, 4f));
            zombieManager.SpawnZombie(ZombieType.Blindado, new Vector3(18f, 12f, 18f));
            zombieManager.SpawnZombie(ZombieType.Mutante, new Vector3(20f, 12f, 6f));

            var light = new LightSource(new Vector3(10f, 12f, 10f), 1.0f, 12f, 10f);
            var sound = new SoundEvent(new Vector3(8f, 12f, 8f), 1.0f, "Tiro", 3f);
            zombieManager.AddLight(light.Position, light.Intensity, light.Radius, light.LifetimeSeconds);
            zombieManager.AddSound(sound.Position, sound.Intensity, sound.Source, sound.LifetimeSeconds);

            for (var frame = 0; frame < 120; frame++)
            {
                Console.WriteLine($"\n--- Frame {frame + 1} ---");
                zombieManager.Update(0.1f, worldAdapter);

                foreach (var zombie in zombieManager.Zombies)
                {
                    Console.WriteLine($"{zombie.Type} - State: {zombie.State} - Position: ({zombie.Position.X:F1},{zombie.Position.Y:F1},{zombie.Position.Z:F1}) - Health: {zombie.Health:F0}");
                }

                Console.WriteLine($"Base '{baseTarget.Name}' Integrity: {baseTarget.Integrity:F0}");

                if (!player.Stats.IsAlive)
                {
                    Console.WriteLine("Jogador morreu!");
                    break;
                }

                if (baseTarget.IsDestroyed)
                {
                    Console.WriteLine("Base destruída!");
                    break;
                }
            }

            Console.WriteLine("\n=== Exemplo Finalizado ===");
        }
    }
}
