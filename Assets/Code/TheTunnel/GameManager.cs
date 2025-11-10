using UnityEngine;
using TheTunnel.Core;
using TheTunnel.Level;
using TMPro;
using TheTunnel.Components;
using cowsins;

namespace TheTunnel.Manager
{
    public class GameManager : MonoBehaviour
    {
        private TimeCounter _timeCounter;

        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
                return _instance;
            }
        }


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

        public void StartCountDown()
        {
            if (_timeCounter == null)
            {
                GetReferences();
            }
            _timeCounter.StartTimer(GameConstant.TIME_GAP_BETWEEN_LEVEL_BY_SECONDS);
            UIEvents.countDownStarted?.Invoke();
        }

        private void GetReferences()
        {
            _timeCounter = GetComponent<TimeCounter>();
            if (_timeCounter == null)
            {
                _timeCounter = gameObject.AddComponent<TimeCounter>();
            }
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            _timeCounter.onTimerTick -= OnTimerTick;
            _timeCounter.onTimerComplete -= OnTimerComplete;

            _timeCounter.onTimerTick += OnTimerTick;
            _timeCounter.onTimerComplete += OnTimerComplete;
        }

        private void OnTimerTick(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            string formattedTime = $"{minutes:00}:{seconds:00}";

            UIEvents.countDownUpdated?.Invoke(formattedTime);
        }

        private void OnTimerComplete()
        {
            UIEvents.countDownFinished?.Invoke();
            LevelManager.Instance.LoadDefenseLevel(PlayerData.Instance.CurrentLevel);
        }
    }
}
