using TheTunnel.Enemy;
using UnityEngine;

namespace TheTunnel.CharacterControl
{
    public class EnemyAttackState : ICharacterState
    {
        private EnemyAttack _enemyAttack;

        public void Enter(CharacterStateManager characterStateManager)
        {
            _enemyAttack = characterStateManager.GetComponent<EnemyAttack>();
            if (_enemyAttack == null)
            {
                Debug.LogError("EnemyAttack is missing");
                return;
            }
            _enemyAttack.Attack();

        }

        public void Exit(CharacterStateManager characterStateManager)
        {
        }

        public void Execute(CharacterStateManager characterStateManager)
        {
            EnemyStateManager enemyStateManager = (EnemyStateManager)characterStateManager;
            AnimatorStateInfo stateInfo = characterStateManager.animator.GetCurrentAnimatorStateInfo(0);
            // "Punch" is name of animation state
            if (stateInfo.IsName("Punch") && stateInfo.normalizedTime >= 0.99f)
            {
                enemyStateManager.SetAttacking(false);
                enemyStateManager.SetRunning(false);
                enemyStateManager.ChangeState(enemyStateManager.IdleState);
            }
        }
    }
}