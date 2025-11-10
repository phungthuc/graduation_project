using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using TheTunnel.GOAP;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class RiseUpAction : ActionBase<CommonData>, IInjectable
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, CommonData data)
        {
            agent.GetComponent<Transform>().LookAt(data.Target.Position);
            data.Animator.SetBool(CommonData.Appearing, true);
        }

        public override ActionRunState Perform(IMonoAgent agent, CommonData data, ActionContext context)
        {
            float distance = Vector3.Distance(agent.transform.position, data.Target.Position);
            if (distance < 5)
            {
                return ActionRunState.Stop;
            }
            agent.GetComponent<Transform>().LookAt(data.Target.Position);
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
