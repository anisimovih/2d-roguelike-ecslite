using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Components;
using Roguelike.Features.SmoothMove;

namespace Roguelike.Features.Render
{
    public class RenderPositionSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<PositionComponent, ViewComponent>, Exc<SmoothMoveComponent>> _instantMovableFilter = default;

        private readonly EcsPoolInject<PositionComponent> _positionPool = default;
        private readonly EcsPoolInject<ViewComponent> _viewPool = default;
        
        public void Run(IEcsSystems systems)
        {
            foreach (var e in _instantMovableFilter.Value)
            {
                var pos = _positionPool.Value.Get(e);
                ref var view = ref _viewPool.Value.Get(e);
                view.GameObject.transform.position = new Vector3(pos.X, pos.Y, 0f);
            }
        }
    }
}