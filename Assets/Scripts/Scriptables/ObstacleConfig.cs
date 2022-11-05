using UnityEngine;
using UnityEngine.Serialization;

namespace Roguelike.Scriptables
{
    [CreateAssetMenu]
    internal sealed class ObstacleConfig : ScriptableObject, ILayoutableConfig
    {
        [FormerlySerializedAs("_prefab")] public GameObject prefab;
        public GameObject Prefab => prefab;
        
        public Sprite damagedImage;
    }
}