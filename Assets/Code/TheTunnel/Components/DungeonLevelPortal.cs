using cowsins;
using TheTunnel.Core;
using TheTunnel.Level;
using Unity.Netcode;
using UnityEngine;

namespace TheTunnel.Components
{
    public class DungeonLevelPortal : NetworkBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == GameConstant.PLAYER_TAG)
            {
                this.enabled = false;

                PlayerData.Instance.ResetData();

                if (NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.Shutdown();

                    UnityEngine.SceneManagement.SceneManager.LoadScene(
                    GameConstant.SCENE_MAIN_NAME,
                    UnityEngine.SceneManagement.LoadSceneMode.Single
                    );
                }

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
