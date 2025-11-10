using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;
using UnityEngine.AI;

namespace TheTunnel.GOAP
{
    public class RunAction : ActionBase<CommonData>, IInjectable
    {
        private AttackConfigSO _attackConfig;
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, CommonData data)
        {
            data.Animator.SetBool(CommonData.Running, true);
            agent.GetComponent<NavMeshAgent>().speed = 8f;
        }

        public override ActionRunState Perform(IMonoAgent agent, CommonData data, ActionContext context)
        {
            if (Vector3.Distance(agent.transform.position, data.Target.Position) <= _attackConfig.attackRange)
            {
                return ActionRunState.Stop;
            }
            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, CommonData data)
        {
            data.Animator.SetBool(CommonData.Running, false);
            agent.GetComponent<NavMeshAgent>().speed = 1f;
        }

        public void Inject(DependencyInjector injector)
        {
            _attackConfig = injector.attackConfigSo;
        }
    }
}
