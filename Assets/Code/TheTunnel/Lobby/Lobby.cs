using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheTunnel.Core;

namespace TheTunnel.Lobby
{
    namespace TheTunnel.Lobby
    {
        public class LobbyUI : MonoBehaviour
        {
            [SerializeField] private LobbyNetManager lobbyNet;
            [SerializeField] private TextMeshProUGUI statusText;

            bool isReady = false;

            public void OnReadyClicked()
            {
                isReady = !isReady;
                lobbyNet.SetReadyServerRpc(isReady);
            }

            public void OnStartGameClicked()
            {
                // if (!lobbyNet.AllReady())
                // {
                //     SetStatus("Not all players are ready");
                //     return;
                // }

                SetStatus("Loading Play scene...");
                NetworkManager.Singleton.SceneManager.LoadScene(
                    GameConstant.SCENE_DEFENSE_NAME,
                    UnityEngine.SceneManagement.LoadSceneMode.Single
                );
            }

            void SetStatus(string s)
            {
                if (statusText != null) statusText.text = s;
                Debug.Log(s);
            }
        }
    }
}