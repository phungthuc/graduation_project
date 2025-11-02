using Unity.Netcode;
using UnityEngine;

namespace TheTunnel.Scripts.UI.Network
{
    public class NetworkStartUI : MonoBehaviour
    {
        void OnGUI()
        {
            if (NetworkManager.Singleton == null)
            {
                GUI.Label(new Rect(10f, 10f, 300f, 30f), "NetworkManager not found in scene!");
                return;
            }

            float width = 200f, height = 30f;
            float x = 10f, y = 10f;

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                if (GUI.Button(new Rect(x, y, width, height), "Host")) NetworkManager.Singleton.StartHost();
                if (GUI.Button(new Rect(x, y + height + 10f, width, height), "Client")) NetworkManager.Singleton.StartClient();
                if (GUI.Button(new Rect(x, y + 2 * (height + 10f), width, height), "Server")) NetworkManager.Singleton.StartServer();
            }
        }
    }
}