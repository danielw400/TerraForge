namespace TerraForge.UI
{
    public sealed class InventoryUiStyle
    {
        public float ScreenMargin { get; init; } = 18f;
        public float PanelCornerRadius { get; init; } = 12f;
        public float SectionSpacing { get; init; } = 18f;
        public float SlotSize { get; init; } = 56f;
        public float SlotSpacing { get; init; } = 8f;
        public float PanelPadding { get; init; } = 14f;
        public float SectionTitleSize { get; init; } = 16f;
        public float ItemTextSize { get; init; } = 14f;
        public float CountTextSize { get; init; } = 12f;
        public float PanelBorderThickness { get; init; } = 1.5f;

        public UiColor PanelBackground { get; init; } = UiColor.FromHex(0x111B2D, 0.85f);
        public UiColor PanelBorder { get; init; } = UiColor.FromHex(0x9AA5B0, 0.25f);
        public UiColor SlotBackground { get; init; } = UiColor.FromHex(0x1F2C45, 0.90f);
        public UiColor SlotBorder { get; init; } = UiColor.FromHex(0x6F829D, 0.35f);
        public UiColor SlotHover { get; init; } = UiColor.FromHex(0x5F7CCF, 0.25f);
        public UiColor SlotActive { get; init; } = UiColor.FromHex(0x8AB2FF, 0.35f);
        public UiColor TextPrimary { get; init; } = UiColor.FromHex(0xF4F7FF);
        public UiColor TextSecondary { get; init; } = UiColor.FromHex(0xA9B8D1);
        public UiColor TitleText { get; init; } = UiColor.FromHex(0xFFFFFF);
        public UiColor DragPreviewBackground { get; init; } = UiColor.FromHex(0x121D31, 0.95f);
        public UiColor HotbarActiveBackground { get; init; } = UiColor.FromHex(0x8AB2FF, 0.30f);

        public static InventoryUiStyle Default => new InventoryUiStyle();
    }
}
