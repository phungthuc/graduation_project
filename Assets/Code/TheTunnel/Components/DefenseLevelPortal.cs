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
            if (other.gameObject.tag == GameConstant.PLAYER_TAG)
            {
                this.enabled = false;

                NetworkManager.Singleton.SceneManager.LoadScene(
                    GameConstant.SCENE_DUNGEON_NAME,
                    UnityEngine.SceneManagement.LoadSceneMode.Single
                );
            }
        }
    }
}
