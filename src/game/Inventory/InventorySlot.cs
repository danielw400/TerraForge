using System;

namespace TerraForge.Game.Inventory
{
    public sealed class InventorySlot
    {
        public InventoryItem Item { get; private set; }

        public bool IsEmpty => Item == null || Item.IsEmpty;
        public bool CanAccept(InventoryItem item)
        {
            if (item == null || item.IsEmpty) return true;
            if (IsEmpty) return true;
            return Item.Definition.Id == item.Definition.Id && Item.Quantity < Item.Definition.MaxStack;
        }

        public InventorySlot()
        {
            Item = null;
        }

        public void SetItem(InventoryItem item)
        {
            Item = item;
        }

        public bool TryAdd(InventoryItem item)
        {
            if (item == null || item.IsEmpty) return true;
            if (!CanAccept(item)) return false;
            if (IsEmpty)
            {
                Item = new InventoryItem(item.Definition, item.Quantity);
                return true;
            }

            var remainder = Item.Add(item.Quantity);
            if (remainder > 0)
            {
                item = new InventoryItem(item.Definition, remainder);
            }

            return true;
        }

        public InventoryItem RemoveAll()
        {
            var result = Item;
            Item = null;
            return result;
        }
    }
}
