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

            // Tìm spawn point
            Transform spawnPoint = FindSpawnPoint();
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            Debug.Log($"Spawn point found at: {spawnPosition} with rotation: {spawnRotation}");

            // Xử lý spawn/update position cho tất cả clients
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                ulong clientId = client.ClientId;
                NetworkObject existingPlayer = client.PlayerObject;

                if (existingPlayer != null && existingPlayer.IsSpawned)
                {
                    // Client đã có player, chỉ cần update position
                    Debug.Log($"Client {clientId} already has player. Updating position to spawn point.");
                    SetPlayerPosition(existingPlayer, spawnPosition, spawnRotation);

                    // Đợi một chút để NetworkTransform sync position đến clients
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    // Client chưa có player, spawn mới
                    Debug.Log($"Spawning new player for client {clientId} at: {spawnPosition}");
                    NetworkObject newPlayer = Instantiate(playerPrefab, spawnPosition, spawnRotation);
                    newPlayer.SpawnAsPlayerObject(clientId);

                    // Đợi một frame để spawn hoàn tất
                    yield return null;

                    // Đảm bảo position được set đúng
                    if (newPlayer != null && newPlayer.IsSpawned)
                    {
                        SetPlayerPosition(newPlayer, spawnPosition, spawnRotation);

                        // Đợi thêm một chút để NetworkTransform sync position đến clients
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }

            Debug.Log("All players processed successfully.");

            // Gọi gameManager logic sau khi spawn xong
            if (gameManager != null)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == GameConstant.SCENE_DEFENSE_NAME)
                {
                    gameManager.StartCountDown();
                }
                else
                {
                    gameManager.LoadDungeonLevel();
                }
            }
        }

        /// <summary>
        /// Tìm spawn point với retry logic
        /// </summary>
        private Transform FindSpawnPoint()
        {
            GameObject spawnPointObj = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
            if (spawnPointObj != null)
            {
                return spawnPointObj.transform;
            }

            Debug.LogWarning("PlayerSpawnPoint not found! Using default position (0,0,0).");
            return null;
        }

        /// <summary>
        /// Set position và rotation cho player (đảm bảo sync đúng cho tất cả clients)
        /// Sử dụng ClientRpc để đảm bảo client tự set position (có authority)
        /// </summary>
        private void SetPlayerPosition(NetworkObject playerObject, Vector3 position, Quaternion rotation)
        {
            if (playerObject == null || !playerObject.IsSpawned)
            {
                Debug.LogWarning("Cannot set position: Player object is null or not spawned.");
                return;
            }

            // Set trên server trước
            if (playerObject.TryGetComponent<Rigidbody>(out var rigidbody))
            {
                playerObject.transform.position = position;
                playerObject.transform.rotation = rotation;
                rigidbody.position = position;
                rigidbody.rotation = rotation;
                rigidbody.linearVelocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }
            else
            {
                playerObject.transform.position = position;
                playerObject.transform.rotation = rotation;
            }

            // Gọi ClientRpc để client tự set position (có authority với NetworkTransform)
            // Chỉ gửi đến client owner của player này
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { playerObject.OwnerClientId }
                }
            };
            TeleportPlayerClientRpc(playerObject.NetworkObjectId, position, rotation, clientRpcParams);

            Debug.Log($"[Server] Requested teleport for player {playerObject.OwnerClientId} to {position}");
        }

        /// <summary>
        /// ClientRpc để client tự teleport player (có authority với NetworkTransform)
        /// </summary>
        [ClientRpc]
        private void TeleportPlayerClientRpc(ulong networkObjectId, Vector3 position, Quaternion rotation, ClientRpcParams rpcParams = default)
        {
            // Tìm player object từ networkObjectId
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var playerObject))
            {
                // Chỉ owner mới có thể teleport với NetworkTransform
                if (playerObject.IsOwner)
                {
                    // Sử dụng NetworkTransform.Teleport() nếu có
                    var networkTransform = playerObject.GetComponent<Unity.Netcode.Components.NetworkTransform>();
                    if (networkTransform != null)
                    {
                        networkTransform.Teleport(position, rotation, playerObject.transform.localScale);
                        Debug.Log($"[Client] Teleported own player to {position} via NetworkTransform");
                    }

                    // Đồng thời set trực tiếp để đảm bảo ngay lập tức
                    if (playerObject.TryGetComponent<Rigidbody>(out var rigidbody))
                    {
                        playerObject.transform.position = position;
                        playerObject.transform.rotation = rotation;
                        rigidbody.position = position;
                        rigidbody.rotation = rotation;
                        rigidbody.linearVelocity = Vector3.zero;
                        rigidbody.angularVelocity = Vector3.zero;
                    }
                    else
                    {
                        playerObject.transform.position = position;
                        playerObject.transform.rotation = rotation;
                    }

                    // Cập nhật camera rotation
                    var playerMovement = playerObject.GetComponent<cowsins.PlayerMovement>();
                    if (playerMovement != null && playerMovement.playerCam != null)
                    {
                        playerMovement.playerCam.rotation = rotation;
                    }
                }
                else
                {
                    // Nếu không phải owner, chỉ set transform (NetworkTransform sẽ sync từ owner)
                    playerObject.transform.position = position;
                    playerObject.transform.rotation = rotation;
                    Debug.Log($"[Client] Set remote player position to {position}");
                }
            }
        }
    }
}