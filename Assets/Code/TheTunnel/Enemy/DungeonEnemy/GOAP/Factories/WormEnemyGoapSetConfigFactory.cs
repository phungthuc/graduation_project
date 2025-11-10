using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes.Builders;
using CrashKonijn.Goap.Configs.Interfaces;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Resolver;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class WormEnemyGoapSetConfigFactory : GoapSetFactoryBase
    {
        private DependencyInjector _injector;

        public override IGoapSetConfig Create()
        {
            _injector = GetComponentInParent<DependencyInjector>();
            GoapSetBuilder builder = new("WormEnemyDungeonSet");

            BuildGoals(builder);
            BuildActions(builder);
            BuildSensors(builder);

            return builder.Build();
        }

        private void BuildGoals(GoapSetBuilder builder)
        {
            builder.AddGoal<HiddenGoal>()
                .AddCondition<IsHiding>(Comparison.GreaterThanOrEqual, 1);

            builder.AddGoal<KillPlayerGoal>()
                .AddCondition<PlayerHealth>(Comparison.SmallerThanOrEqual, 0);
        }
        private void BuildActions(GoapSetBuilder builder)
        {
            builder.AddAction<HideAction>()
                .SetTarget<PlayerTarget>()
                .AddEffect<IsHiding>(EffectType.Increase)
                .SetBaseCost(5)
                .SetInRange(_injector.wormAttackConfigSo.sensorRadius);

            builder.AddAction<RiseUpAction>()
               .SetTarget<PlayerTarget>()
               .AddEffect<PlayerDistance>(EffectType.Decrease)
               .AddEffect<PlayerHealth>(EffectType.Decrease)
               .SetBaseCost(3)
               .SetInRange(_injector.wormAttackConfigSo.sensorRadius);

            builder.AddAction<WormAttackAction>()
               .AddCondition<PlayerDistance>(Comparison.SmallerThanOrEqual, 5)
               .SetTarget<PlayerTarget>()
               .AddEffect<PlayerDistance>(EffectType.Decrease)
               .AddEffect<PlayerHealth>(EffectType.Decrease)
               .SetBaseCost(1)
               .SetInRange(_injector.wormAttackConfigSo.sensorRadius);
        }
        private void BuildSensors(GoapSetBuilder builder)
        {
            builder.AddTargetSensor<PlayerTargetSensor>()
                .SetTarget<PlayerTarget>();

            builder.AddWorldSensor<PlayerDistanceSensor>()
                .SetKey<PlayerDistance>();
        }
    }
}