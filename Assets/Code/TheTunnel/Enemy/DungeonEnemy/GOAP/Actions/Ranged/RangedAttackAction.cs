using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using TheTunnel.Goap;
using UnityEngine;
using UnityEngine.AI;

namespace TheTunnel.GOAP
{
    public class RangedAttackAction : ActionBase<CommonData>, IInjectable
    {
        private AttackConfigSO _attackConfig;
        private AttackBehavior _attackBehavior;
        private Collider[] _colliders = new Collider[1];
        private float _rotationSpeed = 5f;

        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, CommonData data)
        {
            _attackBehavior = agent.GetComponent<AttackBehavior>();
            _attackBehavior.SetFireRate(_attackConfig.attackDelay);
            data.timer = _attackConfig.attackDelay;
            data.Animator.SetBool(CommonData.Aiming, true);
            agent.GetComponent<NavMeshAgent>().speed = 0;
        }

        public override ActionRunState Perform(IMonoAgent agent, CommonData data, ActionContext context)
        {
            if (data.Target == null || _attackBehavior == null)
                return ActionRunState.Stop;

            // float distance = Vector3.Distance(agent.transform.position, data.Target.Position);
            // if (distance > _attackConfig.attackRange)
            // {
            //     _attackBehavior.SetTarget(null);
            //     return ActionRunState.Stop;
            // }

            if (Physics.OverlapSphereNonAlloc(
                    agent.transform.position,
                    2,
                    _colliders,
                    LayerMask.GetMask("Metal")
                ) <= 0)
            {
                _attackBehavior.SetTarget(null);
                return ActionRunState.Stop;
            }
            else
            {
                Vector3 targetDirection = (data.Target.Position - agent.transform.position).normalized;
                targetDirection.y = 0; // Keep rotation only on Y axis
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                agent.transform.rotation = Quaternion.Lerp(agent.transform.rotation, targetRotation, _rotationSpeed * context.DeltaTime);

                // Attack
                _attackBehavior.SetTarget(data.Target);
                return ActionRunState.Continue;
            }

        }

        public override void End(IMonoAgent agent, CommonData data)
        {
            _attackBehavior.SetTarget(null);
            data.Animator.SetBool(CommonData.Aiming, false);
            agent.GetComponent<NavMeshAgent>().speed = 1;
        }

        public void Inject(DependencyInjector injector)
        {
            _attackConfig = injector.rangedAttackConfigSo;
        }
    }
}