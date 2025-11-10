using System;
using cowsins;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using UnityEngine;

namespace TheTunnel.NPC
{
    public class EnemyTargetSensor : LocalTargetSensorBase
    {
        private Transform _enemyTransform;
        private Collider[] _colliders = new Collider[50]; // Increased collider count

        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IMonoAgent agent, IComponentReference references)
        {
            Array.Clear(_colliders, 0, _colliders.Length);

            var coverQuantity = Physics.OverlapSphereNonAlloc(
                agent.transform.position,
                50,
                _colliders,
                LayerMask.GetMask("Enemy")
            );

            float closestDistance = float.MaxValue;
            _enemyTransform = null;

            for (int i = 0; i < coverQuantity; i++)
            {
                var collider = _colliders[i];
                if (collider != null)
                {
                    float distance = Vector3.Distance(agent.transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        _enemyTransform = collider.transform;
                    }
                }
            }

            if (_enemyTransform != null)
            {
                return new TransformTarget(_enemyTransform);
            }

            return null;
        }
    }
}
