using UnityEngine;

namespace TheTunnel.CharacterControl
{
    public class EnemyIdleState : ICharacterState
    {
        private static readonly int IsRunning = Animator.StringToHash("isRunning");

        public void Enter(CharacterStateManager characterStateManager)
        {
            EnemyStateManager enemyStateManager = (EnemyStateManager)characterStateManager;
            enemyStateManager.animator.SetBool(IsRunning, false);
        }

        public void Exit(CharacterStateManager characterStateManager)
        {
        }

        public void Execute(CharacterStateManager characterStateManager)
        {
            EnemyStateManager enemyStateManager = (EnemyStateManager)characterStateManager;
            if (enemyStateManager.isRunning && !enemyStateManager.isAttacking)
            {
                enemyStateManager.ChangeState(enemyStateManager.Run);
            }

            if (enemyStateManager.isAttacking)
            {
                enemyStateManager.ChangeState(enemyStateManager.AttackState);
            }
        }
    }
}