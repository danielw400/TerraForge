using TerraForge.Game.Inventory;

namespace TerraForge.UI
{
    public sealed class InventoryUiSlotViewModel
    {
        public int ContainerId { get; }
        public int SlotIndex { get; }
        public InventorySlot Slot { get; }
        public EquipmentSlotType? EquipmentSlot { get; }
        public bool IsCraftingOutput { get; }
        public bool IsHotbar { get; }
        public bool IsChest { get; }
        public string SectionTitle { get; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public InventoryItem Item => Slot?.Item;
        public bool IsEmpty => Slot == null || Slot.IsEmpty;

        public InventoryUiSlotViewModel(int containerId, int slotIndex, InventorySlot slot, string sectionTitle, bool isHotbar = false, bool isChest = false, bool isCraftingOutput = false, EquipmentSlotType? equipmentSlot = null)
        {
            ContainerId = containerId;
            SlotIndex = slotIndex;
            Slot = slot;
            SectionTitle = sectionTitle;
            IsHotbar = isHotbar;
            IsChest = isChest;
            IsCraftingOutput = isCraftingOutput;
            EquipmentSlot = equipmentSlot;
        }
    }
}
