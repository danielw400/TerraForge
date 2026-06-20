using System;
using System.Collections.Generic;
using System.Linq;
using TerraForge.Game.Inventory;
using TerraForge.Input;

namespace TerraForge.UI
{
    public sealed class InventoryUiManager
    {
        private readonly InventorySession _session;
        private readonly InventoryUiConfig _config;
        private readonly InventoryUiStyle _style;
        private readonly List<InventoryUiSlotViewModel> _slotViews = new List<InventoryUiSlotViewModel>();

        public InventoryUiState State { get; }
        public IReadOnlyList<InventoryUiSlotViewModel> SlotViews => _slotViews;

        public InventoryUiManager(InventorySession session, InventoryUiConfig config, InventoryUiStyle style = null)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _style = style ?? InventoryUiStyle.Default;
            State = new InventoryUiState { IsOpen = false, ActiveHotbarIndex = 0 };
        }

        public void ToggleOpen()
        {
            State.IsOpen = !State.IsOpen;
        }

        public void Update(float deltaTime, IInputProvider input, int screenWidth, int screenHeight)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (input.GetButtonDown("Inventory"))
            {
                ToggleOpen();
            }

            if (input.GetButtonDown("Hotbar1")) HandleHotbarKey(0);
            if (input.GetButtonDown("Hotbar2")) HandleHotbarKey(1);
            if (input.GetButtonDown("Hotbar3")) HandleHotbarKey(2);
            if (input.GetButtonDown("Hotbar4")) HandleHotbarKey(3);
            if (input.GetButtonDown("Hotbar5")) HandleHotbarKey(4);
            if (input.GetButtonDown("Hotbar6")) HandleHotbarKey(5);
            if (input.GetButtonDown("Hotbar7")) HandleHotbarKey(6);
            if (input.GetButtonDown("Hotbar8")) HandleHotbarKey(7);
            if (input.GetButtonDown("Hotbar9")) HandleHotbarKey(8);

            var pointer = input.GetPointerPosition();
            State.PointerX = pointer.x;
            State.PointerY = pointer.y;

            BuildViewModels(screenWidth, screenHeight);
            State.HoveredSlot = FindSlotAt(State.PointerX, State.PointerY);

            var leftClick = input.GetButtonDown("Mouse0");
            var rightClick = input.GetButtonDown("Mouse1");

            if (leftClick)
            {
                HandleLeftClick();
            }
            else if (rightClick)
            {
                HandleRightClick();
            }
        }

        public InventoryItem GetActiveHotbarItem()
        {
            if (!_session.TryGetContainer(_config.HotbarContainerId, out var hotbar)) return null;
            if (State.ActiveHotbarIndex < 0 || State.ActiveHotbarIndex >= hotbar.Capacity) return null;
            return hotbar.Slots[State.ActiveHotbarIndex].Item;
        }

        public void CancelDrag()
        {
            _session.CancelDrag();
            State.DraggedItem = null;
        }

        private void HandleHotbarKey(int index)
        {
            State.ActiveHotbarIndex = index;

            if (!State.IsOpen || State.HoveredSlot == null) return;
            if (State.HoveredSlot.IsHotbar) return;
            if (!_session.TryGetContainer(_config.HotbarContainerId, out var hotbar)) return;
            if (!_session.TryGetContainer(State.HoveredSlot.ContainerId, out var sourceContainer)) return;

            var targetIndex = index;
            var sourceSlot = sourceContainer.Slots[State.HoveredSlot.SlotIndex];
            if (sourceSlot.IsEmpty) return;

            var targetSlot = hotbar.Slots[targetIndex];
            if (targetSlot.IsEmpty)
            {
                targetSlot.SetItem(sourceSlot.RemoveAll());
                return;
            }

            if (targetSlot.CanAccept(sourceSlot.Item))
            {
                var remainder = targetSlot.Item.Add(sourceSlot.Item.Quantity);
                if (remainder <= 0)
                {
                    sourceSlot.RemoveAll();
                }
                else
                {
                    sourceSlot.SetItem(new InventoryItem(sourceSlot.Item.Definition, remainder));
                }
            }
            else
            {
                var sourceItem = sourceSlot.RemoveAll();
                var targetItem = targetSlot.RemoveAll();
                targetSlot.SetItem(sourceItem);
                sourceSlot.SetItem(targetItem);
            }
        }

        private void HandleLeftClick()
        {
            if (State.HoveredSlot == null)
            {
                if (State.IsDragging)
                {
                    CancelDrag();
                }
                return;
            }

            if (State.IsDragging)
            {
                if (TryDropToSlot(State.HoveredSlot))
                {
                    return;
                }
            }
            else
            {
                BeginDrag(State.HoveredSlot.ContainerId, State.HoveredSlot.SlotIndex, false);
            }
        }

        private void HandleRightClick()
        {
            if (State.HoveredSlot == null)
            {
                return;
            }

            if (State.IsDragging)
            {
                TryPlaceOneIntoSlot(State.HoveredSlot);
                return;
            }

            var slot = State.HoveredSlot.Slot;
            if (slot == null || slot.IsEmpty) return;
            if (slot.Item.Quantity <= 1)
            {
                BeginDrag(State.HoveredSlot.ContainerId, State.HoveredSlot.SlotIndex, false);
                return;
            }

            var splitItem = new InventoryItem(slot.Item.Definition, 1);
            slot.Item.Remove(1);
            if (slot.Item.Quantity <= 0)
            {
                slot.RemoveAll();
            }

            State.DraggedItem = splitItem;
            State.DragSourceContainerId = State.HoveredSlot.ContainerId;
            State.DragSourceSlotIndex = State.HoveredSlot.SlotIndex;
        }

        private void BeginDrag(int containerId, int slotIndex, bool splitStack)
        {
            if (!_session.TryGetContainer(containerId, out var container)) return;
            if (slotIndex < 0 || slotIndex >= container.Capacity) return;
            var slot = container.Slots[slotIndex];
            if (slot.IsEmpty) return;

            if (splitStack && slot.Item.Quantity > 1)
            {
                var half = slot.Item.Quantity / 2;
                var remainder = slot.Item.Quantity - half;
                var pickedItem = new InventoryItem(slot.Item.Definition, half);
                slot.SetItem(new InventoryItem(slot.Item.Definition, remainder));
                State.DraggedItem = pickedItem;
            }
            else
            {
                State.DraggedItem = slot.RemoveAll();
            }

            State.DragSourceContainerId = containerId;
            State.DragSourceSlotIndex = slotIndex;
        }

        private bool TryDropToSlot(InventoryUiSlotViewModel target)
        {
            if (State.DraggedItem == null || State.DraggedItem.IsEmpty) return false;

            var targetSlot = target.Slot;
            if (targetSlot == null) return false;

            if (targetSlot.CanAccept(State.DraggedItem))
            {
                if (targetSlot.IsEmpty)
                {
                    targetSlot.SetItem(State.DraggedItem);
                    State.DraggedItem = null;
                    return true;
                }

                var remainder = targetSlot.Item.Add(State.DraggedItem.Quantity);
                if (remainder <= 0)
                {
                    State.DraggedItem = null;
                    return true;
                }

                State.DraggedItem = new InventoryItem(State.DraggedItem.Definition, remainder);
                return true;
            }

            var previousDrag = State.DraggedItem;
            var displacedItem = targetSlot.RemoveAll();
            targetSlot.SetItem(previousDrag);
            State.DraggedItem = displacedItem;
            return true;
        }

        private bool TryPlaceOneIntoSlot(InventoryUiSlotViewModel target)
        {
            if (State.DraggedItem == null || State.DraggedItem.IsEmpty) return false;
            var targetSlot = target.Slot;
            if (targetSlot == null || !targetSlot.CanAccept(new InventoryItem(State.DraggedItem.Definition, 1))) return false;

            if (targetSlot.IsEmpty)
            {
                targetSlot.SetItem(new InventoryItem(State.DraggedItem.Definition, 1));
            }
            else
            {
                targetSlot.Item.Add(1);
            }

            State.DraggedItem.Remove(1);
            if (State.DraggedItem.Quantity <= 0)
            {
                State.DraggedItem = null;
            }

            return true;
        }

        private InventoryUiSlotViewModel FindSlotAt(float x, float y)
        {
            return _slotViews.FirstOrDefault(slot =>
                x >= slot.X && x <= slot.X + slot.Width &&
                y >= slot.Y && y <= slot.Y + slot.Height);
        }

        private void BuildViewModels(int screenWidth, int screenHeight)
        {
            _slotViews.Clear();
            var margin = _style.ScreenMargin;
            var contentWidth = screenWidth - margin * 2f;
            var contentHeight = screenHeight - margin * 2f;
            var halfWidth = contentWidth * 0.58f;
            var rightWidth = contentWidth - halfWidth - _style.SectionSpacing;
            var leftX = margin;
            var rightX = margin + halfWidth + _style.SectionSpacing;
            var y = margin;

            y = AddSection("Inventário", _config.MainContainerId, _config.MainColumns, leftX, y, halfWidth, false, false, false, false);
            y += _style.SectionSpacing;
            y = AddSection("Barra Rápida", _config.HotbarContainerId, _config.HotbarColumns, leftX, y, halfWidth, true, false, false, false);

            var rightY = margin;
            rightY = AddEquipmentSection(rightX, rightY, rightWidth);
            rightY += _style.SectionSpacing;
            rightY = AddCraftingSection(rightX, rightY, rightWidth);
            if (_config.HasChest)
            {
                rightY += _style.SectionSpacing;
                rightY = AddSection("Baú", _config.ChestContainerId, _config.ChestColumns, rightX, rightY, rightWidth, false, true, false, false);
            }
        }

        private float AddSection(string title, int containerId, int columns, float x, float y, float width, bool isHotbar, bool isChest, bool isCraftingInput, bool isCraftingOutput)
        {
            if (!_session.TryGetContainer(containerId, out var container))
            {
                return y;
            }

            var sectionTitleHeight = _style.SectionTitleSize + _style.PanelPadding * 0.5f;
            var availableWidth = width;
            var slotSize = _style.SlotSize;
            var spacing = _style.SlotSpacing;
            var rows = Math.Max(1, (int)Math.Ceiling(container.Capacity / (float)columns));
            var totalHeight = sectionTitleHeight + rows * slotSize + (rows - 1) * spacing + _style.PanelPadding;

            var slotX = x;
            var slotY = y + sectionTitleHeight;

            for (var index = 0; index < container.Capacity; index++)
            {
                var columnIndex = index % columns;
                var rowIndex = index / columns;
                var positionX = x + columnIndex * (slotSize + spacing);
                var positionY = slotY + rowIndex * (slotSize + spacing);
                var viewModel = new InventoryUiSlotViewModel(containerId, index, container.Slots[index], title, isHotbar, isChest, isCraftingOutput, null)
                {
                    X = positionX,
                    Y = positionY,
                    Width = slotSize,
                    Height = slotSize
                };
                _slotViews.Add(viewModel);
            }

            return y + totalHeight;
        }

        private float AddEquipmentSection(float x, float y, float width)
        {
            var title = "Equipamento";
            var titleHeight = _style.SectionTitleSize + _style.PanelPadding * 0.5f;
            var slotSize = _style.SlotSize;
            var spacing = _style.SlotSpacing;
            var positions = new Dictionary<EquipmentSlotType, (float x, float y)>
            {
                { EquipmentSlotType.Head, (x + width * 0.5f - slotSize * 0.5f, y + titleHeight) },
                { EquipmentSlotType.Chest, (x + width * 0.5f - slotSize * 0.5f, y + titleHeight + slotSize + spacing) },
                { EquipmentSlotType.Legs, (x + width * 0.5f - slotSize * 0.5f, y + titleHeight + 2 * (slotSize + spacing)) },
                { EquipmentSlotType.Feet, (x + width * 0.5f - slotSize * 0.5f, y + titleHeight + 3 * (slotSize + spacing)) },
                { EquipmentSlotType.MainHand, (x + width * 0.5f - slotSize * 0.5f - slotSize - spacing, y + titleHeight + slotSize + spacing * 0.5f) },
                { EquipmentSlotType.OffHand, (x + width * 0.5f + slotSize + spacing - slotSize * 0.5f, y + titleHeight + slotSize + spacing * 0.5f) },
                { EquipmentSlotType.Accessory, (x + width * 0.5f - slotSize * 0.5f, y + titleHeight + 4 * (slotSize + spacing)) }
            };

            foreach (var slotType in positions.Keys)
            {
                var position = positions[slotType];
                var index = GetEquipmentSlotIndex(slotType);
                if (index < 0 || index >= _session.Equipment.EquipmentSlots.Length) continue;
                var slot = _session.Equipment.EquipmentSlots[index];
                var viewModel = new InventoryUiSlotViewModel(-1, index, slot, title, false, false, false, slotType)
                {
                    X = position.x,
                    Y = position.y,
                    Width = slotSize,
                    Height = slotSize
                };
                _slotViews.Add(viewModel);
            }

            return y + titleHeight + 5 * (slotSize + spacing);
        }

        private float AddCraftingSection(float x, float y, float width)
        {
            var title = "Crafting";
            var titleHeight = _style.SectionTitleSize + _style.PanelPadding * 0.5f;
            var slotSize = _style.SlotSize;
            var spacing = _style.SlotSpacing;
            var gridColumns = _config.CraftingColumns;
            var gridRows = _config.CraftingRows;
            var tableWidth = gridColumns * slotSize + (gridColumns - 1) * spacing;
            var tableHeight = gridRows * slotSize + (gridRows - 1) * spacing;
            var startX = x + (width - tableWidth) * 0.5f;
            var startY = y + titleHeight;

            if (_session.TryGetContainer(_config.CraftingInputContainerId, out var inputContainer))
            {
                for (var index = 0; index < inputContainer.Capacity; index++)
                {
                    var col = index % gridColumns;
                    var row = index / gridColumns;
                    var viewModel = new InventoryUiSlotViewModel(_config.CraftingInputContainerId, index, inputContainer.Slots[index], false, false, true, null)
                    {
                        X = startX + col * (slotSize + spacing),
                        Y = startY + row * (slotSize + spacing),
                        Width = slotSize,
                        Height = slotSize
                    };
                    _slotViews.Add(viewModel);
                }
            }

            if (_session.TryGetContainer(_config.CraftingOutputContainerId, out var outputContainer) && outputContainer.Capacity > 0)
            {
                var outputX = startX + tableWidth + spacing * 2f;
                var outputY = startY + (tableHeight - slotSize) * 0.5f;
                var viewModel = new InventoryUiSlotViewModel(_config.CraftingOutputContainerId, 0, outputContainer.Slots[0], title, false, false, true, null)
                {
                    X = outputX,
                    Y = outputY,
                    Width = slotSize,
                    Height = slotSize
                };
                _slotViews.Add(viewModel);
            }

            return y + titleHeight + Math.Max(tableHeight, slotSize) + _style.PanelPadding;
        }

        private static int GetEquipmentSlotIndex(EquipmentSlotType slotType)
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
