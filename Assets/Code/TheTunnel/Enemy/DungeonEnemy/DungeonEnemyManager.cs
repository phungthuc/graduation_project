using System.Collections.Generic;
using cowsins;
using TheTunnel.Enemy;
using TheTunnel.Level;
using UnityEngine;
using UnityEngine.Events;

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
        private void Awake()
        {
            _enemySpawner = GetComponent<DungeonEnemySpawner>();
            if (_enemySpawner == null)
            {
                Debug.LogError("EnemySpawner component not found");
            }
            _enemySpawner.EnemyDied += OnEnemyDied;
        }

        public void LoadDungeonData(DungeonData dungeonData)
        {
            currentDungeonData = dungeonData;
            _enemyDiedCount = 0;
            SpawnPlayer();
        }

        public void SpawnDungeonWave(string zoneName)
        {
            if (currentDungeonData == null ||
                !currentDungeonData.EnemySpawnData.ContainsKey(zoneName))
            {
                Debug.LogWarning($"No spawn data found for zone: {zoneName}");
                return;
            }

            var spawnData = currentDungeonData.EnemySpawnData[zoneName];
            foreach (var enemyData in spawnData.EnemyData)
            {
                for (int i = 0; i < enemyData.Amount; i++)
                {
                    SpawnEnemy(enemyData.Id);
                }
                _enemySpawnCount += enemyData.Amount;
            }
        }

        private void SpawnEnemy(string enemyId)
        {
            Vector3 randomPosition = new Vector3(-4, -20, 27);
            _enemySpawner.Spawn(enemyId, randomPosition);
        }

        private void OnEnemyDied()
        {
            _enemyDiedCount++;
            if (_enemyDiedCount >= _enemySpawnCount)
            {
                EnemyCleaned?.Invoke();
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
            // Tách chuỗi bằng dấu phẩy
            string[] values = input.Split(',');

            // Đảm bảo chuỗi có đủ 3 phần tử
            if (values.Length != 3)
            {
                Debug.LogError("Invalid input format for Vector3!");
                return Vector3.zero;
            }

            // Chuyển đổi từng phần tử thành float và tạo Vector3
            return new Vector3(
                float.Parse(values[0]),
                float.Parse(values[1]),
                float.Parse(values[2])
            );
        }
    }
}
