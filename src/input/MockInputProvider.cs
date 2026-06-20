using System;
using System.Collections.Generic;

namespace TerraForge.Input
{
    public sealed class MockInputProvider : IInputProvider
    {
        private readonly Dictionary<string, bool> _currentButtons = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> _previousButtons = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> _axes = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        private (float x, float y) _pointerPos = (0f, 0f);
        private (float x, float y) _pointerDelta = (0f, 0f);

        public void SetButton(string name, bool pressed)
        {
            _currentButtons[name] = pressed;
        }

        public void SetAxis(string name, float value)
        {
            _axes[name] = value;
        }

        public void SetPointer((float x, float y) pos, (float x, float y) delta)
        {
            _pointerPos = pos;
            _pointerDelta = delta;
        }

        public void AdvanceFrame()
        {
            // copy current into previous
            _previousButtons.Clear();
            foreach (var kv in _currentButtons)
            {
                _previousButtons[kv.Key] = kv.Value;
            }

            // pointer delta reset; user must set again each frame if needed
            _pointerDelta = (0f, 0f);
        }

        public bool GetButton(string name)
        {
            return _currentButtons.TryGetValue(name, out var v) && v;
        }

        public bool GetButtonDown(string name)
        {
            var cur = _currentButtons.TryGetValue(name, out var vcur) && vcur;
            var prev = _previousButtons.TryGetValue(name, out var vprev) && vprev;
            return cur && !prev;
        }

        public bool GetButtonUp(string name)
        {
            var cur = _currentButtons.TryGetValue(name, out var vcur) && vcur;
            var prev = _previousButtons.TryGetValue(name, out var vprev) && vprev;
            return !cur && prev;
        }

        public float GetAxis(string name)
        {
            return _axes.TryGetValue(name, out var v) ? v : 0f;
        }

        public (float x, float y) GetPointerDelta() => _pointerDelta;
        public (float x, float y) GetPointerPosition() => _pointerPos;
    }
}
