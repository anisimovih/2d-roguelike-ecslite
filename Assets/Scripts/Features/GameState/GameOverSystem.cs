using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Audio;
using Roguelike.Features.Health;
using Roguelike.Features.Input;
using Roguelike.Features.WorldComponents;

namespace Roguelike.Features.GameState
{
    internal sealed class GameOverSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<GameOverComponent>> _gameOverFilter;
        private readonly EcsFilterInject<Inc<ControllableComponent, HealthChangeEventComponent>> _characterHealthChangedFilter;

        private readonly EcsPoolInject<HealthComponent> _healthPool = default;
        private readonly EcsPoolInject<GameOverComponent> _gameOverPool = default;
        private readonly EcsPoolInject<AudioPlayEventComponent> _audioPool = default;
        private readonly EcsPoolInject<AudioResourcesComponentNew> _newAudioPool = default;

        public void Run(IEcsSystems systems)
        {
            if (_gameOverFilter.Value.GetEntitiesCount() > 0) return;

            var characterHealthChangedFilter = _characterHealthChangedFilter.Value;
            if (characterHealthChangedFilter.GetEntitiesCount() == 0) return;

            foreach (var character in characterHealthChangedFilter)
            {
                ref var healthPool = ref _healthPool.Value.Get(character);
                if (healthPool.CurrentHealth <= 0)
                {
                    _gameOverPool.Value.Add(_healthPool.Value.GetWorld().NewEntity());
                    AddAudio(character, AudioClipType.DIE);
                    return;
                }
            }
        }

        private void AddAudio(int controllableId, AudioClipType audioClipType)
        {
            var world = _gameOverPool.Value.GetWorld();
            var source = _newAudioPool.Value.Get(controllableId);
            ref var audio = ref _audioPool.Value.Add(world.NewEntity());
            audio.Clips = source.TypeToClips[audioClipType];
            audio.RandomizePitch = audio.Clips.Length > 1;
        }
    }
}
