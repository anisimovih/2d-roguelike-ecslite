using UnityEngine.UI;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Unity.Ugui;

using Roguelike.Features.Input;
using Roguelike.Scriptables;
using Roguelike.Services;

namespace Roguelike.Features.Health
{
    public class HealthUISystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<HealthComponent, ControllableComponent>> _characterHealthFilter = default;
        private readonly EcsFilterInject<Inc<HealthChangeEventComponent, ControllableComponent>> _characterHealthChangedFilter = default;

        private readonly EcsCustomInject<Configuration> _configuration = default;

        private readonly EcsPoolInject<HealthChangeEventComponent> _healthChangeEventPool = default;
        private readonly EcsPoolInject<HealthComponent> _healthPool = default;
        
        [EcsUguiNamed (Idents.Ui.FoodLeft)] private readonly Text _foodLeft = default;
        
        public void Init(IEcsSystems systems)
        {
            if (_characterHealthFilter.Value.GetEntitiesCount() == 0) return;

            var character = _characterHealthFilter.Value.GetRawEntities()[0];
            _foodLeft.text = $"Food: {_healthPool.Value.Get(character).CurrentHealth.ToString()}";
        }
        
        public void Run(IEcsSystems systems)
        {
            var configuration = _configuration.Value;
            var healthPool = _healthPool.Value;
            
            foreach (var e in _characterHealthChangedFilter.Value)
            {
                var diff = _healthChangeEventPool.Value.Get(e).HealthChangeAmount + configuration.FOOD_LOST_PER_TURN;
                var prefix = "";
                if (diff > 0)
                    prefix = $"+{diff} ";
                else if (diff < 0)
                    prefix = $"{diff} ";

                var currentHealth = healthPool.Get(e).CurrentHealth;
                _foodLeft.text = $"{prefix}Food: {currentHealth}";
            }
        }
    }
}