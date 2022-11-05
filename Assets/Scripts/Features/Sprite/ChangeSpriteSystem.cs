using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Render;

namespace Roguelike.Features.Sprite
{
    public class ChangeSpriteSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<SpriteChangeEventComponent, ViewComponent>> _changeSpriteViewFilter = default;

        private readonly EcsPoolInject<SpriteChangeEventComponent> _changeSpritePool = default;
        private readonly EcsPoolInject<ViewComponent> _viewPool = default;

        public void Run(IEcsSystems systems)
        {
            var changeSprite = _changeSpritePool.Value;
            var viewPool = _viewPool.Value;
            
            foreach (var e in _changeSpriteViewFilter.Value)
            {
                var gameObject = viewPool.Get(e).GameObject;
                var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                
                spriteRenderer.sprite = changeSprite.Get(e).sprite;
            }
        }
    }
}