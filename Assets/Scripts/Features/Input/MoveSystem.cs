using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Enums;
using Roguelike.Features.AIMove;
using Roguelike.Features.Animation;
using Roguelike.Features.Audio;
using Roguelike.Features.Components;
using Roguelike.Features.Health;
using Roguelike.Features.Render;
using Roguelike.Features.Turn;
using Roguelike.Features.WorldComponents;
using Roguelike.Scriptables;
using Roguelike.Extensions;
using Roguelike.Services;

namespace Roguelike.Features.Input
{
    public class MoveSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<LevelTransitionDelayComponent>> _levelTransitionDelayFilter = default;
        private readonly EcsFilterInject<Inc<MoveInputEventComponent>> _moveInputFilter = default;
        private readonly EcsFilterInject<Inc<ControllableComponent>> _controllableFilter = default;
        private readonly EcsFilterInject<Inc<GameOverComponent>> _gameOverFilter = default;

        private readonly EcsCustomInject<Configuration> _config = default;
        private readonly EcsCustomInject<GameBoardService> _gameBoardService = default;

        private readonly EcsPoolInject<ActiveTurnBasedComponent> _activeTurnBasedPool = default;
        private readonly EcsPoolInject<MoveInputEventComponent> _moveInputPool = default;
        private readonly EcsPoolInject<PositionComponent> _positionPool = default;
        private readonly EcsPoolInject<PositionChangeEventComponent> _positionChangedPool = default;
        private readonly EcsPoolInject<AIMoveComponent> _aiMovePool = default;
        private readonly EcsPoolInject<HealthComponent> _healthPool = default;
        private readonly EcsPoolInject<HealthChangeEventComponent> _healthChangeEventPool = default;
        private readonly EcsPoolInject<ViewComponent> _viewPool = default;
        private readonly EcsPoolInject<AnimationPlayEventComponent> _animationPlayEventPool = default;

        private readonly EcsPoolInject<AudioPlayEventComponent> _audioPool = default;
        private readonly EcsPoolInject<AudioResourcesComponentNew> _newAudioPool = default;

        private static Vector2 ToVector(Movement movement)
        {
            switch (movement)
            {
                case Movement.UP:
                    return new Vector2(0, 1);
                case Movement.RIGHT:
                    return new Vector2(1, 0);
                case Movement.DOWN:
                    return new Vector2(0, -1);
                case Movement.LEFT:
                default:
                    return new Vector2(-1, 0);
            }
        }

        public void Run(IEcsSystems systems)
        {
            var moveInputCount = _moveInputFilter.Value.GetEntitiesCount();
            if (_gameOverFilter.Value.GetEntitiesCount() > 0
                || _controllableFilter.Value.GetEntitiesCount() == 0
                || _levelTransitionDelayFilter.Value.GetEntitiesCount() > 0
                || moveInputCount == 0)
            {
                // ignore input
                return;
            }

            var moveInput = _moveInputPool.Value.Get(_moveInputFilter.Value.GetRawEntities()[0]);

            foreach (var controllableId in _controllableFilter.Value)
            {
                if (!_activeTurnBasedPool.Value.Has(controllableId))
                {
                    // ignore input
                    continue;
                }

                var healthChangePool = _healthChangeEventPool.Value;
                if (_healthChangeEventPool.Value.Has(controllableId))
                {
                    ref var healthChange = ref healthChangePool.Get(controllableId);
                    healthChange.HealthChangeAmount -= 1;
                }
                else
                {
                    ref var healthChange = ref healthChangePool.Add(controllableId);
                    healthChange.HealthChangeAmount = -1;
                }

                var movement = moveInput.Movement;
                var movementPos = ToVector(movement);

                ref var currentPos = ref _positionPool.Value.Get(controllableId);
                int newX = currentPos.X + (int) movementPos.x;
                int newY = currentPos.Y + (int) movementPos.y;

                (bool canMove, ICollection<int> existing) = _gameBoardService.Value.IsGameBoardPositionOpen(newX, newY);
                if (existing != null)
                {
                    canMove = PrepareMove(controllableId, existing);
                }

                if (canMove)
                {
                    AddAudioNew(controllableId, AudioClipType.FOOTSTEP);
                    
                    _gameBoardService.Value.Grid.Remove(currentPos.X, currentPos.Y, controllableId);
                    currentPos.SetPositions(newX, newY);
                    _gameBoardService.Value.Grid.Add(newX, newY, controllableId);
                    _positionChangedPool.Value.Add(controllableId);
                }

                _activeTurnBasedPool.Value.Del(controllableId);
            }
        }

        private void AddAudio(int controllableId, AudioClipType audioClipType)
        {
            var world = _positionChangedPool.Value.GetWorld();
            var source = _newAudioPool.Value.Get(controllableId);
            ref var audio = ref _audioPool.Value.Add(world.NewEntity());
            audio.Clips = source.TypeToClips[audioClipType];
            audio.RandomizePitch = audio.Clips.Length > 1;
        }

        private void AddAudioNew(int controllableId, AudioClipType audioClipType)
        {
            var world = _positionChangedPool.Value.GetWorld();
            var source = _newAudioPool.Value.Get(controllableId);
            ref var audio = ref _audioPool.Value.Add(world.NewEntity());
            audio.Clips = source.TypeToClips[audioClipType];
            audio.RandomizePitch = audio.Clips.Length > 1;
        }

        private bool PrepareMove(int player, ICollection<int> entitiesInSpot)
        {
            foreach (var id in entitiesInSpot)
            {
                // enemy there, can't do anything
                if (_aiMovePool.Value.Has(id)) return false;
            }

            // handle walls
            foreach (var id in entitiesInSpot)
            {
                // enemy there, can't do anything
                if (_healthPool.Value.Has(id))
                {
                    ref var healthChange = ref _healthChangeEventPool.Value.Add(id);
                    healthChange.HealthChangeAmount = -1;
                    AddAudioNew(player, AudioClipType.CHOP);

                    if (_viewPool.Value.Has(player))
                    {
                        /*_animationPool.Value.Add(player);
                        ref var animationComponent = ref _animationPool.Value.Get(player);
                        animationComponent.Animation = _config.Value.playerConfig.chop;*/
                        ref var animationPlay = ref _animationPlayEventPool.Value.Add(player);
                        animationPlay.TriggerName = _config.Value.playerConfig.chopTriggerName;
                    }
                    
                    // nothing to do now that we've chopped
                    return false;
                }
            }
            
            // otherwise we can move
            return true;
        }
    }
}