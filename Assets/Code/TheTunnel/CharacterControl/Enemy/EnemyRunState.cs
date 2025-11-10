using UnityEngine;

namespace TheTunnel.CharacterControl
{
    public class EnemyRunState : ICharacterState
    {
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        
        public void Enter(CharacterStateManager characterStateManager)
        {
            EnemyStateManager enemyStateManager = (EnemyStateManager) characterStateManager;
            enemyStateManager.animator.SetBool(IsRunning, true);
        }

        public void Exit(CharacterStateManager characterStateManager)
        {
        }

        public void Execute(CharacterStateManager characterStateManager)
        {
            if (characterStateManager is EnemyStateManager enemyStateManager)
            {
                if (enemyStateManager.isAttacking)
                {
                    enemyStateManager.ChangeState(enemyStateManager.AttackState);
                }
            }
        }
    }
}