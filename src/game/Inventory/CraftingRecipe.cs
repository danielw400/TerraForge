using System;
using System.Collections.Generic;
using System.Linq;

namespace TerraForge.Game.Inventory
{
    public sealed class CraftingIngredient
    {
        public ushort ItemId { get; }
        public int Quantity { get; }

        public CraftingIngredient(ushort itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }

    public sealed class CraftingRecipe
    {
        public ushort ResultItemId { get; }
        public int ResultQuantity { get; }
        public IReadOnlyList<CraftingIngredient> Ingredients { get; }

        public CraftingRecipe(ushort resultItemId, int resultQuantity, IReadOnlyList<CraftingIngredient> ingredients)
        {
            ResultItemId = resultItemId;
            ResultQuantity = resultQuantity;
            Ingredients = ingredients ?? throw new ArgumentNullException(nameof(ingredients));
        }

        public bool Matches(InventoryContainer container)
        {
            foreach (var ingredient in Ingredients)
            {
                var required = ingredient.Quantity;
                foreach (var slot in container.Slots)
                {
                    if (slot.IsEmpty) continue;
                    if (slot.Item.Definition.Id != ingredient.ItemId) continue;
                    required -= slot.Item.Quantity;
                    if (required <= 0) break;
                }

                if (required > 0) return false;
            }

            return true;
        }
    }
}
