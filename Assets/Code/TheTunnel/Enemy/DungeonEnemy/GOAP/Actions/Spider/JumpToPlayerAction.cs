using System.Collections;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using TheTunnel.Goap;
using UnityEngine;
using UnityEngine.AI;

namespace TheTunnel.GOAP
{
    public class JumpToPlayerAction : ActionBase<CommonData>, IInjectable
    {
        private AttackConfigSO _attackConfig;
        private AttackBehavior _attackBehavior;

        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, CommonData data)
        {
            _attackBehavior = agent.GetComponent<AttackBehavior>();
        }

        public override ActionRunState Perform(IMonoAgent agent, CommonData data, ActionContext context)
        {
            var distance = Vector3.Distance(agent.transform.position, data.Target.Position);
            if (distance > _attackConfig.attackRange || (distance > 2f && distance < 8f))
            {
                return ActionRunState.Stop;
            }

            _attackBehavior.SetTarget(data.Target);
            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, CommonData data)
        {
            _attackBehavior.SetTarget(null);
        }

        public void Inject(DependencyInjector injector)
        {
            _attackConfig = injector.spiderAttackConfigSo;
        }

    }
}
