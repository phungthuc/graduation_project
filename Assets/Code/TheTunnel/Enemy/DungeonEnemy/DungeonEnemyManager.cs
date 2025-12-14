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

        // Wave system (tương tự EnemyManager)
        private List<string> _zoneList; // Danh sách zones để spawn tuần tự
        private int _currentZoneIndex;
        private Dictionary<string, int> _zoneSpawnCount; // Track spawn count cho mỗi zone
        private Dictionary<string, int> _zoneDiedCount; // Track died count cho mỗi zone

        private void Awake()
        {
            _enemySpawner = GetComponent<DungeonEnemySpawner>();
            if (_enemySpawner == null)
            {
                Debug.LogError("DungeonEnemySpawner component not found");
                return;
            }

            // Subscribe to spawner events (tương tự EnemyManager)
            _enemySpawner.EnemySpawned += OnEnemySpawned;
            _enemySpawner.EnemyDied += OnEnemyDied;
        }

        private void OnDestroy()
        {
            // Cleanup tất cả enemies trước khi destroy
            CleanupAllEnemies();

            // Unsubscribe để tránh memory leak
            if (_enemySpawner != null)
            {
                _enemySpawner.EnemySpawned -= OnEnemySpawned;
                _enemySpawner.EnemyDied -= OnEnemyDied;
            }
        }

        /// <summary>
        /// Load dungeon data và spawn zone đầu tiên (tương tự LoadWaveData trong EnemyManager)
        /// </summary>
        public void LoadDungeonData(DungeonData dungeonData)
        {

            Debug.Log("LoadDungeonData: " + NetworkManager.Singleton.IsServer);
            Debug.Log("NetworkManager.Singleton.IsListening: " + NetworkManager.Singleton.IsListening);
            Debug.Log("NetworkManager.Singleton.IsClient: " + NetworkManager.Singleton.IsClient);
            Debug.Log("NetworkManager.Singleton.IsServer: " + NetworkManager.Singleton.IsServer);
            Debug.Log("NetworkManager.Singleton.IsHost: " + NetworkManager.Singleton.IsHost);
            // Chỉ server mới load và spawn enemies (tương tự EnemyManager)
            if (NetworkManager.Singleton != null &&
                NetworkManager.Singleton.IsListening &&
                !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            // Cleanup tất cả enemies từ session trước (nếu có)
            CleanupAllEnemies();

            currentDungeonData = dungeonData;
            _enemySpawnCount = 0;
            _enemyDiedCount = 0;
            _isPaused = false;

            // Khởi tạo zone list từ dungeon data (tương tự wave list)
            _zoneList = new List<string>(dungeonData.EnemySpawnData.Keys);
            _currentZoneIndex = 0;
            _zoneSpawnCount = new Dictionary<string, int>();
            _zoneDiedCount = new Dictionary<string, int>();

            Debug.Log($"Loaded dungeon data for level. Zones: {_zoneList.Count}");

            // Spawn zone đầu tiên ngay lập tức (tương tự spawn wave đầu tiên)
            if (_zoneList.Count > 0)
            {
                SpawnDungeonWave(_zoneList[_currentZoneIndex]);
            }
        }

        /// <summary>
        /// Alias method để tương thích với LevelManager (LoadDungeonData3)
        /// </summary>
        public void LoadDungeonData3(DungeonData dungeonData)
        {
            LoadDungeonData(dungeonData);
        }

        /// <summary>
        /// Stop spawning enemies (tương tự StopWave trong EnemyManager)
        /// </summary>
        public void StopWave()
        {
            _isPaused = true;
            // Có thể thêm logic stop enemies nếu cần
            Debug.Log("Dungeon enemy spawning stopped");
        }

        /// <summary>
        /// Cleanup tất cả enemies từ session trước để tránh lỗi khi load dungeon mới
        /// </summary>
        private void CleanupAllEnemies()
        {
            if (_enemySpawner == null) return;

            Debug.Log("Cleaning up all enemies from previous session...");

            // Stop tất cả enemies
            _enemySpawner.StopAllEnemies();

            // Despawn tất cả enemies trong pools
            _enemySpawner.CleanupAllEnemies();

            // Reset counters
            _enemySpawnCount = 0;
            _enemyDiedCount = 0;
            _zoneSpawnCount?.Clear();
            _zoneDiedCount?.Clear();

            Debug.Log("Cleanup completed.");
        }

        /// <summary>
        /// Spawn enemies cho một zone (tương tự SpawnWave trong EnemyManager)
        /// Hỗ trợ nhiều spawn points cho mỗi zone
        /// </summary>
        public void SpawnDungeonWave(string zoneName)
        {
            // Chỉ server mới spawn enemies (tương tự EnemyManager)
            if (Unity.Netcode.NetworkManager.Singleton != null &&
                !Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                return;
            }

            if (_isPaused)
            {
                Debug.LogWarning("Enemy spawning is paused");
                return;
            }

            if (currentDungeonData == null ||
                !currentDungeonData.EnemySpawnData.ContainsKey(zoneName))
            {
                Debug.LogWarning($"No spawn data found for zone: {zoneName}");
                return;
            }

            var spawnPointsList = currentDungeonData.EnemySpawnData[zoneName];

            if (spawnPointsList == null || spawnPointsList.Count == 0)
            {
                Debug.LogWarning($"No spawn points found for zone: {zoneName}");
                return;
            }

            Debug.Log($"Spawning enemies for zone: {zoneName} with {spawnPointsList.Count} spawn point(s)");

            // Reset tracking cho zone này
            int zoneSpawnCount = 0;
            _zoneSpawnCount[zoneName] = 0;
            _zoneDiedCount[zoneName] = 0;

            // Xử lý từng spawn point trong zone
            foreach (var spawnPoint in spawnPointsList)
            {
                // Parse spawn position từ JSON
                Vector3 baseSpawnPosition = ParseStringToVector3(spawnPoint.SpawnPosition);

                if (baseSpawnPosition == Vector3.zero && spawnPoint.SpawnPosition != "0,0,0")
                {
                    Debug.LogWarning($"Failed to parse spawn position '{spawnPoint.SpawnPosition}' for zone: {zoneName}. Skipping this spawn point.");
                    continue;
                }

                Debug.Log($"Processing spawn point at position: {baseSpawnPosition} for zone: {zoneName}");

                // Spawn enemies cho spawn point này
                if (spawnPoint.EnemyData == null || spawnPoint.EnemyData.Count == 0)
                {
                    Debug.LogWarning($"Spawn point at {baseSpawnPosition} for zone {zoneName} has no enemyData. Skipping...");
                    continue;
                }

                foreach (var enemyData in spawnPoint.EnemyData)
                {
                    if (string.IsNullOrEmpty(enemyData.Id))
                    {
                        Debug.LogWarning($"Found enemyData with empty Id in zone {zoneName}. Skipping...");
                        continue;
                    }

                    if (enemyData.Amount <= 0)
                    {
                        Debug.LogWarning($"Enemy ID '{enemyData.Id}' in zone {zoneName} has invalid amount: {enemyData.Amount}. Skipping...");
                        continue;
                    }

                    Debug.Log($"Spawning {enemyData.Amount} enemy(ies) of type '{enemyData.Id}' at position {baseSpawnPosition} for zone: {zoneName}");

                    // Spawn từng enemy với đúng ID và position (có delay như EnemyManager)
                    for (int i = 0; i < enemyData.Amount; i++)
                    {
                        // Randomize position xung quanh base position
                        Vector3 spawnPosition = GetSpawnPosition(baseSpawnPosition, enemyData.Amount, i);

                        // Spawn với delay ngẫu nhiên (tương tự EnemyManager)
                        float delay = UnityEngine.Random.Range(0, 2f);
                        StartCoroutine(SpawnEnemyWithDelay(enemyData.Id, spawnPosition, delay));
                        zoneSpawnCount++;
                    }
                }
            }

            _zoneSpawnCount[zoneName] = zoneSpawnCount;
            _enemySpawnCount += zoneSpawnCount;
            Debug.Log($"Spawned {zoneSpawnCount} enemies for zone: {zoneName} across {spawnPointsList.Count} spawn point(s). Total enemies: {_enemySpawnCount}");
        }

        /// <summary>
        /// Spawn enemy với delay (tương tự EnemyManager)
        /// </summary>
        private System.Collections.IEnumerator SpawnEnemyWithDelay(string enemyId, Vector3 position, float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnEnemy(enemyId, position);
        }

        /// <summary>
        /// Lấy spawn position cho enemy (có thể randomize xung quanh base position nếu có nhiều enemy)
        /// </summary>
        private Vector3 GetSpawnPosition(Vector3 basePosition, int totalAmount, int currentIndex)
        {
            // Nếu chỉ có 1 enemy, spawn tại đúng vị trí
            if (totalAmount == 1)
            {
                return basePosition;
            }

            // Nếu có nhiều enemy, randomize position xung quanh base position
            // Tạo một vòng tròn xung quanh base position
            float angle = (360f / totalAmount) * currentIndex;
            float radius = UnityEngine.Random.Range(1f, 3f); // Random radius từ 1-3 units

            float xOffset = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float zOffset = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

            return basePosition + new Vector3(xOffset, 0, zOffset);
        }

        private void SpawnEnemy(string enemyId, Vector3 position)
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                Debug.LogWarning($"Invalid enemy ID: {enemyId}");
                return;
            }

            Debug.Log($"Spawning enemy: {enemyId} at position: {position}");
            _enemySpawner.Spawn(enemyId, position);
        }

        /// <summary>
        /// Callback khi enemy được spawn (tương tự EnemyManager)
        /// </summary>
        private void OnEnemySpawned()
        {
            // Tracking được xử lý trong SpawnDungeonWave
            // Có thể thêm logic khác nếu cần
        }

        /// <summary>
        /// Callback khi enemy chết (tương tự EnemyManager)
        /// </summary>
        private void OnEnemyDied()
        {
            _enemyDiedCount++;

            Debug.Log($"Enemy died. Total: {_enemyDiedCount}/{_enemySpawnCount}");

            // Kiểm tra tất cả zones đã được spawn chưa
            bool allZonesSpawned = _currentZoneIndex >= _zoneList.Count;

            // Nếu tất cả zones đã spawn và tất cả enemies đã chết, trigger win
            if (allZonesSpawned && _enemySpawnCount > 0 && _enemyDiedCount >= _enemySpawnCount)
            {
                Debug.Log("All dungeon enemies cleaned! Triggering win condition.");
                EnemyCleaned?.Invoke();
                return;
            }

            // Kiểm tra zone hiện tại đã clean chưa (chỉ khi chưa spawn hết zones)
            if (!allZonesSpawned && _currentZoneIndex < _zoneList.Count)
            {
                string currentZone = _zoneList[_currentZoneIndex];
                int zoneSpawnCount = _zoneSpawnCount.GetValueOrDefault(currentZone, 0);
                int zoneDiedCount = _zoneDiedCount.GetValueOrDefault(currentZone, 0) + 1;
                _zoneDiedCount[currentZone] = zoneDiedCount;

                Debug.Log($"Enemy died in zone {currentZone}. Zone: {zoneDiedCount}/{zoneSpawnCount}, Total: {_enemyDiedCount}/{_enemySpawnCount}");

                // Kiểm tra nếu zone hiện tại đã clean
                if (zoneSpawnCount > 0 && zoneDiedCount >= zoneSpawnCount)
                {
                    Debug.Log($"Zone {currentZone} cleaned!");

                    // Spawn zone tiếp theo (tương tự spawn wave tiếp theo)
                    _currentZoneIndex++;
                    if (_currentZoneIndex < _zoneList.Count)
                    {
                        string nextZone = _zoneList[_currentZoneIndex];
                        Debug.Log($"Spawning next zone: {nextZone}");
                        SpawnDungeonWave(nextZone);
                    }
                    else
                    {
                        // Tất cả zones đã được spawn, kiểm tra win condition
                        Debug.Log("All zones spawned. Checking win condition...");
                        CheckWinCondition();
                    }
                }
            }
            else if (allZonesSpawned)
            {
                // Tất cả zones đã spawn, chỉ cần kiểm tra win condition
                CheckWinCondition();
            }
        }

        /// <summary>
        /// Kiểm tra win condition dựa trên tổng số enemies
        /// </summary>
        private void CheckWinCondition()
        {
            if (_enemySpawnCount > 0 && _enemyDiedCount >= _enemySpawnCount)
            {
                Debug.Log($"Win condition met! All enemies killed: {_enemyDiedCount}/{_enemySpawnCount}");
                EnemyCleaned?.Invoke();
            }
            else
            {
                Debug.Log($"Win condition not met yet. Enemies: {_enemyDiedCount}/{_enemySpawnCount}");
            }
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
                Debug.LogError("ParseStringToVector3: Input string is null or empty!");
                return Vector3.zero;
            }

            // Loại bỏ spaces và tách chuỗi bằng dấu phẩy
            string cleanedInput = input.Replace(" ", ""); // Loại bỏ spaces (ví dụ: "-4, 12,-70" -> "-4,12,-70")
            string[] values = cleanedInput.Split(',');

            // Đảm bảo chuỗi có đủ 3 phần tử
            if (values.Length != 3)
            {
                Debug.LogError($"ParseStringToVector3: Invalid input format '{input}'! Expected format: 'x,y,z' or 'x, y, z'. Got {values.Length} values.");
                return Vector3.zero;
            }

            // Chuyển đổi từng phần tử thành float và tạo Vector3
            try
            {
                float x = float.Parse(values[0].Trim());
                float y = float.Parse(values[1].Trim());
                float z = float.Parse(values[2].Trim());
                return new Vector3(x, y, z);
            }
            catch (System.FormatException e)
            {
                Debug.LogError($"ParseStringToVector3: Failed to parse '{input}' to Vector3. Error: {e.Message}");
                return Vector3.zero;
            }
        }
    }
}
