using System;
using cowsins;
using TheTunnel.Core;
using TheTunnel.Turret.Projectiles;
using UnityEngine;

namespace TheTunnel.Turret
{
    public class TurretController : Interactable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform turretHead;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField, Tooltip("Speed of rotation interpolation.")] private float lerpSpeed = 5f;
        
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField, Min(0)] private float projectileSpeed, projectileDamage, projectileDuration;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private float fireRate = 2f;
        [SerializeField] private int magazineCapacity = 30;
        [SerializeField] private float reloadTime = 2f;
        
        private bool _canShoot = false;
        private float _fireCooldown = 0f;
        private Vector3 _targetDirection;
        private Quaternion _targetRotation;

        private Transform _targetEnemyTransform;
        private Quaternion _initialRotation;
        
        private float _idleTimeCounter = 0f;
        private readonly float _idleTime = 5f;
        private int _currentBullets;
        
        private void Awake()
        {
            _initialRotation = turretHead.rotation;
            _currentBullets = magazineCapacity;
        }

        private void Update()
        {
            if (_targetEnemyTransform == null || _targetEnemyTransform.gameObject.activeInHierarchy == false)
            {
                GameObject target = FindTargetInRange();
                if (target == null)
                {
                    ResetTurret();
                    return;
                }
                Transform hitTransform = target.transform.Find("HitPoint");
                _targetEnemyTransform = hitTransform != null ? hitTransform : target.transform;
            }
            
            _targetDirection = _targetEnemyTransform.position - firePoint.position;
            _canShoot = true;
            _targetRotation = Quaternion.LookRotation(_targetDirection);
            turretHead.rotation = Quaternion.Lerp(turretHead.rotation, _targetRotation, lerpSpeed * Time.deltaTime);
            _fireCooldown -= Time.deltaTime;
            _idleTimeCounter = 0;
            Fire();
        }

        public void Reload()
        {
            FillMagazine();
        }
        

        public bool IsMagazineFull()
        {
            return _currentBullets == magazineCapacity;
        }

        private void FillMagazine()
        {
            _currentBullets = magazineCapacity;
        }
        
        private void ResetTurret()
        {
            _fireCooldown = fireRate;
            _canShoot = false;
            if (_idleTimeCounter >= _idleTime)
            {
                turretHead.rotation = Quaternion.Lerp(turretHead.rotation, _initialRotation, lerpSpeed * Time.deltaTime);
            }
            else
            {
                _idleTimeCounter += Time.deltaTime;
            }
        }
        
        private GameObject FindTargetInRange()
        {
            Collider[] targetsInRange = Physics.OverlapSphere(transform.position, detectionRange);
            if (targetsInRange.Length == 0)
            {
                return null;
            }
            for (int i = 0; i < targetsInRange.Length; i++)
            {
                if (targetsInRange[i].gameObject.CompareTag(GameConstant.ENEMY_TAG) && targetsInRange[i].gameObject.activeInHierarchy)
                {
                    return targetsInRange[i].gameObject;
                }
            }
            return null;
        }

        private void Fire()
        {
            if (_canShoot && _fireCooldown <= 0 && _currentBullets > 0)
            {
                if (animator != null) animator.SetTrigger("Fire");
                _fireCooldown = fireRate;
                _currentBullets--;
                Projectiles.TurretProjectile proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity).GetComponent<Projectiles.TurretProjectile>();
                Instantiate(muzzleFlash, firePoint.position, _targetRotation);
                proj.dir = _targetDirection;
                proj.damage = projectileDamage;
                proj.speed = projectileSpeed;
                Destroy(proj.gameObject, projectileDuration);
            }
        }
    }
}
