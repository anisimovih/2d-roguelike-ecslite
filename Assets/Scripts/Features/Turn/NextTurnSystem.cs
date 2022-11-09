using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Extensions;
using Roguelike.External.easyevents;
using Roguelike.Features.Input;
using Roguelike.Features.WorldComponents;
using Roguelike.Scriptables;
using Roguelike.Services;

namespace Roguelike.Features.Turn
{
    public class NextTurnSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<SkipTurnComponent> _skipTurnPool = default;
        private readonly EcsPoolInject<ActiveTurnBasedComponent> _activeTurnBasedPool = default;

        private readonly EcsCustomInject<EventsBus> _eventsBus = default;
        private readonly EcsCustomInject<TurnOrderService> _turnOrderService = default;
        private readonly EcsCustomInject<Configuration> _configuration = default;

        public void Run(IEcsSystems systems)
        {
            var eventsBus = _eventsBus.Value;
            
            if (eventsBus.HasEventSingleton<GameOverComponent>() 
                || !eventsBus.HasEventSingleton<MoveInputEventComponent>()) return;

            var turnOrderService = _turnOrderService.Value;

            var turnOrder = turnOrderService.TurnOrder;
            if (turnOrder.Empty())
            {
                return;
            }

            Activate(turnOrderService, eventsBus);
        }

        private void Activate(TurnOrderService turnOrderService, EventsBus eventsBus)
        {
            var currentTurnNode = turnOrderService.TurnOrder.First;
            while (currentTurnNode != null)
            {
                var nextEntity = currentTurnNode.Value;
                if (_skipTurnPool.Value.Has(nextEntity))
                {
                    _skipTurnPool.Value.Del(nextEntity);
                }
                else
                {
                    if (!_activeTurnBasedPool.Value.Has(nextEntity))
                        _activeTurnBasedPool.Value.Add(nextEntity);
                }
                currentTurnNode = currentTurnNode.Next;
            }

            eventsBus.NewEventSingleton<NextTurnDelayComponent>().SecondsLeft = _configuration.Value.TURN_DELAY;
        }
    }
}