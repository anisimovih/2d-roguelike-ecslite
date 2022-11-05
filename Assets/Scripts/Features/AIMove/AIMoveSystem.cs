using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Animation;
using Roguelike.Features.Audio;
using Roguelike.Features.Components;
using Roguelike.Features.Consumables;
using Roguelike.Features.Health;
using Roguelike.Features.Input;
using Roguelike.Features.Stats;
using Roguelike.Features.Turn;
using Roguelike.Extensions;
using Roguelike.Scriptables;
using Roguelike.Services;

namespace Roguelike.Features.AIMove
{
    internal sealed class AIMoveSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<
            Inc<AIMoveComponent, ActiveTurnBasedComponent, TurnBasedComponent, PositionComponent>> 
            _aiMove = default;
        
        private readonly EcsCustomInject<Configuration> _config = default;

        private readonly EcsPoolInject<AIMoveComponent> _aiMovePool = default;
        private readonly EcsPoolInject<PositionComponent> _positionPool = default;
        private readonly EcsPoolInject<SkipTurnComponent> _skipTurnPool = default;
        private readonly EcsPoolInject<ActiveTurnBasedComponent> _activeTurnBasedPool = default;
        private readonly EcsPoolInject<ControllableComponent> _controllablePool = default;
        private readonly EcsPoolInject<HealthChangeEventComponent> _healthChangePool = default;
        private readonly EcsPoolInject<FoodComponent> _foodPool = default;
        private readonly EcsPoolInject<DamageComponent> _damagePool = default;
        private readonly EcsPoolInject<AnimationPlayEventComponent> _animationPool = default;
        private readonly EcsPoolInject<AudioPlayEventComponent> _audioPool = default;
        private readonly EcsPoolInject<AudioResourcesComponentNew> _newAudioPool = default;
        private readonly EcsPoolInject<AnimationKeysComponent> _animationResourcePool = default;

        private readonly EcsCustomInject<GameBoardService> _gameBoardService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var movingEntity in _aiMove.Value)
            {
                ref var AIMove = ref _aiMovePool.Value.Get(movingEntity);

                var targetPos = _positionPool.Value.Get(AIMove.Target);
                ref var currentPos = ref _positionPool.Value.Get(movingEntity);

                var moveX = 0;
                var moveY = 0;
                
                bool moveYish = Mathf.Abs(targetPos.X - currentPos.X) == 0;
                if (moveYish)
                {
                    moveY = targetPos.Y > currentPos.Y ? 1 : -1;
                }
                else
                {
                    moveX = targetPos.X > currentPos.X ? 1 : -1;
                }

                int newX = currentPos.X + moveX;
                int newY = currentPos.Y + moveY;
                
                (bool canMove, ICollection<int> existing) = _gameBoardService.Value.IsGameBoardPositionOpen(newX, newY);
                if (existing != null && !existing.Empty())
                {
                    canMove = PrepareMove(movingEntity, existing);
                }

                if (canMove)
                {
                    _gameBoardService.Value.Grid.Remove(currentPos.X, currentPos.Y, movingEntity);
                    currentPos.X = newX;
                    currentPos.Y = newY;
                    _gameBoardService.Value.Grid.Add(currentPos.X, currentPos.Y, movingEntity);
                }

                // skip next turn
                _skipTurnPool.Value.Add(movingEntity);
                _activeTurnBasedPool.Value.Del(movingEntity);
            }
        }

        private bool PrepareMove(int enemy, ICollection<int> entitiesInSpot)
        {
            bool playerFound = false;
            int player = 0;
            foreach (var entity in entitiesInSpot)
            {
                if (!_controllablePool.Value.Has(entity)) continue;

                playerFound = true;
                player = entity;
                break;
            }
            
            if (playerFound)
            {
                if (_healthChangePool.Value.Has(player))
                {
                    ref var healthChange = ref _healthChangePool.Value.Get(player);
                    healthChange.HealthChangeAmount -= _damagePool.Value.Get(enemy).Points;
                }
                else
                {
                    ref var healthChange = ref _healthChangePool.Value.Add(player);
                    healthChange.HealthChangeAmount = -_damagePool.Value.Get(enemy).Points;
                }

                TryAddAnimationTrigger(enemy, AnimationType.ATTACK);
                TryAddAnimationTrigger(player, AnimationType.RECEIVE_DAMAGE);
                TryAddAudio(enemy, AudioClipType.ENEMY);
                return false;
            }

            // Consumables do not block the cell
            return entitiesInSpot.Count == 1 && _foodPool.Value.Has(entitiesInSpot.First());
        }

        private void TryAddAudio(int controllableId, AudioClipType audioClipType)
        {
            var world = _audioPool.Value.GetWorld();
            var source = _newAudioPool.Value.Get(controllableId);
            source.TypeToClips.TryGetValue(audioClipType, out var clips);
            if (clips == null)
            {
                Debug.LogWarning($"entity {controllableId} has no {audioClipType} audio resource");
                return;
            }
            ref var audio = ref _audioPool.Value.Add(world.NewEntity());
            audio.Clips = clips;
            audio.RandomizePitch = clips.Length > 1;
        }

        private void TryAddAnimationTrigger(int controllableId, AnimationType animationType)
        {
            ref var animationResource = ref _animationResourcePool.Value.Get(controllableId);
            animationResource.TypeToKey.TryGetValue(animationType, out var animation);
            if (animation == null)
            {
                Debug.LogWarning($"entity {controllableId} has no {animationType} animation key");
                return;
            }
            ref var playerAnimation = ref _animationPool.Value.Add(controllableId);
            playerAnimation.TriggerName = animation;
        }
    }
}
