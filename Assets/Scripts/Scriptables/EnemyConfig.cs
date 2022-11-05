using UnityEngine;
using UnityEngine.Serialization;

namespace Roguelike.Scriptables
{
    [CreateAssetMenu]
    internal sealed class EnemyConfig : ScriptableObject, ILayoutableConfig
    {
        [FormerlySerializedAs("_prefab")] public GameObject prefab;
        public GameObject Prefab => prefab;

        public int attackDamage;
        
        public string attackTriggerName = "enemyAttack";
        
        public AudioClip[] attackSounds;
        public AudioClip[] footstepSounds;
    }
}