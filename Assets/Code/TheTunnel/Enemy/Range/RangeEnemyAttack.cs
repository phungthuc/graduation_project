using System;
using cowsins;
using TheTunnel.Enemy;
using TheTunnel.Manager;
using TheTunnel.Projectile;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheTunnel.Enemy
{
    public class RangeEnemyAttack : EnemyAttack
    {
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed, projectileDuration;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject muzzleFlash;

        [SerializeField, Tooltip("Shots per second."), Header("Shooting")]
        private float fireRate = 2f;
        [SerializeField]
        private float fireCooldown = 0f;

        [SerializeField] private Transform turretHead;
        [SerializeField] private float lerpSpeed = 5f;

        [HideInInspector]
        public Transform targetTransform;

        private Vector3 _targetDirection;
        private Quaternion _targetRotation;

        private void OnDisable()
        {
            fireCooldown = 0f;
        }

        public override void Attack()
        {
            if (targetTransform == null)
            {
                return;
            }
            _targetDirection = targetTransform.position - transform.position;
            _targetRotation = Quaternion.LookRotation(_targetDirection);
            turretHead.rotation = Quaternion.Lerp(turretHead.rotation, _targetRotation, lerpSpeed * Time.deltaTime);
            fireCooldown -= Time.deltaTime;
            if (fireCooldown <= 0)
            {
                fireCooldown = fireRate;
                Instantiate(muzzleFlash, firePoint.position, _targetRotation);
                GameSoundManager.Instance.PlaySound(fireSound, 2, true, 0, transform.position);
                TankProjectile proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity).gameObject.GetComponent<TankProjectile>();
                proj.dir = _targetDirection;
                proj.damage = damage;
                proj.speed = projectileSpeed;
                Destroy(proj.gameObject, projectileDuration);
            }
        }
    }
}
