using System;
using System.Collections.Generic;
using System.Linq;
using TerraForge.Game.Inventory;

namespace TerraForge.UI
{
    public sealed class InventoryUiRenderer
    {
        private readonly InventoryUiStyle _style;

        public InventoryUiRenderer(InventoryUiStyle style = null)
        {
            _style = style ?? InventoryUiStyle.Default;
        }

        public void Render(IUiRenderer renderer, InventoryUiManager uiManager, int screenWidth, int screenHeight)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (uiManager == null) throw new ArgumentNullException(nameof(uiManager));

            if (!uiManager.State.IsOpen)
            {
                return;
            }

            RenderSectionBackgrounds(renderer, uiManager.SlotViews);

            foreach (var slot in uiManager.SlotViews)
            {
                RenderSlot(renderer, slot);
            }

            RenderDragPreview(renderer, uiManager);
        }

        private void RenderSectionBackgrounds(IUiRenderer renderer, IReadOnlyList<InventoryUiSlotViewModel> slots)
        {
            var grouped = slots.GroupBy(slot => slot.SectionTitle);
            foreach (var group in grouped)
            {
                var title = group.Key;
                if (string.IsNullOrWhiteSpace(title)) continue;

                var minX = group.Min(slot => slot.X);
                var minY = group.Min(slot => slot.Y);
                var maxX = group.Max(slot => slot.X + slot.Width);
                var maxY = group.Max(slot => slot.Y + slot.Height);
                var groupPadding = _style.PanelPadding;
                var titleHeight = _style.SectionTitleSize + _style.PanelPadding * 0.5f;
                var backgroundX = minX - groupPadding * 0.5f;
                var backgroundY = minY - titleHeight - groupPadding * 0.5f;
                var backgroundWidth = (maxX - minX) + groupPadding;
                var backgroundHeight = (maxY - minY) + titleHeight + groupPadding;

                renderer.DrawRectangle(backgroundX, backgroundY, backgroundWidth, backgroundHeight, _style.PanelBackground);
                renderer.DrawBorder(backgroundX, backgroundY, backgroundWidth, backgroundHeight, _style.PanelBorderThickness, _style.PanelBorder);
                renderer.DrawText(title, minX + 6f, backgroundY + 6f, _style.TitleText, _style.SectionTitleSize);
            }
        }

        private void RenderSlot(IUiRenderer renderer, InventoryUiSlotViewModel slot)
        {
            var background = _style.SlotBackground;
            var border = _style.SlotBorder;
            if (slot.IsHotbar && slot.Slot != null && slot.Slot.Item != null && slot.Slot.Item.Quantity > 0)
            {
                background = _style.HotbarActiveBackground;
            }

            renderer.DrawRectangle(slot.X, slot.Y, slot.Width, slot.Height, background);
            renderer.DrawBorder(slot.X, slot.Y, slot.Width, slot.Height, _style.PanelBorderThickness, border);

            if (!slot.IsEmpty)
            {
                var text = slot.Item.Definition.Name;
                var count = slot.Item.Quantity > 1 ? slot.Item.Quantity.ToString() : string.Empty;
                renderer.DrawText(text, slot.X + 8f, slot.Y + 8f, _style.TextPrimary, _style.ItemTextSize);
                if (!string.IsNullOrEmpty(count))
                {
                    renderer.DrawText(count, slot.X + slot.Width - 8f, slot.Y + slot.Height - 8f, _style.TextSecondary, _style.CountTextSize, TextAlignment.Right);
                }
            }

            if (slot.IsCraftingOutput && slot.Slot != null && !slot.Slot.IsEmpty)
            {
                renderer.DrawText("Output", slot.X + 8f, slot.Y + slot.Height + 4f, _style.TextSecondary, _style.CountTextSize);
            }

            if (slot.EquipmentSlot.HasValue)
            {
                renderer.DrawText(slot.EquipmentSlot.Value.ToString(), slot.X + 8f, slot.Y + 8f, _style.TextSecondary, _style.CountTextSize);
            }
        }

        private void RenderDragPreview(IUiRenderer renderer, InventoryUiManager uiManager)
        {
            if (!uiManager.State.IsDragging) return;
            var x = uiManager.State.PointerX + 12f;
            var y = uiManager.State.PointerY + 12f;
            var width = _style.SlotSize;
            var height = _style.SlotSize;

            renderer.DrawRectangle(x, y, width, height, _style.DragPreviewBackground);
            renderer.DrawBorder(x, y, width, height, _style.PanelBorderThickness, _style.PanelBorder);
            renderer.DrawText(uiManager.State.DraggedItem.Definition.Name, x + 8f, y + 8f, _style.TextPrimary, _style.ItemTextSize);
            renderer.DrawText(uiManager.State.DraggedItem.Quantity.ToString(), x + width - 8f, y + height - 8f, _style.TextSecondary, _style.CountTextSize, TextAlignment.Right);
        }
    }
}
