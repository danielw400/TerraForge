using System;
using TerraForge.Core;
using TerraForge.Game;
using TerraForge.Input;

namespace TerraForge.Examples
{
    public static class WorldIntegrationRunner
    {
        public static void Main()
        {
            Console.WriteLine("TerraForge World Integration Runner\n");

            var blockRegistry = new BlockRegistry();
            var worldGen = new WorldGenerator(blockRegistry, seed: 12345, seaLevel: 32);
            var chunkManager = new ChunkManager(worldGen, loadRadius: 2, unloadRadius: 3);

            // place player above ground at origin
            var playerStart = new Vector3(0.5f, 64f, 0.5f);
            var playerStats = new PlayerStats();
            var player = new PlayerController(playerStats, playerStart);

            // preload chunks around player
            var playerChunk = new ChunkCoord(0, 4, 0); // approximate chunk Y for 64 height (64/16=4)
            chunkManager.UpdateChunks(playerChunk);

            var adapter = new VoxelWorldAdapter(chunkManager, new Vector3(0.3f, 1.0f, 0.3f));

            Console.WriteLine("Player starting at " + playerStart.X + "," + playerStart.Y + "," + playerStart.Z);

            // simulate a few frames falling to ground
            var input = new PlayerInput();
            for (var i = 0; i < 120; i++)
            {
                // no input; gravity should pull player down
                player.Update(1f / 60f, input, adapter.GetSweepCollisionFunction(), adapter.GetIsClimbableFunction(), adapter.GetIsWaterFunction(), adapter.GetGroundInfoFunction());
                if (i % 10 == 0)
                {
                    Console.WriteLine($"Frame {i}: Pos=({player.Position.X:0.00},{player.Position.Y:0.00},{player.Position.Z:0.00}) Health={player.Stats.Health:0.0} State={player.CurrentState}");
                }
            }

            Console.WriteLine("After fall: Health=" + player.Stats.Health);

            // interact: raycast down and remove block under player
            var origin = player.Position + new Vector3(0, 0.5f, 0);
            var dir = new Vector3(0, -1, 0);
            var removed = adapter.RemoveBlockAt(origin, dir, 5f);
            Console.WriteLine("Block removed under player: " + removed);

            // place a stone block in front
            var placeDir = new Vector3(1, 0, 0);
            var placed = adapter.PlaceBlockAt(origin, placeDir, BlockTypeIds.Stone, 5f);
            Console.WriteLine("Block placed in front: " + placed);

            Console.WriteLine("World integration demo finished.");
        }
    }
}
