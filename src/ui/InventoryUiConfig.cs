namespace TerraForge.UI
{
    public sealed class InventoryUiConfig
    {
        public int MainContainerId { get; init; }
        public int HotbarContainerId { get; init; }
        public int CraftingInputContainerId { get; init; }
        public int CraftingOutputContainerId { get; init; }
        public int ChestContainerId { get; init; }

        public int MainColumns { get; init; } = 9;
        public int HotbarColumns { get; init; } = 9;
        public int CraftingColumns { get; init; } = 2;
        public int CraftingRows { get; init; } = 2;
        public int ChestColumns { get; init; } = 9;

        public bool HasChest => ChestContainerId > 0;
        public bool HasCrafting => CraftingInputContainerId > 0 && CraftingOutputContainerId > 0;
    }
}
