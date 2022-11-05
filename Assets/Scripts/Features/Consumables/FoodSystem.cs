using System.Collections.Generic;
using System.Linq;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Audio;
using Roguelike.Features.Components;
using Roguelike.Features.Health;
using Roguelike.Features.Input;
using Roguelike.Features.Render;
using Roguelike.Extensions;
using Roguelike.Services;

namespace Roguelike.Features.Consumables
{
    public class FoodSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<PositionChangeEventComponent, ControllableComponent>> _charPositionChangedFilter = default;

        private readonly EcsCustomInject<GameBoardService> _gameBoardService = default;

        private readonly EcsPoolInject<PositionComponent> _positionPool = default;
        private readonly EcsPoolInject<FoodComponent> _foodPool = default;
        private readonly EcsPoolInject<ResourceComponentNew> _resourceComponentPool = default;
        private readonly EcsPoolInject<HealthChangeEventComponent> _healthChangedPool = default;
        private readonly EcsPoolInject<AudioPlayEventComponent> _audioPool = default;
        private readonly EcsPoolInject<AudioResourcesComponentNew> _newAudioPool = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var movedEntity in _charPositionChangedFilter.Value)
            {
                ref var position = ref _positionPool.Value.Get(movedEntity);
                (bool canMove, ICollection<int> posEntities) = _gameBoardService.Value.IsGameBoardPositionOpen(position.X, position.Y);
                if (posEntities == null || posEntities.Empty()) continue;

                var foodPool = _foodPool.Value;
                int foodEntity = posEntities.FirstOrDefault(entity => foodPool.Has(entity));
                if (foodEntity == default) continue;
                
                var world = _audioPool.Value.GetWorld();
                var newAudio = _newAudioPool.Value.Get(foodEntity);
                ref var audio = ref _audioPool.Value.Add(world.NewEntity());
                audio.Clips = newAudio.TypeToClips[AudioClipType.FOOD];
                audio.RandomizePitch = true;
                
                var foodHeal = foodPool.Get(foodEntity).Points;
                if (_healthChangedPool.Value.Has(movedEntity))
                {
                    ref var healthChanged = ref _healthChangedPool.Value.Get(movedEntity);
                    healthChanged.HealthChangeAmount += foodHeal;
                }
                else
                {
                    ref var healthChanged = ref _healthChangedPool.Value.Add(movedEntity);
                    healthChanged.HealthChangeAmount = foodHeal;
                }
                
                _resourceComponentPool.Value.Del(foodEntity);
                _gameBoardService.Value.Grid.Remove(position.X, position.Y, foodEntity);
            }
        }
    }
}