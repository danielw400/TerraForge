using System;
using System.Collections.Generic;
using System.Linq;

namespace TerraForge.Game.Inventory
{
    public sealed class InventorySession
    {
        private readonly Dictionary<int, InventoryContainer> _containers;
        private readonly EquipmentInventory _equipment;
        private readonly CraftingManager _craftingManager;

        public InventorySession(CraftingManager craftingManager)
        {
            _craftingManager = craftingManager ?? throw new ArgumentNullException(nameof(craftingManager));
            _containers = new Dictionary<int, InventoryContainer>();
            _equipment = new EquipmentInventory();
            DragState = new DragDropState();
        }

        public EquipmentInventory Equipment => _equipment;
        public DragDropState DragState { get; }

        public IReadOnlyDictionary<int, InventoryContainer> Containers => _containers;

        public int RegisterContainer(InventoryContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var id = _containers.Count == 0 ? 1 : _containers.Keys.Max() + 1;
            _containers[id] = container;
            return id;
        }

        public bool TryGetContainer(int containerId, out InventoryContainer container)
        {
            return _containers.TryGetValue(containerId, out container);
        }

        public bool BeginDrag(int containerId, int slotIndex, bool splitStack = false)
        {
            if (!TryGetContainer(containerId, out var container)) return false;
            if (slotIndex < 0 || slotIndex >= container.Capacity) return false;
            var slot = container.Slots[slotIndex];
            if (slot.IsEmpty) return false;

            if (splitStack && slot.Item.Quantity > 1)
            {
                var half = slot.Item.Quantity / 2;
                var remainder = slot.Item.Quantity - half;
                var pickedItem = new InventoryItem(slot.Item.Definition, half);
                slot.SetItem(new InventoryItem(slot.Item.Definition, remainder));
                DragState.BeginDrag(pickedItem, containerId, slotIndex);
                return true;
            }

            DragState.BeginDrag(slot.RemoveAll(), containerId, slotIndex);
            return true;
        }

        public bool DropInto(int containerId, int slotIndex)
        {
            if (!DragState.IsDragging) return false;
            if (!TryGetContainer(containerId, out var container)) return false;
            if (slotIndex < 0 || slotIndex >= container.Capacity) return false;

            var draggedItem = DragState.Item;
            var targetSlot = container.Slots[slotIndex];
            if (targetSlot.CanAccept(draggedItem))
            {
                if (targetSlot.IsEmpty)
                {
                    targetSlot.SetItem(draggedItem);
                    DragState.Cancel();
                    return true;
                }

                var remainder = targetSlot.Item.Add(draggedItem.Quantity);
                if (remainder <= 0)
                {
                    DragState.Cancel();
                    return true;
                }

                DragState.BeginDrag(new InventoryItem(draggedItem.Definition, remainder), DragState.SourceContainerId, DragState.SourceSlotIndex);
                return true;
            }

            return false;
        }

        public bool DropOne(int containerId, int slotIndex)
        {
            if (!DragState.IsDragging) return false;
            if (!TryGetContainer(containerId, out var container)) return false;
            if (slotIndex < 0 || slotIndex >= container.Capacity) return false;

            var draggedItem = DragState.Item;
            if (draggedItem == null || draggedItem.Quantity <= 0) return false;

            var targetSlot = container.Slots[slotIndex];
            var singleItem = new InventoryItem(draggedItem.Definition, 1);
            if (!targetSlot.CanAccept(singleItem)) return false;

            if (targetSlot.IsEmpty)
            {
                targetSlot.SetItem(singleItem);
            }
            else
            {
                targetSlot.Item.Add(1);
            }

            draggedItem.Remove(1);
            if (draggedItem.Quantity <= 0)
            {
                DragState.Cancel();
            }
            else
            {
                DragState.BeginDrag(draggedItem, DragState.SourceContainerId, DragState.SourceSlotIndex);
            }

            return true;
        }

        public void CancelDrag()
        {
            DragState.Cancel();
        }

        public bool Craft(int containerId, int outputContainerId, CraftingRecipe recipe)
        {
            if (!TryGetContainer(containerId, out var source)) return false;
            if (!TryGetContainer(outputContainerId, out var output)) return false;
            return _craftingManager.TryCraft(recipe, source, output);
        }

        public bool EquipItem(int containerId, int slotIndex)
        {
            if (!TryGetContainer(containerId, out var container)) return false;
            if (slotIndex < 0 || slotIndex >= container.Capacity) return false;
            var slot = container.Slots[slotIndex];
            if (slot.IsEmpty || !slot.Item.Definition.IsEquippable) return false;

            if (!_equipment.Equip(slot.Item)) return false;
            slot.RemoveAll();
            return true;
        }

        public bool UnequipItem(EquipmentSlotType slotType, int targetContainerId)
        {
            var item = _equipment.Unequip(slotType);
            if (item == null) return false;
            if (!TryGetContainer(targetContainerId, out var target)) return false;
            return target.AddItem(item);
        }

        public bool TryInteractWithChest(ChestInventory chest, InventoryContainer playerInventory)
        {
            if (chest == null || playerInventory == null) return false;
            return true;
        }
    }
}
