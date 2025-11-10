using cowsins;
using TheTunnel.Core;
using TheTunnel.Manager;
using UnityEngine;

namespace TheTunnel.Custom.cowsins
{
    public class DeathRestart : MonoBehaviour
    {
        private void Update()
        {
            if (InputManager.reloading)
            {
                PlayerData.Instance.ResetData();
                TransitionScene.Instance.PlayTransitionScene(GameConstant.SCENE_PLAY_NAME, () => GameManager.Instance.StartCountDown());
            }
        }
    }
}
