using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes.Builders;
using CrashKonijn.Goap.Configs.Interfaces;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Resolver;
using TheTunnel.GOAP;
using UnityEngine;

namespace TheTunnel.NPC
{
    public class NPCGoapSetConfigFactory : GoapSetFactoryBase
    {
        private DependencyInjector _injector;

        public override IGoapSetConfig Create()
        {
            _injector = GetComponentInParent<DependencyInjector>();
            GoapSetBuilder builder = new("NPCSet");

            BuildGoals(builder);
            BuildActions(builder);
            BuildSensors(builder);

            return builder.Build();
        }

        private void BuildGoals(GoapSetBuilder builder)
        {
            builder.AddGoal<MoveToLocationGoal>()
                .AddCondition<IsMoving>(Comparison.GreaterThanOrEqual, 0);

            builder.AddGoal<AttackEnemyGoal>()
                .AddCondition<IsAttacking>(Comparison.GreaterThanOrEqual, 0);

            builder.AddGoal<HealGoal>()
                .AddCondition<IsHealing>(Comparison.GreaterThanOrEqual, 0);

            builder.AddGoal<HideGoal>()
                .AddCondition<IsHiding>(Comparison.GreaterThanOrEqual, 0);
        }
        private void BuildActions(GoapSetBuilder builder)
        {
            builder.AddAction<CoverPlayerAction>()
                .SetTarget<PlayerTarget>()
                .AddEffect<IsMoving>(EffectType.Increase)
                .SetBaseCost(5)
                .SetInRange(50);

            builder.AddAction<ShootEnemyAction>()
                .SetTarget<EnemyTarget>()
                .AddEffect<IsAttacking>(EffectType.Increase)
                .SetBaseCost(3)
                .SetInRange(50);
        }
        private void BuildSensors(GoapSetBuilder builder)
        {
            builder.AddTargetSensor<PlayerTargetSensor>()
                .SetTarget<PlayerTarget>();

            builder.AddTargetSensor<EnemyTargetSensor>()
                .SetTarget<EnemyTarget>();

            builder.AddWorldSensor<EnemyQuantitySensor>()
                .SetKey<EnemyQuantity>();
        }
    }
}