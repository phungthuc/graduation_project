using UnityEngine;
using Unity.Netcode;

public class Test : MonoBehaviour
{
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            Debug.Log("Client connected to server: " + id);
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            Debug.Log("Client disconnected from server: " + id);
        };
    }
}