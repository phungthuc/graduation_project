using UnityEngine;

namespace TheTunnel.GOAP
{
    [CreateAssetMenu(menuName = "AI/Attack Config", fileName = "Attack Config", order = 1)]
    public class AttackConfigSO : ScriptableObject
    {
        public float sensorRadius = 10f;
        public float attackRange = 1f;
        public int attackCost = 1;
        public float attackDelay = 1f;
        public LayerMask targetLayer;
    }
}