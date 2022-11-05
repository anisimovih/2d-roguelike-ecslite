using UnityEngine;

namespace Roguelike.Scriptables
{
    [CreateAssetMenu]
    internal sealed class PlayerConfig : ScriptableObject, ILayoutableConfig
    {
        public GameObject prefab;
        public GameObject Prefab => prefab;
        
        public string chopTriggerName = "playerChop";
        public string damageRecieveTriggerName = "playerHit";
        
        public AudioClip[] footstepSounds;
        public AudioClip[] chopSounds;
        public AudioClip[] dieSounds;
    }
}