using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Roguelike.Features.Render
{
    public class RemoveViewSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<ViewComponent>, Exc<ResourceComponentNew>> _viewWithoutResourceFilter = default;
        private readonly EcsPoolInject<ViewComponent> _viewPool = default;

        public void Run(IEcsSystems systems)
        {
            var viewPool = _viewPool.Value;
            var world = viewPool.GetWorld();
            foreach (var e in _viewWithoutResourceFilter.Value)
            {
                ref var view = ref viewPool.Get(e);
                Object.Destroy(view.GameObject);
                world.DelEntity(e);
            }
        }
    }
}