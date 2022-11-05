using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Components;
using Roguelike.Features.Input;
using Roguelike.Features.WorldComponents;
using Roguelike.Services;

namespace Roguelike.Features.Exit
{
    public class NextLevelSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<PositionChangeEventComponent, ControllableComponent>> _charPositionChangedFilter = default;
        private readonly EcsFilterInject<Inc<ExitComponent>> _exitFilter = default;

        private readonly EcsCustomInject<LevelService> _levelService = default;

        private readonly EcsPoolInject<PositionComponent> _positionPool = default;
        private readonly EcsPoolInject<LevelTransitionEventComponent> _levelTransitionPool = default;

        public void Run(IEcsSystems systems)
        {
            var positionPool = _positionPool.Value;
            
            foreach (var character in _charPositionChangedFilter.Value)
            {
                ref var charPos = ref positionPool.Get(character);
                foreach (var exit in _exitFilter.Value)
                {
                    ref var exitPos = ref positionPool.Get(exit);
                    if (charPos.Equals(exitPos))
                    {
                        _levelService.Value.Level += 1;
                        _levelTransitionPool.Value.Add(positionPool.GetWorld().NewEntity());
                        return;
                    }
                }
            }
        }
    }
}