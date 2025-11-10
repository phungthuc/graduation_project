using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using UnityEngine;
using UnityEngine.AI;

namespace TheTunnel.GOAP
{
    public class PatrolTargetSensors : LocalTargetSensorBase, IInjectable
    {
        private WanderConfigSO _wanderConfig;

        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IMonoAgent agent, IComponentReference references)
        {
            Vector3 position = GetRandomPosition(agent);
            return new PositionTarget(position);
        }
        
        private Vector3 GetRandomPosition(IMonoAgent  agent)
        {
            int count = 0;

            while (count < 5)
            {
                Vector2 random = Random.insideUnitCircle * _wanderConfig.wanderRadius;
                Vector3 position = agent.transform.position + new Vector3(
                    random.x,
                    0,
                    random.y
                );
                if (NavMesh.SamplePosition(position, out NavMeshHit hit, 2, NavMesh.AllAreas))
                {
                    return hit.position;
                }

                count++;
            }

            return agent.transform.position;
        }

        public void Inject(DependencyInjector injector)
        {
            _wanderConfig = injector.wanderConfigSo;
        }
    }
}