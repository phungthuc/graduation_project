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
    }
}