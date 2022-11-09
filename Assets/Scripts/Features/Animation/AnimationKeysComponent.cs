using System.Collections.Generic;
using Roguelike.Features.Actions;

namespace Roguelike.Features.Animation
{
    internal struct AnimationKeysComponent
    {
        public Dictionary<GameAction, string> TypeToKey;
    }
}
