using System;
using System.Collections.Generic;
using System.Linq;

namespace TerraForge.Game.Inventory
{
    public sealed class CraftingManager
    {
        private readonly Dictionary<ushort, ItemDefinition> _items;
        private readonly List<CraftingRecipe> _recipes;

        public CraftingManager(IEnumerable<ItemDefinition> itemDefinitions, IEnumerable<CraftingRecipe> recipes)
        {
            _items = new Dictionary<ushort, ItemDefinition>();
            _recipes = new List<CraftingRecipe>(recipes ?? Array.Empty<CraftingRecipe>());

            foreach (var item in itemDefinitions ?? Array.Empty<ItemDefinition>())
            {
                _items[item.Id] = item;
            }
        }

        public IReadOnlyCollection<CraftingRecipe> Recipes => _recipes.AsReadOnly();

        public bool CanCraft(CraftingRecipe recipe, InventoryContainer container)
        {
            if (recipe == null || container == null) return false;
            if (!_items.ContainsKey(recipe.ResultItemId)) return false;
            return recipe.Matches(container);
        }

        public bool TryCraft(CraftingRecipe recipe, InventoryContainer sourceContainer, InventoryContainer outputContainer)
        {
            if (!CanCraft(recipe, sourceContainer) || outputContainer == null) return false;

            var resultDefinition = GetItem(recipe.ResultItemId);
            if (resultDefinition == null) return false;

            var resultItem = new InventoryItem(resultDefinition, recipe.ResultQuantity);
            if (!outputContainer.AddItem(resultItem)) return false;

            foreach (var ingredient in recipe.Ingredients)
            {
                var remaining = ingredient.Quantity;
                foreach (var slot in sourceContainer.Slots)
                {
                    if (slot.IsEmpty || slot.Item.Definition.Id != ingredient.ItemId) continue;
                    var removed = slot.Item.Remove(Math.Min(remaining, slot.Item.Quantity));
                    remaining -= removed;
                    if (slot.Item.IsEmpty) slot.RemoveAll();
                    if (remaining <= 0) break;
                }
            }

            return true;
        }

        public ItemDefinition GetItem(ushort id)
        {
            return _items.TryGetValue(id, out var item) ? item : null;
        }
    }
}
