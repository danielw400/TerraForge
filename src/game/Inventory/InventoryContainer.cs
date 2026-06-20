using System;
using System.Linq;

namespace TerraForge.Game.Inventory
{
    public sealed class InventoryContainer
    {
        public string Name { get; }
        public InventorySlot[] Slots { get; }
        public int Capacity => Slots.Length;

        public InventoryContainer(string name, int slotCount)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Container name is required.", nameof(name));
            if (slotCount <= 0) throw new ArgumentOutOfRangeException(nameof(slotCount), "Slot count must be positive.");

            Name = name;
            Slots = Enumerable.Range(0, slotCount).Select(_ => new InventorySlot()).ToArray();
        }

        public bool AddItem(InventoryItem item)
        {
            if (item == null || item.IsEmpty) return false;

            foreach (var slot in Slots)
            {
                if (!slot.IsEmpty && slot.Item.Definition.Id == item.Definition.Id && slot.Item.Quantity < slot.Item.Definition.MaxStack)
                {
                    var remainder = slot.Item.Add(item.Quantity);
                    if (remainder <= 0) return true;
                    item = new InventoryItem(item.Definition, remainder);
                }
            }

            foreach (var slot in Slots)
            {
                if (slot.IsEmpty)
                {
                    slot.SetItem(new InventoryItem(item.Definition, item.Quantity));
                    return true;
                }
            }

            return false;
        }

        public InventoryItem RemoveItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Capacity) return null;
            return Slots[slotIndex].RemoveAll();
        }

        public bool MoveItem(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= Capacity) return false;
            if (toIndex < 0 || toIndex >= Capacity) return false;
            if (fromIndex == toIndex) return true;

            var source = Slots[fromIndex];
            var target = Slots[toIndex];
            if (source.IsEmpty) return true;

            if (target.IsEmpty)
            {
                target.SetItem(source.RemoveAll());
                return true;
            }

            if (target.CanAccept(source.Item))
            {
                var remainder = target.Item.Add(source.Item.Quantity);
                if (remainder <= 0)
                {
                    source.RemoveAll();
                    return true;
                }

                source.SetItem(new InventoryItem(source.Item.Definition, remainder));
                return true;
            }

            var sourceItem = source.RemoveAll();
            var targetItem = target.RemoveAll();
            source.SetItem(targetItem);
            target.SetItem(sourceItem);
            return true;
        }
    }
}
