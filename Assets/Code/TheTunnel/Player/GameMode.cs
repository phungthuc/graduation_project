using TheTunnel.Core;
using TheTunnel.Manager;
using Unity.Netcode;
using UnityEngine;

namespace TheTunnel.Player
{

    public class GameMode : NetworkBehaviour
    {
        [Header("Player Spawn")]
        [SerializeField] private NetworkObject playerPrefab;

        private bool playersSpawned = false;

        private GameManager gameManager;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
        }

        private new void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (!IsServer) return;

            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
            {
                if (!playersSpawned)
                {
                    playersSpawned = true;
                    SpawnPlayersForAllClients();
                }
            }
        }

        private void SpawnPlayersForAllClients()
        {
            Debug.Log("Spawning players for all clients... " + NetworkManager.Singleton.ConnectedClientsList.Count);

            // Đảm bảo scene đã load xong và spawn point đã sẵn sàng
            StartCoroutine(SpawnPlayersCoroutine());
        }

        private System.Collections.IEnumerator SpawnPlayersCoroutine()
        {
            // Đợi một frame để đảm bảo scene đã hoàn toàn load xong
            yield return null;

            // Tìm spawn point với retry logic
            Transform spawnPoint = null;
            int maxRetries = 10;
            int retryCount = 0;

            while (spawnPoint == null && retryCount < maxRetries)
            {
                GameObject spawnPointObj = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
                if (spawnPointObj != null)
                {
                    spawnPoint = spawnPointObj.transform;
                }
                else
                {
                    retryCount++;
                    Debug.LogWarning($"Spawn point not found, retrying... ({retryCount}/{maxRetries})");
                    yield return new WaitForSeconds(0.1f);
                }
            }

            if (spawnPoint == null)
            {
                Debug.LogError("PlayerSpawnPoint not found after retries! Using default position.");
            }

            Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            Debug.Log($"Spawn point found at: {pos} with rotation: {rot}");

            // Spawn players cho tất cả clients
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                ulong clientId = client.ClientId;

                Debug.Log($"Spawning player for client {clientId} at: {pos} with rotation: {rot}");

                // Server instantiate Player
                NetworkObject playerInstance = Instantiate(playerPrefab, pos, rot);

                // Gắn Player này cho clientId (bao gồm cả Host)
                playerInstance.SpawnAsPlayerObject(clientId);

                // Đợi một frame để NetworkTransform sync
                yield return null;

                // Đảm bảo position được set đúng sau khi spawn
                // Kiểm tra lại position và set lại nếu cần
                if (playerInstance != null && playerInstance.IsSpawned)
                {
                    // Sử dụng TeleportPlayer nếu có PlayerMovement component
                    var playerMovement = playerInstance.GetComponent<cowsins.PlayerMovement>();
                    if (playerMovement != null)
                    {
                        playerMovement.TeleportPlayer(pos, rot);
                        Debug.Log($"Teleported player {clientId} to {pos} via PlayerMovement");
                    }
                    else
                    {
                        // Fallback: Set trực tiếp transform
                        // Xử lý CharacterController nếu có
                        if (playerInstance.TryGetComponent<UnityEngine.CharacterController>(out var characterController))
                        {
                            characterController.enabled = false;
                            playerInstance.transform.position = pos;
                            playerInstance.transform.rotation = rot;
                            characterController.enabled = true;
                        }
                        else
                        {
                            playerInstance.transform.position = pos;
                            playerInstance.transform.rotation = rot;
                        }
                        Debug.Log($"Set player {clientId} position to {pos} via transform");
                    }

                    // Đợi thêm một frame để đảm bảo NetworkTransform đã sync
                    yield return null;

                    // Verify position sau khi set và retry nếu cần
                    if (playerInstance != null && playerInstance.IsSpawned)
                    {
                        if (Vector3.Distance(playerInstance.transform.position, pos) > 0.1f)
                        {
                            Debug.LogWarning($"Player {clientId} position mismatch! Expected: {pos}, Actual: {playerInstance.transform.position}. Retrying...");
                            // Retry setting position
                            playerMovement = playerInstance.GetComponent<cowsins.PlayerMovement>();
                            if (playerMovement != null)
                            {
                                playerMovement.TeleportPlayer(pos, rot);
                            }
                            else
                            {
                                if (playerInstance.TryGetComponent<UnityEngine.CharacterController>(out var characterController))
                                {
                                    characterController.enabled = false;
                                    playerInstance.transform.position = pos;
                                    playerInstance.transform.rotation = rot;
                                    characterController.enabled = true;
                                }
                                else
                                {
                                    playerInstance.transform.position = pos;
                                    playerInstance.transform.rotation = rot;
                                }
                            }
                        }
                    }
                }
            }

            Debug.Log("All players spawned successfully.");

            // Gọi gameManager logic sau khi spawn xong
            if (gameManager != null)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == GameConstant.SCENE_DEFENSE_NAME) //check current scene is defense level
                {
                    gameManager.StartCountDown();
                }
                else
                {
                    gameManager.LoadDungeonLevel();
                }
            }
        }
    }
}