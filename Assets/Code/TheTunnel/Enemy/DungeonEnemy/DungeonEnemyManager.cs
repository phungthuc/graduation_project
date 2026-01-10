using System.Collections.Generic;
using cowsins;
using TheTunnel.Enemy;
using TheTunnel.Level;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

namespace TheTunnel
{
    public class DungeonEnemyManager : MonoBehaviour
    {
        [SerializeField] private List<Vector3> spawnPositions;
        public UnityEvent EnemyCleaned;

        private DungeonEnemySpawner _enemySpawner;
        private int _enemySpawnCount;
        private int _enemyDiedCount;
        private DungeonData currentDungeonData;
        private bool _isPaused;

        private List<string> _zoneList;
        private int _currentZoneIndex;
        private Dictionary<string, int> _zoneSpawnCount;
        private Dictionary<string, int> _zoneDiedCount;
        private bool _isDungeonDataLoaded = false;

        [SerializeField] private GameObject winGameUI;

        private void Awake()
        {
            _enemySpawner = GetComponent<DungeonEnemySpawner>();
            if (_enemySpawner == null)
            {
                return;
            }
            _enemySpawner.EnemySpawned += OnEnemySpawned;
            _enemySpawner.EnemyDied += OnEnemyDied;
        }

        private void OnDestroy()
        {
            CleanupAllEnemies();
            if (_enemySpawner != null)
            {
                _enemySpawner.EnemySpawned -= OnEnemySpawned;
                _enemySpawner.EnemyDied -= OnEnemyDied;
            }
        }
        public void LoadDungeonData(DungeonData dungeonData)
        {
            if (NetworkManager.Singleton != null &&
                NetworkManager.Singleton.IsListening &&
                !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            if (_isDungeonDataLoaded)
            {
                Debug.LogWarning("[DungeonEnemyManager] LoadDungeonData called multiple times. Ignoring duplicate call to prevent double spawning.");
                return;
            }

            CleanupAllEnemies();

            currentDungeonData = dungeonData;
            _enemySpawnCount = 0;
            _enemyDiedCount = 0;
            _isPaused = false;
            _isDungeonDataLoaded = true;

            _zoneList = new List<string>(dungeonData.EnemySpawnData.Keys);
            _currentZoneIndex = 0;
            _zoneSpawnCount = new Dictionary<string, int>();
            _zoneDiedCount = new Dictionary<string, int>();

            if (_zoneList.Count > 0)
                if (_zoneList.Count > 0)
                {
                    SpawnDungeonWave(_zoneList[_currentZoneIndex]);
                }
        }

        /// <summary>
        /// </summary>
        public void LoadDungeonData3(DungeonData dungeonData)
        {
            LoadDungeonData(dungeonData);
        }

        /// <summary>
        /// </summary>
        public void StopWave()
        {
            _isPaused = true;
        }

        /// <summary>
        /// </summary>
        private void CleanupAllEnemies()
        {
            if (_enemySpawner == null) return;

            _enemySpawner.StopAllEnemies();

            _enemySpawner.CleanupAllEnemies();

            _enemySpawnCount = 0;
            _enemyDiedCount = 0;
            _zoneSpawnCount?.Clear();
            _zoneDiedCount?.Clear();
            _isDungeonDataLoaded = false;
        }

        /// <summary>
        /// </summary>
        public void SpawnDungeonWave(string zoneName)
        {
            if (Unity.Netcode.NetworkManager.Singleton != null &&
                !Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                return;
            }

            if (_isPaused)
            {
                return;
            }

            if (currentDungeonData == null ||
                !currentDungeonData.EnemySpawnData.ContainsKey(zoneName))
            {
                return;
            }

            var spawnPointsList = currentDungeonData.EnemySpawnData[zoneName];

            if (spawnPointsList == null || spawnPointsList.Count == 0)
            {
                return;
            }

            int zoneSpawnCount = 0;
            _zoneSpawnCount[zoneName] = 0;
            _zoneDiedCount[zoneName] = 0;

            foreach (var spawnPoint in spawnPointsList)
            {
                Vector3 baseSpawnPosition = ParseStringToVector3(spawnPoint.SpawnPosition);

                if (baseSpawnPosition == Vector3.zero && spawnPoint.SpawnPosition != "0,0,0")
                {
                    continue;
                }

                if (spawnPoint.EnemyData == null || spawnPoint.EnemyData.Count == 0)
                {
                    continue;
                }

                foreach (var enemyData in spawnPoint.EnemyData)
                {
                    if (string.IsNullOrEmpty(enemyData.Id))
                    {
                        continue;
                    }

                    if (enemyData.Amount <= 0)
                    {
                        continue;
                    }

                    for (int i = 0; i < enemyData.Amount; i++)
                    {
                        Vector3 spawnPosition = GetSpawnPosition(baseSpawnPosition, enemyData.Amount, i);

                        float delay = UnityEngine.Random.Range(0, 2f);
                        StartCoroutine(SpawnEnemyWithDelay(enemyData.Id, spawnPosition, delay));
                        zoneSpawnCount++;
                    }
                }
            }

            _zoneSpawnCount[zoneName] = zoneSpawnCount;
            _enemySpawnCount += zoneSpawnCount;
        }

        /// <summary>
        /// </summary>
        private System.Collections.IEnumerator SpawnEnemyWithDelay(string enemyId, Vector3 position, float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnEnemy(enemyId, position);
        }

        /// <summary>
        /// </summary>
        private Vector3 GetSpawnPosition(Vector3 basePosition, int totalAmount, int currentIndex)
        {
            if (totalAmount == 1)
            {
                return basePosition;
            }

            float angle = (360f / totalAmount) * currentIndex;
            float radius = UnityEngine.Random.Range(1f, 3f);

            float xOffset = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float zOffset = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

            return basePosition + new Vector3(xOffset, 0, zOffset);
        }

        private void SpawnEnemy(string enemyId, Vector3 position)
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                return;
            }

            _enemySpawner.Spawn(enemyId, position);
        }

        private void OnEnemySpawned()
        {
        }

        private void OnEnemyDied()
        {
            _enemyDiedCount++;

            bool allZonesSpawned = _currentZoneIndex >= _zoneList.Count;

            if (allZonesSpawned && _enemySpawnCount > 0 && _enemyDiedCount >= _enemySpawnCount)
            {
                EnemyCleaned?.Invoke();
                return;
            }

            if (!allZonesSpawned && _currentZoneIndex < _zoneList.Count)
            {
                string currentZone = _zoneList[_currentZoneIndex];
                int zoneSpawnCount = _zoneSpawnCount.GetValueOrDefault(currentZone, 0);
                int zoneDiedCount = _zoneDiedCount.GetValueOrDefault(currentZone, 0) + 1;
                _zoneDiedCount[currentZone] = zoneDiedCount;

                if (zoneSpawnCount > 0 && zoneDiedCount >= zoneSpawnCount)
                {
                    _currentZoneIndex++;
                    if (_currentZoneIndex < _zoneList.Count)
                    {
                        string nextZone = _zoneList[_currentZoneIndex];
                        SpawnDungeonWave(nextZone);
                    }
                    else
                    {
                        CheckWinCondition();
                    }
                }
            }
            else if (allZonesSpawned)
            {
                CheckWinCondition();
            }
        }

        /// <summary>
        /// </summary>
        private void CheckWinCondition()
        {
            if (_enemySpawnCount > 0 && _enemyDiedCount >= _enemySpawnCount)
            {
                EnemyCleaned?.Invoke();
            }
        }

        private void OnWinGame()
        {
        }

        private void SpawnPlayer()
        {
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            Vector3 playerPosition = ParseStringToVector3(currentDungeonData.PlayerPosition);
            player.TeleportPlayer(playerPosition, Quaternion.identity);
        }

        Vector3 ParseStringToVector3(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Vector3.zero;
            }

            string cleanedInput = input.Replace(" ", "");
            string[] values = cleanedInput.Split(',');

            if (values.Length != 3)
            {
                return Vector3.zero;
            }

            try
            {
                float x = float.Parse(values[0].Trim());
                float y = float.Parse(values[1].Trim());
                float z = float.Parse(values[2].Trim());
                return new Vector3(x, y, z);
            }
            catch (System.FormatException)
            {
                return Vector3.zero;
            }
        }
    }
}
