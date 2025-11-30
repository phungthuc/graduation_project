using System;
using cowsins;
using TheTunnel.Enemy;
using TheTunnel.Manager;
using TheTunnel.Projectile;
using Unity.Netcode;
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

        private EnemyBase enemyBase;

        private void Awake()
        {
            enemyBase = GetComponent<EnemyBase>();
        }

        public override void Attack()
        {
            if (enemyBase == null || !enemyBase.IsServer) return; // Chỉ server thực hiện attack logic

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
                FireProjectile();
            }
        }

        private void FireProjectile()
        {
            // Server spawn projectile
            SpawnProjectile(firePoint.position, _targetDirection, _targetRotation);
        }

        private void SpawnProjectile(Vector3 position, Vector3 direction, Quaternion rotation)
        {
            // Spawn projectile như NetworkObject (nếu projectile có NetworkObject)
            GameObject projObj = Instantiate(projectilePrefab, position, rotation);
            TankProjectile proj = projObj.GetComponent<TankProjectile>();
            
            if (proj != null)
            {
                proj.dir = direction;
                proj.damage = damage;
                proj.speed = projectileSpeed;
            }

            // Spawn projectile trên network nếu có NetworkObject
            NetworkObject projNetworkObject = projObj.GetComponent<NetworkObject>();
            if (projNetworkObject != null)
            {
                projNetworkObject.Spawn();
            }

            // Play effects trên tất cả clients
            PlayFireEffects(position, rotation);

            // Destroy sau duration (chỉ trên server)
            Destroy(projObj, projectileDuration);
        }

        private void PlayFireEffects(Vector3 position, Quaternion rotation)
        {
            // Play sound và muzzle flash trên tất cả clients
            Instantiate(muzzleFlash, position, rotation);
            GameSoundManager.Instance.PlaySound(fireSound, 2, true, 0, position);
        }
    }
}
