using TheTunnel.Core;
using TheTunnel.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace TheTunnel.Goap
{
    public class DungeonTeleportGate : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == GameConstant.PLAYER_TAG)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(
                    GameConstant.SCENE_MAIN_NAME,
                    UnityEngine.SceneManagement.LoadSceneMode.Single
                );
                this.enabled = false;
            }
        }
    }
}
