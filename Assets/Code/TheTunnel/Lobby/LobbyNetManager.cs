using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TheTunnel.Lobby
{
    public class LobbyNetManager : NetworkBehaviour
    {
        private readonly Dictionary<ulong, bool> _readyStates = new();

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                _readyStates[client.ClientId] = false;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(bool ready, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            _readyStates[clientId] = ready;
            Debug.Log($"Client {clientId} ready = {ready}");
        }

        public bool AllReady()
        {
            if (_readyStates.Count == 0) return false;
            foreach (var kv in _readyStates)
            {
                if (!kv.Value) return false;
            }
            return true;
        }
    }
}
