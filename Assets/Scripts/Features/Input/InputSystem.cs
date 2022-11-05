using System;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Enums;
using Roguelike.Features.WorldComponents;
using Roguelike.Services;

namespace Roguelike.Features.Input
{
    public class InputSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<MoveInputEventComponent>> _moveInputFilter = default;
        private readonly EcsFilterInject<Inc<NextTurnDelayComponent>> _smoothMoveInProgressFilter = default;

        private readonly EcsPoolInject<MoveInputEventComponent> _moveInputPool = default;

        public void Run(IEcsSystems systems)
        {
            if (_moveInputFilter.Value.GetEntitiesCount() != 0) return;
            if (_smoothMoveInProgressFilter.Value.GetEntitiesCount() != 0) return;
            
            int vertical = Mathf.RoundToInt(UnityEngine.Input.GetAxisRaw(Idents.Input.VerticalAxis));
            int horizontal = Mathf.RoundToInt(UnityEngine.Input.GetAxisRaw(Idents.Input.HorizontalAxis));
            if (horizontal != 0 || vertical != 0)
            {
                var movement = ToMovement(horizontal, vertical);
                var moveInputPool = _moveInputPool.Value;
                int entity = moveInputPool.GetWorld().NewEntity();
                ref var moveComponent = ref moveInputPool.Add(entity);
                moveComponent.Movement = movement;
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