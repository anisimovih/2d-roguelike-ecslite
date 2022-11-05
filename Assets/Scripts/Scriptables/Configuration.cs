using UnityEngine;

namespace Roguelike.Scriptables
{
    [CreateAssetMenu]
    internal sealed class Configuration : ScriptableObject
    {
        public Vector2Int gridColumnsRows = new(8, 8);
        public Vector2Int foodCountMinMax = new(1, 5);
        public Vector2Int obstacleCountMinMax = new(5, 9);
        public int ENEMY_COUNT_MULTIPLIER = 1;
        public float TURN_DELAY = 1f;
        public float LEVEL_TRANSITION_DELAY = 1.5f;
        public int START_FOOD = 100;
        public int FOOD_LOST_PER_TURN = 1;
        
        public GameObject exit;
        public GameObject[] floors;
        public GameObject[] outerWalls;
        public ObstacleConfig[] obstacleConfigs;

        public PlayerConfig playerConfig;
        public EnemyConfig[] enemyConfigs;
        public FoodConfig[] foodConfigsNew;
    }
}