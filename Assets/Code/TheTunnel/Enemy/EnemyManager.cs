using System;
using System.Collections;
using System.Collections.Generic;
using TheTunnel.Level;
using UnityEngine;
using UnityEngine.Events;

namespace TheTunnel.Enemy
{
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField]
        private Bounds spawnBounds;

        public UnityEvent EnemyCleaned;

        private EnemySpawner _enemySpawner;
        private List<WaveData> _waveDataList;
        private float _timeNextWave;
        private float _timeCounter;
        private int _currentWaveIndex;
        private int _enemySpawnCount;
        private int _enemyDiedCount;
        private bool _isPaused;

        private void Awake()
        {
            _enemySpawner = GetComponent<EnemySpawner>();
            if (_enemySpawner == null)
            {
                Debug.LogError("EnemySpawner component not found");
            }
            _enemySpawner.EnemySpawned += OnEnemySpawned;
            _enemySpawner.EnemyDied += OnEnemyDied;
        }

        private void Update()
        {
            if (_isPaused)
            {
                return;
            }
            if (_waveDataList == null || _waveDataList.Count == 0)
            {
                return;
            }
            if (_currentWaveIndex >= _waveDataList.Count)
            {
                return;
            }
            _timeCounter += Time.deltaTime;
            if (_timeCounter > _timeNextWave)
            {
                _timeCounter = 0;
                _currentWaveIndex++;
                if (_currentWaveIndex < _waveDataList.Count)
                {
                    SpawnWave(_waveDataList[_currentWaveIndex]);
                }
            }
        }

        public void LoadWaveData(List<WaveData> waveDataList)
        {
            _waveDataList = waveDataList;
            _currentWaveIndex = 0;
            _timeCounter = 0;
            _enemySpawnCount = 0;
            _enemyDiedCount = 0;
            _timeNextWave = 0;
            SpawnWave(_waveDataList[_currentWaveIndex]);
        }

        public void StopWave()
        {
            _isPaused = true;
            _enemySpawner.StopAllEnemies();
        }

        private void SpawnWave(WaveData data)
        {
            _timeNextWave = _waveDataList[_currentWaveIndex].TimeNextWave;
            for (int i = 0; i < data.EnemyList.Count; i++)
            {
                for (int j = 0; j < data.EnemyList[i].Amount; j++)
                {
                    float delay = UnityEngine.Random.Range(0, 2f);
                    StartCoroutine(SpawnEnemyWithDelay(data.EnemyList[i].Id, delay));
                }
            }
        }

        private IEnumerator SpawnEnemyWithDelay(string enemyId, float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnEnemy(enemyId);
        }

        private void SpawnEnemy(string enemyId)
        {
            Vector3 randomPosition = new Vector3(
                UnityEngine.Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                UnityEngine.Random.Range(spawnBounds.min.y, spawnBounds.max.y),
                UnityEngine.Random.Range(spawnBounds.min.z, spawnBounds.max.z)
            );
            _enemySpawner.Spawn(enemyId, randomPosition);
        }

        private void OnEnemySpawned()
        {
            _enemySpawnCount++;
        }

        private void OnEnemyDied()
        {
            _enemyDiedCount++;
            if (_enemySpawnCount <= _enemyDiedCount && _currentWaveIndex >= _waveDataList.Count - 1)
            {
                EnemyCleaned?.Invoke();
            }
        }
    }
}