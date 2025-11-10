using UnityEngine;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using UnityEngine.AI;

namespace TheTunnel.NPC
{
    public class ShootEnemyAction : ActionBase<CommonNPCData>
    {
        private NPCAttackBehavior _attackBehavior;
        public override void Created()
        {

        }

        public override void Start(IMonoAgent agent, CommonNPCData data)
        {
            _attackBehavior = agent.GetComponent<NPCAttackBehavior>();
            agent.GetComponent<NavMeshAgent>().speed = 0;
            data.Animator.SetBool(CommonNPCData.Running, false);
            data.Animator.SetBool(CommonNPCData.Aiming, true);
            _attackBehavior.SetFireRate(0.5f);
        }

        public override ActionRunState Perform(IMonoAgent agent, CommonNPCData data, ActionContext context)
        {
            if (data.Target == null)
            {
                _attackBehavior.SetTarget(null);
                return ActionRunState.Stop;
            }
            _attackBehavior.SetTarget(data.Target);

            Vector3 targetDirection = (data.Target.Position - agent.transform.position).normalized;
            targetDirection.y = 0; // Keep rotation only on Y axis
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            agent.transform.rotation = Quaternion.Lerp(agent.transform.rotation, targetRotation, 5 * context.DeltaTime);

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, CommonNPCData data)
        {
            _attackBehavior.SetTarget(null);
            agent.GetComponent<NavMeshAgent>().speed = 1;
            data.Animator.SetBool(CommonNPCData.Aiming, false);
        }
    }
}