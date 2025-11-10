using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using TheTunnel.GOAP;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class WormAttackAction : ActionBase<CommonData>, IInjectable
    {
        private AttackConfigSO _attackConfig;

        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, CommonData data)
        {
            data.timer = _attackConfig.attackDelay;
        }

        public override ActionRunState Perform(IMonoAgent agent, CommonData data, ActionContext context)
        {
            float distance = Vector3.Distance(agent.transform.position, data.Target.Position);
            if (distance > 5)
            {
                return ActionRunState.Stop;
            }
            data.timer -= context.DeltaTime;
            if (data.timer < 0)
            {
                data.Animator.SetTrigger(CommonData.Attack);
                agent.GetComponent<Transform>().LookAt(data.Target.Position);
                return ActionRunState.Stop;
            }
            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, CommonData data)
        {
        }

        public void Inject(DependencyInjector injector)
        {
            _attackConfig = injector.wormAttackConfigSo;
        }
    }
}
