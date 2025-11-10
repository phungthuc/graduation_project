using UnityEngine;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using UnityEngine.AI;

namespace TheTunnel.NPC
{
    public class CoverPlayerAction : ActionBase<CommonNPCData>
    {
        private NPCAttackBehavior _attackBehavior;
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, CommonNPCData data)
        {
            _attackBehavior = agent.GetComponent<NPCAttackBehavior>();
            _attackBehavior.SetTarget(null);
            agent.GetComponent<NavMeshAgent>().speed = 6;
            data.Animator.SetBool(CommonNPCData.Running, true);
        }

        public override ActionRunState Perform(IMonoAgent agent, CommonNPCData data, ActionContext context)
        {
            var PlayerDistance = Vector3.Distance(agent.transform.position, data.Target.Position);
            if (PlayerDistance < 7f)
            {
                agent.GetComponent<NavMeshAgent>().speed = 0;
                data.Animator.SetBool(CommonNPCData.Running, false);
            }
            else
            {
                agent.GetComponent<NavMeshAgent>().speed = 6;
                data.Animator.SetBool(CommonNPCData.Running, true);
            }
            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, CommonNPCData data)
        {
            agent.GetComponent<NavMeshAgent>().speed = 1;
            data.Animator.SetBool(CommonNPCData.Running, false);
        }
    }
}