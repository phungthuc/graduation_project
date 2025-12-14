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
            if (!IsServer) return; // Chỉ server điều khiển logic

            // Kiểm tra null references để tránh lỗi khi enemy đã bị despawn nhưng GameObject vẫn còn
            if (!attack || !movement || !health)
            {
                return;
            }

            if (isPaused.Value)
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
            if (movement != null)
            {
                movement.SetMoving(!paused);
            }
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
            if (!movement || !attack) return;
            movement.SetMoving(false);
            attack.Attack();
        }
    }
}
