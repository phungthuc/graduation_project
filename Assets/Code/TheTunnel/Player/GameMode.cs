using TheTunnel.Core;
using Unity.Netcode;
using UnityEngine;

namespace TheTunnel.Player
{

    public class GameMode : NetworkBehaviour
    {
        [Header("Player Spawn")]
        [SerializeField] private NetworkObject playerPrefab;
        [SerializeField] private Transform[] spawnPoints;

        private bool playersSpawned = false;

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

            // Đảm bảo ta xử lý cho scene Play
            if (sceneEvent.SceneEventType == SceneEventType.LoadComplete &&
                sceneEvent.SceneName == GameConstant.SCENE_PLAY_NAME)
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

                if (spawnPoints != null && spawnPoints.Length > 0)
                {
                    int index = i % spawnPoints.Length;
                    pos = spawnPoints[index].position;
                    rot = spawnPoints[index].rotation;
                }

                // Server instantiate Player
                NetworkObject playerInstance =
                    Instantiate(playerPrefab, pos, rot);

                // Gắn Player này cho clientId (bao gồm cả Host)
                playerInstance.SpawnAsPlayerObject(clientId);

                i++;
            }
        }
    }

}