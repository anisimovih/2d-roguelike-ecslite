using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Components;
using Roguelike.Features.Consumables;
using Roguelike.Features.Health;
using Roguelike.Features.Input;
using Roguelike.Features.Stats;
using Roguelike.Features.Turn;
using Roguelike.Extensions;
using Roguelike.Features.Actions;
using Roguelike.Services;

namespace Roguelike.Features.AIMove
{
    internal sealed class AIMoveSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<
            Inc<AIMoveComponent, ActiveTurnBasedComponent, TurnBasedComponent, PositionComponent>> 
            _aiMove = default;

        private readonly EcsPoolInject<AIMoveComponent> _aiMovePool = default;
        private readonly EcsPoolInject<PositionComponent> _positionPool = default;
        private readonly EcsPoolInject<SkipTurnComponent> _skipTurnPool = default;
        private readonly EcsPoolInject<ActiveTurnBasedComponent> _activeTurnBasedPool = default;
        private readonly EcsPoolInject<ControllableComponent> _controllablePool = default;
        private readonly EcsPoolInject<HealthChangeEventComponent> _healthChangePool = default;
        private readonly EcsPoolInject<FoodComponent> _foodPool = default;
        private readonly EcsPoolInject<DamageComponent> _damagePool = default;
        private readonly EcsPoolInject<ActionComponent> _actionPool = default;

        private readonly EcsCustomInject<GameBoardService> _gameBoardService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var movingEntity in _aiMove.Value)
            {
                ref var AIMove = ref _aiMovePool.Value.Get(movingEntity);

                var targetPos = _positionPool.Value.Get(AIMove.Target);
                ref var currentPos = ref _positionPool.Value.Get(movingEntity);

                var moveX = 0;
                var moveY = 0;
                
                bool moveYish = Mathf.Abs(targetPos.X - currentPos.X) == 0;
                if (moveYish)
                {
                    moveY = targetPos.Y > currentPos.Y ? 1 : -1;
                }
                else
                {
                    moveX = targetPos.X > currentPos.X ? 1 : -1;
                }

                int newX = currentPos.X + moveX;
                int newY = currentPos.Y + moveY;
                
                (bool canMove, ICollection<int> existing) = _gameBoardService.Value.IsGameBoardPositionOpen(newX, newY);
                if (existing != null && !existing.Empty())
                {
                    canMove = PrepareMove(movingEntity, existing);
                }

                if (canMove)
                {
                    _gameBoardService.Value.Grid.Remove(currentPos.X, currentPos.Y, movingEntity);
                    currentPos.X = newX;
                    currentPos.Y = newY;
                    _gameBoardService.Value.Grid.Add(currentPos.X, currentPos.Y, movingEntity);
                }

                // skip next turn
                _skipTurnPool.Value.Add(movingEntity);
                _activeTurnBasedPool.Value.Del(movingEntity);
            }
        }

        private bool PrepareMove(int enemy, ICollection<int> entitiesInSpot)
        {
            bool playerFound = false;
            int player = 0;
            foreach (var entity in entitiesInSpot)
            {
                if (!_controllablePool.Value.Has(entity)) continue;

                playerFound = true;
                player = entity;
                break;
            }
            
            if (playerFound)
            {
                if (_healthChangePool.Value.Has(player))
                {
                    ref var healthChange = ref _healthChangePool.Value.Get(player);
                    healthChange.HealthChangeAmount -= _damagePool.Value.Get(enemy).Points;
                }
                else
                {
                    ref var healthChange = ref _healthChangePool.Value.Add(player);
                    healthChange.HealthChangeAmount = -_damagePool.Value.Get(enemy).Points;
                }

                AddAction(player, GameAction.RECEIVE_DAMAGE);
                AddAction(enemy, GameAction.ATTACK);
                return false;
            }

            // Consumables do not block the cell
            return entitiesInSpot.Count == 1 && _foodPool.Value.Has(entitiesInSpot.First());
        }

        private void AddAction(int entity, GameAction gameAction)
        {
            var actionPool = _actionPool.Value;
            ref var action = ref actionPool.Has(entity) ? ref _actionPool.Value.Get(entity) : ref _actionPool.Value.Add(entity);
            action.GameAction = gameAction;
        }
    }
}
