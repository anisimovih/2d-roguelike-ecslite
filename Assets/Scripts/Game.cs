using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;
using Leopotam.EcsLite.Unity.Ugui;
using Roguelike.Features.Actions;
using Roguelike.Features.AIMove;
using Roguelike.Features.Animation;
using Roguelike.Features.Audio;
using Roguelike.Features.Components;
using Roguelike.Features.Consumables;
using Roguelike.Features.Exit;
using Roguelike.Features.GameBoard;
using Roguelike.Features.GameState;
using Roguelike.Features.Health;
using Roguelike.Features.Input;
using Roguelike.Features.Render;
using Roguelike.Features.SmoothMove;
using Roguelike.Features.Sprite;
using Roguelike.Features.Turn;
using Roguelike.Features.UI;
using Roguelike.ScenesData;
using Roguelike.Scriptables;
using Roguelike.Services;

namespace Roguelike
{
    [RequireComponent(typeof(SceneData))]
    internal sealed class Game : MonoBehaviour
    {
        [SerializeField] private Configuration _configuration;
        private EcsSystems _systems;

        private SceneData _sceneData;

        private void Awake()
        {
            _sceneData = GetComponent<SceneData>();
        }

        private void Start()
        {
            var world = new EcsWorld();
            _systems = new EcsSystems(world);
            var gbs = new GameBoardService();
            var ls = new LevelService();
            var nvcs = new NestedViewContainerService();
            var tos = new TurnOrderService();
            var vcs = new ViewContainerService();

            _systems
                .Add(new InputSystem())
                .Add(new NextTurnSystem())
                
                .Add(new AIMoveSystem())
                .Add(new MoveSystem()) // After input, NextTurnSystem
                
                .Add(new NextTurnDelaySystem())
                
                .Add(new NextLevelSystem())  // After move
                .Add(new GameBoardSystem())

                .Add(new QueueSystem()) // After input, UpdateGameBoard
                .Add(new FoodSystem())  // Before HealthSystem
                .Add(new HealthSystem())
                .Add(new DeathSystem())
                
                .Add(new GameOverSystem())
                
                .Add(new AudioSystem(_sceneData.AudioSource))
                
                //UI
                .Add(new LevelTransitionScreenSystem())
                .Add(new HealthUISystem())

                // Render
                .Add(new AnimationSystem())
                .Add(new ChangeSpriteSystem())
                .Add(new RemoveViewSystem())
                .Add(new AddViewSystem())
                .Add(new RenderPositionSystem())
                .Add(new SmoothMoveSystem())
                
                .DelHere<MoveInputEventComponent>()
                .DelHere<PositionChangeEventComponent>()
                .DelHere<HealthChangeEventComponent>()
                
                .DelHere<ActionComponent>()
                .DelHere<SpriteChangeEventComponent>()
                
                .AddWorld(new EcsWorld(), Idents.Worlds.Events)
#if UNITY_EDITOR
                .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
                .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem(Idents.Worlds.Events))
#endif
                .Inject(gbs, tos, ls, nvcs, vcs, _configuration)
                .InjectUgui (_sceneData.UguiEmitter, Idents.Worlds.Events)
                .Init();
        }

        private void Update()
        {
            _systems?.Run();
        }

        private void OnDestroy()
        {
            _systems?.Destroy();
            _systems?.GetWorld()?.Destroy();
            _systems = null;
        }
    }
}