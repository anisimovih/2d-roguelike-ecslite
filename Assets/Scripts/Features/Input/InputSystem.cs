using System;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Enums;
using Roguelike.External.easyevents;
using Roguelike.Features.WorldComponents;
using Roguelike.Services;

namespace Roguelike.Features.Input
{
    public class InputSystem : IEcsRunSystem
    {
        private readonly EcsCustomInject<EventsBus> _eventsBus = default;

        public void Run(IEcsSystems systems)
        {
            var eventsBus = _eventsBus.Value;
            
            if (eventsBus.HasEventSingleton<MoveInputEventComponent>() 
                || eventsBus.HasEventSingleton<NextTurnDelayComponent>()) return;
            
            int vertical = Mathf.RoundToInt(UnityEngine.Input.GetAxisRaw(Idents.Input.VerticalAxis));
            int horizontal = Mathf.RoundToInt(UnityEngine.Input.GetAxisRaw(Idents.Input.HorizontalAxis));
            if (horizontal != 0 || vertical != 0)
            {
                eventsBus.NewEventSingleton<MoveInputEventComponent>().Movement = ToMovement(horizontal, vertical);
            }
        }
        
        private Movement ToMovement(int x, int y)
        {
            // only allow 1 direction, prioritize horizontal over vertical
            if (x != 0)
            {
                return x > 0 ? Movement.RIGHT : Movement.LEFT;
            }
            if (y != 0)
            {
                return y > 0 ? Movement.UP : Movement.DOWN;
            }
            throw new ArgumentException(
                String.Format("Can't translate x:{0} y:{1} to movement.", x, y));
        }
    }
}