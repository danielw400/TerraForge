using TerraForge.Game.Inventory;
using TerraForge.Input;

namespace TerraForge.UI
{
    public sealed class InventoryUiFacade
    {
        private readonly InventoryUiManager _manager;
        private readonly InventoryUiRenderer _renderer;

        public InventoryUiFacade(InventorySession session, InventoryUiConfig config, InventoryUiStyle style = null)
        {
            _manager = new InventoryUiManager(session, config, style);
            _renderer = new InventoryUiRenderer(style);
        }

        public InventoryUiState State => _manager.State;

        public void Update(float deltaTime, IInputProvider input, int screenWidth, int screenHeight)
        {
            _manager.Update(deltaTime, input, screenWidth, screenHeight);
        }

        public void Render(IUiRenderer renderer, int screenWidth, int screenHeight)
        {
            _renderer.Render(renderer, _manager, screenWidth, screenHeight);
        }

        public InventoryItem GetActiveHotbarItem() => _manager.GetActiveHotbarItem();
    }
}
