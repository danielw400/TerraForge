using System;

namespace TerraForge.Game.Inventory
{
    public enum ItemCategory
    {
        Consumable,
        Material,
        Tool,
        Weapon,
        Armor,
        Equipment,
        Crafting,
        Misc
    }

    public sealed class ItemDefinition
    {
        public ushort Id { get; }
        public string Name { get; }
        public string Description { get; }
        public ItemCategory Category { get; }
        public int MaxStack { get; }
        public EquipmentSlotType EquipSlot { get; }

        public bool IsStackable => MaxStack > 1;
        public bool IsEquippable => EquipSlot != EquipmentSlotType.None;

        public ItemDefinition(ushort id, string name, string description, ItemCategory category, int maxStack = 64, EquipmentSlotType equipSlot = EquipmentSlotType.None)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Item name is required.", nameof(name));
            if (maxStack <= 0) throw new ArgumentOutOfRangeException(nameof(maxStack), "MaxStack must be greater than zero.");

            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            Category = category;
            MaxStack = maxStack;
            EquipSlot = equipSlot;
        }
    }
}
