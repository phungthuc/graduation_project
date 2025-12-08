using cowsins;
using TheTunnel.Core;
using TheTunnel.Manager;
using UnityEngine;
using Unity.Netcode;

namespace TheTunnel.Custom.cowsins
{
    public class DeathRestart : MonoBehaviour
    {
        private void Update()
        {
            if (InputManager.reloading)
            {
                PlayerData.Instance.ResetData();
                // TransitionScene.Instance.PlayTransitionScene(GameConstant.SCENE_DEFENSE_NAME, () => GameManager.Instance.StartCountDown());
                // NetworkManager.Singleton.SceneManager.LoadScene(
                //         GameConstant.SCENE_MAIN_NAME,
                //         UnityEngine.SceneManagement.LoadSceneMode.Single
                //     );
            }
        }
    }
}
