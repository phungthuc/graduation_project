using UnityEngine;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using Unity.Multiplayer.Widgets;
using System;

namespace TheTunnel.Multiplayer
{
    /// <summary>
    /// Xử lý lỗi khi join session và đảm bảo NetworkManager sẵn sàng
    /// Fix lỗi: SessionException: Unexpected exception processing network metadata
    /// </summary>
    public class SessionErrorHandler : MonoBehaviour
    {
        private void OnEnable()
        {
            // Subscribe to session events nếu có
            try
            {
                // Đảm bảo NetworkManager không đang chạy trước khi join
                EnsureNetworkManagerReady();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in SessionErrorHandler.OnEnable: {e.Message}");
            }
        }

        /// <summary>
        /// Đảm bảo NetworkManager ở trạng thái sẵn sàng để Unity Services có thể khởi tạo
        /// </summary>
        public static void EnsureNetworkManagerReady()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("NetworkManager.Singleton is null! Cannot proceed with session join.");
                return;
            }

            // Nếu NetworkManager đang chạy, shutdown nó để Unity Services có thể khởi tạo lại
            if (NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
            {
                Debug.Log("NetworkManager is already running. Shutting down to allow Unity Services to initialize it properly.");

                try
                {
                    NetworkManager.Singleton.Shutdown();

                    // Đợi một frame để shutdown hoàn tất
                    // Note: Trong thực tế, bạn nên đợi callback OnClientDisconnectCallback hoặc OnServerStoppedCallback
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error shutting down NetworkManager: {e.Message}");
                }
            }

            // Validate network configuration
            var config = NetworkManager.Singleton.NetworkConfig;
            if (config == null)
            {
                Debug.LogError("NetworkManager.NetworkConfig is null!");
                return;
            }

            // Đảm bảo các cấu hình quan trọng
            if (!config.ForceSamePrefabs)
            {
                Debug.LogWarning("ForceSamePrefabs is disabled. Enabling it to prevent prefab mismatch.");
                config.ForceSamePrefabs = true;
            }

            if (!config.EnableSceneManagement)
            {
                Debug.LogWarning("EnableSceneManagement is disabled. Enabling it for Unity Services.");
                config.EnableSceneManagement = true;
            }

            if (config.UseCMBService)
            {
                Debug.LogWarning("UseCMBService is enabled. Disabling it when using Unity Services Multiplayer.");
                config.UseCMBService = false;
            }

            Debug.Log("NetworkManager is ready for Unity Services Multiplayer session join.");
        }

        /// <summary>
        /// Gọi trước khi join session để đảm bảo mọi thứ sẵn sàng
        /// </summary>
        public void OnBeforeJoinSession()
        {
            EnsureNetworkManagerReady();
        }
    }
}
