using System;
using System.Linq;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Extensions;
using Roguelike.Services;

namespace Roguelike.Features.Turn
{
    public class QueueSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<TurnBasedComponent>, Exc<InQueueComponent>> _turnBasedNotInQueueFilter = default;
        private readonly EcsFilterInject<Inc<InQueueComponent>, Exc<TurnBasedComponent>> _inQueueNotTurnBasedFilter = default;

        private readonly EcsPoolInject<TurnBasedComponent> _turnBasedPool = default;
        private readonly EcsPoolInject<InQueueComponent> _inQueuePool = default;

        private readonly EcsCustomInject<TurnOrderService> _turnOrderService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _inQueueNotTurnBasedFilter.Value)
            {
                RemoveTurnBasedEntityFromQueue(entity);
                _inQueuePool.Value.Del(entity);
            }
            foreach (var entity in _turnBasedNotInQueueFilter.Value)
            {
                AddTurnBasedEntityToQueue(entity);
                _inQueuePool.Value.Add(entity);
            }
        }

        private void AddTurnBasedEntityToQueue(int entity)
        {
            var turnOrderService = _turnOrderService.Value;
            var turnOrder = turnOrderService.TurnOrder;
            if (turnOrder == null)
            {
                throw new ArgumentException("turnOrder not initialized");
            }
            
            if (turnOrder.Empty())
            {
                turnOrder.AddFirst(entity);
                return;
            }

            var turnBasedPool = _turnBasedPool.Value;
            var newIndex = turnBasedPool.Get(entity).Index;
            var firstIndex = turnBasedPool.Get(turnOrder.First.Value).Index;
            if (firstIndex >= newIndex)
            {
                turnOrder.AddFirst(entity);
                return;
            }

            var lastIndex = turnBasedPool.Get(turnOrder.Last.Value).Index;
            if (lastIndex <= newIndex)
            {
                turnOrder.AddLast(entity);
                return;
            }

            var match = turnOrder.Nodes()
                .FirstOrDefault(n => turnBasedPool.Get(n.Next.Value).Index >= newIndex);
            turnOrder.AddAfter(match, entity);
        }

        private void RemoveTurnBasedEntityFromQueue(int entity)
        {
            _turnOrderService.Value.TurnOrder.Remove(entity);
        }
    }
}