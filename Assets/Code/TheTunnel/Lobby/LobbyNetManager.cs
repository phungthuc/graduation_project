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

            var networkManager = NetworkManager.Singleton;

            networkManager.OnClientConnectedCallback += HandleClientConnected;
            networkManager.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (var client in networkManager.ConnectedClientsList)
            {
                // Host counts as ready by default so they can start solo.
                _readyStates[client.ClientId] = client.ClientId == networkManager.LocalClientId;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;

            var networkManager = NetworkManager.Singleton;
            networkManager.OnClientConnectedCallback -= HandleClientConnected;
            networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            // New joiners start as not ready; host stays ready.
            var isHost = clientId == NetworkManager.Singleton.LocalClientId;
            _readyStates[clientId] = isHost;
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            // Remove leavers to avoid ghost ready states blocking start.
            _readyStates.Remove(clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(bool ready, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            _readyStates[clientId] = ready;
        }

        public bool AllReady()
        {
            if (_readyStates.Count == 0) return false;

            // If only host is present, allow immediate start.
            if (_readyStates.Count == 1 && _readyStates.ContainsKey(NetworkManager.Singleton.LocalClientId))
            {
                return true;
            }

            foreach (var kv in _readyStates)
            {
                if (!kv.Value) return false;
            }
            return true;
        }
    }
}
