using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using Unity.Netcode;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class PlayerTargetSensor : LocalTargetSensorBase
    {
        private Transform _playerTransform;

        public override void Created()
        {
        }

        public override void Update()
        {
        }

        private Transform FindNearestPlayer(Vector3 agentPosition)
        {
            Transform nearestPlayer = null;
            float nearestDistance = float.MaxValue;

            // CHỈ tìm players trên server (GOAP system có thể chạy trên client nhưng sensor chỉ nên chạy trên server)
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                // Nếu không phải server, return null (sensor không nên chạy trên client)
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

        public override ITarget Sense(IMonoAgent agent, IComponentReference references)
        {
            // CHỈ chạy trên server
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return null;
            }

            // Tìm player gần nhất với agent position
            _playerTransform = FindNearestPlayer(agent.transform.position);

            if (_playerTransform != null)
            {
                return new TransformTarget(_playerTransform);
            }

            return null;
        }
    }
}