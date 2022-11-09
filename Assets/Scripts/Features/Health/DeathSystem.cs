using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Components;
using Roguelike.Features.Render;
using Roguelike.Extensions;
using Roguelike.Services;

namespace Roguelike.Features.Health
{
    public class DeathSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<HealthChangeEventComponent>> _healthChangedEventFilter = default;

        private readonly EcsCustomInject<GameBoardService> _gameBoardService = default;

        private readonly EcsPoolInject<HealthComponent> _healthPool = default;
        private readonly EcsPoolInject<ResourceComponentNew> _resourcePool = default;
        private readonly EcsPoolInject<PositionComponent> _positionPool = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var e in _healthChangedEventFilter.Value)
            {
                if (_healthPool.Value.Get(e).CurrentHealth > 0) continue;

                if (_positionPool.Value.Has(e))
                {
                    ref var positionPool = ref _positionPool.Value.Get(e);
                    _gameBoardService.Value.Grid.Remove(positionPool.X, positionPool.Y, e);
                }
                
                var resourcePool = _resourcePool.Value;
                if (resourcePool.Has(e))
                    resourcePool.Del(e);
            }
        }
    }
}