using System.Collections.Generic;
using TerraForge.Game;

namespace TerraForge.UI
{
    public sealed class HudState
    {
        public IReadOnlyList<HudMetric> Metrics { get; }
        public float Experience { get; }
        public float MaxExperience { get; }

        public HudState(IReadOnlyList<HudMetric> metrics, float experience, float maxExperience)
        {
            Metrics = metrics;
            Experience = experience;
            MaxExperience = maxExperience;
        }

        public float ExperienceNormalized => MaxExperience <= 0f ? 0f : Experience / MaxExperience;
        public string ExperienceLabel => $"EXP {Experience:0}/{MaxExperience:0}";

        public static HudState FromPlayerStats(PlayerStats stats, float experience, float maxExperience)
        {
            var metrics = new List<HudMetric>
            {
                new HudMetric("Vida", stats.Health, stats.MaxHealth, HudStyle.Default.HealthColor, "♥"),
                new HudMetric("Fome", stats.Hunger, stats.MaxHunger, HudStyle.Default.HungerColor, "🍖"),
                new HudMetric("Sede", stats.Thirst, stats.MaxThirst, HudStyle.Default.ThirstColor, "💧"),
                new HudMetric("Energia", stats.Stamina, stats.MaxStamina, HudStyle.Default.StaminaColor, "⚡")
            };

            return new HudState(metrics, experience, maxExperience);
        }
    }
}
