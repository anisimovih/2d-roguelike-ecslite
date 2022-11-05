using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Components;
using Roguelike.Services;

namespace Roguelike.Features.Render
{
    public class AddViewSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<ResourceComponentNew>, Exc<ViewComponent>> _resourcesFilter = default;

        private readonly EcsCustomInject<ViewContainerService> _viewContainerService = default;
        private readonly EcsCustomInject<NestedViewContainerService> _nestedViewContainerService = default;

        private readonly EcsPoolInject<PositionComponent> _positionPool = default;
        private readonly EcsPoolInject<ViewComponent> _viewPool = default;
        private readonly EcsPoolInject<ResourceComponentNew> _resourcePool = default;
        private readonly EcsPoolInject<NestedViewComponent> _nestedViewPool = default;

        public void Init(IEcsSystems systems)
        {
            _viewContainerService.Value.Transform = new GameObject("Views").transform;
            _nestedViewContainerService.Value.View = new Dictionary<string, Transform>();
        }
        
        public void Run(IEcsSystems systems)
        {
            var viewContainer = _viewContainerService.Value.Transform;
            var nestedViewContainer = _nestedViewContainerService.Value.View;

            foreach (var e in _resourcesFilter.Value)
            {
                ref var resource = ref _resourcePool.Value.Get(e);
                var gameObject = Object.Instantiate(resource.Prefab);

                var parent = _nestedViewPool.Value.Has(e) ?
                    GetNested(_nestedViewPool.Value.Get(e).Name, viewContainer, nestedViewContainer) :
                    viewContainer;
                gameObject.transform.SetParent(parent, false);
                ref var view = ref _viewPool.Value.Add(e);
                view.GameObject = gameObject;

                if (_positionPool.Value.Has(e))
                {
                    ref var pos = ref _positionPool.Value.Get(e);
                    gameObject.transform.position = new Vector3(pos.X, pos.Y, 0f);
                }
            }
        }

        private static Transform GetNested(string name, Transform viewContainer, IDictionary<string, Transform> nestedViewContainer)
        {
            if (nestedViewContainer.ContainsKey(name))
            {
                return nestedViewContainer[name];
            }

            var nestedView = new GameObject(name).transform;
            nestedView.SetParent(viewContainer, false);
            nestedViewContainer[name] = nestedView;
            return nestedView;
        }
    }
}