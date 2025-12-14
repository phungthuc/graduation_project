using cowsins;
using TheTunnel.Core;
using TheTunnel.Level;
using Unity.Netcode;
using UnityEngine;

namespace TheTunnel.Components
{
    public class DefenseLevelPortal : NetworkBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("OnTriggerEnter: " + other.gameObject.tag);
            if (other.gameObject.tag == GameConstant.PLAYER_TAG)
            {
                this.enabled = false;
                Debug.Log("Loading Dungeon scene...");

                NetworkManager.Singleton.SceneManager.LoadScene(
                    GameConstant.SCENE_DUNGEON_NAME,
                    UnityEngine.SceneManagement.LoadSceneMode.Single
                );
            }
        }
    }
}
