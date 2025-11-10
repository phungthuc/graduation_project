using System;
using cowsins;
using TheTunnel.CharacterControl;
using TheTunnel.Core;
using TheTunnel.Manager;
using TheTunnel.Target;
using TheTunnel.Weapon;
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

        public override void Attack()
        {
            GameSoundManager.Instance.PlaySound(swingSound, 0, true, 1f, transform.position);
            _enemyStateManager.animator.SetTrigger(AttackTriggerParam);
        }

        private void OnHit(GameObject hitGo)
        {
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
                _playerStat.Damage(damage, false);
            }

            if (hitGo.CompareTag(GameConstant.CASTLE_TAG))
            {
                Castle.Instance.TakeDamage(damage);
            }
        }
    }
}