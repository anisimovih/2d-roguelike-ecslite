using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Extensions;
using Roguelike.Features.Actions;

namespace Roguelike.Features.Audio
{
    internal sealed class AudioSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<ActionComponent, AudioResourcesComponentNew>> _audioResourceActionFilter = default;

        private readonly EcsPoolInject<ActionComponent> _actionPool = default;
        private readonly EcsPoolInject<AudioResourcesComponentNew> _audioResourcePool = default;
        
        private AudioSource _efxSource;
        private float lowPitchRange = .95f;
        private float highPitchRange = 1.05f;

        public AudioSystem(AudioSource efxSource)
        {
            _efxSource = efxSource;
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _audioResourceActionFilter.Value)
            {
                var action = _actionPool.Value.Get(entity).GameAction;
                _audioResourcePool.Value.Get(entity).TypeToClips.TryGetValue(action, out var audioClips );
                if (audioClips == null || audioClips.Length == 0) continue;
                Play(audioClips.Random(), true); // ToDo
            }
        }

        private void Play(AudioClip clip, bool randomize = false)
        {
            _efxSource.clip = clip;

            if (!randomize)
            {
                _efxSource.Play();
                return;
            }

            var originalPitch = _efxSource.pitch;
            _efxSource.pitch = Random.Range(lowPitchRange, highPitchRange);
            _efxSource.Play();
            _efxSource.pitch = originalPitch;
        }
    }
}
