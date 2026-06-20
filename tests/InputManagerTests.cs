using System;
using TerraForge.Input;

namespace TerraForge.Tests
{
    public static class InputManagerTests
    {
        public static void RunAll()
        {
            Console.WriteLine("Iniciando testes do InputManager...");
            TestJumpPressedDown();
            TestAttackPressedEvent();
            TestMoveAxisMapping();
            Console.WriteLine("Todos os testes concluídos com sucesso.");
        }

        private static void TestJumpPressedDown()
        {
            var mock = new MockInputProvider();
            var im = new InputManager(mock);
            BindingsPreset.ApplyDefaults(im);

            var jump = im.GetAction("Jump");
            if (jump == null) throw new Exception("Ação Jump não registrada");

            // frame 1: not pressed
            mock.SetButton("Space", false);
            im.Update(1f / 60f);
            mock.AdvanceFrame();

            // frame 2: press
            mock.SetButton("Space", true);
            im.Update(1f / 60f);

            if (!jump.PressedDown)
            {
                throw new Exception("Teste falhou: JumpPressedDown não detectado");
            }

            Console.WriteLine("TestJumpPressedDown OK");
            mock.AdvanceFrame();
        }

        private static void TestAttackPressedEvent()
        {
            var mock = new MockInputProvider();
            var im = new InputManager(mock);
            BindingsPreset.ApplyDefaults(im);

            var attack = im.GetAction("Attack");
            var triggered = false;
            attack!.OnPressed += () => triggered = true;

            // simulate attack press
            mock.SetButton("Mouse0", true);
            im.Update(1f / 60f);

            if (!triggered)
            {
                throw new Exception("Teste falhou: evento Attack não disparado");
            }

            Console.WriteLine("TestAttackPressedEvent OK");
            mock.AdvanceFrame();
        }

        private static void TestMoveAxisMapping()
        {
            var mock = new MockInputProvider();
            var im = new InputManager(mock);
            BindingsPreset.ApplyDefaults(im);

            mock.SetAxis("Horizontal", 0.5f);
            mock.SetAxis("Vertical", -0.25f);
            im.Update(1f / 60f);

            var move = im.GetAction("Move")!.Vector2Value;
            if (Math.Abs(move.x - 0.5f) > 0.001f || Math.Abs(move.y + 0.25f) > 0.001f)
            {
                throw new Exception("Teste falhou: Axis Move incorreto");
            }

            Console.WriteLine("TestMoveAxisMapping OK");
            mock.AdvanceFrame();
        }
    }
}
