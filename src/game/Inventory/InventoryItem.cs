using System;

namespace TerraForge.Game.Inventory
{
    public sealed class InventoryItem
    {
        public ItemDefinition Definition { get; }
        public int Quantity { get; private set; }

        public bool IsEmpty => Definition == null || Quantity <= 0;

        public InventoryItem(ItemDefinition definition, int quantity)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Quantity = Math.Clamp(quantity, 0, definition.MaxStack);
        }

        public InventoryItem() { }

        public int Add(int amount)
        {
            if (IsEmpty) return amount;
            var available = Definition.MaxStack - Quantity;
            var toAdd = Math.Clamp(amount, 0, available);
            Quantity += toAdd;
            return amount - toAdd;
        }

        public int Remove(int amount)
        {
            if (IsEmpty) return 0;
            var toRemove = Math.Clamp(amount, 0, Quantity);
            Quantity -= toRemove;
            return toRemove;
        }

        public InventoryItem Split(int amount)
        {
            if (IsEmpty || amount <= 0) return null;
            var taken = Math.Clamp(amount, 0, Quantity);
            Quantity -= taken;
            return new InventoryItem(Definition, taken);
        }
    }
}
