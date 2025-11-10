using CrashKonijn.Goap.Behaviours;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class GoapSetBinder : MonoBehaviour
    {
        private GoapRunnerBehaviour _goapRunner;
        public AgentType agentType;

        private void Awake()
        {
            _goapRunner = GameObject.Find("GOAP Config").GetComponent<GoapRunnerBehaviour>();
            AgentBehaviour agent = GetComponent<AgentBehaviour>();
            agent.GoapSet = _goapRunner.GetGoapSet(DetermineGoal());
        }

        private string DetermineGoal()
        {
            switch (agentType)
            {
                case AgentType.EnemyDungeonSet:
                    return "EnemyDungeonSet";
                case AgentType.RangedEnemyDungeonSet:
                    return "RangedEnemyDungeonSet";
                case AgentType.SpiderEnemyDungeonSet:
                    return "SpiderEnemyDungeonSet";
                case AgentType.WormEnemyDungeonSet:
                    return "WormEnemyDungeonSet";
                case AgentType.NPC:
                    return "NPCSet";
            }

            return null;
        }
    }

    public enum AgentType
    {
        EnemyDungeonSet,
        RangedEnemyDungeonSet,
        SpiderEnemyDungeonSet,
        WormEnemyDungeonSet,
        NPC,
    }
}