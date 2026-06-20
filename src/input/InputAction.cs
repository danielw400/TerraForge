using System;

namespace TerraForge.Input
{
    public enum ActionValueType
    {
        Button,
        Axis,
        Vector2
    }

    public sealed class InputAction
    {
        public string Name { get; }
        public ActionValueType ValueType { get; }

        // Runtime state
        public bool Pressed { get; internal set; }
        public bool PressedDown { get; internal set; }
        public bool PressedUp { get; internal set; }
        public float AxisValue { get; internal set; }
        public (float x, float y) Vector2Value { get; internal set; }

        // Events
        public event Action OnPressed;
        public event Action OnReleased;
        public event Action<float> OnAxis; // axis value
        public event Action<(float x, float y)> OnVector2;

        public InputAction(string name, ActionValueType valueType)
        {
            Name = name;
            ValueType = valueType;
        }

        internal void ClearFrame()
        {
            PressedDown = false;
            PressedUp = false;
            AxisValue = 0f;
            Vector2Value = (0f, 0f);
        }

        internal void RaiseEvents()
        {
            if (PressedDown) OnPressed?.Invoke();
            if (PressedUp) OnReleased?.Invoke();
            if (ValueType == ActionValueType.Axis) OnAxis?.Invoke(AxisValue);
            if (ValueType == ActionValueType.Vector2) OnVector2?.Invoke(Vector2Value);
        }
    }
}
