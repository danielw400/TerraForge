using System;
using TerraForge.Core;
using TerraForge.Game;

namespace TerraForge.Tests
{
    public static class WorldIntegrationTests
    {
        public static void RunAll()
        {
            Console.WriteLine("Running world integration tests...");
            TestCollisionWithGround();
            Console.WriteLine("World integration tests passed.");
        }

        private static void TestCollisionWithGround()
        {
            var registry = new BlockRegistry();
            var generator = new WorldGenerator(registry, seed: 42, seaLevel: 32);
            var cm = new ChunkManager(generator, loadRadius: 1, unloadRadius: 2);
            cm.UpdateChunks(new ChunkCoord(0, 4, 0));

            var adapter = new VoxelWorldAdapter(cm, new Vector3(0.3f, 1f, 0.3f));

            // place at high Y and let gravity drop into terrain
            var stats = new PlayerStats();
            var player = new PlayerController(stats, new Vector3(0.5f, 64f, 0.5f));
            var input = new PlayerInput();

            for (var i = 0; i < 200; i++)
            {
                player.Update(1f / 60f, input, adapter.GetSweepCollisionFunction(), adapter.GetIsClimbableFunction(), adapter.GetIsWaterFunction(), adapter.GetGroundInfoFunction());
            }

            if (player.Position.Y <= 10f)
            {
                throw new Exception("Player fell too deep; collision likely failed");
            }

            Console.WriteLine("TestCollisionWithGround OK");
        }
    }
}
