using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes.Builders;
using CrashKonijn.Goap.Configs.Interfaces;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Resolver;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class RangedEnemyGoapSetConfigFactory : GoapSetFactoryBase
    {
        private DependencyInjector _injector;

        public override IGoapSetConfig Create()
        {
            _injector = GetComponentInParent<DependencyInjector>();
            GoapSetBuilder builder = new("RangedEnemyDungeonSet");

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

            builder.AddAction<TakeCoverAction>()
                .AddCondition<CoverQuantity>(Comparison.GreaterThan, 1)
                .SetTarget<CoverTarget>()
                .AddEffect<PlayerHealth>(EffectType.Decrease)
                .AddEffect<CoverDistance>(EffectType.Decrease)
                .AddEffect<CoverQuantity>(EffectType.Decrease)
                .SetBaseCost(5)
                .SetInRange(_injector.rangedAttackConfigSo.sensorRadius);

            builder.AddAction<RangedAttackAction>()
                .AddCondition<CoverQuantity>(Comparison.GreaterThan, 1)
                .AddCondition<CoverDistance>(Comparison.SmallerThanOrEqual, 1)
                .SetTarget<PlayerTarget>()
                .AddEffect<PlayerHealth>(EffectType.Decrease)
                .AddEffect<CoverDistance>(EffectType.Decrease)
                .AddEffect<CoverQuantity>(EffectType.Decrease)
                .SetBaseCost(_injector.rangedAttackConfigSo.attackCost)
                .SetInRange(40);

            builder.AddAction<RangedAttackWithoutCoverAction>()
                .AddCondition<CoverQuantity>(Comparison.SmallerThanOrEqual, 1)
                .SetTarget<PlayerTarget>()
                .AddEffect<PlayerHealth>(EffectType.Decrease)
                .SetBaseCost(_injector.rangedAttackConfigSo.attackCost)
                .SetInRange(40);
        }
        private void BuildSensors(GoapSetBuilder builder)
        {
            builder.AddTargetSensor<PatrolTargetSensors>()
                .SetTarget<PatrolTarget>();

            builder.AddTargetSensor<PlayerTargetSensor>()
                .SetTarget<PlayerTarget>();

            builder.AddTargetSensor<CoverTargetSensor>()
                .SetTarget<CoverTarget>();

            builder.AddWorldSensor<CoverTargetDistanceSensor>()
                .SetKey<CoverDistance>();

            builder.AddWorldSensor<CoverQuantitySensor>()
                .SetKey<CoverQuantity>();
        }
    }
}