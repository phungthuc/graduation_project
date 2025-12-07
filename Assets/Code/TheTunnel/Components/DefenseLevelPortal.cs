using cowsins;
using TheTunnel.Core;
using TheTunnel.Level;
using Unity.Netcode;
using UnityEngine;

namespace TheTunnel.Components
{
    public class DefenseLevelPortal : NetworkBehaviour
    {
        /// <summary>
        /// Despawn tất cả players trong scene (chỉ Server mới có thể gọi)
        /// </summary>
        private void DespawnAllPlayers()
        {
            if (!IsServer) return;

            Debug.Log($"Despawning all players... Connected clients: {NetworkManager.Singleton.ConnectedClientsList.Count}");

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject != null)
                {
                    NetworkObject playerNetworkObject = client.PlayerObject;
                    if (playerNetworkObject != null && playerNetworkObject.IsSpawned)
                    {
                        Debug.Log($"Despawning player for client {client.ClientId}");
                        playerNetworkObject.Despawn();
                    }
                }
            }

            Debug.Log("All players have been despawned.");
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsOwner) { return; }
            if (collision.gameObject.tag == GameConstant.PLAYER_TAG)
            {
                // UIController.instance.crosshair.SetVisibility(false);
                // TransitionScene.Instance.PlayTransitionScene(GameConstant.SCENE_DUNGEON_NAME, () => LevelManager.Instance.LoadDungeonLevel(PlayerData.Instance.CurrentLevel));
                this.enabled = false;
                Debug.Log("Loading Dungeon scene...");

                // Despawn tất cả players trong scene (chỉ Server mới có thể despawn)
                if (IsServer)
                {
                    // DespawnAllPlayers();
                }

                NetworkManager.Singleton.SceneManager.LoadScene(
                    GameConstant.SCENE_DUNGEON_NAME,
                    UnityEngine.SceneManagement.LoadSceneMode.Single
                );
            }
        }
    }
}
