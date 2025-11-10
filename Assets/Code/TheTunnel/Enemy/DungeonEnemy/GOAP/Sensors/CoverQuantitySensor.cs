using System.Linq;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class CoverQuantitySensor : LocalWorldSensorBase, IInjectable
    {
        private AttackConfigSO _attackConfig;
        private Collider[] _colliders = new Collider[50];
        public override void Created() {}
        public override void Update() {}

        public override SenseValue Sense(IMonoAgent agent, IComponentReference references)
        {
            var coverQuantity = Physics.OverlapSphereNonAlloc(
                    agent.transform.position,
                    _attackConfig.sensorRadius,
                    _colliders,
                    LayerMask.GetMask("Metal")
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