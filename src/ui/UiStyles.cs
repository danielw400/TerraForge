namespace TerraForge.UI
{
    public sealed class HudStyle
    {
        public float ScreenMargin { get; init; } = 24f;
        public float BarHeight { get; init; } = 22f;
        public float BarSpacing { get; init; } = 10f;
        public float BottomBarHeight { get; init; } = 28f;
        public float TopPanelWidthRatio { get; init; } = 0.24f;
        public float BottomBarWidthRatio { get; init; } = 0.62f;
        public float TextSize { get; init; } = 14f;
        public float LabelTextSize { get; init; } = 12f;
        public float PanelCornerRadius { get; init; } = 10f;

        public UiColor PanelBackground { get; init; } = UiColor.FromHex(0x0B1220, 0.70f);
        public UiColor PanelBorder { get; init; } = UiColor.FromHex(0x9AA5B0, 0.22f);
        public UiColor BarBackground { get; init; } = UiColor.FromHex(0x293145, 0.80f);
        public UiColor TextColor { get; init; } = UiColor.FromHex(0xF4F7FF, 0.95f);
        public UiColor HealthColor { get; init; } = UiColor.FromHex(0xE0514D);
        public UiColor HungerColor { get; init; } = UiColor.FromHex(0xF3A43B);
        public UiColor ThirstColor { get; init; } = UiColor.FromHex(0x4FB4F8);
        public UiColor StaminaColor { get; init; } = UiColor.FromHex(0x8DE06A);
        public UiColor ExperienceBarColor { get; init; } = UiColor.FromHex(0x6C8EFF);
        public UiColor ExperienceBackgroundColor { get; init; } = UiColor.FromHex(0x1F2A44, 0.85f);

        public static HudStyle Default => new HudStyle();
    }
}
