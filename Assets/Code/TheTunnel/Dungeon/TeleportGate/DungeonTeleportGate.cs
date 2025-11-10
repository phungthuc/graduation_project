using TheTunnel.Core;
using TheTunnel.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheTunnel.Goap
{
    public class DungeonTeleportGate : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == GameConstant.PLAYER_TAG)
            {
                TransitionScene.Instance.PlayTransitionScene(GameConstant.SCENE_PLAY_NAME, () => GameManager.Instance.StartCountDown());
                this.enabled = false;
            }
        }
    }
}
