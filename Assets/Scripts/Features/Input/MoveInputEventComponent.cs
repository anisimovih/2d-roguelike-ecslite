using Roguelike.Enums;
using Roguelike.External.easyevents;

namespace Roguelike.Features.Input
{
    internal struct MoveInputEventComponent: IEventSingleton
    {
        public Movement Movement;
    }
}