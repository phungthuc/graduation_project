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
        public event Action EnemyCleaned;

        private Dictionary<string, GameObjectPool<EnemyBase>> _enemyPoolDict = new();

        private int _enemySpawnCount = 0;

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
            if (NetworkManager.Singleton != null &&
                NetworkManager.Singleton.IsListening &&
                !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            SpawnEnemyInternal(enemyId, position);
        }

        private void SpawnEnemyInternal(string enemyId, Vector3 position)
        {
            if (NetworkManager.Singleton != null &&
                NetworkManager.Singleton.IsListening &&
                !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            if (_enemyPoolDict.TryGetValue(enemyId, out var pool))
            {
                var enemy = pool.GetObject();
                enemy.transform.position = position;

                NetworkObject networkObject = enemy.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    pool.ReturnObject(enemy);
                    return;
                }

                if (!networkObject.IsSpawned)
                {
                    networkObject.Spawn();
                }

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
                    _enemySpawnCount--;
                    if (_enemySpawnCount <= 0)
                    {
                        EnemyCleaned?.Invoke();
                        _enemySpawnCount = 0;
                    }
                };
                enemy.Died.AddListener(onDiedAction);
                enemy.OnReset();
                _enemySpawnCount++;
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
                if (pool == null) continue;

                List<EnemyBase> enemies = pool.GetActiveObjects();
                if (enemies == null) continue;

                foreach (var enemy in enemies)
                {
                    if (enemy != null)
                    {
                        enemy.SetPaused(true);
                    }
                }
            }
        }
    }
}