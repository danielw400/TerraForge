using System;

namespace TerraForge.Input
{
    public enum BindingType
    {
        Button,
        Axis,
        Vector2
    }

    public sealed class InputBinding
    {
        public string ActionName { get; }
        public BindingType Type { get; }

        // For Button bindings: `Key` holds the button name.
        // For Axis bindings: `AxisName` holds the axis identifier.
        // For Vector2 bindings: `AxisX` and `AxisY` hold axis names (ex: MouseX/MouseY or Horizontal/Vertical).
        public string Key { get; }
        public string AxisName { get; }
        public string AxisX { get; }
        public string AxisY { get; }

        public InputBinding(string actionName, string key)
        {
            ActionName = actionName;
            Type = BindingType.Button;
            Key = key;
        }

        public InputBinding(string actionName, string axisName, bool axis)
        {
            ActionName = actionName;
            Type = BindingType.Axis;
            AxisName = axisName;
        }

        public InputBinding(string actionName, string axisX, string axisY)
        {
            ActionName = actionName;
            Type = BindingType.Vector2;
            AxisX = axisX;
            AxisY = axisY;
        }
    }
}
