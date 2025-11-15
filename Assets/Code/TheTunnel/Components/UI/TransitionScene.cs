using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_NETCODE_GAMEOBJECTS
using Unity.Netcode;
#endif

namespace TheTunnel
{
    public class TransitionScene : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private Animator animator;
        #endregion

        #region Private Fields
        private static TransitionScene instance;

        private Action _sceneLoadedCallback;

        private string _targetSceneName;
        private AsyncOperation _asyncOperation;
        #endregion

        #region Public Properties
        public static TransitionScene Instance
        {
            get { return instance; }
        }
        #endregion

        #region Unity Lifecycle
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
        #endregion

        #region Public Methods
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
        #endregion

        #region Private Methods
        private IEnumerator LoadSceneAsync()
        {
#if UNITY_NETCODE_GAMEOBJECTS
            // Check if NetworkManager exists and is active
            if (IsNetworkActive())
            {
                yield return StartCoroutine(LoadSceneNetwork());
            }
            else
            {
                yield return StartCoroutine(LoadSceneLocal());
            }
#else
            yield return StartCoroutine(LoadSceneLocal());
#endif
        }

#if UNITY_NETCODE_GAMEOBJECTS
        private bool IsNetworkActive()
        {
            return NetworkManager.Singleton != null && 
                   NetworkManager.Singleton.IsListening && 
                   NetworkManager.Singleton.SceneManager != null;
        }

        private IEnumerator LoadSceneNetwork()
        {
            var networkManager = NetworkManager.Singleton;
            var sceneManager = networkManager.SceneManager;

            // Only server can load scenes in network mode
            if (networkManager.IsServer)
            {
                // Load scene on server (will sync to all clients automatically)
                sceneManager.LoadScene(_targetSceneName, LoadSceneMode.Single);

                // Wait for scene to be loaded on server
                while (SceneManager.GetActiveScene().name != _targetSceneName)
                {
                    yield return null;
                }

                // Wait one frame to ensure all objects are initialized
                yield return null;
            }
            else
            {
                // Client: Wait for server to load the scene
                // NetworkSceneManager will automatically sync scene changes
                string currentScene = SceneManager.GetActiveScene().name;
                float timeout = 10f; // 10 second timeout
                float elapsed = 0f;

                while (currentScene == SceneManager.GetActiveScene().name && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (elapsed >= timeout)
                {
                    Debug.LogWarning($"[TransitionScene] Timeout waiting for network scene load: {_targetSceneName}");
                }
            }

            // Wait one frame to ensure all objects are initialized
            yield return null;
            
            // Now it's safe to call the callback
            animator.SetTrigger("out");
            _sceneLoadedCallback?.Invoke();
        }
#endif

        private IEnumerator LoadSceneLocal()
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
        #endregion
    }
}
