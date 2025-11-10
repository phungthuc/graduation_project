using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes.Builders;
using CrashKonijn.Goap.Configs.Interfaces;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Resolver;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class SpiderEnemyGoapSetConfigFactory : GoapSetFactoryBase
    {
        private DependencyInjector _injector;

        public override IGoapSetConfig Create()
        {
            _injector = GetComponentInParent<DependencyInjector>();
            GoapSetBuilder builder = new("SpiderEnemyDungeonSet");

            BuildGoals(builder);
            BuildActions(builder);
            BuildSensors(builder);

            return builder.Build();
        }

        private void BuildGoals(GoapSetBuilder builder)
        {
            builder.AddGoal<PatrolGoal>()
                .AddCondition<IsPatrolling>(Comparison.GreaterThanOrEqual, 1);

            builder.AddGoal<KillPlayerGoal>()
                .AddCondition<PlayerHealth>(Comparison.SmallerThanOrEqual, 0);
        }
        private void BuildActions(GoapSetBuilder builder)
        {
            builder.AddAction<PatrolAction>()
                .SetTarget<PatrolTarget>()
                .AddEffect<IsPatrolling>(EffectType.Increase)
                .SetBaseCost(5)
                .SetInRange(10);

            builder.AddAction<SpiderRunAction>()
                .SetTarget<PlayerTarget>()
                .AddEffect<PlayerDistance>(EffectType.Increase)
                .AddEffect<PlayerHealth>(EffectType.Decrease)
                .SetBaseCost(3)
                .SetInRange(_injector.attackConfigSo.sensorRadius);

            builder.AddAction<JumpToPlayerAction>()
                .AddCondition<PlayerDistance>(Comparison.SmallerThanOrEqual, (int)_injector.spiderAttackConfigSo.attackRange)
                .AddCondition<PlayerDistance>(Comparison.GreaterThanOrEqual, 8)
                .SetTarget<PlayerTarget>()
                .AddEffect<PlayerDistance>(EffectType.Increase)
                .AddEffect<PlayerHealth>(EffectType.Decrease)
                .SetBaseCost(1)
                .SetInRange(_injector.attackConfigSo.sensorRadius);

            builder.AddAction<MeleeAction>()
               .AddCondition<PlayerDistance>(Comparison.SmallerThanOrEqual, 2)
               .SetTarget<PlayerTarget>()
               .AddEffect<PlayerDistance>(EffectType.Increase)
               .AddEffect<PlayerHealth>(EffectType.Decrease)
               .SetBaseCost(1)
               .SetInRange(_injector.attackConfigSo.sensorRadius);
        }
        private void BuildSensors(GoapSetBuilder builder)
        {
            builder.AddTargetSensor<PatrolTargetSensors>()
                .SetTarget<PatrolTarget>();

            builder.AddTargetSensor<PlayerTargetSensor>()
                .SetTarget<PlayerTarget>();

            builder.AddWorldSensor<PlayerDistanceSensor>()
                .SetKey<PlayerDistance>();
        }
    }
}