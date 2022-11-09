using System.Collections.Generic;
using UnityEngine;
using Roguelike.Features.Actions;

namespace Roguelike.Features.Audio
{
    public struct AudioResourcesComponentNew
    {
        public Dictionary<GameAction, AudioClip[]> TypeToClips;
    }
}
