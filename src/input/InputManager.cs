using System;
using System.Collections.Generic;
using System.Linq;
using TerraForge.Game;

namespace TerraForge.Input
{
    public sealed class InputManager
    {
        private readonly IInputProvider _provider;
        private readonly Dictionary<string, InputAction> _actions = new Dictionary<string, InputAction>(StringComparer.OrdinalIgnoreCase);
        private readonly List<InputBinding> _bindings = new List<InputBinding>();

        public InputManager(IInputProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        // Action registration
        public InputAction AddAction(string name, ActionValueType type)
        {
            if (_actions.ContainsKey(name)) return _actions[name];
            var action = new InputAction(name, type);
            _actions.Add(name, action);
            return action;
        }

        public void AddBinding(InputBinding binding)
        {
            _bindings.Add(binding);
        }

        public InputAction? GetAction(string name)
        {
            _actions.TryGetValue(name, out var a);
            return a;
        }

        // Frame update: query provider and update action states
        public void Update(float deltaTime)
        {
            // Reset frame-only flags
            foreach (var a in _actions.Values)
            {
                a.ClearFrame();
            }

            // Read bindings
            foreach (var b in _bindings)
            {
                if (!_actions.TryGetValue(b.ActionName, out var action)) continue;

                switch (b.Type)
                {
                    case BindingType.Button:
                        var pressed = _provider.GetButton(b.Key);
                        var down = _provider.GetButtonDown(b.Key);
                        var up = _provider.GetButtonUp(b.Key);
                        action.Pressed = pressed;
                        action.PressedDown = down;
                        action.PressedUp = up;
                        break;
                    case BindingType.Axis:
                        var val = _provider.GetAxis(b.AxisName);
                        action.AxisValue = val;
                        break;
                    case BindingType.Vector2:
                        var x = _provider.GetAxis(b.AxisX);
                        var y = _provider.GetAxis(b.AxisY);
                        action.Vector2Value = (x, y);
                        break;
                }
            }

            // Dispatch events
            foreach (var a in _actions.Values)
            {
                a.RaiseEvents();
            }
        }

        // Helper: map to PlayerInput (shallow mapping, can be customized)
        public PlayerInput ToPlayerInput()
        {
            var pi = new PlayerInput();

            var moveAction = GetAction("Move");
            if (moveAction != null)
            {
                pi.Move = new TerraForge.Core.Vector3(moveAction.Vector2Value.x, 0, moveAction.Vector2Value.y);
            }

            pi.IsRunning = GetAction("Run")?.Pressed ?? false;
            pi.IsCrouching = GetAction("Crouch")?.Pressed ?? false;
            pi.Jump = GetAction("Jump")?.PressedDown ?? false;
            pi.IsClimbing = GetAction("Climb")?.Pressed ?? false;
            pi.IsSwimming = GetAction("Swim")?.Pressed ?? false;

            // Interact, Attack, Inventory, Build could be events subscribed to directly
            return pi;
        }
    }
}
