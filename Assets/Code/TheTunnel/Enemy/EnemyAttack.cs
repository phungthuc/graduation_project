using System;
using TheTunnel.Core;
using UnityEngine;

namespace TheTunnel.Enemy
{
    public class EnemyAttack : MonoBehaviour
    {
        [Tooltip("Range of the attack")]
        public float range;
        
        [Tooltip("Damage of the attack")]
        public float damage;
        
        protected virtual void Start() { }
        
        public GameObject FindTargetInRange()
        {
            Collider[] targetsInRange = Physics.OverlapSphere(transform.position, range);
            if (targetsInRange.Length == 0)
            {
                return null;
            }
            for (int i = 0; i < targetsInRange.Length; i++)
            {
                if (targetsInRange[i].gameObject.CompareTag(GameConstant.PLAYER_TAG) || targetsInRange[i].gameObject.CompareTag(GameConstant.CASTLE_TAG))
                {
                    return targetsInRange[i].gameObject;
                }
            }
            return null;
        }

        public virtual void Attack() {}
    }
}