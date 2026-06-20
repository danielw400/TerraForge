using System;

namespace TerraForge.Input
{
    // Minimal provider that delegates hardware queries to user-supplied callbacks.
    // This keeps the InputManager engine-agnostic and easy to integrate.
    public sealed class KeyboardMouseProvider : IInputProvider
    {
        public Func<string, bool> GetKey { get; set; }
        public Func<string, bool> GetKeyDown { get; set; }
        public Func<string, bool> GetKeyUp { get; set; }
        public Func<string, float> GetAxisValue { get; set; }
        public Func<(float x, float y)> PointerDelta { get; set; }
        public Func<(float x, float y)> PointerPosition { get; set; }

        public KeyboardMouseProvider(
            Func<string, bool> getKey,
            Func<string, bool> getKeyDown,
            Func<string, bool> getKeyUp,
            Func<string, float> getAxisValue,
            Func<(float x, float y)> pointerDelta,
            Func<(float x, float y)> pointerPosition)
        {
            GetKey = getKey ?? throw new ArgumentNullException(nameof(getKey));
            GetKeyDown = getKeyDown ?? throw new ArgumentNullException(nameof(getKeyDown));
            GetKeyUp = getKeyUp ?? throw new ArgumentNullException(nameof(getKeyUp));
            GetAxisValue = getAxisValue ?? throw new ArgumentNullException(nameof(getAxisValue));
            PointerDelta = pointerDelta ?? (() => (0f, 0f));
            PointerPosition = pointerPosition ?? (() => (0f, 0f));
        }

        public bool GetButton(string name) => GetKey(name);
        public bool GetButtonDown(string name) => GetKeyDown(name);
        public bool GetButtonUp(string name) => GetKeyUp(name);
        public float GetAxis(string name) => GetAxisValue(name);
        public (float x, float y) GetPointerDelta() => PointerDelta();
        public (float x, float y) GetPointerPosition() => PointerPosition();
    }
}
