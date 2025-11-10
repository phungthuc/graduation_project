using UnityEngine;
using UnityEngine.Serialization;

namespace TheTunnel.Enemy
{
    [System.Serializable]
    public class EnemyStat
    {
        public float speed;
        public float health;
        public float attackDamage;
        public float attackRange;
    }
}