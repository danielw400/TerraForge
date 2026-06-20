namespace TerraForge.Game
{
    public interface IPlayerAbility
    {
        string Name { get; }
        bool IsActive { get; }
        void Activate(PlayerController player);
        void Deactivate(PlayerController player);
        void Update(PlayerController player, float deltaTime, PlayerInput input);
    }
}
