using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Actions;
using Roguelike.Features.Render;

namespace Roguelike.Features.Animation
{
    public class AnimationSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<ActionComponent, ViewComponent, AnimationKeysComponent>> _animationViewActionFilter = default;

        private readonly EcsPoolInject<ActionComponent> _actionPool = default;
        private readonly EcsPoolInject<AnimationKeysComponent> _animationKeysPool = default;
        private readonly EcsPoolInject<ViewComponent> _viewPool = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var e in _animationViewActionFilter.Value)
            {
                var animator = _viewPool.Value.Get(e).GameObject.GetComponent<Animator>();
                if (animator == null) continue;

                var action = _actionPool.Value.Get(e).GameAction;
                _animationKeysPool.Value.Get(e).TypeToKey.TryGetValue(action, out var animationKey );
                if (animationKey == null) continue;
                
                animator.SetTrigger(animationKey); // we are using triggers only so we can assume SetTrigger
            }
        }
    }
}