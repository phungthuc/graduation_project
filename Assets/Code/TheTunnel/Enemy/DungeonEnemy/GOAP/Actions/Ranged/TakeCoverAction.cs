using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using TheTunnel.Goap;
using UnityEngine;
using UnityEngine.AI;

namespace TheTunnel.GOAP
{
    public class TakeCoverAction : ActionBase<CommonData>, IInjectable
    {
        private bool _reachedCover;
        private AttackBehavior _attackBehavior;
        private Collider[] _colliders = new Collider[50];
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, CommonData data)
        {
            _reachedCover = false;
            data.Animator.SetBool(CommonData.Running, true);
            agent.GetComponent<NavMeshAgent>().speed = 5f;
            _attackBehavior = agent.GetComponent<AttackBehavior>();
            _attackBehavior.SetTarget(null);
        }

        public override ActionRunState Perform(IMonoAgent agent, CommonData data, ActionContext context)
        {
            if (data.Target == null)
                return ActionRunState.Stop;

            if (!_reachedCover)
            {
                float distance = Vector3.Distance(agent.transform.position, data.Target.Position);
                if (distance < 1f)
                {
                    _reachedCover = true;
                }
                return ActionRunState.Continue;
            }

            return ActionRunState.Stop;
        }

        public override void End(IMonoAgent agent, CommonData data)
        {
            data.Animator.SetBool(CommonData.Running, false);
            agent.GetComponent<NavMeshAgent>().speed = 1f;
        }

        public void Inject(DependencyInjector injector)
        {
        }
    }
}