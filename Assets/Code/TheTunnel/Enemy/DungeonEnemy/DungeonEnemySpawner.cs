using System;
using System.Collections.Generic;
using CrashKonijn.Goap.Behaviours;
using TheTunnel.Enemy;
using TheTunnel.GOAP;
using TheTunnel.Pool;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace TheTunnel.Enemy
{
    public class DungeonEnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<DungeonEnemyData> enemyDataList = new();

        public event Action EnemySpawned;
        public event Action EnemyDied;

        private Dictionary<string, GameObjectPool<EnemyHealth>> _enemyPoolDict = new();
        private int _enemySpawnCount = 0;

        private void Awake()
        {
            InitializePools();
        }

        private void InitializePools()
        {
            if (enemyDataList == null || enemyDataList.Count == 0)
            {
                return;
            }

            foreach (var enemyData in enemyDataList)
            {
                if (enemyData == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(enemyData.id))
                {
                    continue;
                }

                if (enemyData.enemyPrefab == null)
                {
                    continue;
                }

                if (_enemyPoolDict.ContainsKey(enemyData.id))
                {
                    continue;
                }

                var pool = new GameObjectPool<EnemyHealth>(enemyData.enemyPrefab, transform, 0);
                _enemyPoolDict.Add(enemyData.id, pool);
            }
        }

        public void Spawn(string enemyId, Vector3 position)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                return;
            }

            SpawnEnemyInternal(enemyId, position);
        }

        private void SpawnEnemyInternal(string enemyId, Vector3 position)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer) return;

            if (!_enemyPoolDict.TryGetValue(enemyId, out var pool))
            {
                return;
            }

            var enemy = pool.GetObject();

            var enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase == null)
            {
                pool.ReturnObject(enemy);
                return;
            }

            Vector3 spawnPosition = SampleNavMeshPosition(position);
            enemy.transform.position = spawnPosition;

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
                StartCoroutine(SetupNavMeshAgent(agent, spawnPosition));
            }

            UnityAction onDiedAction = null;
            onDiedAction = () =>
            {
                enemy.events.OnDeath.RemoveListener(onDiedAction);
                DespawnEnemy(enemy, pool);
                EnemyDied?.Invoke();
                _enemySpawnCount--;
            };
            enemy.events.OnDeath.AddListener(onDiedAction);
            _enemySpawnCount++;
            EnemySpawned?.Invoke();
        }

        private void DespawnEnemy(EnemyHealth enemy, GameObjectPool<EnemyHealth> pool)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer) return;
            if (enemy == null) return;

            if (enemy.events != null)
            {
                enemy.events.OnDeath.RemoveAllListeners();
            }

            NetworkObject networkObject = enemy.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                networkObject.Despawn();
            }

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = false;
            }

            pool.ReturnObject(enemy);
        }

        public void StopAllEnemies()
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer) return;

            foreach (var pool in _enemyPoolDict.Values)
            {
                List<EnemyHealth> enemies = pool.GetActiveObjects();
                foreach (var enemy in enemies)
                {
                    if (enemy == null) continue;

                    var enemyBase = enemy.GetComponent<EnemyBase>();
                    if (enemyBase != null)
                    {
                        enemyBase.SetPaused(true);
                    }
                }
            }
        }

        /// <summary>
        /// Cleanup tất cả enemies - despawn và return về pool
        /// </summary>
        public void CleanupAllEnemies()
        {
            if (Unity.Netcode.NetworkManager.Singleton == null || !Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                return;
            }

            foreach (var kvp in _enemyPoolDict)
            {
                string enemyId = kvp.Key;
                var pool = kvp.Value;

                if (pool == null) continue;

                List<EnemyHealth> enemies = pool.GetActiveObjects();

                foreach (var enemy in enemies)
                {
                    if (enemy == null) continue;

                    if (enemy.events != null)
                    {
                        enemy.events.OnDeath.RemoveAllListeners();
                    }

                    DespawnEnemy(enemy, pool);
                }
            }

            _enemySpawnCount = 0;
        }

        /// <summary>
        /// Sample NavMesh position để đảm bảo spawn position nằm trên NavMesh
        /// </summary>
        private Vector3 SampleNavMeshPosition(Vector3 position)
        {
            UnityEngine.AI.NavMeshHit hit;
            float sampleRadius = 5f;

            if (UnityEngine.AI.NavMesh.SamplePosition(position, out hit, sampleRadius, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }

            return position;
        }

        /// <summary>
        /// Setup NavMeshAgent sau khi NetworkObject đã spawn
        /// </summary>
        private System.Collections.IEnumerator SetupNavMeshAgent(NavMeshAgent agent, Vector3 position)
        {
            yield return null;

            if (agent != null)
            {
                agent.enabled = true;

                if (agent.isOnNavMesh)
                {
                    agent.Warp(position);
                }
                else
                {
                    agent.enabled = false;
                    yield return null;
                    agent.enabled = true;
                    yield return null;

                    if (agent.isOnNavMesh)
                    {
                        agent.Warp(position);
                    }
                }
            }
        }
    }
}
