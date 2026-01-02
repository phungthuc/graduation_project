using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheTunnel.Core;
using UnityEngine.Events;

namespace TheTunnel.Lobby
{
    namespace TheTunnel.Lobby
    {
        public class LobbyUI : MonoBehaviour
        {
            [SerializeField] private LobbyNetManager lobbyNet;
            [SerializeField] private TextMeshProUGUI statusText;

            [SerializeField] private GameObject startGameButton;
            [SerializeField] private GameObject readyButton;

            [SerializeField] private TextMeshProUGUI readyText;

            bool isReady = false;

            private void OnEnable()
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            private void Start()
            {
                readyButton.SetActive(false);
                startGameButton.SetActive(false);
            }

            public void OnReadyClicked()
            {
                isReady = !isReady;
                readyText.text = isReady ? "Not Ready" : "Ready";
                lobbyNet.SetReadyServerRpc(isReady);
            }

            public void OnStartGameClicked()
            {
                if (!lobbyNet.AllReady())
                {
                    SetStatus("Not all players are ready");
                    return;
                }

                SetStatus("Loading Play scene...");
                NetworkManager.Singleton.SceneManager.LoadScene(
                    GameConstant.SCENE_DEFENSE_NAME,
                    UnityEngine.SceneManagement.LoadSceneMode.Single
                );
            }

            void SetStatus(string s)
            {
                if (statusText != null) statusText.text = s;
            }

            public void OnUserJoined()
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    startGameButton.SetActive(true);
                    readyButton.SetActive(false);
                }
                else
                {
                    readyButton.SetActive(true);
                    startGameButton.SetActive(false);
                    readyText.text = "Ready";
                }
            }
        }
    }
}