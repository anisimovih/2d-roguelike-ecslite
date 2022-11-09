using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Enums;
using Roguelike.Features.AIMove;
using Roguelike.Features.Components;
using Roguelike.Features.Health;
using Roguelike.Features.Turn;
using Roguelike.Features.WorldComponents;
using Roguelike.Extensions;
using Roguelike.External.easyevents;
using Roguelike.Features.Actions;
using Roguelike.Services;

namespace Roguelike.Features.Input
{
    public class MoveSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<ControllableComponent>> _controllableFilter = default;

        private readonly EcsCustomInject<EventsBus> _eventsBus = default;
        private readonly EcsCustomInject<GameBoardService> _gameBoardService = default;

        private readonly EcsPoolInject<ActiveTurnBasedComponent> _activeTurnBasedPool = default;
        private readonly EcsPoolInject<PositionComponent> _positionPool = default;
        private readonly EcsPoolInject<PositionChangeEventComponent> _positionChangedPool = default;
        private readonly EcsPoolInject<AIMoveComponent> _aiMovePool = default;
        private readonly EcsPoolInject<HealthComponent> _healthPool = default;
        private readonly EcsPoolInject<HealthChangeEventComponent> _healthChangeEventPool = default;
        private readonly EcsPoolInject<ActionComponent> _actionPool = default;

        private static Vector2 ToVector(Movement movement)
        {
            switch (movement)
            {
                case Movement.UP:
                    return new Vector2(0, 1);
                case Movement.RIGHT:
                    return new Vector2(1, 0);
                case Movement.DOWN:
                    return new Vector2(0, -1);
                case Movement.LEFT:
                default:
                    return new Vector2(-1, 0);
            }
        }

        public void Run(IEcsSystems systems)
        {
            var eventsBus = _eventsBus.Value;
            
            if (!eventsBus.HasEventSingleton<MoveInputEventComponent>(out var moveInput)
                || eventsBus.HasEventSingleton<GameOverComponent>()
                || _controllableFilter.Value.GetEntitiesCount() == 0
                || eventsBus.HasEventSingleton<LevelTransitionDelayComponent>())
            {
                // ignore input
                return;
            }

            foreach (var controllableId in _controllableFilter.Value)
            {
                if (!_activeTurnBasedPool.Value.Has(controllableId))
                {
                    // ignore input
                    continue;
                }

                var healthChangePool = _healthChangeEventPool.Value;
                if (_healthChangeEventPool.Value.Has(controllableId))
                {
                    ref var healthChange = ref healthChangePool.Get(controllableId);
                    healthChange.HealthChangeAmount -= 1;
                }
                else
                {
                    ref var healthChange = ref healthChangePool.Add(controllableId);
                    healthChange.HealthChangeAmount = -1;
                }

                var movement = moveInput.Movement;
                var movementPos = ToVector(movement);

                ref var currentPos = ref _positionPool.Value.Get(controllableId);
                int newX = currentPos.X + (int) movementPos.x;
                int newY = currentPos.Y + (int) movementPos.y;

                (bool canMove, ICollection<int> existing) = _gameBoardService.Value.IsGameBoardPositionOpen(newX, newY);
                if (existing != null)
                {
                    canMove = PrepareMove(controllableId, existing);
                }

                if (canMove)
                {
                    AddAction(controllableId, GameAction.MOVE);
                    
                    _gameBoardService.Value.Grid.Remove(currentPos.X, currentPos.Y, controllableId);
                    currentPos.SetPositions(newX, newY);
                    _gameBoardService.Value.Grid.Add(newX, newY, controllableId);
                    _positionChangedPool.Value.Add(controllableId);
                }

                _activeTurnBasedPool.Value.Del(controllableId);
            }
        }

        private bool PrepareMove(int player, ICollection<int> entitiesInSpot)
        {
            foreach (var id in entitiesInSpot)
            {
                // enemy there, can't do anything
                if (_aiMovePool.Value.Has(id)) return false;
            }

            // handle walls
            foreach (var id in entitiesInSpot)
            {
                // enemy there, can't do anything
                if (_healthPool.Value.Has(id))
                {
                    ref var healthChange = ref _healthChangeEventPool.Value.Add(id);
                    healthChange.HealthChangeAmount = -1;

                    AddAction(player, GameAction.CHOP);
                    
                    // nothing to do now that we've chopped
                    return false;
                }
            }
            
            // otherwise we can move
            return true;
        }

        private void AddAction(int entity, GameAction gameAction)
        {
            var actionPool = _actionPool.Value;
            ref var action = ref actionPool.Has(entity) ? ref _actionPool.Value.Get(entity) : ref _actionPool.Value.Add(entity);
            action.GameAction = gameAction;
        }
    }
}