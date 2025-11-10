using UnityEngine;
using UnityEngine.Events;
using TheTunnel.Components;
using cowsins;
using System;

namespace TheTunnel.Components
{
    public class TimeCounter : MonoBehaviour
    {
        public event Action onTimerComplete;
        public event Action<float> onTimerTick;

        private float currentTime;
        private bool isCountingDown;
        private bool isPaused;

        public void StartTimer(float duration)
        {
            currentTime = duration;
            isCountingDown = true;
            isPaused = false;
        }

        public void PauseTimer()
        {
            isPaused = true;
        }

        public void ResumeTimer()
        {
            isPaused = false;
        }

        private void Update()
        {
            if (!isCountingDown || isPaused || PauseMenu.isPaused) return;

            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                isCountingDown = false;
                onTimerComplete?.Invoke();
            }

            onTimerTick?.Invoke(currentTime);
        }
    }
}
