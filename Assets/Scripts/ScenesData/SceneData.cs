using UnityEngine;

using Leopotam.EcsLite.Unity.Ugui;

namespace Roguelike.ScenesData
{
    public class SceneData : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        public AudioSource AudioSource => _audioSource;
        
        [SerializeField] private EcsUguiEmitter _uguiEmitter;
        public EcsUguiEmitter UguiEmitter => _uguiEmitter;
    }
}