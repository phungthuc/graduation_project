using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TheTunnel.Level
{
    public class LevelData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("waveData")]
        public List<WaveData> WaveList { get; set; } = new();

        [JsonProperty("dungeon")]
        public string DungeonId { get; set; }

        [JsonProperty("dungeonData")]
        public DungeonData DungeonData { get; set; }
    }

    public class WaveData
    {
        [JsonProperty("enemyData")]
        public List<EnemyPerWaveData> EnemyList { get; set; } = new();

        [JsonProperty("timeNextWave")]
        public float TimeNextWave { get; set; }
    }

    public class EnemyPerWaveData
    {
        [JsonProperty("enemyId")]
        public string Id { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }
    }

    public class DungeonData
    {
        [JsonProperty("playerPosition")]
        public string PlayerPosition { get; set; }

        [JsonProperty("enemySpawnData")]
        public Dictionary<string, SpawnAreaData> EnemySpawnData { get; set; } = new();
    }

    public class SpawnAreaData
    {
        [JsonProperty("spawnPosition")]
        public string SpawnPosition { get; set; }

        [JsonProperty("enemyData")]
        public List<EnemyPerWaveData> EnemyData { get; set; } = new();
    }
}