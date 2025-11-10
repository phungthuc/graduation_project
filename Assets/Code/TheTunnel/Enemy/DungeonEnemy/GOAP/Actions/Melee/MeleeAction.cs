using UnityEngine;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;

namespace TheTunnel.GOAP
{
    public class MeleeAction : ActionBase<CommonData>, IInjectable
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
            if (distance > _attackConfig.attackRange)
            {
                return ActionRunState.Stop;
            }else
            {
                data.Animator.SetTrigger(CommonData.Attack);
            }
            data.timer -= context.DeltaTime;
            if (data.timer > 0)
            {
                return ActionRunState.Continue;
            }
            return ActionRunState.Stop;
        }

        public override void End(IMonoAgent agent, CommonData data)
        {
        }

        public void Inject(DependencyInjector injector)
        {
            _attackConfig = injector.attackConfigSo;
        }
    }
}