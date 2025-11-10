using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TheTunnel.Enemy.DungeonEnemy
{
    public class DungeonEnemyTracker : MonoBehaviour
    {
        [SerializeField] private GameObject enemiesParent;
        private List<EnemyHealth> _enemies = new List<EnemyHealth>();
        private int _aliveCount;
        public UnityEvent EnemyCleaned;
        [SerializeField] private GameObject teleportGate;
        public void Start()
        {
            _enemies = new List<EnemyHealth>(enemiesParent.GetComponentsInChildren<EnemyHealth>());
            _aliveCount = _enemies.Count;
            teleportGate.SetActive(false);
            // Subscribe to health changes
            foreach (var enemy in _enemies)
            {
                enemy.events.OnDeath.AddListener(OnEnemyDeath);
            }

            EnemyCleaned.AddListener(OnEnemyLevelCleaned);
        }

        private void OnEnemyDeath()
        {
            UpdateAliveCount();
        }

        private void UpdateAliveCount()
        {
            _enemies.RemoveAll(e => e == null || e.health <= 0);
            _aliveCount = _enemies.Count;

            if (_aliveCount <= 0)
            {
                EnemyCleaned?.Invoke();
            }
        }

        public int GetAliveEnemyCount()
        {
            return _aliveCount;
        }

        private void OnEnemyLevelCleaned()
        {
            PlayerData.Instance.SetDungeonLevelCompleted(PlayerData.Instance.CurrentLevel);
            PlayerData.Instance.CurrentLevel++;
            teleportGate.SetActive(true);
        }
    }
}
