using System.Collections.Generic;
using Roguelike.Features.Audio;

namespace Roguelike.Features.Sprite
{
    internal struct SpriteResourceComponent
    {
        public Dictionary<SpriteType, UnityEngine.Sprite> TypeToSprite;
    }
}