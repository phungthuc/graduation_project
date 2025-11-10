using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TheTunnel;
using TheTunnel.Core;

public class NetworkButtonManager : MonoBehaviour
{
    [SerializeField] private Button _serverButton;
    [SerializeField] private Button _clientButton;
    [SerializeField] private Button _hostButton;

    [SerializeField] private NetworkManager _networkManager;

    private void Start()
    {
        RegisterListeners();
    }

    private void RegisterListeners()
    {
        _serverButton.onClick.RemoveAllListeners();
        _clientButton.onClick.RemoveAllListeners();
        _hostButton.onClick.RemoveAllListeners();
        _serverButton.onClick.AddListener(OnServerButtonClicked);
        _clientButton.onClick.AddListener(OnClientButtonClicked);
        _hostButton.onClick.AddListener(OnHostButtonClicked);
    }

    private void OnServerButtonClicked()
    {
        Debug.Log("Server button clicked");
        _networkManager.StartServer();
    }

    private void OnClientButtonClicked()
    {
        Debug.Log("Client button clicked");
        TransitionScene.Instance?.PlayTransitionScene(GameConstant.DUNGEON_SCENE_NAME, () =>
        {
            Debug.Log("Client button clicked");
            _networkManager.StartClient();
        });
    }

    private void OnHostButtonClicked()
    {
        Debug.Log("Host button clicked");
        TransitionScene.Instance?.PlayTransitionScene(GameConstant.DUNGEON_SCENE_NAME, () =>
        {
            _networkManager.StartHost();
        });
    }
}
