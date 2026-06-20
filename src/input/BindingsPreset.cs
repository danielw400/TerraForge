using System;

namespace TerraForge.Input
{
    public static class BindingsPreset
    {
        public static void ApplyDefaults(InputManager im)
        {
            im.AddAction("Move", ActionValueType.Vector2);
            im.AddAction("Run", ActionValueType.Button);
            im.AddAction("Jump", ActionValueType.Button);
            im.AddAction("Crouch", ActionValueType.Button);
            im.AddAction("Interact", ActionValueType.Button);
            im.AddAction("UseItem", ActionValueType.Button);
            im.AddAction("Attack", ActionValueType.Button);
            im.AddAction("Build", ActionValueType.Button);
            im.AddAction("Inventory", ActionValueType.Button);
            im.AddAction("Menu", ActionValueType.Button);
            im.AddAction("Hotbar1", ActionValueType.Button);
            im.AddAction("Hotbar2", ActionValueType.Button);
            im.AddAction("Hotbar3", ActionValueType.Button);
            im.AddAction("Hotbar4", ActionValueType.Button);
            im.AddAction("Hotbar5", ActionValueType.Button);
            im.AddAction("Hotbar6", ActionValueType.Button);
            im.AddAction("Hotbar7", ActionValueType.Button);
            im.AddAction("Hotbar8", ActionValueType.Button);
            im.AddAction("Hotbar9", ActionValueType.Button);

            im.AddBinding(new InputBinding("Move", "Horizontal", "Vertical"));
            im.AddBinding(new InputBinding("Run", "LeftShift"));
            im.AddBinding(new InputBinding("Jump", "Space"));
            im.AddBinding(new InputBinding("Crouch", "LeftCtrl"));
            im.AddBinding(new InputBinding("Interact", "E"));
            im.AddBinding(new InputBinding("UseItem", "F"));
            im.AddBinding(new InputBinding("Attack", "Mouse0"));
            im.AddBinding(new InputBinding("Build", "Mouse1"));
            im.AddBinding(new InputBinding("Inventory", "Tab"));
            im.AddBinding(new InputBinding("Menu", "Escape"));
            im.AddBinding(new InputBinding("Hotbar1", "Alpha1"));
            im.AddBinding(new InputBinding("Hotbar2", "Alpha2"));
            im.AddBinding(new InputBinding("Hotbar3", "Alpha3"));
            im.AddBinding(new InputBinding("Hotbar4", "Alpha4"));
            im.AddBinding(new InputBinding("Hotbar5", "Alpha5"));
            im.AddBinding(new InputBinding("Hotbar6", "Alpha6"));
            im.AddBinding(new InputBinding("Hotbar7", "Alpha7"));
            im.AddBinding(new InputBinding("Hotbar8", "Alpha8"));
            im.AddBinding(new InputBinding("Hotbar9", "Alpha9"));
        }
    }
}
