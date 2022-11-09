using System.Collections.Generic;
using System.Linq;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Components;
using Roguelike.Features.Health;
using Roguelike.Features.Input;
using Roguelike.Features.Render;
using Roguelike.Extensions;
using Roguelike.Features.Actions;
using Roguelike.Services;

namespace Roguelike.Features.Consumables
{
    public class FoodSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<PositionChangeEventComponent, ControllableComponent>> _charPositionChangedFilter = default;

        private readonly EcsCustomInject<GameBoardService> _gameBoardService = default;

        private readonly EcsPoolInject<PositionComponent> _positionPool = default;
        private readonly EcsPoolInject<FoodComponent> _foodPool = default;
        private readonly EcsPoolInject<ResourceComponentNew> _resourceComponentPool = default;
        private readonly EcsPoolInject<HealthChangeEventComponent> _healthChangedPool = default;
        private readonly EcsPoolInject<ActionComponent> _actionPool = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var movedEntity in _charPositionChangedFilter.Value)
            {
                ref var position = ref _positionPool.Value.Get(movedEntity);
                (bool canMove, ICollection<int> posEntities) = _gameBoardService.Value.IsGameBoardPositionOpen(position.X, position.Y);
                if (posEntities == null || posEntities.Empty()) continue;

                var foodPool = _foodPool.Value;
                int foodEntity = posEntities.FirstOrDefault(entity => foodPool.Has(entity));
                if (foodEntity == default) continue;

                AddAction(foodEntity, GameAction.CONSUME);
                
                var foodHeal = foodPool.Get(foodEntity).Points;
                if (_healthChangedPool.Value.Has(movedEntity))
                {
                    ref var healthChanged = ref _healthChangedPool.Value.Get(movedEntity);
                    healthChanged.HealthChangeAmount += foodHeal;
                }
                else
                {
                    ref var healthChanged = ref _healthChangedPool.Value.Add(movedEntity);
                    healthChanged.HealthChangeAmount = foodHeal;
                }
                
                _resourceComponentPool.Value.Del(foodEntity);
                _gameBoardService.Value.Grid.Remove(position.X, position.Y, foodEntity);
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