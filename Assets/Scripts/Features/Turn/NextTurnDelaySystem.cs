using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Roguelike.Features.WorldComponents;

namespace Roguelike.Features.Turn
{
    public class NextTurnDelaySystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<NextTurnDelayComponent>> _nextTurnDelayCFilter = default;

        private readonly EcsPoolInject<NextTurnDelayComponent> _nextTurnDelayCPool = default;

        public void Run(IEcsSystems systems)
        {
            var nextTurnDelayCPool = _nextTurnDelayCPool.Value;
            foreach (var entity in _nextTurnDelayCFilter.Value)
            {
                ref var nextTurnDelay = ref nextTurnDelayCPool.Get(entity);
                nextTurnDelay.SecondsLeft -= Time.deltaTime;
                if (nextTurnDelay.SecondsLeft <= 0)
                    nextTurnDelayCPool.Del(entity);
            }
        }
    }
}