using System;
using System.Linq;

namespace TerraForge.Game.Inventory
{
    public sealed class EquipmentInventory
    {
        public InventorySlot[] EquipmentSlots { get; }

        public EquipmentInventory()
        {
            EquipmentSlots = Enum.GetValues<EquipmentSlotType>()
                .Where(slot => slot != EquipmentSlotType.None)
                .Select(_ => new InventorySlot())
                .ToArray();
        }

        public bool Equip(InventoryItem item)
        {
            if (item == null || item.IsEmpty || !item.Definition.IsEquippable) return false;
            var index = GetSlotIndex(item.Definition.EquipSlot);
            if (index < 0) return false;

            var slot = EquipmentSlots[index];
            if (!slot.IsEmpty)
            {
                return false;
            }

            slot.SetItem(new InventoryItem(item.Definition, 1));
            return true;
        }

        public InventoryItem Unequip(EquipmentSlotType slotType)
        {
            var index = GetSlotIndex(slotType);
            if (index < 0) return null;

            return EquipmentSlots[index].RemoveAll();
        }

        public InventoryItem GetEquipped(EquipmentSlotType slotType)
        {
            var index = GetSlotIndex(slotType);
            return index < 0 ? null : EquipmentSlots[index].IsEmpty ? null : EquipmentSlots[index].Item;
        }

        private static int GetSlotIndex(EquipmentSlotType slotType)
        {
            var values = Enum.GetValues<EquipmentSlotType>();
            var index = 0;
            foreach (var value in values)
            {
                if (value == EquipmentSlotType.None) continue;
                if (value == slotType) return index;
                index++;
            }

            return -1;
        }
    }
}
