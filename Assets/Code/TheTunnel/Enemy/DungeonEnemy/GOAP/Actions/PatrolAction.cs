using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using TheTunnel.Goap;
using UnityEngine;
using UnityEngine.AI;

namespace TheTunnel.GOAP
{
    public class PatrolAction : ActionBase<CommonData>
    {
        private AttackBehavior _attackBehavior;
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, CommonData data)
        {
            data.timer = 5;
            data.Animator.SetBool(CommonData.Walking, true);
            _attackBehavior = agent.GetComponent<AttackBehavior>();
            _attackBehavior.SetTarget(null);
        }

        public override ActionRunState Perform(IMonoAgent agent, CommonData data, ActionContext context)
        {
            data.timer -= context.DeltaTime;

            if (data.timer > 0)
            {
                return ActionRunState.Continue;
            }

            return ActionRunState.Stop;
        }

        public override void End(IMonoAgent agent, CommonData data)
        {
            data.Animator.SetBool(CommonData.Walking, false);
        }
    }
}