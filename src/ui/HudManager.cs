using TerraForge.Game;

namespace TerraForge.UI
{
    public sealed class HudManager
    {
        private readonly HudRenderer _renderer;
        private HudState _state;

        public HudManager(HudStyle style = null)
        {
            _renderer = new HudRenderer(style);
            _state = new HudState(Array.Empty<HudMetric>(), 0f, 1f);
        }

        public void Update(PlayerStats stats, float experience, float maxExperience)
        {
            _state = HudState.FromPlayerStats(stats, experience, maxExperience);
        }

        public void Render(IUiRenderer renderer, int screenWidth, int screenHeight)
        {
            _renderer.Render(renderer, _state, screenWidth, screenHeight);
        }
    }
}
