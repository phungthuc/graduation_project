using TheTunnel.Enemy;
using Unity.Netcode;
using Unity.Netcode.Components;
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
        private NetworkAnimatorHelper _networkAnimatorHelper;
        private NetworkAnimator _networkAnimator;

        public override void Start()
        {
            base.Start();
            _meleeEnemyAttack = GetComponent<MeleeEnemyAttack>();
            if (_meleeEnemyAttack == null)
            {
                Debug.LogError("MeleeEnemyAttack is missing");
            }

            // Tìm NetworkAnimator hoặc NetworkAnimatorHelper
            _networkAnimator = GetComponent<NetworkAnimator>();
            _networkAnimatorHelper = GetComponent<NetworkAnimatorHelper>();

            // Nếu không có NetworkAnimator, thêm warning
            if (_networkAnimator == null && _networkAnimatorHelper == null)
            {
                Debug.LogWarning("EnemyStateManager: Không tìm thấy NetworkAnimator hoặc NetworkAnimatorHelper. Animation sẽ không được sync qua network!");
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

        /// <summary>
        /// Set animator bool parameter - NetworkAnimator tự động sync nếu có
        /// </summary>
        public void SetAnimatorBool(int hash, bool value)
        {
            if (animator != null)
            {
                // NetworkAnimator tự động sync tất cả Animator parameters
                // Chỉ cần set trên Animator, NetworkAnimator sẽ tự sync
                animator.SetBool(hash, value);
            }
        }

        /// <summary>
        /// Set animator trigger - NetworkAnimator tự động sync nếu có
        /// </summary>
        public void SetAnimatorTrigger(int hash)
        {
            if (_networkAnimator != null)
            {
                _networkAnimator.SetTrigger(hash);
            }
            else if (animator != null)
            {
                // Fallback: set trực tiếp (NetworkAnimator sẽ tự sync nếu có)
                animator.SetTrigger(hash);
            }
        }
    }
}