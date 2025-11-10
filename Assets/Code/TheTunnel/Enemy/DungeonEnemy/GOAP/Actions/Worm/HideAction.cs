using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using TheTunnel.GOAP;
using UnityEngine;
using UnityEngine.AI;

namespace TheTunnel.GOAP
{
    public class HideAction : ActionBase<CommonData>, IInjectable
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, CommonData data)
        {
            agent.GetComponent<NavMeshAgent>().speed = 0;
            data.Animator.SetBool(CommonData.Appearing, false);
        }

        public override ActionRunState Perform(IMonoAgent agent, CommonData data, ActionContext context)
        {
            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, CommonData data)
        {
        }

        public void Inject(DependencyInjector injector)
        {
        }
    }
}
