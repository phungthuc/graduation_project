using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class CoverTargetSensor : LocalTargetSensorBase, IInjectable
    {
        private AttackConfigSO _attackConfig;
        private Collider[] _colliders = new Collider[50]; // Increased collider count
        private Transform _targetPoint;

        public override void Created() { }
        public override void Update() { }

        public override ITarget Sense(IMonoAgent agent, IComponentReference references)
        {
            if (Physics.OverlapSphereNonAlloc(
                    agent.transform.position,
                    _attackConfig.sensorRadius,
                    _colliders,
                    LayerMask.GetMask("Metal")
                ) > 0)
            {
                var closestCover = GetClosestCover(agent.transform.position, _colliders);
                if (closestCover != null)
                {
                    // Find targetPoint in the cover object
                    _targetPoint = closestCover.GetChild(0);
                    if (_targetPoint != null)
                    {
                        return new TransformTarget(_targetPoint);
                    }
                }
            }

            return null;
        }

        private Transform GetClosestCover(Vector3 position, Collider[] colliders)
        {
            Transform closest = null;
            float closestDistance = float.MaxValue;

            foreach (var collider in colliders)
            {
                if (collider == null) continue;

                float distance = Vector3.Distance(position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = collider.transform;
                }
            }

            return closest;
        }

        public void Inject(DependencyInjector injector)
        {
            _attackConfig = injector.rangedAttackConfigSo;
        }
    }
}