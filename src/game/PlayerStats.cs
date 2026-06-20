namespace TerraForge.Game
{
    public sealed class PlayerStats
    {
        public float Health { get; private set; }
        public float Hunger { get; private set; }
        public float Thirst { get; private set; }
        public float Stamina { get; private set; }

        public float MaxHealth { get; } = 100f;
        public float MaxHunger { get; } = 100f;
        public float MaxThirst { get; } = 100f;
        public float MaxStamina { get; } = 100f;

        public bool IsAlive => Health > 0f;

        public PlayerStats(float health = 100f, float hunger = 100f, float thirst = 100f, float stamina = 100f)
        {
            Health = health;
            Hunger = hunger;
            Thirst = thirst;
            Stamina = stamina;
        }

        public void AddHealth(float amount)
        {
            Health = Clamp(Health + amount, 0f, MaxHealth);
        }

        public void AddHunger(float amount)
        {
            Hunger = Clamp(Hunger + amount, 0f, MaxHunger);
        }

        public void AddThirst(float amount)
        {
            Thirst = Clamp(Thirst + amount, 0f, MaxThirst);
        }

        public void AddStamina(float amount)
        {
            Stamina = Clamp(Stamina + amount, 0f, MaxStamina);
        }

        public void Update(float deltaTime, bool isRunning, bool isCrouching, bool isSwimming)
        {
            var hungerDrain = 0.5f * deltaTime;
            var thirstDrain = 0.7f * deltaTime;
            var staminaDrain = 0f;

            if (isRunning)
            {
                staminaDrain += 15f * deltaTime;
                hungerDrain += 0.1f * deltaTime;
                thirstDrain += 0.15f * deltaTime;
            }
            else if (isCrouching)
            {
                staminaDrain += 2f * deltaTime;
            }
            else if (isSwimming)
            {
                staminaDrain += 10f * deltaTime;
                thirstDrain += 0.2f * deltaTime;
            }
            else
            {
                staminaDrain += 5f * deltaTime;
            }

            Hunger = Clamp(Hunger - hungerDrain, 0f, MaxHunger);
            Thirst = Clamp(Thirst - thirstDrain, 0f, MaxThirst);
            Stamina = Clamp(Stamina - staminaDrain, 0f, MaxStamina);

            if (Hunger <= 10f || Thirst <= 10f)
            {
                AddHealth(-5f * deltaTime);
            }
        }

        private static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}
