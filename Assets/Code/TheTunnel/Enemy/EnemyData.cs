using UnityEngine;

namespace TheTunnel.Enemy
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "The Tunnel/Enemy/Enemy Data", order = 0)]
    public class EnemyData : ScriptableObject
    {
        public string id;
        public EnemyStat stat;
        public EnemyBase enemyPrefab;
    }
}