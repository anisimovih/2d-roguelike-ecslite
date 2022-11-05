using UnityEngine;
using UnityEngine.Serialization;

namespace Roguelike.Scriptables
{
    [CreateAssetMenu]
    public class FoodConfig : ScriptableObject, ILayoutableConfig
    {
        [FormerlySerializedAs("_prefab")] public GameObject prefab;
        public GameObject Prefab => prefab;
        
        public AudioClip[] consumeSounds;
        public int healPoints;
    }
}