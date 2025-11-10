using System;
using cowsins;
using TheTunnel.CharacterControl;
using TheTunnel.Target;
using UnityEngine;

namespace TheTunnel.Enemy
{
    public class MeleeEnemyController : EnemyBase
    {
        [SerializeField] private MeleeEnemyAttack attack;
        [SerializeField] private EnemyHealth health;
        [SerializeField] private EnemyMovement movement;
        [SerializeField] private EnemyStateManager stateManager;

        private void Start()
        {
            health.events.OnDeath.RemoveListener(OnDied);
            health.events.OnDeath.AddListener(OnDied);
            stateManager.SetRunning(false);
        }

        private void FixedUpdate()
        {
            if (isPaused)
            {
                return;
            }
            if (attack.FindTargetInRange() != null || stateManager.isAttacking)
            {
                HandleAttack();
            }
            else
            {
                stateManager.SetRunning(true);
                movement.SetMoving(true);
            }
        }

        public override void SetPaused(bool paused)
        {
            base.SetPaused(paused);
            stateManager.IsPaused = paused;
            if (paused)
            {
                movement.SetMoving(false);
                stateManager.ChangeState(stateManager.IdleState);

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
            if (stateManager.isAttacking)
            {
                return;
            }
            stateManager.SetAttacking(true);
            movement.SetMoving(false);
        }
    }
}