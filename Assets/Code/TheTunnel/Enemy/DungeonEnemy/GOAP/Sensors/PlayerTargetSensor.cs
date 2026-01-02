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

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return null;
            }

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
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return null;
            }

            _playerTransform = FindNearestPlayer(agent.transform.position);

            if (_playerTransform != null)
            {
                return new TransformTarget(_playerTransform);
            }

            return null;
        }
    }
}