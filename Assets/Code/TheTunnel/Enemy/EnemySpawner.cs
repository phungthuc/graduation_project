using System;
using System.Collections.Generic;
using TheTunnel.Pool;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace TheTunnel.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<EnemyData> enemyDataList = new();

        public event Action EnemySpawned;
        public event Action EnemyDied;

        private Dictionary<string, GameObjectPool<EnemyBase>> _enemyPoolDict = new();

        private void Awake()
        {
            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var enemyData in enemyDataList)
            {
                var pool = new GameObjectPool<EnemyBase>(enemyData.enemyPrefab, transform, 5, (EnemyBase enemy) =>
                {
                    enemy.Init(enemyData.stat);
                });
                _enemyPoolDict.Add(enemyData.id, pool);
            }
        }

        public void Spawn(string enemyId, Vector3 position)
        {
            // Chỉ server mới spawn Enemy
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                return;
            }

            SpawnEnemyInternal(enemyId, position);
        }

        private void SpawnEnemyInternal(string enemyId, Vector3 position)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer) return;

            if (_enemyPoolDict.TryGetValue(enemyId, out var pool))
            {
                var enemy = pool.GetObject();
                enemy.transform.position = position;

                // Lấy NetworkObject component
                NetworkObject networkObject = enemy.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    Debug.LogError($"Enemy {enemyId} không có NetworkObject component!");
                    pool.ReturnObject(enemy);
                    return;
                }

                // Spawn enemy như NetworkObject
                if (!networkObject.IsSpawned)
                {
                    networkObject.Spawn();
                }

                // Update position for navmesh agent
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.Warp(position);
                    agent.enabled = true;
                }

                UnityAction onDiedAction = null;
                onDiedAction = () =>
                {
                    enemy.Died.RemoveListener(onDiedAction);
                    DespawnEnemy(enemy, pool);
                    EnemyDied?.Invoke();
                };
                enemy.Died.AddListener(onDiedAction);
                enemy.OnReset();
                EnemySpawned?.Invoke();
            }
        }

        private void DespawnEnemy(EnemyBase enemy, GameObjectPool<EnemyBase> pool)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer) return;

            NetworkObject networkObject = enemy.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                networkObject.Despawn();
            }
            pool.ReturnObject(enemy);
        }

        public void StopAllEnemies()
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer) return;

            foreach (var pool in _enemyPoolDict.Values)
            {
                List<EnemyBase> enemies = pool.GetActiveObjects();
                foreach (var enemy in enemies)
                {
                    enemy.SetPaused(true);
                }
            }
        }
    }
}