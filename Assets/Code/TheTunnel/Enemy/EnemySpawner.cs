using System;
using System.Collections.Generic;
using TheTunnel.Pool;
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
            if (_enemyPoolDict.TryGetValue(enemyId, out var pool))
            {
                var enemy = pool.GetObject();
                enemy.transform.position = position;
                // update position for navmesh agent
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
                    pool.ReturnObject(enemy);
                    EnemyDied?.Invoke();
                };
                enemy.Died.AddListener(onDiedAction);
                enemy.OnReset();
                EnemySpawned?.Invoke();
            }
        }

        public void StopAllEnemies()
        {
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