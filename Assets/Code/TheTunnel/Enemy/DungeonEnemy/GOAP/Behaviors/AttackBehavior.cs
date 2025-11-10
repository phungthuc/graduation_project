using System.Collections;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Interfaces;
using TheTunnel.GOAP;
using TheTunnel.Projectile;
using UnityEngine;

namespace TheTunnel.Goap
{
    public class AttackBehavior : MonoBehaviour
    {
        private GoapSetBinder _goapSetBinder;
        private Animator _animator;

        [Header("Melee Attack")]
        private DungeonEnemyWeapom _enemyWeapon;

        [Header("Ranged Attack")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed, projectileDuration, damage;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private int maxAmmo = 15;
        [SerializeField] private float reloadTime = 2f;
        private int _currentAmmo;
        private bool _isReloading = false;
        private Vector3 _targetDirection;
        private Quaternion _targetRotation;
        private ITarget _currentTarget;
        private float _fireRate;
        private float _fireCooldown = 0f;

        [Header("Spider Attack")]
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float jumpDuration = 1f;
        private bool _isJumping = false;

        public void Start()
        {
            _goapSetBinder = GetComponent<GoapSetBinder>();
            _animator = GetComponentInChildren<Animator>();
            _currentAmmo = maxAmmo;
        }
        public void Update()
        {
            if (_currentTarget != null)
            {
                AttackType();
            }
        }

        private void AttackType()
        {
            switch (_goapSetBinder.agentType)
            {
                case AgentType.EnemyDungeonSet:
                    // _enemyWeapon.Attack();
                    break;
                case AgentType.RangedEnemyDungeonSet:
                    RangedAttack(_currentTarget.Position);
                    break;
                case AgentType.SpiderEnemyDungeonSet:
                    if (ShouldJump(_currentTarget.Position))
                    {
                        JumpToTarget(_currentTarget.Position);
                    }
                    break;
            }
        }

        #region Ranged Attack
        public void RangedAttack(Vector3 targetPosition)
        {
            if (_isReloading)
            {
                return;
            }

            _fireCooldown -= Time.deltaTime;
            if (_fireCooldown <= 0)
            {
                _targetDirection = targetPosition - transform.position;
                _targetRotation = Quaternion.LookRotation(_targetDirection);
                _animator.SetTrigger(CommonData.Attack);
                StartCoroutine(ShootWithDelay(3, 0.2f));
                _fireCooldown = _fireRate;

                if (_currentAmmo < 1)
                {
                    StartCoroutine(Reload());
                }
            }
        }

        private IEnumerator Reload()
        {
            _isReloading = true;
            _animator.SetBool(CommonData.Reloading, _isReloading);
            yield return new WaitForSeconds(reloadTime);
            _currentAmmo = maxAmmo;
            _isReloading = false;
            _animator.SetBool(CommonData.Reloading, _isReloading);
        }

        private IEnumerator ShootWithDelay(int bulletCount, float delay)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                Instantiate(muzzleFlash, firePoint.position, _targetRotation);
                _currentAmmo--;

                EnemyProjectile proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity).GetComponent<EnemyProjectile>();
                proj.dir = _targetDirection;
                proj.damage = damage;
                proj.speed = projectileSpeed;
                Destroy(proj.gameObject, projectileDuration);

                yield return new WaitForSeconds(delay);
            }
        }


        public void SetTarget(ITarget target)
        {
            _currentTarget = target;
        }

        public void SetFireRate(float fireRate)
        {
            _fireRate = fireRate;
        }
        #endregion

        #region Spider Attack
        private bool ShouldJump(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(transform.position, targetPosition);
            return distance > 8f && distance <= 10f && !_isJumping;
        }

        public void JumpToTarget(Vector3 targetPosition)
        {
            if (_isJumping) return;

            _isJumping = true;
            _animator.SetTrigger(CommonData.SpiderJumpToPlayer);
            transform.LookAt(targetPosition);
            StartCoroutine(JumpCoroutine(transform.position, targetPosition));
        }

        private IEnumerator JumpCoroutine(Vector3 start, Vector3 end)
        {
            float elapsed = 0f;
            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / jumpDuration;

                Vector3 horizontalPosition = Vector3.Lerp(start, end, t);
                float verticalOffset = jumpHeight * Mathf.Sin(Mathf.PI * t);
                transform.position = horizontalPosition + Vector3.up * verticalOffset;

                yield return null;
            }

            transform.position = end;
            yield return new WaitForSeconds(1f);
            _isJumping = false;
        }
        #endregion
    }
}
