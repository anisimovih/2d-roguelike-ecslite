namespace Roguelike.Features.Health
{
    internal struct HealthComponent
    {
        public int CurrentHealth;
        public int MaxHealth;

        public void SetupHealth(int maxHealth, int currentHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
        }
        
        public bool IsDamaged
        {
            get { return CurrentHealth < MaxHealth;  }
        }
    }
}