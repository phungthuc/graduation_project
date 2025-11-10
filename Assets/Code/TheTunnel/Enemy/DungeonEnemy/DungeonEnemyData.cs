using TheTunnel.Enemy;
using UnityEngine;

namespace TheTunnel.Enemy
{
    [CreateAssetMenu(fileName = "DungeonEnemyData", menuName = "The Tunnel/Enemy/DungeonEnemy Data", order = 0)]
    public class DungeonEnemyData : ScriptableObject
    {
        public string id;
        public EnemyHealth enemyPrefab;
    }
}
