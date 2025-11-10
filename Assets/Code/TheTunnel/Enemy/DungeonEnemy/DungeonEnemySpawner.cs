using System;
using System.Collections.Generic;
using CrashKonijn.Goap.Behaviours;
using TheTunnel.Enemy;
using TheTunnel.GOAP;
using TheTunnel.Pool;
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

        private void Awake()
        {
            foreach (var enemyData in enemyDataList)
            {
                var pool = new GameObjectPool<EnemyHealth>(enemyData.enemyPrefab, transform, 0);
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
                    enemy.events.OnDeath.RemoveListener(onDiedAction);
                    pool.ReturnObject(enemy);
                    EnemyDied?.Invoke();
                };
                enemy.events.OnDeath.AddListener(onDiedAction);
                EnemySpawned?.Invoke();
            }
        }
    }
}
