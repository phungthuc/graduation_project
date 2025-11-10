using TheTunnel.Enemy;
using UnityEngine;

namespace TheTunnel.CharacterControl
{
    public class EnemyStateManager : CharacterStateManager
    {
        public ICharacterState IdleState { get; private set; }
        public ICharacterState AttackState { get; private set; }
        public ICharacterState Run { get; private set; }

        public bool isRunning;
        public bool isAttacking;

        private MeleeEnemyAttack _meleeEnemyAttack;

        public override void Start()
        {
            base.Start();
            _meleeEnemyAttack = GetComponent<MeleeEnemyAttack>();
            if (_meleeEnemyAttack == null)
            {
                Debug.LogError("MeleeEnemyAttack is missing");
            }
            IdleState = new EnemyIdleState();
            AttackState = new EnemyAttackState();
            Run = new EnemyRunState();

            currentState = IdleState;
            IdleState.Enter(this);
        }

        public void SetRunning(bool running)
        {
            isRunning = running;
        }

        public void SetAttacking(bool attacking)
        {
            isAttacking = attacking;
            _meleeEnemyAttack.SetEnableAttack(attacking);
        }
    }
}