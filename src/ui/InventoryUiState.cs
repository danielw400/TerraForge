using TerraForge.Game.Inventory;

namespace TerraForge.UI
{
    public sealed class InventoryUiState
    {
        public bool IsOpen { get; set; }
        public int ActiveHotbarIndex { get; set; }
        public InventoryItem DraggedItem { get; set; }
        public int DragSourceContainerId { get; set; }
        public int DragSourceSlotIndex { get; set; }
        public float PointerX { get; set; }
        public float PointerY { get; set; }

        public InventoryUiSlotViewModel HoveredSlot { get; set; }
        public InventoryUiSlotViewModel SelectedSlot { get; set; }

        public bool IsDragging => DraggedItem != null && !DraggedItem.IsEmpty;
    }
}
