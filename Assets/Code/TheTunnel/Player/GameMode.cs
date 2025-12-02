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

        private void OnDestroy()
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

            int i = 0;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                ulong clientId = client.ClientId;

                Vector3 pos = Vector3.zero;
                Quaternion rot = Quaternion.identity;

                Transform spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").transform;
                if (spawnPoint != null)
                {
                    pos = spawnPoint.position;
                    rot = spawnPoint.rotation;
                }

                Debug.Log("Spawning player at: " + pos + " with rotation: " + rot);

                // Server instantiate Player
                NetworkObject playerInstance =
                    Instantiate(playerPrefab, pos, rot);

                // Gắn Player này cho clientId (bao gồm cả Host)
                playerInstance.SpawnAsPlayerObject(clientId);

                i++;
            }

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