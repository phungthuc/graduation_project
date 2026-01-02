using System.Collections.Generic;
using System.Linq;
using TheTunnel.Enemy;
using TheTunnel.Level;
using UnityEngine;

namespace TheTunnel.Config
{
    /// <summary>
    /// </summary>
    public class LevelDataValidator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private DungeonEnemySpawner dungeonEnemySpawner;

        /// <summary>
        /// </summary>
        [ContextMenu("Validate Level Data")]
        public void ValidateLevelData()
        {
            if (GameConfig.Instance == null || GameConfig.Instance.LevelDataList == null || GameConfig.Instance.LevelDataList.Count == 0)
            {
                Debug.LogError("[LevelDataValidator] GameConfig.Instance.LevelDataList is null or empty!");
                return;
            }

            bool hasErrors = false;

            if (enemySpawner != null)
            {
                hasErrors |= ValidateDefenseLevelEnemies();
            }

            if (dungeonEnemySpawner != null)
            {
                hasErrors |= ValidateDungeonLevelEnemies();
            }

        }

        /// <summary>
        /// </summary>
        private bool ValidateDefenseLevelEnemies()
        {
            bool hasErrors = false;
            var availableEnemyIds = GetAvailableEnemyIds(enemySpawner);

            foreach (var levelData in GameConfig.Instance.LevelDataList)
            {
                if (levelData.WaveList == null) continue;

                foreach (var wave in levelData.WaveList)
                {
                    if (wave.EnemyList == null) continue;

                    foreach (var enemyData in wave.EnemyList)
                    {
                        if (string.IsNullOrEmpty(enemyData.Id))
                        {
                            Debug.LogError($"[LevelDataValidator] Level '{levelData.Name}': Found empty enemyId in waveData!");
                            hasErrors = true;
                            continue;
                        }

                        if (!availableEnemyIds.Contains(enemyData.Id))
                        {
                            Debug.LogError($"[LevelDataValidator] Level '{levelData.Name}': Enemy ID '{enemyData.Id}' not found in EnemySpawner.enemyDataList! " +
                                $"Available IDs: {string.Join(", ", availableEnemyIds)}");
                            hasErrors = true;
                        }

                        if (enemyData.Amount <= 0)
                        {
                            Debug.LogWarning($"[LevelDataValidator] Level '{levelData.Name}': Enemy ID '{enemyData.Id}' has invalid amount: {enemyData.Amount}");
                        }
                    }
                }
            }

            return hasErrors;
        }

        /// <summary>
        /// </summary>
        private bool ValidateDungeonLevelEnemies()
        {
            bool hasErrors = false;
            var availableEnemyIds = GetAvailableDungeonEnemyIds(dungeonEnemySpawner);

            foreach (var levelData in GameConfig.Instance.LevelDataList)
            {
                if (levelData.DungeonData == null || levelData.DungeonData.EnemySpawnData == null) continue;

                foreach (var zoneKvp in levelData.DungeonData.EnemySpawnData)
                {
                    string zoneName = zoneKvp.Key;
                    var spawnPoints = zoneKvp.Value;

                    if (spawnPoints == null) continue;

                    foreach (var spawnPoint in spawnPoints)
                    {
                        if (spawnPoint.EnemyData == null) continue;

                        foreach (var enemyData in spawnPoint.EnemyData)
                        {
                            if (string.IsNullOrEmpty(enemyData.Id))
                            {
                                Debug.LogError($"[LevelDataValidator] Level '{levelData.Name}', Zone '{zoneName}': Found empty enemyId!");
                                hasErrors = true;
                                continue;
                            }

                            if (!availableEnemyIds.Contains(enemyData.Id))
                            {
                                Debug.LogError($"[LevelDataValidator] Level '{levelData.Name}', Zone '{zoneName}': Enemy ID '{enemyData.Id}' not found in DungeonEnemySpawner.enemyDataList! " +
                                    $"Available IDs: {string.Join(", ", availableEnemyIds)}");
                                hasErrors = true;
                            }

                            if (enemyData.Amount <= 0)
                            {
                                Debug.LogWarning($"[LevelDataValidator] Level '{levelData.Name}', Zone '{zoneName}': Enemy ID '{enemyData.Id}' has invalid amount: {enemyData.Amount}");
                            }
                        }
                    }
                }
            }

            return hasErrors;
        }

        /// <summary>
        /// </summary>
        private HashSet<string> GetAvailableEnemyIds(EnemySpawner spawner)
        {
            var enemyIds = new HashSet<string>();

            if (spawner == null) return enemyIds;

            var enemyDataListField = typeof(EnemySpawner).GetField("enemyDataList",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (enemyDataListField != null)
            {
                var enemyDataList = enemyDataListField.GetValue(spawner) as List<EnemyData>;
                if (enemyDataList != null)
                {
                    foreach (var enemyData in enemyDataList)
                    {
                        if (enemyData != null && !string.IsNullOrEmpty(enemyData.id))
                        {
                            enemyIds.Add(enemyData.id);
                        }
                    }
                }
            }

            return enemyIds;
        }

        /// <summary>
        /// </summary>
        private HashSet<string> GetAvailableDungeonEnemyIds(DungeonEnemySpawner spawner)
        {
            var enemyIds = new HashSet<string>();

            if (spawner == null) return enemyIds;

            var enemyDataListField = typeof(DungeonEnemySpawner).GetField("enemyDataList",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (enemyDataListField != null)
            {
                var enemyDataList = enemyDataListField.GetValue(spawner) as List<DungeonEnemyData>;
                if (enemyDataList != null)
                {
                    foreach (var enemyData in enemyDataList)
                    {
                        if (enemyData != null && !string.IsNullOrEmpty(enemyData.id))
                        {
                            enemyIds.Add(enemyData.id);
                        }
                    }
                }
            }

            return enemyIds;
        }

        /// <summary>
        /// </summary>
        [ContextMenu("Check TextAsset Assignment")]
        public void CheckTextAssetAssignment()
        {
            var gameConfigLoader = FindFirstObjectByType<GameConfigLoader>();
            if (gameConfigLoader == null)
            {
                Debug.LogError("[LevelDataValidator] GameConfigLoader not found in scene!");
                return;
            }

            var levelDataField = typeof(GameConfigLoader).GetField("levelData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (levelDataField != null)
            {
                var levelDataAsset = levelDataField.GetValue(gameConfigLoader) as TextAsset;
                if (levelDataAsset == null)
                {
                    Debug.LogError("[LevelDataValidator] GameConfigLoader.levelData TextAsset is not assigned!");
                }
                else if (string.IsNullOrEmpty(levelDataAsset.text))
                {
                    Debug.LogError("[LevelDataValidator] GameConfigLoader.levelData TextAsset is empty!");
                }
            }
        }

        /// <summary>
        /// </summary>
        [ContextMenu("Generate Enemy Data Report")]
        public void GenerateEnemyDataReport()
        {
            if (GameConfig.Instance == null || GameConfig.Instance.LevelDataList == null)
            {
                Debug.LogError("[LevelDataValidator] GameConfig.Instance.LevelDataList is null!");
                return;
            }

            var defenseEnemyIds = new HashSet<string>();
            var dungeonEnemyIds = new HashSet<string>();

            foreach (var levelData in GameConfig.Instance.LevelDataList)
            {
                if (levelData.WaveList != null)
                {
                    foreach (var wave in levelData.WaveList)
                    {
                        if (wave.EnemyList != null)
                        {
                            foreach (var enemy in wave.EnemyList)
                            {
                                if (!string.IsNullOrEmpty(enemy.Id))
                                {
                                    defenseEnemyIds.Add(enemy.Id);
                                }
                            }
                        }
                    }
                }

                if (levelData.DungeonData != null && levelData.DungeonData.EnemySpawnData != null)
                {
                    foreach (var zone in levelData.DungeonData.EnemySpawnData.Values)
                    {
                        if (zone != null)
                        {
                            foreach (var spawnPoint in zone)
                            {
                                if (spawnPoint.EnemyData != null)
                                {
                                    foreach (var enemy in spawnPoint.EnemyData)
                                    {
                                        if (!string.IsNullOrEmpty(enemy.Id))
                                        {
                                            dungeonEnemyIds.Add(enemy.Id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

