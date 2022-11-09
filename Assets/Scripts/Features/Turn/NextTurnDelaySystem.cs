using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Roguelike.External.easyevents;
using Roguelike.Features.WorldComponents;

namespace Roguelike.Features.Turn
{
    public class NextTurnDelaySystem : IEcsRunSystem
    {
        private readonly EcsCustomInject<EventsBus> _eventsBus = default;

        public void Run(IEcsSystems systems)
        {
            var eventsBus = _eventsBus.Value;
            
            if (!eventsBus.HasEventSingleton<NextTurnDelayComponent>()) return;
            
            ref var nextTurnDelay = ref eventsBus.GetEventBodySingleton<NextTurnDelayComponent>();
            nextTurnDelay.SecondsLeft -= Time.deltaTime;
            if (nextTurnDelay.SecondsLeft <= 0)
            {
                eventsBus.DestroyEventSingleton<NextTurnDelayComponent>();
            }
        }
    }
}