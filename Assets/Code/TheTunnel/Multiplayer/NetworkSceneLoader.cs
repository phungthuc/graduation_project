using UnityEngine;
using Unity.Netcode;
using TheTunnel.Core;

namespace TheTunnel.Multiplayer
{
    public class NetworkSceneLoader : MonoBehaviour
    {
        void Start()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientStopped += OnClientStoppedHandler;
            }
        }

        private void OnClientStoppedHandler(bool obj)
        {
            if (!NetworkManager.Singleton.IsHost)
            {

                TransitionScene.Instance.PlayTransitionScene(GameConstant.SCENE_MAIN_NAME);
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientStopped -= OnClientStoppedHandler;
            }
        }
    }
}