using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Render;

namespace Roguelike.Features.Animation
{
    public class AnimationSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<AnimationPlayEventComponent, ViewComponent>> _animationViewFilter = default;

        private readonly EcsPoolInject<AnimationPlayEventComponent> _animationPlayEventPool = default;
        private readonly EcsPoolInject<ViewComponent> _viewPool = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var e in _animationViewFilter.Value)
            {
                var animator = _viewPool.Value.Get(e).GameObject.GetComponent<Animator>();
                if (animator == null) continue;
                
                var animation = _animationPlayEventPool.Value.Get(e).TriggerName;
                animator.SetTrigger(animation); // we are using triggers only so we can assume SetTrigger
            }
        }
    }
}