using cowsins;
using TheTunnel.Components;
using TheTunnel.Config;
using TheTunnel.Core;
using TheTunnel.Custom.cowsins;
using TheTunnel.Enemy;
using TheTunnel.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheTunnel.Level
{
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager _instance;

        public static LevelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("LevelManager");
                    _instance = go.AddComponent<LevelManager>();
                }
                return _instance;
            }
        }

        private EnemyManager _enemyManager;
        private DungeonEnemyManager _dungeonEnemyManager;

        private PlayerStats _playerStats;

        private float _keyHoldTimer = 0f;
        private bool _isDungeonLevel = false;
        private bool _isExitingDungeonLevel = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // cheat exit dungeon level on hold key 3
            if (!_isDungeonLevel)
            {
                return;
            }
            if (Custom_InputManager.exitDungeonLevel || _isExitingDungeonLevel)
            {
                _keyHoldTimer += Time.deltaTime;

                if (_keyHoldTimer >= 3f)
                {
                    ExitDungeonLevel();
                    _keyHoldTimer = 0f;
                }
            }
            else
            {
                _keyHoldTimer = 0f;
            }
        }

        public void ExitDungeonLevel()
        {
            if (SceneManager.GetActiveScene().name != GameConstant.SCENE_DUNGEON_NAME)
            {
                return;
            }
            _isExitingDungeonLevel = true;
            PlayerData.Instance.SetDungeonLevelCompleted(PlayerData.Instance.CurrentLevel);
            PlayerData.Instance.CurrentLevel++;
            TransitionScene.Instance.PlayTransitionScene(GameConstant.SCENE_PLAY_NAME, () =>
            {
                _isExitingDungeonLevel = false;
                GameManager.Instance.StartCountDown();
            });
        }

        #region DEFENSE LEVEL
        public void LoadDefenseLevel(int levelIndex = 0)
        {
            if (levelIndex >= GameConfig.Instance.LevelDataList.Count)
            {
                levelIndex = GameConfig.Instance.LevelDataList.Count - 1;
            }
            GetLevelDefenseReference();
            LevelData levelData = GameConfig.Instance.LevelDataList[levelIndex];
            if (levelData == null)
            {
                Debug.LogError("Failed to load level data");
                return;
            }
            _enemyManager.LoadWaveData(levelData.WaveList);
            _isDungeonLevel = false;
            RegisterDefenseLevelEvents();
        }

        public void StopDefenseLevel()
        {
            _enemyManager.StopWave();
        }

        private void GetLevelDefenseReference()
        {
            _enemyManager = FindAnyObjectByType<EnemyManager>();
            if (_enemyManager == null)
            {
                Debug.LogError("EnemyManager component not found");
                return;
            }
        }

        private void RegisterDefenseLevelEvents()
        {
            _playerStats = FindAnyObjectByType<PlayerStats>();
            if (_playerStats == null)
            {
                Debug.LogError("PlayerStats component not found");
                return;
            }
            _playerStats.events.OnDeath.RemoveListener(OnPlayerDied);
            _playerStats.events.OnDeath.AddListener(OnPlayerDied);
        }

        private void OnPlayerDied()
        {
            StopDefenseLevel();
        }

        #endregion

        #region DUNGEON LEVEL
        public void LoadDungeonLevel(int levelIndex = 0)
        {
            if (levelIndex >= GameConfig.Instance.LevelDataList.Count)
            {
                levelIndex = GameConfig.Instance.LevelDataList.Count - 1;
            }
            GetLevelDungeonReference();
            LevelData levelData = GameConfig.Instance.LevelDataList[levelIndex];
            if (levelData == null)
            {
                Debug.LogError("Failed to load level data");
                return;
            }

            _dungeonEnemyManager.LoadDungeonData(levelData.DungeonData);
            GameObject dungeon = Resources.Load<GameObject>($"Dungeons/{levelData.DungeonId}");
            if (dungeon == null)
            {
                Debug.LogError($"Failed to load dungeon prefab: {levelData.DungeonId}");
                return;
            }
            _isDungeonLevel = true;
            Instantiate(dungeon);
        }

        private void GetLevelDungeonReference()
        {
            _dungeonEnemyManager = FindAnyObjectByType<DungeonEnemyManager>();
            if (_dungeonEnemyManager == null)
            {
                Debug.LogError("DungeonEnemyManager component not found");
                return;
            }
        }
        #endregion
    }
}
