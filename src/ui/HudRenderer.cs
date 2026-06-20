using System;

namespace TerraForge.UI
{
    public sealed class HudRenderer
    {
        public HudStyle Style { get; }

        public HudRenderer(HudStyle style = null)
        {
            Style = style ?? HudStyle.Default;
        }

        public void Render(IUiRenderer renderer, HudState state, int screenWidth, int screenHeight)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (state == null) throw new ArgumentNullException(nameof(state));

            var margin = Style.ScreenMargin;
            var panelWidth = screenWidth * Style.TopPanelWidthRatio;
            var panelHeight = state.Metrics.Count * (Style.BarHeight + Style.BarSpacing) - Style.BarSpacing + 16f;
            var panelX = margin;
            var panelY = margin;

            renderer.DrawRectangle(panelX - 8f, panelY - 8f, panelWidth + 16f, panelHeight + 16f, Style.PanelBackground);
            renderer.DrawBorder(panelX - 8f, panelY - 8f, panelWidth + 16f, panelHeight + 16f, 1.5f, Style.PanelBorder);

            var y = panelY;
            foreach (var metric in state.Metrics)
            {
                renderer.DrawRectangle(panelX, y, panelWidth, Style.BarHeight, Style.BarBackground);
                renderer.DrawRectangle(panelX, y, panelWidth * metric.Normalized, Style.BarHeight, metric.FillColor);

                var label = string.IsNullOrWhiteSpace(metric.Icon)
                    ? metric.Label
                    : $"{metric.Icon} {metric.Label}";

                renderer.DrawText(label, panelX + 10f, y + Style.BarHeight * 0.5f, Style.TextColor, Style.TextSize, TextAlignment.Left);
                renderer.DrawText(metric.PercentageText, panelX + panelWidth - 10f, y + Style.BarHeight * 0.5f, Style.TextColor, Style.TextSize, TextAlignment.Right);
                y += Style.BarHeight + Style.BarSpacing;
            }

            var experienceWidth = screenWidth * Style.BottomBarWidthRatio;
            var experienceHeight = Style.BottomBarHeight;
            var experienceX = (screenWidth - experienceWidth) * 0.5f;
            var experienceY = screenHeight - margin - experienceHeight;

            renderer.DrawRectangle(experienceX, experienceY, experienceWidth, experienceHeight, Style.ExperienceBackgroundColor);
            renderer.DrawRectangle(experienceX, experienceY, experienceWidth * state.ExperienceNormalized, experienceHeight, Style.ExperienceBarColor);
            renderer.DrawBorder(experienceX, experienceY, experienceWidth, experienceHeight, 1.5f, Style.PanelBorder);
            renderer.DrawText(state.ExperienceLabel, experienceX + experienceWidth * 0.5f, experienceY + experienceHeight * 0.5f, Style.TextColor, Style.TextSize, TextAlignment.Center);
        }
    }
}
