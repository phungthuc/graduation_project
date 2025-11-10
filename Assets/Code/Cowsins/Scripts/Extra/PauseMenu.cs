using UnityEngine;
using System;

namespace cowsins
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject playerUI;
        [SerializeField] private bool disablePlayerUIWhilePaused;
        [SerializeField] private CanvasGroup menu;
        [SerializeField] private float fadeSpeed;

        public static PauseMenu Instance { get; private set; }

        /// <summary>
        /// Returns the Pause State of the game
        /// </summary>
        public static bool isPaused { get; private set; }

        [HideInInspector] public PlayerStats stats;

        public event Action OnPause;
        public event Action OnUnpause;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initially, the game is not paused
            isPaused = false;
            menu.gameObject.SetActive(false);
            menu.alpha = 0;
        }

        private void Update()
        {
            HandlePauseInput();
        }

        private void HandlePauseInput()
        {
            if (InputManager.pausing)
            {
                TogglePause();
            }
        }
        
        public void UnPause()
        {
            isPaused = false;
            UnpauseControl();
            playerUI.SetActive(true);
            OnUnpause?.Invoke();
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void TogglePause()
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                PauseControl();
                menu.alpha = 1;
                menu.gameObject.SetActive(true);
                if (disablePlayerUIWhilePaused && !stats.isDead)
                {
                    playerUI.SetActive(false);
                }
            
                OnPause?.Invoke();
            }
            else
            {
                menu.gameObject.SetActive(false);
                menu.alpha = 0;
                playerUI.SetActive(true);
                UnpauseControl();
                OnUnpause?.Invoke();
            }
        }

        public void PauseControl()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            stats.LoseControl();
        }

        public void UnpauseControl()
        {
            stats.CheckIfCanGrantControl();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
