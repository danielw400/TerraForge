using TerraForge.Core;

namespace TerraForge.Game.Building
{
    public sealed class BlockHealth
    {
        public Vector3Int Position { get; }
        public float MaxHealth { get; }
        public float CurrentHealth { get; private set; }

        public bool IsDestroyed => CurrentHealth <= 0f;

        public BlockHealth(Vector3Int position, float maxHealth)
        {
            Position = position;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            CurrentHealth -= amount;
            if (CurrentHealth < 0f) CurrentHealth = 0f;
        }

        public void Repair(float amount)
        {
            CurrentHealth += amount;
            if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
        }

        public float GetHealthPercentage() => MaxHealth > 0f ? CurrentHealth / MaxHealth : 0f;
    }
}
