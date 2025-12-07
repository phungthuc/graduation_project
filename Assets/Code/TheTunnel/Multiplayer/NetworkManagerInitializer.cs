using UnityEngine;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using System;

namespace TheTunnel.Multiplayer
{
    /// <summary>
    /// Đảm bảo NetworkManager được khởi tạo đúng cách trước khi Unity Services xử lý metadata
    /// Fix lỗi: SessionException: Unexpected exception processing network metadata
    /// </summary>
    public class NetworkManagerInitializer : MonoBehaviour
    {
        private static bool _isInitialized = false;

        void Awake()
        {
            if (_isInitialized) return;

            // Đảm bảo NetworkManager tồn tại và được cấu hình đúng
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("NetworkManager.Singleton is null! Please ensure NetworkManager exists in the scene.");
                return;
            }

            // Đảm bảo NetworkManager chưa được start khi join session
            // Unity Services sẽ tự động start NetworkManager khi join
            if (NetworkManager.Singleton.IsListening)
            {
                Debug.LogWarning("NetworkManager is already listening. Shutting down to allow Unity Services to initialize it properly.");
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
                catch (Exception e)
                {
                    Debug.LogWarning($"Error shutting down NetworkManager: {e.Message}");
                }
            }

            // Đảm bảo NetworkManager có cấu hình đúng
            var networkConfig = NetworkManager.Singleton.NetworkConfig;
            if (networkConfig == null)
            {
                Debug.LogError("NetworkManager.NetworkConfig is null!");
                return;
            }

            // Kiểm tra NetworkPrefabsList
            if (networkConfig.Prefabs.NetworkPrefabsLists == null || networkConfig.Prefabs.NetworkPrefabsLists.Count == 0)
            {
                Debug.LogWarning("NetworkManager has no NetworkPrefabsLists configured. This might cause metadata processing errors.");
            }

            // Đảm bảo ForceSamePrefabs được bật để tránh mismatch
            if (!networkConfig.ForceSamePrefabs)
            {
                Debug.LogWarning("ForceSamePrefabs is disabled. Enabling it to prevent prefab mismatch errors.");
                networkConfig.ForceSamePrefabs = true;
            }

            // Đảm bảo EnableSceneManagement được bật (cần cho Unity Services)
            if (!networkConfig.EnableSceneManagement)
            {
                Debug.LogWarning("EnableSceneManagement is disabled. Enabling it for Unity Services Multiplayer.");
                networkConfig.EnableSceneManagement = true;
            }

            // Đảm bảo UseCMBService được tắt (nếu dùng Unity Services thì không dùng CMB)
            if (networkConfig.UseCMBService)
            {
                Debug.LogWarning("UseCMBService is enabled. Disabling it when using Unity Services Multiplayer.");
                networkConfig.UseCMBService = false;
            }

            _isInitialized = true;
            Debug.Log("NetworkManager initialized successfully for Unity Services Multiplayer.");
        }

        void OnDestroy()
        {
            _isInitialized = false;
        }
    }
}
