using System;
using TerraForge.Core;
using TerraForge.Game.Inventory;
using TerraForge.Input;
using TerraForge.UI;

namespace TerraForge.Examples
{
    public static class InventoryUiIntegrationExample
    {
        public static void Run()
        {
            var provider = new MockInputProvider();
            var inputManager = new InputManager(provider);
            BindingsPreset.ApplyDefaults(inputManager);

            var itemRegistry = new[]
            {
                new ItemDefinition(1, "Madeira", "Madeira bruta.", ItemCategory.Material, maxStack: 64),
                new ItemDefinition(2, "Pedra", "Pedra comum.", ItemCategory.Material, maxStack: 64),
                new ItemDefinition(3, "Espada de Ferro", "Uma espada de ferro.", ItemCategory.Weapon, maxStack: 1, equipSlot: EquipmentSlotType.MainHand),
                new ItemDefinition(4, "Peitoral", "Uma armadura leve.", ItemCategory.Armor, maxStack: 1, equipSlot: EquipmentSlotType.Chest)
            };

            var recipes = new[]
            {
                new CraftingRecipe(3, 1, new[] { new CraftingIngredient(1, 2), new CraftingIngredient(2, 1) })
            };

            var craftingManager = new CraftingManager(itemRegistry, recipes);
            var inventorySession = new InventorySession(craftingManager);

            var playerInventory = new InventoryContainer("Inventário do Jogador", 27);
            var hotbar = new InventoryContainer("Barra Rápida", 9);
            var craftingInput = new InventoryContainer("Crafting Input", 4);
            var craftingOutput = new InventoryContainer("Crafting Output", 1);
            var chest = new ChestInventory("Baú", 18);

            var mainId = inventorySession.RegisterContainer(playerInventory);
            var hotbarId = inventorySession.RegisterContainer(hotbar);
            var craftInputId = inventorySession.RegisterContainer(craftingInput);
            var craftOutputId = inventorySession.RegisterContainer(craftingOutput);
            var chestId = inventorySession.RegisterContainer(chest);

            playerInventory.AddItem(new InventoryItem(itemRegistry[0], 16));
            playerInventory.AddItem(new InventoryItem(itemRegistry[1], 10));
            playerInventory.AddItem(new InventoryItem(itemRegistry[2], 1));
            playerInventory.AddItem(new InventoryItem(itemRegistry[3], 1));

            var uiConfig = new InventoryUiConfig
            {
                MainContainerId = mainId,
                HotbarContainerId = hotbarId,
                CraftingInputContainerId = craftInputId,
                CraftingOutputContainerId = craftOutputId,
                ChestContainerId = chestId
            };

            var uiFacade = new InventoryUiFacade(inventorySession, uiConfig);
            var screenWidth = 1920;
            var screenHeight = 1080;
            var renderer = new MockUiRenderer();

            Console.WriteLine("Pressione Tab para abrir/fechar o inventário.");
            Console.WriteLine("Use Mouse0 e Mouse1 para arrastar/soltar e dividir pilhas.");

            for (var frame = 0; frame < 10; frame++)
            {
                // Simular atualizações básicas
                if (frame == 1) provider.SetButton("Inventory", true);
                if (frame == 2) provider.SetButton("Inventory", false);

                inputManager.Update(1f / 60f);
                uiFacade.Update(1f / 60f, provider, screenWidth, screenHeight);
                uiFacade.Render(renderer, screenWidth, screenHeight);

                provider.AdvanceFrame();
            }

            Console.WriteLine("Exemplo de integração do inventário concluído.");
        }
    }

    internal sealed class MockUiRenderer : IUiRenderer
    {
        public void DrawRectangle(float x, float y, float width, float height, UiColor color)
        {
            // Placeholder: renderizar retângulo em sua engine real.
        }

        public void DrawBorder(float x, float y, float width, float height, float thickness, UiColor color)
        {
            // Placeholder: renderizar borda.
        }

        public void DrawText(string text, float x, float y, UiColor color, float fontSize, TextAlignment alignment = TextAlignment.Left)
        {
            // Placeholder: renderizar texto.
        }

        public float MeasureTextWidth(string text, float fontSize)
        {
            return text.Length * fontSize * 0.5f;
        }
    }
}
