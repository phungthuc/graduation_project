using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        [JsonConverter(typeof(SpawnAreaDataConverter))]
        public Dictionary<string, List<SpawnAreaData>> EnemySpawnData { get; set; } = new();
    }

    public class SpawnAreaData
    {
        [JsonProperty("spawnPosition")]
        public string SpawnPosition { get; set; }

        [JsonProperty("enemyData")]
        public List<EnemyPerWaveData> EnemyData { get; set; } = new();
    }

    public class SpawnAreaDataConverter : JsonConverter<Dictionary<string, List<SpawnAreaData>>>
    {
        public override Dictionary<string, List<SpawnAreaData>> ReadJson(JsonReader reader, Type objectType, Dictionary<string, List<SpawnAreaData>> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var result = new Dictionary<string, List<SpawnAreaData>>();
            JObject obj = JObject.Load(reader);

            foreach (var property in obj.Properties())
            {
                string areaName = property.Name;
                JToken value = property.Value;

                List<SpawnAreaData> spawnPoints = new List<SpawnAreaData>();

                if (value.Type == JTokenType.Array)
                {
                    foreach (var item in value)
                    {
                        SpawnAreaData spawnData = item.ToObject<SpawnAreaData>(serializer);
                        if (spawnData != null)
                        {
                            spawnPoints.Add(spawnData);
                        }
                    }
                }
                else if (value.Type == JTokenType.Object)
                {
                    SpawnAreaData spawnData = value.ToObject<SpawnAreaData>(serializer);
                    if (spawnData != null)
                    {
                        spawnPoints.Add(spawnData);
                    }
                }

                result[areaName] = spawnPoints;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, Dictionary<string, List<SpawnAreaData>> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key);

                if (kvp.Value.Count == 1)
                {
                    serializer.Serialize(writer, kvp.Value[0]);
                }
                else
                {
                    writer.WriteStartArray();
                    foreach (var spawnData in kvp.Value)
                    {
                        serializer.Serialize(writer, spawnData);
                    }
                    writer.WriteEndArray();
                }
            }

            writer.WriteEndObject();
        }
    }
}