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
            if (levelData == null)
            {
                Debug.LogError("[GameConfigLoader] levelData TextAsset is not assigned!");
                return;
            }

            if (string.IsNullOrEmpty(levelData.text))
            {
                Debug.LogError("[GameConfigLoader] levelData TextAsset is empty!");
                return;
            }

            List<LevelData> levelDataList = JsonConvert.DeserializeObject<List<LevelData>>(levelData.text);
            if (levelDataList == null || levelDataList.Count == 0)
            {
                Debug.LogError("[GameConfigLoader] Failed to deserialize level data or list is empty!");
                return;
            }

            GameConfig.Instance.LevelDataList = levelDataList;
        }
    }
}