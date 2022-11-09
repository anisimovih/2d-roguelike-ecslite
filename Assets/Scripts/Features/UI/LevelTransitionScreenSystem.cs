using UnityEngine;
using UnityEngine.UI;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Unity.Ugui;
using Roguelike.External.easyevents;
using Roguelike.Features.WorldComponents;
using Roguelike.Scriptables;
using Roguelike.Services;

namespace Roguelike.Features.UI
{
    public class LevelTransitionScreenSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly EcsCustomInject<EventsBus> _eventsBus = default;
        private readonly EcsCustomInject<LevelService> _levelService = default;
        private readonly EcsCustomInject<Configuration> _configService = default;
        
        [EcsUguiNamed (Idents.Ui.LevelTransition)]
        private readonly Text _levelTransition = default;
        [EcsUguiNamed (Idents.Ui.LevelTransitionImage)]
        private readonly Image _levelTransitionImage = default;

        public void Init(IEcsSystems systems)
        {
            _eventsBus.Value.NewEventSingleton<LevelTransitionDelayComponent>();
        }
        
        public void Run(IEcsSystems systems)
        {
            var eventsBus = _eventsBus.Value;
            
            if (eventsBus.HasEventSingleton<GameOverComponent>())
            {
                EnableScreenWithText($"After {_levelService.Value.Level} days, you starved.");
                return;
            }
            
            if (eventsBus.HasEventSingleton<LevelTransitionEventComponent>())
            {
                eventsBus.NewEventSingleton<LevelTransitionDelayComponent>().SecondsLeft = _configService.Value.LEVEL_TRANSITION_DELAY;;
                eventsBus.DestroyEventSingleton<LevelTransitionEventComponent>();
                EnableScreenWithText($"Day {_levelService.Value.Level}");
            }
            else if (eventsBus.HasEventSingleton<LevelTransitionDelayComponent>())
            {
                ref var levelTransitionDelay = ref eventsBus.GetEventBodySingleton<LevelTransitionDelayComponent>();
                levelTransitionDelay.SecondsLeft -= Time.deltaTime;
                if (levelTransitionDelay.SecondsLeft <= 0)
                {
                    eventsBus.DestroyEventSingleton<LevelTransitionDelayComponent>();
                    DisableScreen();
                }
            }
        }

        private void EnableScreenWithText(string text)
        {
            _levelTransition.text = text;
            _levelTransition.enabled = true;
            _levelTransitionImage.enabled = true;
        }
        
        private void DisableScreen()
        {
            _levelTransition.enabled = false;
            _levelTransitionImage.enabled = false;
        }
    }
}