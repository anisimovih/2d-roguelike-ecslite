using UnityEngine;
using UnityEngine.UI;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Unity.Ugui;

using Roguelike.Features.WorldComponents;
using Roguelike.Scriptables;
using Roguelike.Services;

namespace Roguelike.Features.UI
{
    public class LevelTransitionScreenSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<GameOverComponent>> _gameOverFilter = default;
        private readonly EcsFilterInject<Inc<LevelTransitionEventComponent>> _levelTransitionEventFilter = default;
        private readonly EcsFilterInject<Inc<LevelTransitionDelayComponent>> _levelTransitionDellayFilter = default;

        private readonly EcsCustomInject<LevelService> _levelService = default;
        private readonly EcsCustomInject<Configuration> _configService = default;

        private readonly EcsPoolInject<LevelTransitionDelayComponent> _levelTransitionDelayPool = default;
        private readonly EcsPoolInject<LevelTransitionEventComponent> _levelTransitionEventPool = default;
        
        [EcsUguiNamed (Idents.Ui.LevelTransition)]
        private readonly Text _levelTransition = default;
        [EcsUguiNamed (Idents.Ui.LevelTransitionImage)]
        private readonly Image _levelTransitionImage = default;

        public void Init(IEcsSystems systems)
        {
            var levelTransitionEventPool = _levelTransitionEventPool.Value;
            levelTransitionEventPool.Add(levelTransitionEventPool.GetWorld().NewEntity());
        }
        
        public void Run(IEcsSystems systems)
        {
            if (_gameOverFilter.Value.GetEntitiesCount() > 0)
            {
                EnableScreenWithText($"After {_levelService.Value.Level} days, you starved.");
                return;
            }
            
            var levelTransitionEventFilter = _levelTransitionEventFilter.Value;
            var levelTransitionDelayFilter = _levelTransitionDellayFilter.Value;
            if (levelTransitionEventFilter.GetEntitiesCount() != 0)
            {
                var levelTransitionEntity = levelTransitionEventFilter.GetRawEntities()[0];
                ref var levelTransitionDelay = ref _levelTransitionDelayPool.Value.Add(levelTransitionEntity);
                levelTransitionDelay.SecondsLeft = _configService.Value.LEVEL_TRANSITION_DELAY;
                _levelTransitionEventPool.Value.Del(levelTransitionEntity);
                EnableScreenWithText($"Day {_levelService.Value.Level}");
            }
            else if (levelTransitionDelayFilter.GetEntitiesCount() != 0)
            {
                var levelTransitionEntity = levelTransitionDelayFilter.GetRawEntities()[0];
                var levelTransitionDelayPool = _levelTransitionDelayPool.Value;
                ref var levelTransitionDelayComponent = ref levelTransitionDelayPool.Get(levelTransitionEntity);
                levelTransitionDelayComponent.SecondsLeft -= Time.deltaTime;
                
                if (levelTransitionDelayComponent.SecondsLeft <= 0)
                {
                    levelTransitionDelayPool.Del(levelTransitionEntity);
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