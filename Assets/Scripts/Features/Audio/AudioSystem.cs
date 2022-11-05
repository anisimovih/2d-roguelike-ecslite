using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Extensions;

namespace Roguelike.Features.Audio
{
    internal sealed class AudioSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<AudioResourcesComponentNew>> _audioFilter2;
        private readonly EcsFilterInject<Inc<AudioPlayEventComponent>> _audioFilterNew;

        private readonly EcsPoolInject<AudioPlayEventComponent> _audioPoolNew = default;
        
        private AudioSource _efxSource;
        private float lowPitchRange = .95f;
        private float highPitchRange = 1.05f;

        public AudioSystem(AudioSource efxSource)
        {
            _efxSource = efxSource;
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _audioFilterNew.Value)
            {
                ref var audioComponent = ref _audioPoolNew.Value.Get(entity);
                var audioClip = audioComponent.Clips.Random();

                if (audioClip != null)
                {
                    Play(audioClip, audioComponent.RandomizePitch);
                }

                // only play once
                _audioPoolNew.Value.GetWorld().DelEntity(entity);
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
