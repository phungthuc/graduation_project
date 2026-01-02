using UnityEngine;
using Unity.Netcode;

public class Test : MonoBehaviour
{
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
        };
    }
}