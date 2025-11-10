using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace TheTunnel
{
    public class PlayerData
    {
        #region PLAYER PREFS KEYS
        private const string CURRENT_LEVEL_PLAYER_PREF_KEY = "Player-CurrentLevel";
        private const string GAMEPLAY_HISTORY_PLAYER_PREF_KEY = "Player-GameplayHistory";
        #endregion

        public static PlayerData _instance;

        public static PlayerData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlayerData();
                }
                return _instance;
            }
        }

        private int _currentLevel = -1;

        public int CurrentLevel
        {
            get
            {
                if (_currentLevel < 0)
                {
                    _currentLevel = PlayerPrefs.GetInt(CURRENT_LEVEL_PLAYER_PREF_KEY, 0);
                }
                return _currentLevel;
            }
            set
            {
                _currentLevel = value;
                PlayerPrefs.SetInt(CURRENT_LEVEL_PLAYER_PREF_KEY, _currentLevel);
            }
        }

        // int is level index, bool is dungeon completed
        private Dictionary<int, bool> _gameplayHistory;
        public Dictionary<int, bool> GameplayHistory
        {
            get
            {
                if (_gameplayHistory == null)
                {
                    string historyString = PlayerPrefs.GetString(GAMEPLAY_HISTORY_PLAYER_PREF_KEY, "");
                    _gameplayHistory = string.IsNullOrEmpty(historyString)
                        ? new Dictionary<int, bool>()
                        : JsonConvert.DeserializeObject<Dictionary<int, bool>>(historyString);
                }
                return _gameplayHistory;
            }
            set
            {
                _gameplayHistory = value;
                string historyString = JsonConvert.SerializeObject(_gameplayHistory);
                PlayerPrefs.SetString(GAMEPLAY_HISTORY_PLAYER_PREF_KEY, historyString);
            }
        }

        public void SetDefenseLevelCompleted(int levelIndex)
        {
            if (GameplayHistory.ContainsKey(levelIndex))
            {
                return;
            }
            Dictionary<int, bool> tempHistory = new Dictionary<int, bool>(GameplayHistory);
            tempHistory[levelIndex] = false;
            GameplayHistory = tempHistory;
        }

        public void SetDungeonLevelCompleted(int levelIndex)
        {
            Dictionary<int, bool> tempHistory = new Dictionary<int, bool>(GameplayHistory);
            tempHistory[levelIndex] = true;
            GameplayHistory = tempHistory;
        }

        public bool IsDungeonCompleted(int levelIndex)
        {
            if (!GameplayHistory.ContainsKey(levelIndex))
            {
                return false;
            }
            return GameplayHistory[levelIndex]; ;
        }

        public bool IsDefenseLevelCompleted(int levelIndex)
        {
            return GameplayHistory.ContainsKey(levelIndex);
        }

        public void ResetData()
        {
            CurrentLevel = 0;
            GameplayHistory = new Dictionary<int, bool>();
        }
    }
}
