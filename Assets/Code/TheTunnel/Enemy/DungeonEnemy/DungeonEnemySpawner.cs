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
            foreach (var enemyData in enemyDataList)
            {
                var pool = new GameObjectPool<EnemyHealth>(enemyData.enemyPrefab, transform, 0);
                _enemyPoolDict.Add(enemyData.id, pool);
            }
        }

        public void Spawn(string enemyId, Vector3 position)
        {
            // Chỉ server mới spawn Enemy (tương tự EnemySpawner)
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

                // Kiểm tra EnemyBase component (cần thiết cho network health sync)
                var enemyBase = enemy.GetComponent<EnemyBase>();
                if (enemyBase == null)
                {
                    Debug.LogError($"Dungeon Enemy {enemyId} (GameObject: {enemy.gameObject.name}) không có EnemyBase component! Vui lòng thêm EnemyBase component vào prefab.");
                    pool.ReturnObject(enemy);
                    return;
                }

                // Sample NavMesh để đảm bảo position nằm trên NavMesh
                Vector3 spawnPosition = SampleNavMeshPosition(position);
                enemy.transform.position = spawnPosition;

                // Lấy NetworkObject component (tương tự EnemySpawner)
                NetworkObject networkObject = enemy.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    Debug.LogError($"Dungeon Enemy {enemyId} không có NetworkObject component!");
                    pool.ReturnObject(enemy);
                    return;
                }

                // Spawn enemy như NetworkObject (tương tự EnemySpawner)
                if (!networkObject.IsSpawned)
                {
                    networkObject.Spawn();
                }

                // Update position for navmesh agent (sau khi spawn NetworkObject)
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    // Đợi một frame để đảm bảo NetworkObject đã spawn xong
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
        }

        private void DespawnEnemy(EnemyHealth enemy, GameObjectPool<EnemyHealth> pool)
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer) return;

            // Despawn NetworkObject trước khi return về pool (tương tự EnemySpawner)
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
                List<EnemyHealth> enemies = pool.GetActiveObjects();
                foreach (var enemy in enemies)
                {
                    // Có thể thêm logic pause enemy nếu cần
                    var enemyBase = enemy.GetComponent<EnemyBase>();
                    if (enemyBase != null)
                    {
                        enemyBase.SetPaused(true);
                    }
                }
            }
        }

        /// <summary>
        /// Sample NavMesh position để đảm bảo spawn position nằm trên NavMesh
        /// </summary>
        private Vector3 SampleNavMeshPosition(Vector3 position)
        {
            UnityEngine.AI.NavMeshHit hit;
            float sampleRadius = 5f; // Radius để tìm NavMesh point gần nhất

            if (UnityEngine.AI.NavMesh.SamplePosition(position, out hit, sampleRadius, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }

            // Nếu không tìm thấy NavMesh point, log warning và trả về position gốc
            Debug.LogWarning($"Could not find NavMesh position near {position}. Using original position.");
            return position;
        }

        /// <summary>
        /// Setup NavMeshAgent sau khi NetworkObject đã spawn
        /// </summary>
        private System.Collections.IEnumerator SetupNavMeshAgent(NavMeshAgent agent, Vector3 position)
        {
            // Đợi một frame để đảm bảo NetworkObject đã spawn xong
            yield return null;

            if (agent != null)
            {
                // Enable agent trước khi warp
                agent.enabled = true;

                // Warp agent đến position (đảm bảo position nằm trên NavMesh)
                if (agent.isOnNavMesh)
                {
                    agent.Warp(position);
                }
                else
                {
                    // Nếu agent chưa on NavMesh, thử enable lại và warp
                    agent.enabled = false;
                    yield return null;
                    agent.enabled = true;
                    yield return null;

                    if (agent.isOnNavMesh)
                    {
                        agent.Warp(position);
                    }
                    else
                    {
                        Debug.LogWarning($"NavMeshAgent could not be placed on NavMesh at position {position}");
                    }
                }
            }
        }
    }
}
