using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Extensions;
using Roguelike.Features.Input;
using Roguelike.Features.WorldComponents;
using Roguelike.Scriptables;
using Roguelike.Services;

namespace Roguelike.Features.Turn
{
    public class NextTurnSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<GameOverComponent>> _gameOverFilter = default;
        private readonly EcsFilterInject<Inc<MoveInputEventComponent>> _moveInputFilter = default;

        private readonly EcsPoolInject<SkipTurnComponent> _skipTurnPool = default;
        private readonly EcsPoolInject<ActiveTurnBasedComponent> _activeTurnBasedPool = default;
        private readonly EcsPoolInject<NextTurnDelayComponent> _nextTurnDelayPool = default;

        private readonly EcsCustomInject<TurnOrderService> _turnOrderService = default;
        private readonly EcsCustomInject<Configuration> _configuration = default;

        public void Run(IEcsSystems systems)
        {
            if (_moveInputFilter.Value.GetEntitiesCount() == 0) return;
            if (_gameOverFilter.Value.GetEntitiesCount() > 0) return;

            var turnOrderService = _turnOrderService.Value;

            var turnOrder = turnOrderService.TurnOrder;
            if (turnOrder.Empty())
            {
                return;
            }

            Activate(turnOrderService);
        }

        private void Activate(TurnOrderService turnOrderService)
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

            ref var nextTurnDelay = ref _nextTurnDelayPool.Value.Add(_nextTurnDelayPool.Value.GetWorld().NewEntity());
            nextTurnDelay.SecondsLeft = _configuration.Value.TURN_DELAY;
        }
    }
}