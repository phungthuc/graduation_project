using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheTunnel
{
    public class TransitionScene : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        private static TransitionScene instance;

        public static TransitionScene Instance
        {
            get { return instance; }
        }

        private Action _sceneLoadedCallback;

        private string _targetSceneName;
        private AsyncOperation _asyncOperation;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayTransitionScene(string sceneName, Action onSceneLoaded = null)
        {
            animator.SetTrigger("play");
            _targetSceneName = sceneName;
            _sceneLoadedCallback = onSceneLoaded;
        }

        public void ChangeScene()
        {
            StartCoroutine(LoadSceneAsync());
        }

        private IEnumerator LoadSceneAsync()
        {
            _asyncOperation = SceneManager.LoadSceneAsync(_targetSceneName);
            // Prevent scene activation until we're ready
            _asyncOperation.allowSceneActivation = false;

            while (!_asyncOperation.isDone)
            {
                // Check if the load has finished
                if (_asyncOperation.progress >= 0.9f)
                {
                    // Scene has been loaded, now we can activate it
                    _asyncOperation.allowSceneActivation = true;
                }
                yield return null;
            }

            // Wait one frame to ensure all objects are initialized
            yield return null;
            // Now it's safe to call the callback
            animator.SetTrigger("out");
            _sceneLoadedCallback?.Invoke();
        }
    }
}
