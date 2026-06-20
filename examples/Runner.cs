using System;
using TerraForge.Input;
using TerraForge.Game;

namespace TerraForge.Examples
{
    public static class Runner
    {
        public static void Main()
        {
            Console.WriteLine("TerraForge Input Manager Runner - Simulação de input\n");

            var mock = new MockInputProvider();
            var im = new InputManager(mock);
            BindingsPreset.ApplyDefaults(im);

            // subscribe to some actions for logging
            im.GetAction("Jump")!.OnPressed += () => Console.WriteLine("[Event] Jump pressed");
            im.GetAction("Attack")!.OnPressed += () => Console.WriteLine("[Event] Attack pressed");
            im.GetAction("Interact")!.OnPressed += () => Console.WriteLine("[Event] Interact pressed");
            im.GetAction("Inventory")!.OnPressed += () => Console.WriteLine("[Event] Inventory toggled");

            // simulate frames
            for (var frame = 0; frame < 10; frame++)
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
                if (frame >= 0 && frame <= 9)
                {
                    mock.SetAxis("Horizontal", 0f);
                    mock.SetAxis("Vertical", frame <= 5 ? 1f : 0f);
                }

                // update input manager
                im.Update(1f / 60f);

                // log current Move vector & Run state
                var move = im.GetAction("Move")!.Vector2Value;
                var running = im.GetAction("Run")!.Pressed;
                Console.WriteLine($"Move: ({move.x:0.00}, {move.y:0.00}) Run: {running}");

                mock.AdvanceFrame();
            }

            Console.WriteLine("\nRunner finalizado.");
        }
    }
}
