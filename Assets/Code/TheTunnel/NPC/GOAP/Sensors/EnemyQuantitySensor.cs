using System.Linq;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using TheTunnel.GOAP;
using UnityEngine;

namespace TheTunnel.NPC
{
    public class EnemyQuantitySensor : LocalWorldSensorBase, IInjectable
    {
        private AttackConfigSO _attackConfig;
        private Collider[] _colliders = new Collider[50];
        public override void Created() { }
        public override void Update() { }

        public override SenseValue Sense(IMonoAgent agent, IComponentReference references)
        {
            var coverQuantity = Physics.OverlapSphereNonAlloc(
                    agent.transform.position,
                    _attackConfig.sensorRadius,
                    _colliders,
                    LayerMask.GetMask("Enemy")
                );
            if (coverQuantity > 0)
            {
                return new SenseValue(coverQuantity);
            }
            return 0;
        }

        public void Inject(DependencyInjector injector)
        {
            _attackConfig = injector.attackConfigSo;
        }
    }
}