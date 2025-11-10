using cowsins;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class PlayerDistanceSensor : LocalWorldSensorBase
    {
        private Transform _playerTransform;
        
        public override void Created() {}
        public override void Update() {}

        public override SenseValue Sense(IMonoAgent agent, IComponentReference references)
        {
             _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

            return new SenseValue(
                    Mathf.CeilToInt(Vector3.Distance(agent.transform.position, _playerTransform.position))
            );
        }
    }
}