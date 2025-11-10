using System.Collections;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Interfaces;
using TheTunnel.GOAP;
using TheTunnel.Projectile;
using UnityEngine;

namespace TheTunnel.NPC
{
    public class NPCAttackBehavior : MonoBehaviour
    {
        private Animator _animator;

        [Header("Melee Attack")]

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

        public void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _currentAmmo = maxAmmo;
        }
        public void Update()
        {
            if (_currentTarget != null)
            {
                RangedAttack(_currentTarget.Position);
            }
        }


        #region Ranged Attack
        public void RangedAttack(Vector3 targetPosition)
        {
            if (_isReloading)
            {
                return;
            }

            if (_currentAmmo <= 3)
            {
                StartCoroutine(Reload());
            }

            _fireCooldown -= Time.deltaTime;
            if (_fireCooldown <= 0 && _currentAmmo > 3)
            {
                _targetDirection = targetPosition - transform.position;
                _targetRotation = Quaternion.LookRotation(_targetDirection);
                StartCoroutine(ShootWithDelay(3, 0.2f));
                _animator.SetTrigger(CommonData.Attack);
                _fireCooldown = _fireRate;
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

                NPCProjectile proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity).GetComponent<NPCProjectile>();
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
    }
}
