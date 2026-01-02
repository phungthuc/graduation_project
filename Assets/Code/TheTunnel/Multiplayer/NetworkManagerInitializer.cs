using UnityEngine;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using System;
using UnityEngine.SceneManagement;

namespace TheTunnel.Multiplayer
{
    /// <summary>
    /// Đảm bảo NetworkManager được khởi tạo đúng cách trước khi Unity Services xử lý metadata
    /// Fix lỗi: SessionException: Unexpected exception processing network metadata
    /// </summary>
    public class NetworkManagerInitializer : MonoBehaviour
    {
        private static bool _isInitialized = false;
        private static string _lastSceneName = "";

        void Awake()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (_lastSceneName != "" && _lastSceneName != currentSceneName)
            {
                _isInitialized = false;
            }
            _lastSceneName = currentSceneName;

            if (_isInitialized) return;

            if (NetworkManager.Singleton == null)
            {
                return;
            }

            if (NetworkManager.Singleton.IsListening)
            {
                try
                {
                    if (NetworkManager.Singleton.IsServer)
                    {
                        NetworkManager.Singleton.Shutdown();
                    }
                    else if (NetworkManager.Singleton.IsClient)
                    {
                        NetworkManager.Singleton.Shutdown();
                    }
                }
                catch (Exception)
                {
                }
            }

            var networkConfig = NetworkManager.Singleton.NetworkConfig;
            if (networkConfig == null)
            {
                return;
            }

            if (networkConfig.Prefabs.NetworkPrefabsLists == null || networkConfig.Prefabs.NetworkPrefabsLists.Count == 0)
            {
            }

            if (!networkConfig.ForceSamePrefabs)
            {
                networkConfig.ForceSamePrefabs = true;
            }

            if (!networkConfig.EnableSceneManagement)
            {
                networkConfig.EnableSceneManagement = true;
            }

            if (networkConfig.UseCMBService)
            {
                networkConfig.UseCMBService = false;
            }

            _isInitialized = true;
        }

        void OnDestroy()
        {
            // Không reset _isInitialized ở đây vì có thể có nhiều instances
            // Chỉ reset khi scene thay đổi trong Awake()
        }
    }
}
