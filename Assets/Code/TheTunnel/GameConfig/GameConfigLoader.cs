using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TheTunnel.Level;
using UnityEngine;
using UnityEngine.Events;

namespace TheTunnel.Config
{
    public class GameConfigLoader : MonoBehaviour
    {
        [SerializeField] private TextAsset levelData;

        public UnityEvent Loaded;

        private void Start()
        {
            if (GameConfig.Instance.IsLoaded)
            {
                return;
            }
            LoadLevelData();
            GameConfig.Instance.IsLoaded = true;
            Loaded?.Invoke();
        }

        private void LoadLevelData()
        {
            List<LevelData> levelDataList = JsonConvert.DeserializeObject<List<LevelData>>(levelData.text);
            if (levelDataList == null)
            {
                Debug.LogError("Failed to load level data");
                return;
            }
            GameConfig.Instance.LevelDataList = levelDataList;
        }
    }
}