using System;
using TerraForge.Core;
using TerraForge.Input;
using TerraForge.Game;
using TerraForge.Game.Enemies;

namespace TerraForge.Examples
{
    public static class Runner
    {
        public static void Main()
        {
            Console.WriteLine("TerraForge Input Manager Runner - Simulação de input e demo do GameLoop\n");

            var mock = new MockInputProvider();
            var im = new InputManager(mock);
            BindingsPreset.ApplyDefaults(im);

            // Minimal world + player setup for the GameLoop demo
            var blockRegistry = new BlockRegistry();
            var chunkManager = new ChunkManager(blockRegistry);
            var worldAdapter = new VoxelWorldAdapter(chunkManager, new Vector3(0.45f, 0.9f, 0.45f));

            var playerStats = new PlayerStats();
            var player = new PlayerController(playerStats, new Vector3(8f, 12f, 8f));

            var gameLoop = new GameLoop(worldAdapter, player, im);
            gameLoop.Start();

            // subscribe to some actions for logging
            im.GetAction("Jump")!.OnPressed += () => Console.WriteLine("[Event] Jump pressed");
            im.GetAction("Attack")!.OnPressed += () => Console.WriteLine("[Event] Attack pressed");
            im.GetAction("Interact")!.OnPressed += () => Console.WriteLine("[Event] Interact pressed");
            im.GetAction("Inventory")!.OnPressed += () => Console.WriteLine("[Event] Inventory toggled");

            // simulate frames
            for (var frame = 0; frame < 120; frame++)
            {
                Console.WriteLine($"\n-- Frame {frame} --");

                // simulate input timeline
                if (frame == 1)
                {
                    mock.SetButton("Space", true); // jump down
                }

                if (frame == 2)
                {
                    mock.SetButton("Space", false); // jump release
                }

                if (frame == 3)
                {
                    mock.SetButton("Mouse0", true); // attack
                }

                if (frame == 4)
                {
                    mock.SetButton("Mouse0", false);
                }

                if (frame == 5)
                {
                    mock.SetButton("Tab", true);
                }

                if (frame == 6)
                {
                    mock.SetButton("Tab", false);
                }

                // set movement axis (W key simulated as Vertical=1)
                mock.SetAxis("Horizontal", 0f);
                mock.SetAxis("Vertical", frame <= 60 ? 1f : 0f);

                // update input manager
                im.Update(1f / 60f);

                // update game systems (including PlayerController and ZombieManager)
                gameLoop.Update(1f / 60f);

                // log current player and camera positions
                var move = im.GetAction("Move")!.Vector2Value;
                var running = im.GetAction("Run")!.Pressed;
                Console.WriteLine($"Move: ({move.x:0.00}, {move.y:0.00}) Run: {running}");
                Console.WriteLine($"Player: Pos=({gameLoop.Player.Position.X:0.00},{gameLoop.Player.Position.Y:0.00},{gameLoop.Player.Position.Z:0.00}) State={gameLoop.Player.CurrentState}");
                Console.WriteLine($"Camera: Pos=({gameLoop.Camera.Position.X:0.00},{gameLoop.Camera.Position.Y:0.00},{gameLoop.Camera.Position.Z:0.00}) Target=({gameLoop.Camera.Target.X:0.00},{gameLoop.Camera.Target.Y:0.00},{gameLoop.Camera.Target.Z:0.00})");

                // show zombie states (if any)
                var zombies = gameLoop.ZombieManager.Zombies;
                foreach (var z in zombies)
                {
                    Console.WriteLine($"{z.Type} - State: {z.State} - Pos: ({z.Position.X:0.0},{z.Position.Y:0.0},{z.Position.Z:0.0}) HP: {z.Health:0}");
                }

                mock.AdvanceFrame();
            }

            Console.WriteLine("\nRunner finalizado.");
        }
    }
}
