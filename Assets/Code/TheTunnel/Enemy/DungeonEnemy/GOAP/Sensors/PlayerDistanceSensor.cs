using cowsins;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using Unity.Netcode;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class PlayerDistanceSensor : LocalWorldSensorBase
    {
        private Transform _playerTransform;

        public override void Created() { }
        public override void Update() { }

        public override SenseValue Sense(IMonoAgent agent, IComponentReference references)
        {
            // CHỈ chạy trên server
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return new SenseValue(int.MaxValue);
            }

            // Tìm player gần nhất trong multiplayer
            _playerTransform = FindNearestPlayer(agent.transform.position);

            if (_playerTransform == null)
            {
                // Nếu không tìm thấy player, trả về distance rất lớn
                return new SenseValue(int.MaxValue);
            }

            return new SenseValue(
                Mathf.CeilToInt(Vector3.Distance(agent.transform.position, _playerTransform.position))
            );
        }

        private Transform FindNearestPlayer(Vector3 agentPosition)
        {
            Transform nearestPlayer = null;
            float nearestDistance = float.MaxValue;

            // CHỈ tìm players trên server
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return null;
            }

            // Tìm tất cả players trong multiplayer
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
                {
                    Transform playerTransform = client.PlayerObject.transform;
                    float distance = Vector3.Distance(agentPosition, playerTransform.position);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPlayer = playerTransform;
                    }
                }
            }

            return nearestPlayer;
        }
    }
}