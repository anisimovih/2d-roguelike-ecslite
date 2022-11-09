using Roguelike.External.easyevents;

namespace Roguelike.Features.WorldComponents
{
    internal struct NextTurnDelayComponent: IEventSingleton
    {
        public float SecondsLeft;
    }
}
