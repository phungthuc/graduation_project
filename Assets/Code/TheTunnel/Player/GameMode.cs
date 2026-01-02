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

            // Subscribe to disconnect callbacks
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }

        private new void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
            }
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
                    // playersSpawned = true;
                    SpawnPlayersForAllClients();
                }
            }
        }

        private void SpawnPlayersForAllClients()
        {
            StartCoroutine(SpawnPlayersCoroutine());
        }

        private System.Collections.IEnumerator SpawnPlayersCoroutine()
        {
            yield return null;

            Transform spawnPoint = FindSpawnPoint();
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                ulong clientId = client.ClientId;
                NetworkObject existingPlayer = client.PlayerObject;

                if (existingPlayer != null && existingPlayer.IsSpawned)
                {
                    SetPlayerPosition(existingPlayer, spawnPosition, spawnRotation);

                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    NetworkObject newPlayer = Instantiate(playerPrefab, spawnPosition, spawnRotation);
                    newPlayer.SpawnAsPlayerObject(clientId);

                    yield return null;

                    if (newPlayer != null && newPlayer.IsSpawned)
                    {
                        SetPlayerPosition(newPlayer, spawnPosition, spawnRotation);

                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }

            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

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
        /// </summary>
        private Transform FindSpawnPoint()
        {
            GameObject spawnPointObj = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
            if (spawnPointObj != null)
            {
                return spawnPointObj.transform;
            }

            return null;
        }

        /// <summary>
        /// </summary>
        private void SetPlayerPosition(NetworkObject playerObject, Vector3 position, Quaternion rotation)
        {
            if (playerObject == null || !playerObject.IsSpawned)
            {
                return;
            }

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

            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { playerObject.OwnerClientId }
                }
            };
            TeleportPlayerClientRpc(playerObject.NetworkObjectId, position, rotation, clientRpcParams);
        }

        /// <summary>
        /// </summary>
        [ClientRpc]
        private void TeleportPlayerClientRpc(ulong networkObjectId, Vector3 position, Quaternion rotation, ClientRpcParams rpcParams = default)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var playerObject))
            {
                if (playerObject.IsOwner)
                {
                    var networkTransform = playerObject.GetComponent<Unity.Netcode.Components.NetworkTransform>();
                    if (networkTransform != null)
                    {
                        networkTransform.Teleport(position, rotation, playerObject.transform.localScale);
                    }

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

                    var playerMovement = playerObject.GetComponent<cowsins.PlayerMovement>();
                    if (playerMovement != null && playerMovement.playerCam != null)
                    {
                        playerMovement.playerCam.rotation = rotation;
                    }
                }
                else
                {
                    playerObject.transform.position = position;
                    playerObject.transform.rotation = rotation;
                }
            }
        }

        /// <summary>
        /// Handle client disconnect: destroy player object
        /// Note: This is called on the server when a client disconnects
        /// </summary>
        private void HandleClientDisconnect(ulong clientId)
        {
            if (!IsServer) return;

            var networkManager = NetworkManager.Singleton;
            if (networkManager == null) return;

            // Check if disconnected client is the host
            bool isHostDisconnecting = clientId == networkManager.LocalClientId;

            // Get the player object for the disconnected client
            // Note: The client may already be removed from ConnectedClients, so we search in SpawnedObjects
            NetworkObject playerObjectToDestroy = null;

            foreach (var spawnedObject in networkManager.SpawnManager.SpawnedObjects.Values)
            {
                if (spawnedObject != null &&
                    spawnedObject.IsSpawned &&
                    spawnedObject.IsPlayerObject &&
                    spawnedObject.OwnerClientId == clientId)
                {
                    playerObjectToDestroy = spawnedObject;
                    break;
                }
            }

            // If not found in SpawnedObjects, try ConnectedClients (may still be there)
            if (playerObjectToDestroy == null &&
                networkManager.ConnectedClients.TryGetValue(clientId, out var disconnectedClient))
            {
                playerObjectToDestroy = disconnectedClient.PlayerObject;
            }

            // Destroy the player object if found
            if (playerObjectToDestroy != null && playerObjectToDestroy.IsSpawned)
            {
                DestroyPlayerObject(playerObjectToDestroy);
            }

            // Handle host disconnect: cleanup and prepare for host migration
            if (isHostDisconnecting)
            {
                HandleHostDisconnect();
            }
        }

        /// <summary>
        /// Destroy player NetworkObject completely
        /// </summary>
        private void DestroyPlayerObject(NetworkObject playerObject)
        {
            if (playerObject == null || !playerObject.IsSpawned) return;

            // Despawn the NetworkObject first (this will sync to all clients)
            playerObject.Despawn();

            // Then destroy the GameObject
            Destroy(playerObject.gameObject);
        }

        /// <summary>
        /// Handle host disconnect: cleanup host's player objects before shutdown
        /// Note: With Unity Services Multiplayer, host migration is handled automatically
        /// This method ensures all host's player objects are properly cleaned up
        /// </summary>
        private void HandleHostDisconnect()
        {
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null) return;

            // Clean up all player objects owned by the disconnecting host
            CleanupHostPlayerObjects();

            // Get remaining clients (excluding the disconnecting host)
            var remainingClients = new System.Collections.Generic.List<ulong>();
            foreach (var client in networkManager.ConnectedClientsList)
            {
                if (client.ClientId != networkManager.LocalClientId)
                {
                    remainingClients.Add(client.ClientId);
                }
            }

            // If there are remaining clients, Unity Services will automatically handle host migration
            // The new host will be promoted automatically by Unity Services
            // We just need to ensure cleanup is done before shutdown

            // If no remaining clients, the network will shutdown automatically
            // With Unity Services, if there are remaining clients, they will continue the session
            // and a new host will be automatically promoted
        }

        /// <summary>
        /// Clean up all player objects owned by the disconnecting host
        /// </summary>
        private void CleanupHostPlayerObjects()
        {
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null) return;

            ulong hostClientId = networkManager.LocalClientId;

            // Find all spawned NetworkObjects and destroy those owned by the disconnecting host
            var objectsToDestroy = new System.Collections.Generic.List<NetworkObject>();

            foreach (var spawnedObject in networkManager.SpawnManager.SpawnedObjects.Values)
            {
                // Check if this is a player object owned by the disconnecting host
                if (spawnedObject != null &&
                    spawnedObject.IsSpawned &&
                    spawnedObject.IsPlayerObject &&
                    spawnedObject.OwnerClientId == hostClientId)
                {
                    objectsToDestroy.Add(spawnedObject);
                }
            }

            // Destroy all host's player objects
            foreach (var obj in objectsToDestroy)
            {
                DestroyPlayerObject(obj);
            }
        }
    }
}