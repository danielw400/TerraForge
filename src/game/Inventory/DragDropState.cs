using TerraForge.Game.Inventory;

namespace TerraForge.Game.Inventory
{
    public sealed class DragDropState
    {
        public InventoryItem Item { get; private set; }
        public int SourceContainerId { get; private set; }
        public int SourceSlotIndex { get; private set; }

        public bool IsDragging => Item != null && !Item.IsEmpty;

        public void BeginDrag(InventoryItem item, int containerId, int slotIndex)
        {
            Item = item;
            SourceContainerId = containerId;
            SourceSlotIndex = slotIndex;
        }

        public InventoryItem Drop()
        {
            var item = Item;
            Item = null;
            return item;
        }

        public void Cancel()
        {
            Item = null;
        }
    }
}
