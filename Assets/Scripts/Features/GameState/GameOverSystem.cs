using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.External.easyevents;
using Roguelike.Features.Actions;
using Roguelike.Features.Health;
using Roguelike.Features.Input;
using Roguelike.Features.WorldComponents;

namespace Roguelike.Features.GameState
{
    internal sealed class GameOverSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<ControllableComponent, HealthChangeEventComponent>> _characterHealthChangedFilter;
        
        private readonly EcsCustomInject<EventsBus> _eventsBus = default;

        private readonly EcsPoolInject<HealthComponent> _healthPool = default;
        private readonly EcsPoolInject<ActionComponent> _actionPool = default;

        public void Run(IEcsSystems systems)
        {
            var events = _eventsBus.Value;
            if (events.HasEventSingleton<GameOverComponent>()) return;

            var characterHealthChangedFilter = _characterHealthChangedFilter.Value;
            if (characterHealthChangedFilter.GetEntitiesCount() == 0) return;

            foreach (var character in characterHealthChangedFilter)
            {
                ref var healthPool = ref _healthPool.Value.Get(character);
                if (healthPool.CurrentHealth <= 0)
                {
                    events.NewEventSingleton<GameOverComponent>();
                    AddAction(character, GameAction.DIE);
                    return;
                }
            }
        }

        private void AddAction(int entity, GameAction gameAction)
        {
            var actionPool = _actionPool.Value;
            ref var action = ref actionPool.Has(entity) ? ref _actionPool.Value.Get(entity) : ref _actionPool.Value.Add(entity);
            action.GameAction = gameAction;
        }
    }
}
