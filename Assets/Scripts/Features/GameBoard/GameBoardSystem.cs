using System;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.AIMove;
using Roguelike.Features.Animation;
using Roguelike.Features.Audio;
using Roguelike.Features.Components;
using Roguelike.Features.Consumables;
using Roguelike.Features.Exit;
using Roguelike.Features.Health;
using Roguelike.Features.Input;
using Roguelike.Features.Render;
using Roguelike.Features.SmoothMove;
using Roguelike.Features.Sprite;
using Roguelike.Features.Stats;
using Roguelike.Features.Turn;
using Roguelike.Features.WorldComponents;
using Roguelike.Scriptables;
using Roguelike.Extensions;
using Roguelike.Services;

namespace Roguelike.Features.GameBoard
{
    internal sealed class GameBoardSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<LevelTransitionEventComponent>> _levelTransitionEventFilter = default;
        private readonly EcsFilterInject<Inc<DeleteOnExitComponent>> _deleteOneExitFilter = default;

        private readonly EcsCustomInject<Configuration> _configuration = default;
        private readonly EcsCustomInject<GameBoardService> _gameBoardService = default;
        private readonly EcsCustomInject<LevelService> _levelService = default;

        private readonly EcsPoolInject<DeleteOnExitComponent> _deleteOneExitPool = default;
        private readonly EcsPoolInject<PositionComponent> _positionPool = default;
        private readonly EcsPoolInject<ResourceComponentNew> _resourcePool = default;
        private readonly EcsPoolInject<NestedViewComponent> _nestedViewPool = default;
        private readonly EcsPoolInject<GameBoardElementComponent> _gameBoardElementPool = default;
        private readonly EcsPoolInject<FoodComponent> _foodPool = default;
        private readonly EcsPoolInject<TurnBasedComponent> _turnBasedPool = default;
        private readonly EcsPoolInject<AIMoveComponent> _aiMovePool = default;
        private readonly EcsPoolInject<DamageComponent> _foodDamagerPool = default;
        private readonly EcsPoolInject<SmoothMoveComponent> _smoothMovePool = default;
        private readonly EcsPoolInject<ExitComponent> _exitPool = default;
        private readonly EcsPoolInject<ControllableComponent> _controllablePool = default;
        private readonly EcsPoolInject<HealthComponent> _healthPool = default;
        private readonly EcsPoolInject<AudioResourcesComponentNew> _audioResourcePool = default;
        private readonly EcsPoolInject<AnimationKeysComponent> _animationResourcePool = default;
        private readonly EcsPoolInject<SpriteResourceComponent> _spriteResourcePool = default;
        
        private EcsWorld _world;
        private IList<Vector2> _gridPositions;
        
        public void Init(IEcsSystems systems)
        {
            _levelService.Value.Level = 1;
            _world = _deleteOneExitPool.Value.GetWorld();
            SetupScene(_levelService.Value.Level);
        }

        public void Run(IEcsSystems systems)
        {
            if (_levelTransitionEventFilter.Value.GetEntitiesCount() == 0) return;

            // delete previous elements
            foreach (var entity in _deleteOneExitFilter.Value)
            {
                if (_resourcePool.Value.Has(entity))
                    _resourcePool.Value.Del(entity);
                if (_turnBasedPool.Value.Has(entity))
                    _turnBasedPool.Value.Del(entity);
            }
            
            SetupScene(_levelService.Value.Level);
        }

        private void SetupScene(int level)
        {
            var config = _configuration.Value;
            var gameBoard = _gameBoardService.Value;
            gameBoard.ReplaceGameBoard(config.gridColumnsRows.x, config.gridColumnsRows.y);
            ResetGridPositions();
            
            BoardSetup(config);
            var player = CreatePlayer(config);
            CreateExit(config);
            CreateDestructibleWalls(config);
            CreateFood(config);
            CreateEnemies(config, level, player);
        }

        private void BoardSetup(Configuration config)
        {
            var gameBoard = _gameBoardService.Value;
            
            // start at negative 1 to place outer edges
            for (int x = -1; x <= gameBoard.Columns; x++)
            {
                for (int y = -1; y <= gameBoard.Rows; y++)
                {
                    bool edge = x == -1 || x == gameBoard.Columns ||
                                y == -1 || y == gameBoard.Rows;
                    GameObject prefab = edge ? config.outerWalls.Random() : config.floors.Random();
                    var boardSpace = _world.NewEntity();
                    ref var positionComponent = ref _positionPool.Value.Add(boardSpace);
                    positionComponent.SetPositions(x, y);
                    
                    ref var resourceComponent = ref _resourcePool.Value.Add(boardSpace);
                    resourceComponent.Prefab = prefab;
                    
                    _deleteOneExitPool.Value.Add(boardSpace);
                    
                    ref var nestedViewComponent = ref _nestedViewPool.Value.Add(boardSpace);
                    nestedViewComponent.Name = "Board";
                }
            }
        }

        private void ResetGridPositions()
        {
            var gameBoard = _gameBoardService.Value;
            _gridPositions = new List<Vector2>();

            // start at 1 to avoid placing items along the edges
            for (int x = 1; x < gameBoard.Columns - 1; x++)
            {
                for (int y = 1; y < gameBoard.Rows - 1; y++)
                {
                    _gridPositions.Add(new Vector2(x, y));
                }
            }
        }

        private void LayoutObjectAtRandom(ILayoutableConfig[] tileArray, int min, int max,
            Action<int, int, int> postProcess)
        {
            int objectCount = UnityEngine.Random.Range(min, max + 1);

            for (int i = 0; i < objectCount; i++)
            {
                int randomIndex = _gridPositions.RandomIndex();
                var randomPosition = _gridPositions[randomIndex];
                //Remove the entry at randomIndex from the list so that it can't be re-used.
                _gridPositions.RemoveAt(randomIndex);

                var randomTileIndex = tileArray.RandomIndex();
                var tileChoice = tileArray[randomTileIndex];
                var tile = _world.NewEntity();

                _gameBoardElementPool.Value.Add(tile);
                _deleteOneExitPool.Value.Add(tile);
                
                ref var resourceComponent = ref _resourcePool.Value.Add(tile);
                resourceComponent.Prefab = tileChoice.Prefab;
                
                ref var positionComponent = ref  _positionPool.Value.Add(tile);
                positionComponent.SetPositions((int)randomPosition.x, (int)randomPosition.y);
                
                _gameBoardService.Value.Grid.Add((int)randomPosition.x, (int)randomPosition.y, tile);

                postProcess(tile, i, randomTileIndex);
            }
        }

        private int CreateExit(Configuration config)
        {
            var gameBoard = _gameBoardService.Value;
            var exit = _world.NewEntity();
            ref var resourceComponent = ref _resourcePool.Value.Add(exit);
            resourceComponent.Prefab = config.exit;

            _exitPool.Value.Add(exit);
            _gameBoardElementPool.Value.Add(exit);
            _deleteOneExitPool.Value.Add(exit);
            
            ref var positionComponent = ref _positionPool.Value.Add(exit);
            positionComponent.X = gameBoard.Columns - 1;
            positionComponent.Y = gameBoard.Rows - 1;
            return exit;
        }

        private int CreatePlayer(Configuration config)
        {
            var gameBoard = _gameBoardService.Value;
            var configuration = _configuration.Value;
            var player = _world.NewEntity();

            ref var playerResourceComponent = ref _resourcePool.Value.Add(player);
            playerResourceComponent.Prefab = config.playerConfig.Prefab;
            
            _gameBoardElementPool.Value.Add(player);
            _deleteOneExitPool.Value.Add(player);
            ref var position = ref _positionPool.Value.Add(player);
            gameBoard.Grid.Add(position.X, position.Y, player);
            
            ref var smoothMoveComponent = ref _smoothMovePool.Value.Add(player);
            smoothMoveComponent.MoveTime = config.TURN_DELAY;

            _controllablePool.Value.Add(player);
            
            ref var turnBasedComponent = ref _turnBasedPool.Value.Add(player);
            turnBasedComponent.delay = config.TURN_DELAY;

            ref var healthPool = ref _healthPool.Value.Add(player);
            healthPool.SetupHealth(configuration.START_FOOD, configuration.START_FOOD);
            
            ref var animation = ref _animationResourcePool.Value.Add(player);
            animation.TypeToKey = new Dictionary<AnimationType, string>();
            animation.TypeToKey.Add(AnimationType.RECEIVE_DAMAGE, config.playerConfig.damageRecieveTriggerName);
            
            ref var audio = ref _audioResourcePool.Value.Add(player);
            audio.TypeToClips = new Dictionary<AudioClipType, AudioClip[]>();
            audio.TypeToClips.Add(AudioClipType.FOOTSTEP, _configuration.Value.playerConfig.footstepSounds);
            audio.TypeToClips.Add(AudioClipType.CHOP, _configuration.Value.playerConfig.chopSounds);
            audio.TypeToClips.Add(AudioClipType.DIE, _configuration.Value.playerConfig.dieSounds);

            return player;
        }

        private void CreateDestructibleWalls(Configuration config)
        {
            var obstacles = new GameObject[config.obstacleConfigs.Length];
            for (var index = 0; index < config.obstacleConfigs.Length; index++)
            {
                obstacles[index] = config.obstacleConfigs[index].Prefab;
            }

            LayoutObjectAtRandom(config.obstacleConfigs, config.obstacleCountMinMax.x, config.obstacleCountMinMax.y, (e, _, configIndex) =>
            {
                var obstacleConfig = config.obstacleConfigs[configIndex];
                ref var healthPool = ref _healthPool.Value.Add(e);
                healthPool.SetupHealth(2, 2);
                
                ref var sprite = ref _spriteResourcePool.Value.Add(e);
                sprite.TypeToSprite = new Dictionary<SpriteType, UnityEngine.Sprite>();
                sprite.TypeToSprite.Add(SpriteType.DAMAGED, obstacleConfig.damagedImage);
            });
        }

        private void CreateFood(Configuration config)
        {
            LayoutObjectAtRandom(config.foodConfigsNew, config.foodCountMinMax.x, config.foodCountMinMax.y, (e, _, configIndex) =>
            {
                var foodConfig = config.foodConfigsNew[configIndex];
                ref var foodComponent = ref _foodPool.Value.Add(e);
                foodComponent.Points = foodConfig.healPoints;
                ref var audio = ref _audioResourcePool.Value.Add(e);
                audio.TypeToClips = new Dictionary<AudioClipType, AudioClip[]>();
                audio.TypeToClips.Add(AudioClipType.FOOD, foodConfig.consumeSounds);
            });
        }

        private void CreateEnemies(Configuration config, int level, int player)
        {
            int enemyCount = Mathf.FloorToInt(Mathf.Log(level, 2f)) * config.ENEMY_COUNT_MULTIPLIER;
            LayoutObjectAtRandom(config.enemyConfigs, enemyCount, enemyCount, (e, i, configIndex) =>
            {
                var enemyConfig = config.enemyConfigs[configIndex];

                // start at 1 because 0 is reserved for player
                ref var turnBasedComponent = ref _turnBasedPool.Value.Add(e);
                turnBasedComponent.Index = i + 1;
                turnBasedComponent.delay = _configuration.Value.TURN_DELAY;

                ref var aiMove = ref _aiMovePool.Value.Add(e);
                aiMove.Target = player;

                ref var foodDamagerComponent = ref _foodDamagerPool.Value.Add(e);
                foodDamagerComponent.Points = enemyConfig.attackDamage;

                ref var smoothMoveComponent = ref _smoothMovePool.Value.Add(e);
                smoothMoveComponent.MoveTime = config.TURN_DELAY;

                ref var animation = ref _animationResourcePool.Value.Add(e);
                animation.TypeToKey = new Dictionary<AnimationType, string>();
                animation.TypeToKey.Add(AnimationType.ATTACK, enemyConfig.attackTriggerName);

                ref var audio = ref _audioResourcePool.Value.Add(e);
                audio.TypeToClips = new Dictionary<AudioClipType, AudioClip[]>();
                audio.TypeToClips.Add(AudioClipType.ENEMY, enemyConfig.attackSounds);
            });
        }
    }
}
