using TheTunnel.Core;
using TheTunnel.Manager;
using UnityEngine;

namespace TheTunnel.Lobby
{
    public class LobbyManager : MonoBehaviour
    {

        // Update this code to ensure all players are ready to play
        public void LoadPlayScene()
        {
            TransitionScene.Instance.PlayTransitionScene(GameConstant.SCENE_PLAY_NAME, () =>
            {
                GameManager.Instance.StartCountDown();
            });
        }
    }
}