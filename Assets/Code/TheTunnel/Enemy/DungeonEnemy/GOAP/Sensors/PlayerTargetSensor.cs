using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class PlayerTargetSensor : LocalTargetSensorBase
    {
        private Transform _playerTransform;
        
        public override void Created()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IMonoAgent agent, IComponentReference references)
        {

            return new TransformTarget(_playerTransform);
        }
    }
}