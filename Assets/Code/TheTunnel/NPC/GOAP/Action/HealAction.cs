using UnityEngine;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;

namespace TheTunnel.NPC
{
    public class HealAction : ActionBase<CommonNPCData>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, CommonNPCData data)
        {
        }

        public override ActionRunState Perform(IMonoAgent agent, CommonNPCData data, ActionContext context)
        {

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, CommonNPCData data)
        {
        }
    }
}