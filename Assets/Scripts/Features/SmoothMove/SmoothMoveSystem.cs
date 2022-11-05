using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Components;
using Roguelike.Features.Render;

namespace Roguelike.Features.SmoothMove
{
    public class SmoothMoveSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<ViewComponent, PositionComponent, SmoothMoveComponent>> _smoothMovableFilter = default;

        private readonly EcsPoolInject<PositionComponent> _positionPool = default;
        private readonly EcsPoolInject<ViewComponent> _viewPool = default;
        private readonly EcsPoolInject<SmoothMoveComponent> _smoothMovePool = default;
        private readonly EcsPoolInject<SmoothMoveInProgressComponent> _smoothMoveInProgressPool = default;
        
        public void Run(IEcsSystems systems)
        {
            foreach (var e in _smoothMovableFilter.Value)
            {
                ref var position = ref _positionPool.Value.Get(e);
                ref var view = ref _viewPool.Value.Get(e);
                var gameObject = view.GameObject;
                var transform = gameObject.transform;
                var end = new Vector3(position.X, position.Y, 0f);

                if (_smoothMoveInProgressPool.Value.Has(e))
                {
                    ref var smoothMoveInProgress = ref _smoothMoveInProgressPool.Value.Get(e);
                    
                    if (smoothMoveInProgress.MoveSpaceLeft > float.Epsilon)
                    {
                        ref var smoothMove = ref _smoothMovePool.Value.Get(e);
                        var inverseMoveTime = 10f / smoothMove.MoveTime;
                        var rigidBody2d = gameObject.GetComponent<Rigidbody2D>();
                        
                        Vector2 newPostion = Vector2.MoveTowards(rigidBody2d.position, end, inverseMoveTime * Time.deltaTime);
                        rigidBody2d.MovePosition(newPostion);
                        smoothMoveInProgress.MoveSpaceLeft = (transform.position - end).sqrMagnitude;
                    }
                    else
                    {
                        _smoothMoveInProgressPool.Value.Del(e);
                    }
                }
                else
                {
                    var viewPosition = transform.position;
                    var distance = Vector2.Distance(
                        new Vector2(position.X, position.Y),
                        new Vector2(viewPosition.x, viewPosition.y));
                    if (distance > float.Epsilon)
                    {
                        ref var smoothMoveInProgress = ref _smoothMoveInProgressPool.Value.Add(e);
                        smoothMoveInProgress.MoveSpaceLeft = (transform.position - end).sqrMagnitude;
                    }
                }
            }
        }
    }
}