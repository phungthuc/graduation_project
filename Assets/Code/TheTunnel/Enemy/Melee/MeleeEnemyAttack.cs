using System;
using cowsins;
using TheTunnel.CharacterControl;
using TheTunnel.Core;
using TheTunnel.Manager;
using TheTunnel.Target;
using TheTunnel.Weapon;
using Unity.Netcode;
using UnityEngine;

namespace TheTunnel.Enemy
{
    public class MeleeEnemyAttack : EnemyAttack
    {
        [SerializeField] private AudioClip swingSound;
        [SerializeField] private PunchWeapon punchWeapon;

        private static readonly int AttackTriggerParam = Animator.StringToHash("attack");

        private PlayerStats _playerStat;
        private EnemyStateManager _enemyStateManager;

        protected override void Start()
        {
            base.Start();
            _enemyStateManager = GetComponent<EnemyStateManager>();
            if (_enemyStateManager == null)
            {
                Debug.LogError("EnemyStateManager is missing");
            }
            punchWeapon.Hit += OnHit;
            SetEnableAttack(false);
        }

        private void OnEnable()
        {
            GameObject player = GameObject.FindWithTag(GameConstant.PLAYER_TAG);
            if (player == null)
            {
                return;
            }
            _playerStat = player.GetComponent<PlayerStats>();
        }

        private void OnDisable()
        {
            _playerStat = null;
        }

        public void SetEnableAttack(bool enableAttack)
        {
            punchWeapon.gameObject.SetActive(enableAttack);
        }

        private EnemyBase enemyBase;

        private void Awake()
        {
            enemyBase = GetComponent<EnemyBase>();
        }

        public override void Attack()
        {
            if (enemyBase == null || !enemyBase.IsServer) return; // Chỉ server thực hiện attack logic

            // Play sound và animation trên tất cả clients
            PlayAttackEffects();
        }

        private void PlayAttackEffects()
        {
            // Play sound và animation trên tất cả clients
            GameSoundManager.Instance.PlaySound(swingSound, 0, true, 1f, transform.position);

            // Sử dụng SetAnimatorTrigger để sync animation qua network
            if (_enemyStateManager != null)
            {
                _enemyStateManager.SetAnimatorTrigger(AttackTriggerParam);
            }
        }

        private void OnHit(GameObject hitGo)
        {
            if (enemyBase == null || !enemyBase.IsServer) return; // Chỉ server xử lý damage

            ApplyDamageToGameObject(hitGo);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }

        private void ApplyDamageToGameObject(GameObject hitGo)
        {
            if (hitGo.CompareTag(GameConstant.PLAYER_TAG))
            {
                // Tìm NetworkObject của player để gửi damage
                NetworkObject playerNetworkObject = hitGo.GetComponent<NetworkObject>();
                if (playerNetworkObject != null)
                {
                    // Gửi damage qua RPC đến player
                    PlayerStats playerStats = hitGo.GetComponent<PlayerStats>();
                    if (playerStats != null)
                    {
                        playerStats.Damage(damage, false);
                    }
                }
            }

            if (hitGo.CompareTag(GameConstant.CASTLE_TAG))
            {
                // Chỉ server mới xử lý damage cho Castle
                if (Castle.Instance != null)
                {
                    Castle.Instance.TakeDamage(damage);
                }
                else
                {
                    Debug.LogWarning("[MeleeEnemyAttack] Castle.Instance is null");
                }
            }
        }
    }
}