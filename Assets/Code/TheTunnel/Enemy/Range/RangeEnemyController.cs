using cowsins;
using TheTunnel.Enemy;
using TheTunnel.Target;
using UnityEngine;

namespace TheTunnel.Enemy
{
    public class RangeEnemyController : EnemyBase
    {
        [SerializeField] private RangeEnemyAttack attack;
        [SerializeField] private EnemyHealth health;
        [SerializeField] private EnemyMovement movement;

        private void Start()
        {
            health.events.OnDeath.RemoveListener(OnDied);
            health.events.OnDeath.AddListener(OnDied);
        }

        private void FixedUpdate()
        {
            if (isPaused)
            {
                return;
            }
            var target = attack.FindTargetInRange();
            if (target == null)
            {
                movement.SetMoving(true);
                return;
            }
            attack.targetTransform = target.transform;
            HandleAttack();
        }

        public override void SetPaused(bool paused)
        {
            base.SetPaused(paused);
            movement.SetMoving(!paused);
        }

        public override void OnReset()
        {
            base.OnReset();
            health.ResetHealth();
        }

        public override void Init(EnemyStat enemyStat)
        {
            base.Init(enemyStat);
            health.SetHealth(enemyStat.health);
            movement.speed = enemyStat.speed;
            attack.damage = enemyStat.attackDamage;
            attack.range = enemyStat.attackRange;
        }

        private void HandleAttack()
        {
            movement.SetMoving(false);
            attack.Attack();
        }
    }
}
