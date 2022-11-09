using Roguelike.External.easyevents;

namespace Roguelike.Features.WorldComponents
{
    internal struct LevelTransitionDelayComponent: IEventSingleton
    {
        public float SecondsLeft;
    }
}