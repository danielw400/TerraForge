using System;

namespace TerraForge.UI
{
    public sealed class HudMetric
    {
        public string Label { get; }
        public float CurrentValue { get; }
        public float MaxValue { get; }
        public UiColor FillColor { get; }
        public string Icon { get; }

        public HudMetric(string label, float currentValue, float maxValue, UiColor fillColor, string icon = "")
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            CurrentValue = currentValue;
            MaxValue = maxValue;
            FillColor = fillColor;
            Icon = icon;
        }

        public float Normalized => MaxValue <= 0f ? 0f : CurrentValue / MaxValue;
        public string PercentageText => MaxValue <= 0f ? "0%" : $"{(int)(Normalized * 100f)}%";
    }
}
